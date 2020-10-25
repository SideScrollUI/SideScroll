﻿using Atlas.Core;
using System;
using System.Collections;
using System.IO;

namespace Atlas.Serialize
{
	public class TypeRepoArray : TypeRepo
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.Type))
					return new TypeRepoArray(serializer, typeSchema);
				return null;
			}
		}

		private TypeRepo listTypeRepo;
		private int[] sizes;
		private Type elementType;

		public TypeRepoArray(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
			elementType = typeSchema.Type.GetElementType();
		}

		public static bool CanAssign(Type type)
		{
			return typeof(Array).IsAssignableFrom(type);
		}

		public override void InitializeLoading(Log log)
		{
			listTypeRepo = Serializer.GetOrCreateRepo(log, elementType);
		}

		public override void SaveCustomHeader(BinaryWriter writer)
		{
			foreach (IList list in Objects)
			{
				writer.Write((int)list.Count);
			}
		}

		public override void LoadCustomHeader()
		{
			sizes = new int[TypeSchema.NumObjects];
			for (int i = 0; i < TypeSchema.NumObjects; i++)
			{
				int count = Reader.ReadInt32();
				sizes[i] = count;
			}
		}

		public override void AddChildObjects(object obj)
		{
			Array array = (Array)obj;
			foreach (var item in array)
			{
				Serializer.AddObjectRef(item);
			}
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			Array array = (Array)obj;
			
			//writer.Write(array.Length);
			foreach (var item in array)
			{
				Serializer.WriteObjectRef(elementType, item, writer);
			}
		}

		protected override object CreateObject(int objectIndex)
		{
			// Can't use Activator because Array requires parameters in it's constructor
			//int count = reader.ReadInt32();
			int count = sizes[objectIndex];

			Array array = Array.CreateInstance(TypeSchema.Type.GetElementType(), count);
			ObjectsLoaded[objectIndex] = array;
			Serializer.QueueLoading(this, objectIndex);

			return array;
		}

		public override void LoadObjectData(object obj)
		{
			// Can't use Activator because Array requires parameters in it's constructor

			IList iList = (IList)obj;

			for (int j = 0; j < iList.Count; j++)
			{
				object item = listTypeRepo.LoadObjectRef();
				iList[j] = item;
			}
		}

		public override void Clone(object source, object dest)
		{
			Array iSource = (Array)source;
			IList iDest = (IList)dest;
			int i = 0;
			foreach (var item in iSource)
			{
				object clone = Serializer.Clone(item);
				iDest[i++] = clone;
			}
		}
	}
}
