using SideScroll.Extensions;
using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using SideScroll.Serialize.Atlas.TypeRepos;
using SideScroll.Tasks;

namespace SideScroll.Serialize.Atlas;

public class SerializerException(string text, params Tag[] tags) : 
	TaggedException(text, tags);

public class Header
{
	public const uint SideId = 0x45444953; // SIDE -> EDIS: 69, 68, 73, 83, Start of file (little endian format)

	public const string LatestVersion = "1";

	public string? Version { get; set; }
	public string? Name { get; set; }

	public override string ToString() => $"v{Version}: {Name}";

	public void Save(BinaryWriter writer)
	{
		writer.Write(SideId);
		writer.Write(Version ?? LatestVersion);
		writer.Write(Name ?? "");
	}

	public void Load(Log log, BinaryReader reader, string? requiredName = null)
	{
		uint sideId = reader.ReadUInt32();
		if (sideId != SideId)
		{
			log.Throw(new SerializerException("Invalid header Id",
				new Tag("Expected", SideId),
				new Tag("Found", sideId)));
		}
		Version = reader.ReadString();
		Name = reader.ReadString();

		if (!requiredName.IsNullOrEmpty() && Name != requiredName)
		{
			log.Throw(new SerializerException("Loaded name doesn't match required",
				new Tag("Required", requiredName),
				new Tag("Loaded", Name)));
		}
	}
}

public class Serializer : IDisposable
{
	public const uint ScrollId = 0x4C524353; // SCRL -> LRCS: 76, 82, 67, 83, Start of object data (little endian format)

	public Header Header { get; set; } = new();

	public List<TypeSchema> TypeSchemas { get; protected set; } = [];

	public List<TypeRepo> TypeRepos { get; protected set; } = [];
	public Dictionary<Type, TypeRepo> IdxTypeToRepo { get; protected set; } = [];

	public TypeRepoString? TypeRepoString { get; set; } // Reuse string instances to reduce memory use when deep cloning

	public BinaryReader? Reader { get; set; }

	public bool Lazy { get; set; }
	public bool PublicOnly { get; set; }
	public bool EnableFieldToPropertyMapping { get; set; } = true;

	// Convert to Parser class?
	// Use a queue so we don't exceed the stack size due to cross references (i.e. a list with values that refer back to the list)
	public Queue<object> ParserQueue { get; protected set; } = [];
	public List<object?> Primitives { get; protected set; } = []; // primitives are usually serialized inline, but that doesn't work if that's the primary type

	protected Dictionary<object, object> Clones { get; set; } = [];
	protected Queue<Action> CloneQueue { get; set; } = new();

	public TaskInstance? TaskInstance { get; set; }

	public struct LoadItem
	{
		public TypeRepo TypeRepo;
		public int Index;
		public bool Preloaded; // set after IPreloadRepo preloads data

		public override string ToString() => $"{TypeRepo} - {Index}";
	}

	private readonly Queue<LoadItem> _loadQueue = new();

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

	public object? LoadObject(TypeRepo typeRepo, int index)
	{
		object? obj = typeRepo.LoadObject(index);

		ProcessLoadQueue();

		return obj;
	}

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

	public record ObjectsLoaded
	{
		public string Name { get; init; } = default!;
		public int Loaded { get; init; }
	}

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

	// todo: only add types that are used
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

	public void Save(Call call, BinaryWriter writer)
	{
		using CallTimer callSaving = call.Timer("Saving object");

		AddObjectMemberTypes(callSaving.Log);
		//UpdateTypeSchemaDerived();
		Header.Save(writer);
		long schemaPosition = writer.BaseStream.Position;
		writer.Write((long)0); // File Size, will write correct value at end
		SaveSchemas(writer);
		SavePrimitives(callSaving, writer);
		SaveObjects(callSaving.Log, writer);

		// write out schema again for file offsets and size
		writer.Seek((int)schemaPosition, SeekOrigin.Begin);
		writer.Write(writer.BaseStream.Length);
		SaveSchemas(writer);
	}

	public void Load(Call call, BinaryReader reader, string? name = null, bool loadData = true, bool lazy = false)
	{
		Reader = reader;
		Lazy = lazy;

		using LogTimer logTimer = call.Log.Timer("Loading object");

		Header.Load(logTimer, reader, name);
		if (Header.Version != Header.LatestVersion)
		{
			logTimer.Throw(new SerializerException("Header version doesn't match", new Tag("Header", Header)));
		}

		long fileLength = reader.ReadInt64();
		if (reader.BaseStream.Length != fileLength)
		{
			logTimer.Throw(new SerializerException("File size doesn't match",
				new Tag("Expected", fileLength),
				new Tag("Actual", reader.BaseStream.Length)));
		}

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
			//TypeRepo typeRepo = GetOrCreateRepo(obj.GetType());
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
			typeSchema.Validate(TypeSchemas);

			TypeRepo typeRepo = TypeRepo.Create(log, this, typeSchema);

			// Return same input string references for Deep Cloning
			if (TypeRepoString != null && typeRepo is TypeRepoString)
			{
				typeRepo.ObjectsLoaded = TypeRepoString.Objects.ToArray();
			}

			if (typeRepo != null)
			{
				AddTypeRepo(typeRepo);
			}
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
				if (typeRepo == null) // if we want to save after opening?
					continue;

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

	record TypeRepoWriter(TypeRepo typeRepo)
	{
		public TypeRepo TypeRepo => typeRepo;
		public MemoryStream MemoryStream = new();
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

				typeRepoWriter.TypeRepo.TypeSchema.FileDataOffset = writer.BaseStream.Position;
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

	// Adds TypeSchema and TypeRepo if required
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

	private void AddTypeRepo(TypeRepo typeRepo)
	{
		typeRepo.TypeIndex = TypeRepos.Count;
		if (typeRepo.Type != null) // Type might have been removed
		{
			IdxTypeToRepo[typeRepo.Type] = typeRepo;
		}
		TypeRepos.Add(typeRepo);
	}

	public void AddObjectRef(object? obj)
	{
		if (obj != null)
		{
			TypeRepo typeRepo = GetOrCreateRepo(null, obj.GetType());
			typeRepo.GetOrAddObjectRef(obj);
		}
	}

	/*public void AddAndWriteObjectRef(object obj, BinaryWriter writer)
	{
		if (obj == null)
		{
			writer.Write(true);
		}
		else
		{
			writer.Write(false);
			TypeRepo typeRepo = GetOrCreateRepo(obj.GetType());
			int objectIndex = typeRepo.GetOrAddObjectRef(obj);
			if (!typeRepo.type.IsSealed) // sealed classes can't have sub-classes
				writer.Write(typeRepo.typeIndex); // could compress by storing Base Class subtype offset only
			writer.Write(objectIndex);
		}
		//typeRepo.WriteObjectRef(obj, objectIndex, writer);
	}*/

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

	// speed issue, we don't know what objects index will be when enqueued, so we have to lookup again later
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

		if (typeRepo.TypeSchema.IsStatic)
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

	internal void QueueLoading(TypeRepo typeRepo, int objectIndex)
	{
		LoadItem loadItem = new()
		{
			TypeRepo = typeRepo,
			Index = objectIndex
		};

		_loadQueue.Enqueue(loadItem);
	}

	public void Dispose()
	{
		foreach (TypeRepo typeRepo in TypeRepos)
		{
			typeRepo.Dispose();
		}

		Reader?.Dispose();
	}
}
