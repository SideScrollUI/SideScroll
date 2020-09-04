using Atlas.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Serialize
{
	// Represents all the object references for each unique type
	public abstract class TypeRepo : IDisposable
	{
		// Should we switch this to List<Type> instead?
		public static List<IRepoCreator> RepoCreators { get; set; } = new List<IRepoCreator>()
		{
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
			new TypeRepoEnumerable.Creator(),
			//new TypeRepoUnknown.NoConstructorCreator(),
			//new TypeRepoObject.Creator(),
		};

		public Serializer Serializer;
		public TypeSchema TypeSchema;
		public Type Type; // might be null after loading
		public Type LoadableType; // some types get overridden lazy load
		public int TypeIndex; // -1 if null
		public List<object> Objects = new List<object>(); // ordered by index, not filled in when loading
		public int[] ObjectSizes;
		public long[] ObjectOffsets;
		public object[] ObjectsLoaded;
		public int ObjectsLoadedCount = 0;

		public BinaryReader reader;

		// Saving Only
		public Dictionary<object, int> idxObjectToIndex = new Dictionary<object, int>(); // for saving only, not filled in for loading

		// Loading Only

		public int cloned = 0; // for stats


		//protected abstract void SaveObjectData(BinaryWriter writer);
		public abstract void SaveObject(BinaryWriter writer, object obj);
		public abstract void LoadObjectData(object obj);
		protected abstract object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex);
		public abstract void Clone(object source, object dest);
		public abstract void AddChildObjects(object obj);
		public virtual void InitializeLoading(Log log)	{ }
		public virtual void InitializeSaving() { }
		public virtual void SaveCustomHeader(BinaryWriter writer) { }
		public virtual void LoadCustomHeader() { }

		public TypeRepo(Serializer serializer, TypeSchema typeSchema)
		{
			Serializer = serializer;
			TypeSchema = typeSchema;
			Type = typeSchema.Type;
			LoadableType = Type;
			//objects.Capacity = typeSchema.numObjects;
			ObjectsLoaded = new object[typeSchema.NumObjects];
			//CreateObjects();
		}

		public override string ToString()
		{
			return TypeSchema.Name;
		}

		public static TypeRepo Create(Log log, Serializer serializer, TypeSchema typeSchema)
		{
			if (serializer.AllowListOnly && typeSchema.Type != null && !typeSchema.Allowed)
				throw new Exception("Type " + typeSchema.Name + " is not whitelisted");
			//Type type = typeSchema.type;
			TypeRepo typeRepo;

			foreach (var creator in RepoCreators)
			{
				typeRepo = creator.TryCreateRepo(serializer, typeSchema);
				if (typeRepo != null)
				{
					typeRepo.reader = serializer.Reader;
					return typeRepo;
				}
			}
			if (!typeSchema.HasConstructor)
			{
				typeRepo = new TypeRepoUnknown(serializer, typeSchema);
				log.AddWarning("Type has no constructor", new Tag(typeSchema));
			}
			else if (typeSchema.Secure && !serializer.SaveSecure)
			{
				typeRepo = new TypeRepoUnknown(serializer, typeSchema);
			}
			else
			{
				typeRepo = new TypeRepoObject(serializer, typeSchema);
			}
			typeRepo.reader = serializer.Reader;
			return typeRepo;

			// todo: add TypoRepoRef class?
			// can't declare
			/*if (type == null)
			{
				log.AddWarning("Missing type", new Tag(typeSchema));
				typeRepo = new TypeRepoUnknown(serializer, typeSchema);
			}*/
			/*else if (type.IsInterface || type.IsAbstract)
			{

			}*/
			/*else if (type is ISerializable)
			{
				typeRepo =  new TypeSerializable(typeSchema);
			}*/
			/*
			else if (typeof(ICollection<>).IsAssignableFrom(type))
			{
				typeRepo =  new TypeCollection(typeSchema);
			}
			else if (typeof(HashSet<>).IsAssignableFrom(type))
			{
				typeRepo =  new TypeCollection(typeSchema);
			}
			else if (typeof(ICollection).IsAssignableFrom(type))
			{
				typeRepo =  new TypeCollection(typeSchema);
			}
			else if (typeof(ISet<>).IsAssignableFrom(type))
			{
				typeRepo =  new TypeCollection(typeSchema);
			}*/
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
			foreach (int size in ObjectSizes)
			{
				writer.Write(size);
			}
			SaveCustomHeader(writer);
		}

		public void LoadHeader(Log log)
		{
			using (LogTimer logTimer = log.Timer("Loading Headers", new Tag("Type", TypeSchema.Name), new Tag("Count", TypeSchema.NumObjects)))
			{
				//int count = reader.ReadInt32();
				/*for (int i = 0; i < count; i++)
				{
					objectOffsets.Add(reader.ReadInt64());
				}*/
				ObjectOffsets = new long[TypeSchema.NumObjects];
				ObjectSizes = new int[TypeSchema.NumObjects];
				long offset = TypeSchema.FileDataOffset;
				for (int i = 0; i < TypeSchema.NumObjects; i++)
				{
					int size = reader.ReadInt32();
					ObjectOffsets[i] = offset;
					ObjectSizes[i] = size;
					offset += size;
				}
				//objects.AddRange(Enumerable.Repeat(null, count));
				//for (int i = 0; i < count; i++)
				//	objects.Add(null);

				LoadCustomHeader();
			}
		}

		public void SaveObjects(Log log, BinaryWriter writer)
		{
			using (LogTimer logTimer = log.Timer("Serializing (" + TypeSchema.Name + ")"))
			{
				//long start = writer.BaseStream.Position;
				//SaveObjectData(writer);

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
				}

				//long end = writer.BaseStream.Position;

				//typeSchema.fileDataOffset = start;
				//typeSchema.dataSize = end - start;

				logTimer.Add("Saved Object",
					new Tag("Type", Type),
					new Tag("Count", Objects.Count),
					new Tag("Offset", TypeSchema.FileDataOffset),
					new Tag("Bytes", TypeSchema.DataSize));
			}
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
			if (Type.IsPrimitive)
				return -1;
			if (!idxObjectToIndex.TryGetValue(obj, out int index))
			{
				index = idxObjectToIndex.Count;
				idxObjectToIndex[obj] = index;
				Objects.Add(obj);
				Serializer.ParserQueue.Enqueue(obj);
			}
			return index;
		}

		/*public void WriteObjectRef(object obj, int objectIndex, BinaryWriter writer)
		{
			if (obj == null)
			{
				writer.Write(true);
			}
			else
			{
				writer.Write(false);
				if (!type.IsSealed) // sealed classes can't have sub-classes
					writer.Write(typeIndex); // could compress by storing Base Class subtype offset only
				writer.Write(objectIndex);
			}
		}*/

		public TypeRef LoadLazyObjectRef()
		{
			byte flags = reader.ReadByte();
			if (flags == 0)
				return null;

			var typeRef = new TypeRef();

			if (TypeSchema.HasSubType)
			{
				if (flags == 1)
				{
					typeRef.TypeRepo = this;
				}
				else
				{
					int typeIndex = reader.ReadInt16(); // not saved for sealed classes
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
			typeRef.Index = reader.ReadInt32();
			return typeRef;
		}


		public void SkipObjectRef()
		{
			byte flags = reader.ReadByte();
			if (flags == 0)
				return;

			if (TypeSchema.IsPrimitive)
			{
				LoadObject();
				return;
			}

			if (TypeSchema.HasSubType)
			{
				if (flags == 1)
				{
					reader.ReadInt32(); // objectIndex
				}
				else
				{
					int typeIndex = reader.ReadInt16(); // not saved for sealed classes
					TypeRepo typeRepo = Serializer.TypeRepos[typeIndex];
					if (typeRepo.TypeSchema.Type.IsPrimitive)
						LoadObject();
					else
						reader.ReadInt32(); // objectIndex
				}
			}
			else
			{
				reader.ReadInt32(); // objectIndex
			}
		}

		public object LoadObjectRef()
		{
			byte flags = reader.ReadByte();
			if (flags == 0)
				return null;

			if (TypeSchema.IsPrimitive)
			{
				return LoadObject();
			}

			if (TypeSchema.HasSubType)
			{
				if (flags == 1)
				{
					int objectIndex = reader.ReadInt32();
					return LoadObject(objectIndex);
				}
				else
				{
					// has a derived type
					int typeIndex = reader.ReadInt16();
					if (typeIndex >= Serializer.TypeRepos.Count)
						return null;
					TypeRepo typeRepo = Serializer.TypeRepos[typeIndex];
					if (typeRepo.TypeSchema.IsPrimitive) // object ref can point to primitives
						return typeRepo.LoadObject();

					int objectIndex = reader.ReadInt32();
					return typeRepo.LoadObject(objectIndex);
				}
			}
			else
			{
				int objectIndex = reader.ReadInt32();
				return LoadObject(objectIndex);
			}
		}

		public object LoadObjectRef(byte[] bytes, ref int byteOffset)
		{
			bool isNull = Convert.ToBoolean(bytes[byteOffset++]);
			if (isNull)
				return null;

			if (Type.IsPrimitive)
			{
				return LoadObject();
				//return LoadObjectData(bytes, ref byteOffset);
			}

			int objectIndex = BitConverter.ToInt32(bytes, byteOffset);
			byteOffset += sizeof(int);
			if (!TypeSchema.HasSubType)
			{
				//return LoadObjectData(bytes, ref byteOffset, objectIndex);
				return LoadObject(objectIndex);
			}
			else
			{
				//int typeIndex = reader.ReadInt16(); // not saved for sealed classes
				int typeIndex = BitConverter.ToInt16(bytes, byteOffset);
				byteOffset += sizeof(short);
				TypeRepo typeRepo = Serializer.TypeRepos[typeIndex];
				//if (type == null) // type might have disappeared or been renamed
				//	return null;
				
				return typeRepo.LoadObject(objectIndex);
			}
		}

		public virtual object LoadObject()
		{
			return null;
		}

		protected virtual object LoadObjectData(byte[] bytes, ref int byteOffset)
		{
			return null;
		}

		public void LoadObjectData(int objectIndex)
		{
			object obj = ObjectsLoaded[objectIndex];
			reader.BaseStream.Position = ObjectOffsets[objectIndex];
			try
			{
				LoadObjectData(obj);
			}
			catch (Exception)
			{
			}
		}

		/*public virtual object LoadObject(int objectIndex)
		{
			if (objects[objectIndex] != null)
				return objects[objectIndex];

			int size = objectSizes[objectIndex];
			byte[] array = new byte[size];

			reader.BaseStream.Position = objectOffsets[objectIndex];
			reader.Read(array, 0, array.Length);

			//object obj = CreateObject(bytes, ref byteOffset);
			//objects[objectIndex] = obj; // must assign before loading any more refs

			int byteOffset = 0;
			return LoadObjectData(reader, array, ref byteOffset, objectIndex);
		}*/

		public object LoadObject(int objectIndex)
		{
			if (Type == null) // type might have disappeared or been renamed
				return null; // should we pass a "ref bool valid"?
			if (objectIndex >= ObjectsLoaded.Length)
				return null;
			if (ObjectsLoaded[objectIndex] != null)
				return ObjectsLoaded[objectIndex];

			ObjectsLoadedCount++;
			object obj = CreateObject(objectIndex);
			return obj;
		}
		
		public object LoadFullObject(int objectIndex)
		{
			if (ObjectsLoaded[objectIndex] != null)
				return ObjectsLoaded[objectIndex];

			object obj = CreateObject(objectIndex);

			Serializer.ProcessLoadQueue();

			return obj;
		}

		protected virtual object CreateObject(int objectIndex)
		{
			object obj = Activator.CreateInstance(LoadableType, true);
			ObjectsLoaded[objectIndex] = obj; // must assign before loading any more refs
			Serializer.QueueLoading(this, objectIndex);
			return obj;
		}

		/*protected virtual object CreateObject(byte[] bytes, ref int byteOffset)
		{
			return Activator.CreateInstance(type, true);
		}*/

		public virtual void Dispose()
		{
			
		}

		/*
		
		public object LoadObjectRef()
		{
			bool isNull = reader.ReadBoolean();
			if (isNull)
				return null;

			int objectIndex = reader.ReadInt32();
			if (type.IsSealed)
			{
				//return objects[objectIndex];

				if (objects[objectIndex] != null)
					return objects[objectIndex];

				//return typeRepo.objects[objectIndex];
				return LoadObjectData(reader, objectIndex);
			}
			else
			{
				int typeIndex = reader.ReadInt32(); // not saved for sealed classes
				TypeRepo typeRepo = serializer.typeRepos[typeIndex];
				//if (type == null) // type might have disappeared or been renamed
				//	return null;

				if (typeRepo.objects[objectIndex] != null)
					return typeRepo.objects[objectIndex];

				//return typeRepo.objects[objectIndex];
				return typeRepo.LoadObjectData(reader, objectIndex);
			}
		}
		*/

		/*
		// todo: test speed and size, this is a lot cleaner model
		public void SaveObjectRef(object obj, BinaryWriter writer)
		{
			if (obj == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(typeIndex); // could compress by storing Base Class subtype offset only
				int index = idxObjectToIndex[obj];
				writer.Write(index);
			}
		}

		public object LoadObjectRef()
		{
			int typeIndex = reader.ReadInt32();
			if (typeIndex == 0)
				return null;

			int objectIndex = reader.ReadInt32();
			//if (type == null) // type might have disappeared or been renamed
			//	return null;
			
			return objects[objectIndex];
		}

		*/

		/*public void AddObject(Serializer serializer, object obj)
		{
			// todo: switch to TryGetValue
			if (!idxObjectToIndex.ContainsKey(obj))
			{
				//Debug.Assert(objects.Count == idxObjectToIndex.Count);
				//idxObjectToIndex.Add(obj, objects.Count);
				//idxObjectToIndex[obj] = idxObjectToIndex.Count;
				//Debug.Assert(idxObjectToIndex.ContainsKey(obj));
				//objects.Add(obj);
				//Debug.Assert(objects.Contains(obj));
				//Debug.Assert(objects.Count == idxObjectToIndex.Count);

				//idxObjectToIndex.

				AddObjectData(serializer, obj, binaryWriter);
			}
		}*/
	}
}
