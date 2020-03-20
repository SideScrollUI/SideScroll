using Atlas.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Atlas.Serialize
{
	public class Header
	{
		public const string latestVersion = "1";

		public string version = latestVersion;
		public string name = "<Default>";

		public override string ToString() => version.ToString();

		public void Save(BinaryWriter writer)
		{
			writer.Write(version);
			writer.Write(name);
		}

		public void Load(BinaryReader reader)
		{
			version = reader.ReadString();
			name = reader.ReadString();
			//Debug.Assert(version == latestVersion);
		}
	}

	public class Serializer : IDisposable
	{
		public Header header = new Header();

		public List<TypeSchema> typeSchemas = new List<TypeSchema>();

		public List<TypeRepo> typeRepos = new List<TypeRepo>();
		public Dictionary<Type, TypeRepo> idxTypeToRepo = new Dictionary<Type, TypeRepo>();

		public BinaryReader reader;
		public bool lazy;
		public bool whitelistOnly = true;

		// Convert to Parser class?
		// Use a queue so we don't exceed the stack size due to cross references (i.e. a list with values that refer back to the list)
		public Queue<object> parserQueue = new Queue<object>();
		public List<object> primitives = new List<object>(); // primitives are usually serialized inline, but that doesn't work if that's the primary type

		public struct LoadItem
		{
			public TypeRepo typeRepo;
			public int index;

			public override string ToString() => typeRepo.ToString() + " - " + index;
		}

		public Queue<LoadItem> loadQueue = new Queue<LoadItem>();

		public Serializer()
		{
		}

		public object BaseObject()
		{
			if (typeRepos.Count == 0)// || typeRepos[0].objects.Count == 0)
				return null;
			TypeRepo typeRepo = typeRepos[0];
			if (typeRepo.type == null)
				return null;

			if (typeRepo.type.IsPrimitive)
			{
				return primitives[0];
				//return typeRepo.LoadObject();
			}
			return LoadObject(typeRepo, 0);
		}

		public object LoadObject(TypeRepo typeRepo, int index)
		{
			object obj = typeRepo.LoadObject(index);

			ProcessLoadQueue();

			return obj;
		}

		public void ProcessLoadQueue()
		{
			while (loadQueue.Count > 0)
			{
				LoadItem loadItem = loadQueue.Dequeue();
				loadItem.typeRepo.LoadObjectData(loadItem.index);
			}
		}

		public class ObjectsLoaded
		{
			public string Name { get; set; }
			public int Loaded { get; set; }
		}

		public void LogLoadedTypes(Call call)
		{
			List<ObjectsLoaded> loaded = new List<ObjectsLoaded>();
			foreach (TypeRepo typeRepo in typeRepos)
			{
				ObjectsLoaded typeInfo = new ObjectsLoaded()
				{
					Name = typeRepo.ToString(),
					Loaded = typeRepo.objectsLoadedCount
				};
				loaded.Add(typeInfo);
			}
			call.log.Add("Objects Loaded", new Tag("Type Repos", loaded));
		}

		// todo: only add types that are used
		private void AddObjectMemberTypes(Log log)
		{
			int count = typeSchemas.Count;
			for (int i = 0; i < count; i++)
			{
				TypeSchema typeSchema = typeSchemas[i];
				foreach (FieldSchema fieldSchema in typeSchema.FieldSchemas)
				{
					Type type = fieldSchema.nonNullableType;
					fieldSchema.typeSchema = GetOrCreateRepo(log, type).typeSchema;
					fieldSchema.typeIndex = fieldSchema.typeSchema.typeIndex;
				}
				foreach (PropertySchema propertySchema in typeSchema.PropertySchemas)
				{
					Type type = propertySchema.nonNullableType;
					propertySchema.propertyTypeSchema = GetOrCreateRepo(log, type).typeSchema;
					propertySchema.typeIndex = propertySchema.propertyTypeSchema.typeIndex;
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
			using (CallTimer callSaving = call.Timer("Saving to disk"))
			{
				AddObjectMemberTypes(callSaving.log);
				//UpdateTypeSchemaDerived();
				header.Save(writer);
				long schemaPosition = writer.BaseStream.Position;
				writer.Write((long)0); // will write correct value at end
				SaveSchemas(writer);
				SavePrimitives(callSaving, writer);
				SaveObjects(callSaving.log, writer);

				// write out schema again for file offsets and size
				writer.Seek((int)schemaPosition, SeekOrigin.Begin);
				writer.Write(writer.BaseStream.Length);
				SaveSchemas(writer);
			}
		}

		public void Load(Call call, BinaryReader reader, bool lazy = false, bool loadData = true)
		{
			this.reader = reader;
			this.lazy = lazy;
			using (LogTimer logTimer = call.log.Timer("Loading object"))
			{
				header.Load(reader);
				if (header.version != Header.latestVersion)
				{
					logTimer.AddError("Header version doesn't match", new Tag("Header", header));
					return;
				}
				long fileLength = reader.ReadInt64();
				if (reader.BaseStream.Length != fileLength)
				{
					logTimer.AddError("File size doesn't match", new Tag("Expected", fileLength), new Tag("Actual", reader.BaseStream.Length));
					return;
				}
				LoadSchemas(logTimer, reader);
				LoadPrimitives(logTimer, reader);
				if (loadData)
					LoadTypeRepos(logTimer);
			}
		}

		private void SavePrimitives(Call call, BinaryWriter writer)
		{
			writer.Seek(0, SeekOrigin.End);
			writer.Write(primitives.Count);
			foreach (object obj in primitives)
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
					primitives.Add(null);
					continue;
				}
				int typeIndex = reader.ReadInt16();
				TypeRepo typeRepo = typeRepos[typeIndex];
				if (typeRepo.typeSchema.isPrimitive) // object ref can point to primitives
				{
					primitives.Add(typeRepo.LoadObject());
				}
				else
				{
					int objectIndex = reader.ReadInt32();
					primitives.Add(typeRepo.LoadObject(objectIndex));
				}
			}
		}

		private void SaveSchemas(BinaryWriter writer)
		{
			writer.Write(typeSchemas.Count);
			foreach (TypeRepo typeRepo in typeRepos)
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
					TypeSchema typeSchema = new TypeSchema(log, reader)
					{
						typeIndex = i,
					};
					typeSchemas.Add(typeSchema);

					//TypeRepo typeRepo = TypeRepo.Create(this, typeSchema);
					//AddTypeRepo(typeRepo);
				}
			}
			catch (Exception e)
			{
				log.Add(e);
			}

			foreach (TypeSchema typeSchema in typeSchemas)
			{
				typeSchema.Validate(typeSchemas);

				TypeRepo typeRepo = TypeRepo.Create(log, this, typeSchema);
				AddTypeRepo(typeRepo);
			}
		}

		// Types have to be loaded in the correct order since some are references and some aren't
		private List<TypeRepo> OrderedTypes
		{
			get
			{
				List<TypeRepo> primitives = new List<TypeRepo>();
				List<TypeRepo> collections = new List<TypeRepo>();
				List<TypeRepo> others = new List<TypeRepo>();

				foreach (TypeRepo typeRepo in typeRepos)
				{
					if (typeRepo == null) // if we want to save after opening?
						continue;

					if (!typeRepo.typeSchema.CanReference)
						primitives.Add(typeRepo);
					else if (typeRepo.typeSchema.isCollection)
						collections.Add(typeRepo);
					else
						others.Add(typeRepo);
				}

				List<TypeRepo> orderedTypes = new List<TypeRepo>();
				orderedTypes.AddRange(primitives);
				orderedTypes.AddRange(others);
				orderedTypes.AddRange(collections);

				return orderedTypes;
			}
		}

		private const int checksum = 555;

		class TypeRepoWriter
		{
			public TypeRepo typeRepo;
			public MemoryStream memoryStream = new MemoryStream();
		}

		private void SaveObjects(Log log, BinaryWriter writer)
		{
			writer.Write(checksum);

			long headerPosition = writer.BaseStream.Position;
			foreach (TypeRepo typeRepo in OrderedTypes)
			{
				typeRepo.SkipHeader(writer);
			}

			List<TypeRepoWriter> writers = new List<TypeRepoWriter>();
			foreach (TypeRepo typeRepo in OrderedTypes)
			{
				if (typeRepo.type == null)
					continue;

				TypeRepoWriter typeRepoWriter = new TypeRepoWriter()
				{
					typeRepo = typeRepo,
				};
				writers.Add(typeRepoWriter);
			}

			using (LogTimer logSerialize = log.Timer("Serializing Object Data"))
			{
				// todo: add parallel param
				/*Parallel.ForEach(writers, new ParallelOptions() { MaxDegreeOfParallelism = 3 }, typeRepoWriter =>
				{
					using (BinaryWriter binaryWriter = new BinaryWriter(typeRepoWriter.memoryStream, System.Text.Encoding.Default, true))
						typeRepoWriter.typeRepo.SaveObjects(logSerialize, binaryWriter);
				});*/
				foreach (var typeRepoWriter in writers)
				{
					using (BinaryWriter binaryWriter = new BinaryWriter(typeRepoWriter.memoryStream, System.Text.Encoding.Default, true))
						typeRepoWriter.typeRepo.SaveObjects(logSerialize, binaryWriter);
				}
			}

			using (LogTimer logSave = log.Timer("Saving Object Data"))
			{
				foreach (TypeRepoWriter typeRepoWriter in writers)
				{
					byte[] bytes = typeRepoWriter.memoryStream.ToArray();

					typeRepoWriter.typeRepo.typeSchema.FileDataOffset = writer.BaseStream.Position;
					typeRepoWriter.typeRepo.typeSchema.DataSize = bytes.Length;

					writer.Write(bytes);
				}
			}

			using (LogTimer logTimer = log.Timer("Saving Type Repo headers"))
			{
				writer.Seek((int)headerPosition, SeekOrigin.Begin);
				foreach (TypeRepo typeRepo in OrderedTypes)
				{
					typeRepo.SaveHeader(writer);
				}
			}
		}

		private void LoadTypeRepos(Log log)
		{
			int id = reader.ReadInt32();
			Debug.Assert(id == checksum);
			
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
					if (typeRepo.type == null)
						continue;

					typeRepo.InitializeLoading(log);
				}
			}
		}

		// Adds TypeSchema and TypeRepo if required
		public TypeRepo GetOrCreateRepo(Log log, Type type)
		{
			if (idxTypeToRepo.TryGetValue(type, out TypeRepo typeRepo))
				return typeRepo;

			//if (type.IsInterface || type.IsAbstract)
			//	return null;

			TypeSchema typeSchema = new TypeSchema(type)
			{
				typeIndex = typeSchemas.Count,
			};
			typeSchemas.Add(typeSchema);

			typeRepo = TypeRepo.Create(log ?? new Log(), this, typeSchema);
			AddTypeRepo(typeRepo);
			return typeRepo;
		}

		private void AddTypeRepo(TypeRepo typeRepo)
		{
			typeRepo.typeIndex = typeRepos.Count;
			if (typeRepo.type != null) // Type might have been removed
				idxTypeToRepo[typeRepo.type] = typeRepo;
			typeRepos.Add(typeRepo);
		}

		public void AddObjectRef(object obj)
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

		public void WriteObjectRef(Type baseType, object obj, BinaryWriter writer)
		{
			if (obj == null)
			{
				writer.Write((byte)0);
			}
			else
			{
				Type type = obj.GetType();
				if (type == baseType)
					writer.Write((byte)1);
				else
					writer.Write((byte)2);
				TypeRepo typeRepo = idxTypeToRepo[type];
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
						writer.Write((short)typeRepo.typeIndex); // could compress by storing Base Class subtype offset only
						if (type.IsPrimitive)
						{
							typeRepo.SaveObject(writer, obj);
							return;
						}
					}
					int objectIndex = typeRepo.idxObjectToIndex[obj];
					writer.Write(objectIndex);
				}
			}
			//typeRepo.WriteObjectRef(obj, objectIndex, writer);
		}

		// speed issue, we don't know what objects index will be when enqueued, so we have to lookup again later
		public void AddObject(Call call, object obj)
		{
			using (CallTimer callTimer = call.Timer("Parsing object", new Tag("Object", obj.ToString())))
			{
				TypeRepo typeRepo = GetOrCreateRepo(callTimer.log, obj.GetType());
				int objectIndex = typeRepo.GetOrAddObjectRef(obj);
				//parserQueue.Enqueue(obj);
				if (objectIndex < 0)
					primitives.Add(obj);

				while (parserQueue.Count > 0)
				{
					obj = parserQueue.Dequeue();
					Type type = obj.GetType();
					typeRepo = idxTypeToRepo[type]; // optimization? could save the object and TypeRepo reference in a Link struct 
					typeRepo.AddChildObjects(obj);
				}
			}
		}

		public Dictionary<object, object> clones = new Dictionary<object, object>();
		public Queue<Action> cloneQueue = new Queue<Action>();
		public TaskInstance taskInstance;

		//private object CreateInstance(Type, )

		public object Clone(object obj)//, bool throwExceptions)
		{
			if (obj == null)
				return null;
			Type type = obj.GetType();
			if (type.IsPrimitive)
				return obj;
			if (clones.TryGetValue(obj, out object clone))
				return clone;
			Log log = new Log();
			TypeRepo typeRepo = GetOrCreateRepo(log, type);
			if (typeRepo is TypeRepoPrimitive || typeRepo is TypeRepoString || typeRepo is TypeRepoEnum || typeRepo is TypeRepoType)// || typeRepo.typeSchema.isStatic)
			{
				clones[obj] = obj; // optional
				return obj;
			}
			if (typeRepo.typeSchema.isStatic)
			{
				clones[obj] = obj; // optional
				return obj;
			}

			//if (throwExceptions)
			{
				if (typeRepo is TypeRepoArray || typeRepo is TypeRepoArrayBytes)
				{
					clone = Array.CreateInstance(type.GetElementType(), (obj as Array).Length);
				}
				else if (type.IsValueType)
				{
					// struct
					return obj; // move this earlier to primitive check?
				}
				else
				{
					clone = Activator.CreateInstance(type, true);
				}
			}
			//else
			{

			}
			clones[obj] = clone;
			typeRepo.cloned++;
			Action action = new Action(() => typeRepo.Clone(obj, clone));
			cloneQueue.Enqueue(action);
			return clone;
		}
		
		public T Clone<T>(Log log, object obj)
		{
			whitelistOnly = false;
			T clone = (T)Clone(obj);
			using (LogTimer logClone = log.Timer("Clone"))
			{
				while (cloneQueue.Count > 0)
				{
					Action action = cloneQueue.Dequeue();
					action.Invoke();
					//obj = parserQueue.Dequeue();
					//Type type = obj.GetType();
					//typeRepo = idxTypeToInstances[type]; // optimization? could save the object and TypeRepo reference in a Link class 
				}
				
				logClone.Add("Clone Finished", new Tag("Objects", clones.Count));
				LogClonedTypes(logClone);
			}
			return clone;
		}

		public void LogClonedTypes(Log log)
		{
			List<ObjectsLoaded> loaded = new List<ObjectsLoaded>();
			foreach (TypeRepo typeRepo in typeRepos)
			{
				ObjectsLoaded typeInfo = new ObjectsLoaded()
				{
					Name = typeRepo.ToString(),
					Loaded = typeRepo.cloned
				};
				loaded.Add(typeInfo);
			}
			log.Add("Objects Loaded", new Tag("Type Repos", loaded));
		}

		internal void QueueLoading(TypeRepo typeRepo, int objectIndex)
		{
			LoadItem loadItem = new LoadItem()
			{
				typeRepo = typeRepo,
				index = objectIndex
			};

			loadQueue.Enqueue(loadItem);
		}

		public void Dispose()
		{
			foreach (TypeRepo typeRepo in typeRepos)
				typeRepo.Dispose();
			if (reader != null)
				reader.Dispose();
		}
	}
}
