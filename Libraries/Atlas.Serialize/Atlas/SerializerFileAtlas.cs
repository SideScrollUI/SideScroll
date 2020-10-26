using Atlas.Core;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Atlas.Serialize
{
	public class SerializerFileAtlas : SerializerFile
	{
		private const string DataName = "Data.atlas";

		public SerializerFileAtlas(string basePath, string name = "") : base(basePath, name)
		{
			HeaderPath = Paths.Combine(basePath, DataName);
			DataPath = Paths.Combine(basePath, DataName);
		}

		public override void SaveInternal(Call call, object obj, string name = null)
		{
			for (int attempt = 0; attempt < 10; attempt++)
			{
				if (attempt > 0)
					System.Threading.Thread.Sleep(attempt * 10);

				try
				{
					using (var stream = new FileStream(DataPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
					{
						using (var writer = new BinaryWriter(stream))
						{
							var serializer = new Serializer();
							serializer.Header.Name = name;
							serializer.AddObject(call, obj);
							serializer.Save(call, writer);
							break;
						}
					}
				}
				catch (Exception e)
				{
					call.Log.Add(e.Message);
				}
			}
		}

		protected override object LoadInternal(Call call, bool lazy, TaskInstance taskInstance)
		{
			var serializer = new Serializer();
			serializer.TaskInstance = taskInstance;

			MemoryStream memoryStream;
			using (CallTimer callReadAllBytes = call.Timer("Loading file: " + Name))
				memoryStream = new MemoryStream(File.ReadAllBytes(DataPath));

			var reader = new BinaryReader(memoryStream);

			serializer.Load(call, reader, lazy);
			object obj;
			using (CallTimer callLoadBaseObject = call.Timer("Loading base object"))
			{
				obj = serializer.BaseObject(callLoadBaseObject);
			}
			serializer.LogLoadedTypes(call);
			//logTimer.Add("Type Repos", new Tag("Repos", serializer.typeRepos)); // fields don't appear in columns
			if (taskInstance != null)
				taskInstance.Percent = 100;
			if (!lazy)
				serializer.Dispose();
			return obj;
		}

		/*public Header LoadHeader(Call call)
		{
			call = call ?? new Call();

			using (CallTimer callReadAllBytes = call.Timer("Loading header: " + Name))
			{
				var memoryStream = new MemoryStream(File.ReadAllBytes(FilePath));

				var reader = new BinaryReader(memoryStream);
				var header = new Header();
				header.Load(reader);
				return header;
			}
		}*/

		public Serializer LoadSchema(Call call)
		{
			using (var stream = new FileStream(HeaderPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (var reader = new BinaryReader(stream))
				{
					var serializer = new Serializer();
					serializer.Load(call, reader, false);
					return serializer;
				}
			}
		}

		public T LoadOrCreate<T>(Call call = null, bool lazy = false, TaskInstance taskInstance = null)
		{
			call = call ?? new Call();
			T result = default;
			if (Exists)
			{
				result = Load<T>(call, lazy, taskInstance);
			}
			if (result == null)
			{
				T newObject = Activator.CreateInstance<T>();
				return newObject;
			}
			return result;
		}
	}
}
