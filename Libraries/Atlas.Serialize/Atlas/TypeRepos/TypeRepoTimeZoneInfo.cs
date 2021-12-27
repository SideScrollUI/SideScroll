using System;
using System.IO;

namespace Atlas.Serialize
{
	public class TypeRepoTimeZoneInfo : TypeRepo, IDisposable
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				if (CanAssign(typeSchema.Type))
					return new TypeRepoTimeZoneInfo(serializer, typeSchema);
				return null;
			}
		}

		public TypeRepoTimeZoneInfo(Serializer serializer, TypeSchema typeSchema) :
			base(serializer, typeSchema)
		{
		}

		public static bool CanAssign(Type type)
		{
			return type == typeof(TimeZoneInfo);
		}

		public override void SaveObject(BinaryWriter writer, object obj)
		{
			TimeZoneInfo timeZoneInfo = (TimeZoneInfo)obj;
			writer.Write(timeZoneInfo.ToSerializedString());
		}

		protected override object CreateObject(int objectIndex)
		{
			long position = Reader.BaseStream.Position;
			Reader.BaseStream.Position = ObjectOffsets[objectIndex];

			object obj = null;
			try
			{
				obj = TimeZoneInfo.FromSerializedString(Reader.ReadString());
			}
			catch (Exception)
			{
				//log.Add(e);
			}
			Reader.BaseStream.Position = position;

			ObjectsLoaded[objectIndex] = obj; // must assign before loading any more refs
			return obj;
		}

		public override object LoadObject()
		{
			object obj = TimeZoneInfo.FromSerializedString(Reader.ReadString());
			return obj;
		}

		// not called, it's a struct and a value
		public override void Clone(object source, object dest)
		{
			//dest = new DateTime(((DateTime)source).Ticks, ((DateTime)source).Kind);
		}
	}
}
