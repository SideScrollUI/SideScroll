using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using SideScroll.Serialize.Atlas.TypeRepos;

namespace SideScroll.Serialize.Atlas;

/// <summary>
/// Exception thrown when serialization or deserialization errors occur
/// </summary>
public class SerializerException(string text, params Tag[] tags) :
	TaggedException(text, tags);

/// <summary>
/// Core serializer class that handles Atlas format serialization and deserialization
/// </summary>
public class Serializer : IDisposable
{
	/// <summary>
	/// Magic number identifying the start of object data 
	/// SCRL in little endian format
	/// LRCS in big endian format (76, 82, 67, 83)
	/// </summary>
	public const uint ScrollId = 0x4C524353;

	/// <summary>
	/// Gets the serializer header containing metadata about the serialized data
	/// </summary>
	public SerializerHeader Header { get; } = new();

	/// <summary>
	/// Gets the list of type schemas for all types in the serialized data
	/// </summary>
	public List<TypeSchema> TypeSchemas { get; } = [];

	/// <summary>
	/// Gets the list of type repositories managing serialized instances
	/// </summary>
	public List<TypeRepo> TypeRepos { get; } = [];

	/// <summary>
	/// Gets the dictionary mapping types to their type repositories
	/// </summary>
	public Dictionary<Type, TypeRepo> IdxTypeToRepo { get; } = [];

	/// <summary>
	/// Gets or sets the string type repository used to reuse string instances during cloning
	/// </summary>
	public TypeRepoString? TypeRepoString { get; set; }

	/// <summary>
	/// Gets or sets the binary reader for loading serialized data
	/// </summary>
	public BinaryReader? Reader { get; protected set; }

	/// <summary>
	/// Gets or sets whether to use lazy loading for objects
	/// </summary>
	public bool Lazy { get; set; }

	/// <summary>
	/// Gets or sets whether to serialize only public data
	/// </summary>
	public bool PublicOnly { get; set; }

	/// <summary>
	/// Gets or sets whether to enable mapping between fields and properties during deserialization
	/// </summary>
	public bool EnableFieldToPropertyMapping { get; set; } = true;

	/// <summary>
	/// Gets the queue of objects waiting to be parsed
	/// </summary>
	public Queue<object> ParserQueue { get; } = [];

	/// <summary>
	/// Gets the list of primitive values (primitives are usually serialized inline, but not when they're the primary type)
	/// </summary>
	public List<object?> Primitives { get; } = [];

	/// <summary>
	/// Gets the dictionary tracking cloned objects
	/// </summary>
	protected Dictionary<object, object> Clones { get; } = [];

	/// <summary>
	/// Gets the queue of clone operations to process
	/// </summary>
	protected Queue<Action> CloneQueue { get; } = new();

	private bool _disposed;

	/// <summary>
	/// Represents an item in the load queue
	/// </summary>
	private struct LoadItem
	{
		public TypeRepo TypeRepo;
		public int Index;
		public bool Preloaded; // set after IPreloadRepo preloads data

		public readonly override string ToString() => $"{TypeRepo} - {Index}";
	}

	private readonly Queue<LoadItem> _loadQueue = new();

	/// <summary>
	/// Gets the base object from the serialized data
	/// </summary>
	public object? BaseObject(Call call)
	{
		if (TypeRepos.Count == 0)
		{
			call.Log.Throw(new SerializerException("No TypeRepos found"));
		}

		TypeRepo typeRepo = TypeRepos[0];
		if (typeRepo.LoadableType == null)
		{
			call.Log.Throw(new SerializerException("BaseObject type isn't loadable", new Tag("Type", typeRepo.TypeSchema.Name)));
		}

		if (typeRepo.Type!.IsPrimitive)
		{
			return Primitives[0];
		}

		using CallTimer callSaving = call.Timer("Load BaseObject");
		return LoadObject(typeRepo, 0);
	}

	/// <summary>
	/// Loads an object from a type repository at the specified index
	/// </summary>
	public object? LoadObject(TypeRepo typeRepo, int index)
	{
		object? obj = typeRepo.LoadObject(index);

		ProcessLoadQueue();

		return obj;
	}

	/// <summary>
	/// Processes the queue of objects waiting to be loaded
	/// </summary>
	public void ProcessLoadQueue()
	{
		while (_loadQueue.Count > 0)
		{
			LoadItem loadItem = _loadQueue.Dequeue();
			if (loadItem.TypeRepo is IPreloadRepo && !loadItem.Preloaded)
			{
				loadItem.TypeRepo.PreloadObjectData(loadItem.Index);
				loadItem.Preloaded = true;
				_loadQueue.Enqueue(loadItem);
			}
			else
			{
				loadItem.TypeRepo.LoadObjectData(loadItem.Index);
			}
		}
	}

	/// <summary>
	/// Record containing information about loaded objects for a type
	/// </summary>
	public record ObjectsLoaded
	{
		public string Name { get; init; } = default!;
		public int Loaded { get; init; }
	}

	/// <summary>
	/// Logs information about all loaded types
	/// </summary>
	public void LogLoadedTypes(Call call)
	{
		List<ObjectsLoaded> loaded = TypeRepos
			.Select(typeRepo => new ObjectsLoaded
			{
				Name = typeRepo.ToString(),
				Loaded = typeRepo.ObjectsLoadedCount
			})
			.ToList();

		call.Log.Add("Objects Loaded", new Tag("Type Repos", loaded));
	}

	/// <summary>
	/// Adds type repositories for all member types in object schemas
	/// </summary>
	private void AddObjectMemberTypes(Log log)
	{
		// TypeSchemas can grow as members are added, don't use enumerable
		for (int i = 0; i < TypeSchemas.Count; i++)
		{
			TypeSchema typeSchema = TypeSchemas[i];
			if (PublicOnly && !typeSchema.IsPublicOnly)
				continue;

			foreach (FieldSchema fieldSchema in typeSchema.FieldSchemas)
			{
				Type type = fieldSchema.NonNullableType!;
				TypeRepo typeRepo = GetOrCreateRepo(log, type);
				fieldSchema.FieldTypeSchema = typeRepo.TypeSchema;
				fieldSchema.TypeIndex = fieldSchema.FieldTypeSchema.TypeIndex;
			}

			foreach (PropertySchema propertySchema in typeSchema.PropertySchemas)
			{
				Type type = propertySchema.NonNullableType!;
				TypeRepo typeRepo = GetOrCreateRepo(log, type);
				propertySchema.PropertyTypeSchema = typeRepo.TypeSchema;
				propertySchema.TypeIndex = propertySchema.PropertyTypeSchema.TypeIndex;
			}
		}
	}

	// properties might not ever reference base type
	/*private void UpdateTypeSchemaDerived()
	{
		HashSet<Type> derivedTypes = new HashSet<Type>();
		foreach (TypeSchema typeSchema in typeSchemas)
		{
			Type type = typeSchema.nonNullableType;
			while (true)
			{
				type = type.BaseType;
				if (type == null)
					break;
				derivedTypes.Add(type);
			}
		}
		foreach (TypeSchema typeSchema in typeSchemas)
		{
			if (!derivedTypes.Contains(typeSchema.type))
				typeSchema.hasSubType = false;
		}
	}*/

	/// <summary>
	/// Saves all serialized data to the binary writer
	/// </summary>
	public void Save(Call call, BinaryWriter writer)
	{
		using CallTimer callSaving = call.Timer("Saving object");

		AddObjectMemberTypes(callSaving.Log);
		//UpdateTypeSchemaDerived();
		Header.Save(writer);
		long schemaPosition = writer.BaseStream.Position;
		SaveSchemas(writer);
		SavePrimitives(callSaving, writer);
		SaveObjects(callSaving.Log, writer);

		writer.BaseStream.Position = schemaPosition;
		SaveSchemas(writer);

		Header.SaveFileSize(writer);
	}

	/// <summary>
	/// Loads serialized data from the binary reader
	/// </summary>
	public void Load(Call call, BinaryReader reader, string? name = null, bool loadData = true, bool lazy = false)
	{
		Reader = reader;
		Lazy = lazy;

		using LogTimer logTimer = call.Log.Timer("Loading object");

		Header.Load(logTimer, reader, name);

		LoadSchemas(logTimer, reader);

		if (loadData)
		{
			LoadPrimitives(logTimer, reader);
			LoadTypeRepos(logTimer);
		}
	}

	private void SavePrimitives(Call call, BinaryWriter writer)
	{
		writer.Seek(0, SeekOrigin.End);
		writer.Write(Primitives.Count);
		foreach (object? obj in Primitives)
		{
			WriteObjectRef(typeof(object), obj, writer);
		}
	}

	private void LoadPrimitives(Log log, BinaryReader reader)
	{
		int count = reader.ReadInt32();
		for (int i = 0; i < count; i++)
		{
			byte flags = reader.ReadByte();
			if (flags == 0)
			{
				Primitives.Add(null);
				continue;
			}

			int typeIndex = reader.ReadInt16();
			TypeRepo typeRepo = TypeRepos[typeIndex];
			if (typeRepo.TypeSchema.IsPrimitive)
			{
				Primitives.Add(typeRepo.LoadObject());
			}
			else
			{
				// Object ref can point to primitives
				int objectIndex = reader.ReadInt32();
				Primitives.Add(typeRepo.LoadObject(objectIndex));
			}
		}
	}

	private void SaveSchemas(BinaryWriter writer)
	{
		writer.Write(TypeSchemas.Count);
		foreach (TypeRepo typeRepo in TypeRepos)
		{
			typeRepo.SaveSchema(writer);
			typeRepo.InitializeSaving();
		}
	}

	private void LoadSchemas(Log log, BinaryReader reader)
	{
		int count = reader.ReadInt32();
		try
		{
			for (int i = 0; i < count; i++)
			{
				var typeSchema = new TypeSchema(log, this, reader)
				{
					TypeIndex = i,
				};
				TypeSchemas.Add(typeSchema);
			}
		}
		catch (Exception e)
		{
			log.Add(e);
		}

		if (TypeSchemas.Count == 0)
		{
			log.Throw(new SerializerException("No TypeSchemas found", new Tag("Name", Header.Name)));
		}

		foreach (TypeSchema typeSchema in TypeSchemas)
		{
			typeSchema.Validate(this, TypeSchemas);

			TypeRepo typeRepo = TypeRepo.Create(log, this, typeSchema);

			// Return same input string references for Deep Cloning
			if (TypeRepoString != null && typeRepo is TypeRepoString)
			{
				typeRepo.ObjectsLoaded = TypeRepoString.Objects.ToArray();
			}

			AddTypeRepo(typeRepo);
		}
	}

	// Types have to be loaded in the correct order since some are references and some aren't
	private List<TypeRepo> OrderedTypes
	{
		get
		{
			List<TypeRepo> primitives = [];
			List<TypeRepo> collections = [];
			List<TypeRepo> others = [];

			foreach (TypeRepo typeRepo in TypeRepos)
			{
				if (!typeRepo.TypeSchema.CanReference)
				{
					primitives.Add(typeRepo);
				}
				else if (typeRepo.TypeSchema.IsCollection)
				{
					collections.Add(typeRepo);
				}
				else
				{
					others.Add(typeRepo);
				}
			}

			List<TypeRepo> orderedTypes = [.. primitives, .. others, .. collections];

			return orderedTypes;
		}
	}

	private record TypeRepoWriter(TypeRepo TypeRepo)
	{
		public MemoryStream MemoryStream { get; } = new();
	}

	private void SaveObjects(Log log, BinaryWriter writer)
	{
		writer.Write(ScrollId);
		// todo: Add Checksum?

		long headerPosition = writer.BaseStream.Position;
		foreach (TypeRepo typeRepo in OrderedTypes)
		{
			typeRepo.SkipHeader(writer);
		}

		var writers = new List<TypeRepoWriter>();
		foreach (TypeRepo typeRepo in OrderedTypes)
		{
			if (typeRepo.LoadableType == null)
				continue;

			var typeRepoWriter = new TypeRepoWriter(typeRepo);
			writers.Add(typeRepoWriter);
		}

		using (LogTimer logSerialize = log.Timer("Serializing Object Data"))
		{
			// todo: switch to async
			/*Parallel.ForEach(writers, new ParallelOptions() { MaxDegreeOfParallelism = 3 }, typeRepoWriter =>
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(typeRepoWriter.memoryStream, System.Text.Encoding.Default, true))
					typeRepoWriter.typeRepo.SaveObjects(logSerialize, binaryWriter);
			});*/
			foreach (TypeRepoWriter typeRepoWriter in writers)
			{
				using var binaryWriter = new BinaryWriter(typeRepoWriter.MemoryStream, System.Text.Encoding.Default, true);
				typeRepoWriter.TypeRepo.SaveObjects(logSerialize, binaryWriter);
			}
		}

		using (LogTimer logSave = log.Timer("Saving Object Data"))
		{
			foreach (TypeRepoWriter typeRepoWriter in writers)
			{
				byte[] bytes = typeRepoWriter.MemoryStream.ToArray();

				typeRepoWriter.TypeRepo.TypeSchema.StartDataOffset = writer.BaseStream.Position;
				typeRepoWriter.TypeRepo.TypeSchema.DataSize = bytes.Length;

				writer.Write(bytes);
			}
		}

		using LogTimer logTimer = log.Timer("Saving Type Repo headers");
		writer.Seek((int)headerPosition, SeekOrigin.Begin);
		foreach (TypeRepo typeRepo in OrderedTypes)
		{
			typeRepo.SaveHeader(logTimer, writer);
		}
	}

	private void LoadTypeRepos(Log log)
	{
		uint scrollId = Reader!.ReadUInt32();
		if (scrollId != ScrollId)
		{
			log.Throw(new SerializerException("Header id doesn't match",
				new Tag("Expected", ScrollId),
				new Tag("Found", scrollId)));
		}

		using (LogTimer logTimer = log.Timer("Loading Type Repo headers"))
		{
			foreach (TypeRepo typeRepo in OrderedTypes)
			{
				typeRepo.LoadHeader(logTimer);
			}
		}

		using (LogTimer logTimer = log.Timer("Initializing Type Repos"))
		{
			foreach (TypeRepo typeRepo in OrderedTypes)
			{
				if (typeRepo.LoadableType == null)
					continue;

				typeRepo.InitializeLoading(log);
			}
		}
	}

	/// <summary>
	/// Gets an existing type repository or creates a new one for the specified type
	/// </summary>
	public TypeRepo GetOrCreateRepo(Log? log, Type type)
	{
		if (IdxTypeToRepo.TryGetValue(type, out TypeRepo? typeRepo))
			return typeRepo;

		//if (type.IsInterface || type.IsAbstract)
		//	return null;

		var typeSchema = new TypeSchema(type, this)
		{
			TypeIndex = TypeSchemas.Count,
		};
		TypeSchemas.Add(typeSchema);

		typeRepo = TypeRepo.Create(log ?? new Log(), this, typeSchema);
		AddTypeRepo(typeRepo);
		return typeRepo;
	}

	/// <summary>
	/// Adds a type repository to the serializer's collection
	/// </summary>
	private void AddTypeRepo(TypeRepo typeRepo)
	{
		typeRepo.TypeIndex = TypeRepos.Count;
		if (typeRepo.Type != null) // Type might have been removed
		{
			IdxTypeToRepo[typeRepo.Type] = typeRepo;
		}
		TypeRepos.Add(typeRepo);
	}

	/// <summary>
	/// Adds an object reference to be serialized
	/// </summary>
	public void AddObjectRef(object? obj)
	{
		if (obj != null)
		{
			TypeRepo typeRepo = GetOrCreateRepo(null, obj.GetType());
			typeRepo.GetOrAddObjectRef(obj);
		}
	}

	/// <summary>
	/// Writes an object reference to the binary writer
	/// </summary>
	public void WriteObjectRef(Type baseType, object? obj, BinaryWriter writer)
	{
		if (obj == null)
		{
			writer.Write((byte)ObjectType.Null);
		}
		else
		{
			Type type = obj.GetType();
			TypeRepo typeRepo = IdxTypeToRepo[type];
			if (typeRepo is TypeRepoUnknown)
			{
				// different value for non-null?
				writer.Write((byte)ObjectType.Null);
				return;
			}

			if (type == baseType)
			{
				writer.Write((byte)ObjectType.BaseType);
			}
			else
			{
				writer.Write((byte)ObjectType.DerivedType);
			}

			if (baseType != null && baseType.IsPrimitive)
			{
				//if (type != baseType)
				//	writer.Write((short)typeRepo.typeIndex); // could compress by storing Base Class subtype 
				typeRepo.SaveObject(writer, obj);
			}
			else
			{
				if (type != baseType)
				//if (typeRepo.typeSchema.hasSubType) // sealed classes can't have sub-classes
				{
					writer.Write((short)typeRepo.TypeIndex); // could compress by storing Base Class subtype offset only
					if (type.IsPrimitive)
					{
						typeRepo.SaveObject(writer, obj);
						return;
					}
				}
				int objectIndex = typeRepo.IdxObjectToIndex[obj];
				writer.Write(objectIndex);
			}
		}
		//typeRepo.WriteObjectRef(obj, objectIndex, writer);
	}

	/// <summary>
	/// Adds an object and all its child objects to the serialization queue
	/// </summary>
	public void AddObject(Call call, object obj)
	{
		using CallTimer callTimer = call.Timer("Parsing object", new Tag("Object", obj?.ToString()));

		TypeRepo typeRepo = GetOrCreateRepo(callTimer.Log, obj!.GetType());
		int objectIndex = typeRepo.GetOrAddObjectRef(obj);
		//ParserQueue.Enqueue(obj);
		if (objectIndex < 0)
		{
			Primitives.Add(obj);
		}

		while (ParserQueue.Count > 0)
		{
			obj = ParserQueue.Dequeue();
			Type type = obj.GetType();
			typeRepo = IdxTypeToRepo[type]; // optimization? could save the object and TypeRepo reference in a Link struct 
			typeRepo.AddChildObjects(obj);
		}
	}

	/// <summary>
	/// Creates a shallow clone of an object (non-generic version)
	/// </summary>
	public object? Clone(object? obj)
	{
		if (obj == null)
			return null;

		Type type = obj.GetType();
		if (type.IsPrimitive)
			return obj;

		if (Clones.TryGetValue(obj, out object? clone))
			return clone;

		Log log = new();
		TypeRepo typeRepo = GetOrCreateRepo(log, type);

		if (typeRepo is
			TypeRepoPrimitive or
			Atlas.TypeRepos.TypeRepoString or
			TypeRepoEnum or
			TypeRepoType)
		{
			Clones[obj] = obj; // optional
			return obj;
		}

		if (typeRepo.TypeSchema.IsCloneReference)
		{
			Clones[obj] = obj; // optional
			return obj;
		}

		if (typeRepo is TypeRepoArray or TypeRepoArrayBytes)
		{
			clone = Array.CreateInstance(type.GetElementType()!, ((Array)obj).Length);
		}
		else if (type.IsValueType)
		{
			// struct
			return obj; // move this earlier to primitive check?
		}
		else
		{
			clone = Activator.CreateInstance(type, true)!;
		}

		Clones[obj] = clone;
		typeRepo.Cloned++;
		void action() => typeRepo.Clone(obj, clone);
		CloneQueue.Enqueue(action);
		return clone;
	}

	/// <summary>
	/// Creates a shallow clone of an object and processes the clone queue
	/// </summary>
	public T? Clone<T>(Log log, T obj)
	{
		T? clone = (T?)Clone(obj);
		using LogTimer logClone = log.Timer("Clone");

		while (CloneQueue.Count > 0)
		{
			Action action = CloneQueue.Dequeue();
			action.Invoke();
			//obj = parserQueue.Dequeue();
			//Type type = obj.GetType();
			//typeRepo = idxTypeToInstances[type]; // optimization? could save the object and TypeRepo reference in a Link class 
		}

		logClone.Add("Clone Finished", new Tag("Objects", Clones.Count));
		LogClonedTypes(logClone);

		return clone;
	}

	/// <summary>
	/// Logs information about all cloned types
	/// </summary>
	public void LogClonedTypes(Log log)
	{
		List<ObjectsLoaded> loaded = TypeRepos
			.Select(typeRepo => new ObjectsLoaded
			{
				Name = typeRepo.ToString(),
				Loaded = typeRepo.Cloned,
			})
			.ToList();

		log.Add("Objects Loaded", new Tag("Type Repos", loaded));
	}

	/// <summary>
	/// Queues an object for loading from the specified type repository
	/// </summary>
	internal void QueueLoading(TypeRepo typeRepo, int objectIndex)
	{
		LoadItem loadItem = new()
		{
			TypeRepo = typeRepo,
			Index = objectIndex
		};

		_loadQueue.Enqueue(loadItem);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			// Dispose managed resources
			foreach (TypeRepo typeRepo in TypeRepos)
			{
				typeRepo.Dispose();
			}

			Reader?.Dispose();
			Reader = null;

			// Clear collections
			TypeSchemas.Clear();
			TypeRepos.Clear();
			IdxTypeToRepo.Clear();
			ParserQueue.Clear();
			Primitives.Clear();
			Clones.Clear();
			CloneQueue.Clear();
		}

		_disposed = true;
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
