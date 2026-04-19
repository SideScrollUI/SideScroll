using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using System.Diagnostics;

namespace SideScroll.Serialize.Atlas.TypeRepos;

/// <summary>
/// Specifies the type of object reference being serialized
/// </summary>
public enum ObjectType
{
	/// <summary>
	/// Object reference is null
	/// </summary>
	Null,
	/// <summary>
	/// Object is the same type as the base type
	/// </summary>
	BaseType,
	/// <summary>
	/// Object is a derived type from the base type
	/// </summary>
	DerivedType,
}

/// <summary>
/// Interface for creating type-specific repository instances
/// </summary>
public interface IRepoCreator
{
	/// <summary>
	/// Attempts to create a type repository for the given type schema
	/// </summary>
	TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema);
}

/// <summary>
/// Interface for type repositories that need to preload object data (example: TypeRepoHashSet)
/// </summary>
public interface IPreloadRepo
{
	/// <summary>
	/// Preloads object data before full deserialization
	/// </summary>
	public void PreloadObjectData(object? obj);
}

/// <summary>
/// Base class that manages serialization and deserialization of objects for a specific type
/// </summary>
public abstract class TypeRepo : IDisposable
{
	// Should we switch this to List<Type> instead?
	public static List<IRepoCreator> RepoCreators { get; set; } =
	[
		new TypeRepoUnknown.Creator(),
		new TypeRepoPrimitive.Creator(),
		new TypeRepoEnum.Creator(),
		new TypeRepoString.Creator(),
		new TypeRepoDateTime.Creator(),
		new TypeRepoDateTimeOffset.Creator(),
		new TypeRepoTimeSpan.Creator(),
		new TypeRepoTimeZoneInfo.Creator(),
		new TypeRepoType.Creator(),
		new TypeRepoArrayBytes.Creator(),
		new TypeRepoArray.Creator(),
		new TypeRepoList.Creator(),
		new TypeRepoDictionary.Creator(),
		new TypeRepoHashSet.Creator(),
		new TypeRepoVersion.Creator(),
		new TypeRepoDecimal.Creator(),
		//new TypeRepoEnumerable.Creator(),
		//new TypeRepoUnknown.NoConstructorCreator(),
		//new TypeRepoObject.Creator(),
	];

	/// <summary>
	/// Gets the parent serializer instance
	/// </summary>
	public Serializer Serializer { get; }

	/// <summary>
	/// Gets the type schema for this repository
	/// </summary>
	public TypeSchema TypeSchema { get; }

	/// <summary>
	/// Gets the runtime type (might be null after loading if type no longer exists)
	/// </summary>
	public Type? Type { get; }

	/// <summary>
	/// Gets or sets the type that can be loaded (some types get overridden for lazy load, or get removed if [Unserialized])
	/// </summary>
	public Type? LoadableType { get; protected set; }

	/// <summary>
	/// Gets or sets the type index in the serializer's type list
	/// </summary>
	public int TypeIndex { get; set; }

	/// <summary>
	/// Gets the list of objects being serialized (ordered by index, not filled in when loading)
	/// </summary>
	public List<object> Objects { get; } = [];

	/// <summary>
	/// Gets or sets the array of object sizes in bytes
	/// </summary>
	public int[]? ObjectSizes { get; protected set; }

	/// <summary>
	/// Gets or sets the array of object file offsets
	/// </summary>
	public long[]? ObjectOffsets { get; protected set; }

	/// <summary>
	/// Gets or sets the array of loaded objects
	/// </summary>
	public object?[] ObjectsLoaded { get; set; }

	/// <summary>
	/// Gets the count of objects that have been loaded
	/// </summary>
	public int ObjectsLoadedCount { get; protected set; }

	/// <summary>
	/// Gets or sets the binary reader for loading serialized data
	/// </summary>
	public BinaryReader? Reader { get; set; }

	/// <summary>
	/// Gets the dictionary mapping objects to their indices (for saving only, not filled in for loading)
	/// </summary>
	public Dictionary<object, int> IdxObjectToIndex { get; } = [];

	private bool _disposed;

	/// <summary>
	/// Gets or sets the count of cloned objects (for statistics)
	/// </summary>
	public int Cloned { get; set; }

	/// <summary>
	/// Saves an object to the binary writer
	/// </summary>
	public abstract void SaveObject(BinaryWriter writer, object obj);

	/// <summary>
	/// Loads data into an existing object instance
	/// </summary>
	public virtual void LoadObjectData(object obj) { }

	/// <summary>
	/// Clones data from source object to destination object
	/// </summary>
	public abstract void Clone(object source, object dest);

	/// <summary>
	/// Adds child objects to the serialization queue
	/// </summary>
	public virtual void AddChildObjects(object obj) { }

	/// <summary>
	/// Initializes the repository for saving operations
	/// </summary>
	public virtual void InitializeSaving() { }

	/// <summary>
	/// Initializes the repository for loading operations
	/// </summary>
	public virtual void InitializeLoading(Log log) { }

	/// <summary>
	/// Saves custom header data for specialized repositories
	/// </summary>
	protected virtual void SaveCustomHeader(BinaryWriter writer) { }

	/// <summary>
	/// Loads custom header data for specialized repositories
	/// </summary>
	protected virtual void LoadCustomHeader() { }

	public override string ToString() => TypeSchema.Name;

	/// <summary>
	/// Initializes a new instance of the TypeRepo class
	/// </summary>
	protected TypeRepo(Serializer serializer, TypeSchema typeSchema)
	{
		Serializer = serializer;
		TypeSchema = typeSchema;
		Type = typeSchema.Type;
		if (!typeSchema.IsUnserialized && (!serializer.PublicOnly || TypeSchema.IsPublicOnly))
		{
			LoadableType = Type;
		}
		ObjectsLoaded = new object?[typeSchema.NumObjects];
	}

	/// <summary>
	/// Creates the appropriate type repository for the given type schema
	/// </summary>
	public static TypeRepo Create(Log log, Serializer serializer, TypeSchema typeSchema)
	{
		if (typeSchema.IsUnserialized)
		{
			var typeRepoUnknown = new TypeRepoUnknown(serializer, typeSchema)
			{
				Reader = serializer.Reader,
			};
			return typeRepoUnknown;
		}

		if (serializer.PublicOnly && !typeSchema.IsPublicOnly)
		{
			if (!typeSchema.IsPrivate)
			{
				string message = "Type " + typeSchema.Name + " does not specify [PublicData], [ProtectedData], or [PrivateData], ignoring";
				log.AddWarning(message);
				if (Debugger.IsAttached)
				{
					Debug.Fail(message);
				}
				else
				{
					Debug.Print(message); // For unit tests
				}
			}
			var typeRepoUnknown = new TypeRepoUnknown(serializer, typeSchema)
			{
				Reader = serializer.Reader,
			};
			return typeRepoUnknown;
		}

		TypeRepo? typeRepo;

		foreach (IRepoCreator creator in RepoCreators)
		{
			typeRepo = creator.TryCreateRepo(serializer, typeSchema);
			if (typeRepo != null)
			{
				typeRepo.Reader = serializer.Reader;
				return typeRepo;
			}
		}

		// Derived types can still have valid constructors
		if (!typeSchema.HasConstructor)
		{
			typeRepo = new TypeRepoUnknown(serializer, typeSchema);
		}
		else
		{
			typeRepo = new TypeRepoObject(serializer, typeSchema);
		}
		typeRepo.Reader = serializer.Reader;
		return typeRepo;
	}

	/*public virtual void CreateObjects()
	{
		// type might have disappeared (i.e. renamed, deleted, etc)
		if (type == null)
			return;

		// primitives get loaded first
		if (type.IsEnum == false && this is TypePrimitive)
			return;

		for (int i = 0; i < typeSchema.numObjects; i++)
		{
			objects.Add(Activator.CreateInstance(type, true));
		}
	}*/

	/// <summary>
	/// Saves the type schema to the binary writer
	/// </summary>
	public void SaveSchema(BinaryWriter writer)
	{
		TypeSchema.NumObjects = Objects.Count;
		TypeSchema.Save(writer);
	}

	/// <summary>
	/// Reserves space in the file for object headers during the first pass
	/// </summary>
	public void SkipHeader(BinaryWriter writer)
	{
		byte[] buffer = new byte[Objects.Count * sizeof(int)];
		writer.Write(buffer, 0, buffer.Length);

		SaveCustomHeader(writer);
	}

	/// <summary>
	/// Saves the object headers with size information
	/// </summary>
	public void SaveHeader(Log log, BinaryWriter writer)
	{
		// For UnknownTypeRepo
		if (ObjectSizes == null)
			return;

		try
		{
			foreach (int size in ObjectSizes)
			{
				writer.Write(size);
			}
			SaveCustomHeader(writer);
		}
		catch (Exception e)
		{
			log.Throw(e);
		}
	}

	/// <summary>
	/// Loads the object headers including sizes and offsets
	/// </summary>
	public void LoadHeader(Log log)
	{
		using LogTimer logTimer = log.Timer("Loading Headers",
			new Tag("Type", TypeSchema.Name),
			new Tag("Count", TypeSchema.NumObjects));

		ObjectOffsets = new long[TypeSchema.NumObjects];
		ObjectSizes = new int[TypeSchema.NumObjects];
		long offset = TypeSchema.StartDataOffset;
		for (int i = 0; i < TypeSchema.NumObjects; i++)
		{
			int size = Reader!.ReadInt32();
			ObjectOffsets[i] = offset;
			ObjectSizes[i] = size;
			offset += size;
		}

		LoadCustomHeader();
	}

	/// <summary>
	/// Saves all objects managed by this repository
	/// </summary>
	public void SaveObjects(Log log, BinaryWriter writer)
	{
		using LogTimer logTimer = log.Timer("Serializing (" + TypeSchema.Name + ")");

		//long start = writer.BaseStream.Position;

		ObjectSizes = new int[Objects.Count];
		int index = 0;
		foreach (object obj in Objects)
		{
			long objectStart = writer.BaseStream.Position;
			SaveObject(writer, obj);
			long objectEnd = writer.BaseStream.Position;
			ObjectSizes[index++] = (int)(objectEnd - objectStart);

			logTimer.AddDebug("Saved Object", new Tag(TypeSchema.Name, obj));
		}

		logTimer.Add("Saved Type Objects",
			new Tag("Type", Type),
			new Tag("Count", Objects.Count),
			new Tag("Bytes", writer.BaseStream.Position));
	}

	/*public void LoadObjects(Log log)
	{
		using (LogTimer logTimer = log.Timer("Loading " + ToString()))
		{
			logTimer.Add("Loading Object",
				new Tag("Type", this.type),
				new Tag("Count", typeSchema.numObjects),
				new Tag("Offset", typeSchema.fileDataOffset),
				new Tag("Bytes", typeSchema.dataSize));

			reader.BaseStream.Position = typeSchema.fileDataOffset;

			long start = reader.BaseStream.Position;

			LoadObjectData(logTimer, reader);

			long end = reader.BaseStream.Position;
			long size = (end - start);

			//Debug.Assert(size == typeSchema.dataSize);
			// todo: add log
		}
	}*/

	// todo: to avoid an extra dictionary lookup?
	/*public struct ItemLink
	{
		public object obj;
		public TypeRepo typeRepo;
	}*/

	/// <summary>
	/// Gets the index for an existing object or adds it to the repository and returns the new index
	/// </summary>
	public int GetOrAddObjectRef(object obj)
	{
		if (LoadableType == null || LoadableType.IsPrimitive)
			return -1;

		if (!IdxObjectToIndex.TryGetValue(obj, out int index))
		{
			index = IdxObjectToIndex.Count;
			IdxObjectToIndex[obj] = index;
			Objects.Add(obj);
			Serializer.ParserQueue.Enqueue(obj);
		}
		return index;
	}

	/// <summary>
	/// Loads a lazy object reference that will be loaded on demand
	/// </summary>
	public TypeRef? LoadLazyObjectRef()
	{
		ObjectType objectType = (ObjectType)Reader!.ReadByte();
		if (objectType == ObjectType.Null)
			return null;

		var typeRef = new TypeRef();

		if (TypeSchema.HasSubType)
		{
			if (objectType == ObjectType.BaseType)
			{
				typeRef.TypeRepo = this;
			}
			else if (objectType == ObjectType.DerivedType)
			{
				int typeIndex = Reader.ReadInt16(); // not saved for sealed classes
				typeRef.TypeRepo = Serializer.TypeRepos[typeIndex];
				/*if (typeRef.typeRepo.typeSchema.type.IsPrimitive)
				{
					LoadObject(); // throw it away
					return typeRef;
				}*/
			}
		}
		else
		{
			typeRef.TypeRepo = this;
		}
		typeRef.Index = Reader.ReadInt32();
		return typeRef;
	}

	/// <summary>
	/// Skips an object reference in the stream without loading it
	/// </summary>
	public object? SkipObjectRef()
	{
		ObjectType objectType = (ObjectType)Reader!.ReadByte();
		if (objectType == ObjectType.Null) return null;

		if (TypeSchema.IsPrimitive) return LoadObject();

		if (TypeSchema.HasSubType)
		{
			if (objectType == ObjectType.BaseType)
			{
				Reader.ReadInt32(); // objectIndex
			}
			else if (objectType == ObjectType.DerivedType)
			{
				int typeIndex = Reader.ReadInt16(); // not saved for sealed classes
				TypeRepo typeRepo = Serializer.TypeRepos[typeIndex];
				if (typeRepo.TypeSchema.Type!.IsPrimitive)
				{
					LoadObject();
				}
				else
				{
					Reader.ReadInt32(); // objectIndex
				}
			}
		}
		else
		{
			Reader.ReadInt32(); // objectIndex
		}
		return null;
	}

	/// <summary>
	/// Loads an object reference from the binary reader
	/// </summary>
	public object? LoadObjectRef()
	{
		ObjectType objectType = (ObjectType)Reader!.ReadByte();
		if (objectType == ObjectType.Null)
			return null;

		if (TypeSchema.IsPrimitive)
			return LoadObject();

		if (TypeSchema.HasSubType)
		{
			if (objectType == ObjectType.BaseType)
			{
				int objectIndex = Reader.ReadInt32();
				return LoadObject(objectIndex);
			}

			if (objectType == ObjectType.DerivedType)
			{
				int typeIndex = Reader.ReadInt16();
				if (typeIndex >= Serializer.TypeRepos.Count)
					return null;

				TypeRepo typeRepo = Serializer.TypeRepos[typeIndex];
				if (typeRepo.TypeSchema.IsPrimitive) // object ref can point to primitives
					return typeRepo.LoadObject();

				int objectIndex = Reader.ReadInt32();
				return typeRepo.LoadObject(objectIndex);
			}

			return null;
		}
		else
		{
			int objectIndex = Reader.ReadInt32();
			return LoadObject(objectIndex);
		}
	}

	/// <summary>
	/// Loads an object reference from a byte array
	/// </summary>
	public object? LoadObjectRef(byte[] bytes, ref int byteOffset)
	{
		bool isNull = Convert.ToBoolean(bytes[byteOffset++]);
		if (isNull)
			return null;

		if (LoadableType!.IsPrimitive)
			return LoadObject();

		int objectIndex = BitConverter.ToInt32(bytes, byteOffset);
		byteOffset += sizeof(int);

		if (!TypeSchema.HasSubType)
			return LoadObject(objectIndex);

		//int typeIndex = reader.ReadInt16(); // not saved for sealed classes
		int typeIndex = BitConverter.ToInt16(bytes, byteOffset);
		byteOffset += sizeof(short);
		TypeRepo typeRepo = Serializer.TypeRepos[typeIndex];
		//if (type == null) // type might have disappeared or been renamed
		//	return null;

		return typeRepo.LoadObject(objectIndex);
	}

	/// <summary>
	/// Loads an object without an index (for primitive types)
	/// </summary>
	public virtual object? LoadObject()
	{
		return null;
	}

	private object? GetObjectAt(int objectIndex)
	{
		object? obj = ObjectsLoaded[objectIndex];
		Reader!.BaseStream.Position = ObjectOffsets![objectIndex];
		return obj;
	}

	/// <summary>
	/// Preloads object data at the specified index for repositories that implement IPreloadRepo
	/// </summary>
	public void PreloadObjectData(int objectIndex)
	{
		if (this is IPreloadRepo preload)
		{
			object? obj = GetObjectAt(objectIndex);

			try
			{
				preload.PreloadObjectData(obj);
			}
			catch (Exception)
			{
			}
		}
	}

	/// <summary>
	/// Loads object data at the specified index
	/// </summary>
	public void LoadObjectData(int objectIndex)
	{
		object? obj = GetObjectAt(objectIndex);
		if (obj == null) return;

		try
		{
			LoadObjectData(obj);
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// Loads an object at the specified index, creating it if necessary
	/// </summary>
	public object? LoadObject(int objectIndex)
	{
		if (LoadableType == null) // type might have disappeared or been renamed
			return null; // should we pass a "ref bool valid"?

		if (objectIndex >= ObjectsLoaded.Length)
			return null;

		if (ObjectsLoaded[objectIndex] is object existingObject)
			return existingObject;

		ObjectsLoadedCount++;

		object? obj = CreateObject(objectIndex);
		return obj;
	}

	/// <summary>
	/// Loads an object and all its dependencies immediately
	/// </summary>
	public object? LoadFullObject(int objectIndex)
	{
		if (ObjectsLoaded[objectIndex] is object existingObject)
			return existingObject;

		object? obj = CreateObject(objectIndex);

		Serializer.ProcessLoadQueue();

		return obj;
	}

	protected virtual object? CreateObject(int objectIndex)
	{
		object obj = Activator.CreateInstance(LoadableType!, true)!;
		ObjectsLoaded[objectIndex] = obj; // must assign before loading any more refs
		Serializer.QueueLoading(this, objectIndex);
		return obj;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			// Dispose managed resources
			// Note: Reader is shared and should not be disposed here

			// Clear collections
			Objects.Clear();
			IdxObjectToIndex.Clear();
		}

		_disposed = true;
	}

	public virtual void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Validates that the requested number of bytes is available in the stream
	/// </summary>
	public void ValidateBytesAvailable(int requested)
	{
		long available = TypeSchema.EndDataOffset - Reader!.BaseStream.Position;

		if (requested > TypeSchema.DataSize || requested > available)
		{
			throw new SerializerException("Requested byte count is larger than available size",
				new Tag("Requested", requested),
				new Tag("DataSize", TypeSchema.DataSize),
				new Tag("Available", available));
		}
	}
}
