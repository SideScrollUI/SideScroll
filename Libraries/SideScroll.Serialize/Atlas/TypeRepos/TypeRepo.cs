using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using System.Diagnostics;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public enum ObjectType
{
	Null,
	BaseType,
	DerivedType,
}

public interface IRepoCreator
{
	// needs to handle generics (lists, arrays, dictionaries)
	TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema);
}

// TypeRepo's can implement to preload object data first (example: TypeRepoHashSet)
public interface IPreloadRepo
{
	public void PreloadObjectData(object? obj);
}

// Represents all the object references for each unique type
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
		//new TypeRepoEnumerable.Creator(),
		//new TypeRepoUnknown.NoConstructorCreator(),
		//new TypeRepoObject.Creator(),
	];

	public readonly Serializer Serializer;
	public readonly TypeSchema TypeSchema;
	public readonly Type? Type; // might be null after loading
	public Type? LoadableType; // some types get overridden lazy load, or get removed [Unserialized]
	public int TypeIndex; // -1 if null

	public List<object> Objects = []; // ordered by index, not filled in when loading
	public int[]? ObjectSizes;
	public long[]? ObjectOffsets;
	public object?[] ObjectsLoaded;
	public int ObjectsLoadedCount;

	public BinaryReader? Reader;

	// Saving Only
	public Dictionary<object, int> IdxObjectToIndex = []; // for saving only, not filled in for loading

	// Loading Only

	public int Cloned = 0; // for stats

	public abstract void SaveObject(BinaryWriter writer, object obj);
	public virtual void LoadObjectData(object obj) { }
	public abstract void Clone(object source, object dest);
	public virtual void AddChildObjects(object obj) { }
	public virtual void InitializeSaving() { }
	public virtual void InitializeLoading(Log log) { }
	protected virtual void SaveCustomHeader(BinaryWriter writer) { }
	protected virtual void LoadCustomHeader() { }

	public override string ToString() => TypeSchema.Name;

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

	public static TypeRepo Create(Log log, Serializer serializer, TypeSchema typeSchema)
	{
		if (typeSchema.IsUnserialized)
		{
			//string message = "Type " + typeSchema.Name + " is not serializable";
			//Debug.Print(message);
			//log.Add(message);
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
				if (Debugger.IsAttached)
				{
					Debug.Fail(message);
				}
				else
				{
					Debug.Print(message); // For unit tests
				}
				log.AddWarning(message);
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
		if (!typeSchema.HasConstructor && !typeSchema.IsSerialized)
		{
			typeRepo = new TypeRepoUnknown(serializer, typeSchema);
			if (typeSchema.IsSerialized)
			{
				log.AddWarning("Type has no constructor", new Tag(typeSchema));
			}
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

	public void SaveSchema(BinaryWriter writer)
	{
		TypeSchema.NumObjects = Objects.Count;
		TypeSchema.Save(writer);
	}

	public void SkipHeader(BinaryWriter writer)
	{
		// todo: optimize this
		//writer.Write((int)0);
		/*foreach (var item in objects)
		{
			writer.Write((long)0);
		}*/
		foreach (var item in Objects)
		{
			writer.Write((int)0); // object size
		}
		SaveCustomHeader(writer);
	}

	public void SaveHeader(BinaryWriter writer)
	{
		// todo: optimize this
		//writer.Write(objectOffsets.Count);
		// offsets are a better solution if we don't read everything
		/*foreach (long offset in objectOffsets)
		{
			writer.Write(offset);
		}*/
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
		catch (Exception)
		{
		}
	}

	public void LoadHeader(Log log)
	{
		using LogTimer logTimer = log.Timer("Loading Headers",
			new Tag("Type", TypeSchema.Name),
			new Tag("Count", TypeSchema.NumObjects));

		ObjectOffsets = new long[TypeSchema.NumObjects];
		ObjectSizes = new int[TypeSchema.NumObjects];
		long offset = TypeSchema.FileDataOffset;
		for (int i = 0; i < TypeSchema.NumObjects; i++)
		{
			int size = Reader!.ReadInt32();
			ObjectOffsets[i] = offset;
			ObjectSizes[i] = size;
			offset += size;
		}
		//objects.AddRange(Enumerable.Repeat(null, count));
		//for (int i = 0; i < count; i++)
		//	objects.Add(null);

		LoadCustomHeader();
	}

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
			//objectOffsets.Add(objectStart);
			//objectSizes.Add((int)(objectEnd - objectStart));
			ObjectSizes[index++] = (int)(objectEnd - objectStart);

			logTimer.AddDebug("Saved Object", new Tag(TypeSchema.Name, obj));
		}

		//long end = writer.BaseStream.Position;

		//typeSchema.fileDataOffset = start;
		//typeSchema.dataSize = end - start;

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

	// Creates one if required
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
					LoadObject();
				else
					Reader.ReadInt32(); // objectIndex
			}
		}
		else
		{
			Reader.ReadInt32(); // objectIndex
		}
		return null;
	}

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
			else if (objectType == ObjectType.DerivedType)
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
			else
			{
				return null;
			}
		}
		else
		{
			int objectIndex = Reader.ReadInt32();
			return LoadObject(objectIndex);
		}
	}

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

	/*protected virtual object? CreateObject(byte[] bytes, ref int byteOffset)
	{
		return Activator.CreateInstance(type, true);
	}*/

	public virtual void Dispose()
	{

	}
}
