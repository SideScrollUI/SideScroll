using Atlas.Core;
using System;
using System.IO;
using System.Text;

namespace Atlas.Serialize
{
	public class SerializerMemoryAtlas : SerializerMemory
	{
		//private MemoryStream stream = new MemoryStream();

		private TypeRepoString _typeRepoString; // Reuse string instances to reduce memory use when deep cloning

		public SerializerMemoryAtlas()
		{
		}

		private new Serializer Create()
		{
			return new Serializer()
			{
				PublicOnly = PublicOnly,
			};
		}

		public override void Save(Call call, object obj)
		{
			using (CallTimer callTimer = call.Timer("Save"))
			{
				using (var writer = new BinaryWriter(Stream, Encoding.Default, true))
				{
					var serializer = Create();
					serializer.AddObject(callTimer, obj);
					serializer.Save(callTimer, writer);

					if (serializer.IdxTypeToRepo.TryGetValue(typeof(string), out TypeRepo typeRepo))
						_typeRepoString = (TypeRepoString)typeRepo;
				}
			}
		}

		public override T Load<T>(Call call = null)
		{
			call = call ?? new Call();
			using (CallTimer callTimer = call.Timer("Load"))
			{
				Stream.Seek(0, SeekOrigin.Begin);
				using (var reader = new BinaryReader(Stream))
				{
					var serializer = Create();
					serializer.TypeRepoString = _typeRepoString;
					serializer.Load(callTimer, reader);
					return (T)serializer.BaseObject(callTimer);
				}
			}
		}

		public override object Load(Call call = null)
		{
			call = call ?? new Call();
			Stream.Seek(0, SeekOrigin.Begin);
			using (var reader = new BinaryReader(Stream))
			{
				var serializer = Create();
				serializer.Load(call, reader);
				return serializer.BaseObject(call);
			}
		}

		//public static T Clone<T>(Call call, T obj)
		protected override T DeepCloneInternal<T>(Call call, T obj)
		{
			Save(call, obj);
			T copy = Load<T>(call);
			return copy;
		}

		protected override object DeepCloneInternal(Call call, object obj)
		{
			Save(call, obj);
			object copy = Load(call);
			return copy;
		}
	}
}
