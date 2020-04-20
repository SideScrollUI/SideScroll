using Atlas.Core;
using System;
using System.IO;

namespace Atlas.Serialize
{
	public class SerializerFile
	{
		public string filePath;
		private string name;

		public SerializerFile(string filePath, string name = "")
		{
			this.filePath = filePath;
			this.name = name;
		}

		public override string ToString() => filePath;

		// check for writeability and no open locks
		public void TestWrite()
		{
			File.WriteAllText(filePath, "");
		}

		public bool Exists => File.Exists(filePath) && new FileInfo(filePath).Length > 0;

		public void Save(Call call, object obj, string name = null)
		{
			name = name ?? "<Default>";
			string parentDirectory = Path.GetDirectoryName(this.filePath);
			if (!Directory.Exists(parentDirectory))
				Directory.CreateDirectory(parentDirectory);

			using (CallTimer callTimer = call.Timer("Saving object: " + name, new Tag("Path", filePath)))
			{
				using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
				{
					using (BinaryWriter writer = new BinaryWriter(stream))
					{
						Serializer serializer = new Serializer();
						serializer.header.name = name;
						serializer.AddObject(callTimer, obj);
						serializer.Save(callTimer, writer);
					}
				}
			}
		}

		public T Load<T>(Call call = null, bool lazy = false, TaskInstance taskInstance = null)
		{
			call = call ?? new Call();
			object obj = Load(call, lazy, taskInstance);
			if (obj == null)
				return default;

			/*Type type = typeof(T);
			//if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				type = Nullable.GetUnderlyingType(type);
			}

			return (T)Convert.ChangeType(obj, type);*/
			T loaded = (T)obj;
			return loaded;
		}

		public object Load(Call call, bool lazy = false, TaskInstance taskInstance = null)
		{
			using (CallTimer callTimer = call.Timer("Loading object: " + name))
			{
				try
				{
					Serializer serializer = new Serializer();
					serializer.taskInstance = taskInstance;

					MemoryStream memoryStream;
					using (CallTimer callReadAllBytes = callTimer.Timer("Loading file: " + name))
						memoryStream = new MemoryStream(File.ReadAllBytes(filePath));

					BinaryReader reader = new BinaryReader(memoryStream);

					serializer.Load(callTimer, reader, lazy);
					object obj;
					using (CallTimer callLoadBaseObject = callTimer.Timer("Loading base object"))
					{
						obj = serializer.BaseObject();
					}
					serializer.LogLoadedTypes(callTimer);
					//logTimer.Add("Type Repos", new Tag("Repos", serializer.typeRepos)); // fields don't appear in columns
					if (taskInstance != null)
						taskInstance.Percent = 100;
					if (!lazy)
						serializer.Dispose();
					return obj;
				}
				catch (Exception e)
				{
					callTimer.Log.AddError("Exception loading file", new Tag("Exception", e.ToString()));
					return null; // returns null if reference type, otherwise default value (i.e. 0)
				}
			}
		}

		public Header LoadHeader(Call call)
		{
			call = call ?? new Call();

			using (CallTimer callReadAllBytes = call.Timer("Loading header: " + name))
			{
				MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(filePath));

				BinaryReader reader = new BinaryReader(memoryStream);
				Header header = new Header();
				header.Load(reader);
				return header;
			}
		}

		public Serializer LoadSchema(Call call)
		{
			using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					Serializer serializer = new Serializer();
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
