using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Atlas.Serialize
{
	public class DataRepo
	{
		private const string DefaultDirectory = ".Default";
		private const string DataName = "Data.atlas";

		public string RepoPath { get; set; }
		public string RepoName { get; set; }

		//public RepoSettings repoSettings;

		public DataRepo(string repoPath, string repoName)
		{
			RepoPath = repoPath;
			RepoName = repoName;
			Debug.Assert(repoName != null);
		}

		public override string ToString() => RepoName;

		public DataRepoInstance<T> Open<T>(Call call, string saveDirectory)
		{
			return new DataRepoInstance<T>(this, call, saveDirectory);
		}

		public FileInfo GetFileInfo(Type type, string name)
		{
			string dataPath = GetHashedDataPath(type, name);
			return new FileInfo(dataPath);
		}

		public SerializerFile GetSerializerFile(Type type, string name)
		{
			return GetSerializerFile(type, DefaultDirectory, name);
		}

		public SerializerFile GetSerializerFile(Type type, string directory, string name)
		{
			string dataPath = GetHashedDataPath(type, directory, name);
			SerializerFile serializer = new SerializerFile(dataPath, name);
			return serializer;
		}

		// Use ToString()? for name?
		public void Save(string name, object obj, Call call = null)
		{
			Save(null, name, obj, call);
		}

		public void Save(string directory, string name, object obj, Call call = null)
		{
			directory = directory ?? DefaultDirectory;
			call = call ?? new Call();
			//using (Call call = call.Child(this,
			Type type = obj.GetType();
			SerializerFile serializer = GetSerializerFile(type, directory, name); // use hash since filesystems can't handle long names
			serializer.Save(call, obj, name);
		}

		public void Save(object obj, Call call = null)
		{
			Save(obj.GetType().AssemblyQualifiedName, obj, call);
		}

		public T Load<T>(string name, Call call, bool createIfNeeded = false, bool lazy = false)
		{
			return Load<T>(DefaultDirectory, name, call, createIfNeeded, lazy);
		}

		public T Load<T>(string directory, string name, Call call, bool createIfNeeded = false, bool lazy = false)
		{
			SerializerFile serializerFile = GetSerializerFile(typeof(T), directory, name);
			
			if (serializerFile.Exists)
			{
				T obj = serializerFile.Load<T>(call, lazy);
				if (obj != null)
					return obj;
			}

			if (createIfNeeded)
			{
				T newObject = Activator.CreateInstance<T>();
				Debug.Assert(newObject != null);
				return newObject;
			}
			return default;
		}

		public T Load<T>(bool createIfNeeded = false, bool lazy = false, Call call = null)
		{
			call = call ?? new Call();
			return Load<T>(typeof(T).AssemblyQualifiedName, call, createIfNeeded, lazy);
		}

		public SortedDictionary<string, T> LoadAll<T>(Call call = null, string directory = null, bool lazy = false)
		{
			call = call ?? new Call();
			directory = directory ?? DefaultDirectory;

			/*ItemCollection<string> objectIds = GetObjectIds(typeof(T));

			ItemCollection<T> list = new ItemCollection<T>();
			foreach (string id in objectIds)
			{
				T item = Load<T>(id, log, createIfNeeded, lazy, taskInstance);
				if (item != null)
					list.Add(item);
			}*/
			var entries = new SortedDictionary<string, T>();

			string typePath = GetTypePath(typeof(T), directory);
			if (Directory.Exists(typePath))
			{
				foreach (string filePath in Directory.EnumerateDirectories(typePath))
				{
					string fileName = Path.GetFileName(filePath);
					string dataPath = Paths.Combine(filePath, DataName);
					if (File.Exists(dataPath) == false)
						continue;

					SerializerFile serializerFile = new SerializerFile(dataPath, fileName);

					if (serializerFile.Exists)
					{
						T obj = serializerFile.Load<T>(call, lazy);
						if (obj != null)
							entries.Add(serializerFile.LoadHeader(call).name, obj);
					}
				}
			}
			return entries;
		}

		public ItemCollection<Header> LoadHeaders(Type type, Call call = null)
		{
			call = call ?? new Call();

			ItemCollection<Header> list = new ItemCollection<Header>();

			string typePath = GetTypePath(type);
			if (Directory.Exists(typePath))
			{
				foreach (string filePath in Directory.EnumerateDirectories(typePath))
				{
					string fileName = Path.GetFileName(filePath);
					string dataPath = Paths.Combine(filePath, DataName);
					if (File.Exists(dataPath) == false)
						continue;

					SerializerFile serializerFile = new SerializerFile(dataPath, fileName);
					if (serializerFile.Exists)
					{
						Header header = serializerFile.LoadHeader(call);
						if (header != null)
							list.Add(header);
					}
				}
			}
			return list;
		}

		public void DeleteAll<T>(string directory = null)
		{
			DeleteAll(typeof(T), directory);
		}

		public void DeleteAll(Type type, string directory = null)
		{
			string directoryPath = GetTypePath(type, directory);
			if (Directory.Exists(directoryPath))
			{
				try
				{
					Directory.Delete(directoryPath, true);
				}
				catch (Exception)
				{
				}
			}
		}

		// remove all other deletes and add null defaults?
		public void Delete<T>(string directory, string name)
		{
			Delete(typeof(T), directory, name);
		}

		public void Delete(Type type, string directory, string name)
		{
			directory = directory ?? DefaultDirectory;
			string directoryPath = GetDirectoryPath(type, directory, name);
			if (Directory.Exists(directoryPath))
			{
				try
				{
					Directory.Delete(directoryPath, true);
				}
				catch (Exception)
				{
				}
			}
		}

		public void Delete(Type type, string name)
		{
			Delete(type, null, name);
		}

		public void DeleteRepo()
		{
			string path = Paths.Combine(RepoPath, RepoName);
			if (Directory.Exists(path))
			{
				try
				{
					Directory.Delete(path, true);
				}
				catch (Exception)
				{
				}
			}
		}

		// Move to Atlas.Core Extensions?
		public static string ComputeSha256Hash(string rawData)
		{
			// Create a SHA256   
			using (SHA256 sha256Hash = SHA256.Create())
			{
				// ComputeHash - returns byte array  
				byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

				// Convert byte array to a string   
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < bytes.Length; i++)
				{
					builder.Append(bytes[i].ToString("x2"));
				}
				return builder.ToString();
			}
		}

		// Don't use GetHashCode(), it returns a different value each time the process is run
		public string GetTypePath(Type type, string name = null)
		{
			string path = Paths.Combine(RepoPath, RepoName, type.FullName);
			if (name != null)
				path += "/" + ComputeSha256Hash(name);
			return path;
		}

		public string GetHashedDataPath(Type type, string name)
		{
			string typePath = GetTypePath(type, name);
			string dataPath = Paths.Combine(typePath, DataName);
			return dataPath;
		}

		public string GetHashedDataPath(Type type, string directory, string name)
		{
			string typePath = GetDirectoryPath(type, directory, name);
			string dataPath = Paths.Combine(typePath, DataName);
			return dataPath;
		}

		public string GetDirectoryPath(Type type, string directory, string name)
		{
			string directoryPath = GetTypePath(type, directory);
			string dataPath = ComputeSha256Hash(name);
			return Paths.Combine(directoryPath, dataPath);
		}

		// clean this up?
		/*private ItemCollection<string> GetObjectIds(Type type, string name = null)
		{
			ItemCollection<string> list = new ItemCollection<string>();

			string typePath = GetTypePath(type, name);
			if (Directory.Exists(typePath))
			{
				foreach (string filePath in Directory.EnumerateDirectories(typePath))
				{
					string fileName = Path.GetFileName(filePath);
					string dataPath = Paths.Combine(filePath, DataName);
					if (File.Exists(dataPath) == false)
						continue;
					list.Add(fileName);
				}
			}
			return list;
		}*/
	}
	public class DataRepoInstance<T>
	{
		public DataRepo dataRepo;
		private Call call;
		private string saveDirectory;

		public DataRepoInstance(DataRepo dataRepo, Call call, string saveDirectory)
		{
			this.dataRepo = dataRepo;
			this.call = call;
			this.saveDirectory = saveDirectory;
		}

		public SortedDictionary<string, T> LoadAll(Call call = null, bool lazy = false)
		{
			return dataRepo.LoadAll<T>(call, saveDirectory, lazy);
		}

		public void Delete(string key)
		{
			dataRepo.Delete<T>(saveDirectory, key);
		}

		public void DeleteAll()
		{
			dataRepo.DeleteAll<T>();
		}

		public void Save(string name, T sampleItem)
		{
			dataRepo.Save(saveDirectory, name, sampleItem, call);
		}
	}
}
