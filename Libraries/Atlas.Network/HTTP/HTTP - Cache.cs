using Atlas.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Atlas.Network
{
	public class HttpCache : IDisposable
	{
		public const uint latestVersion = 1;

		public uint currentVersion = 1;

		public class Entry
		{
			public string Uri { get; set; }
			public long Offset { get; set; }
			public int Size { get; set; }
			public DateTime Downloaded { get; set; }

			public override string ToString() => Uri;
		}

		public class LoadableEntry : Entry
		{
			public HttpCache httpCache;

			[HiddenColumn]
			public string Text
			{
				get
				{
					return httpCache.GetString(Uri);
				}
			}

			/*public void Download(Call call)
			{
				CachedHTTP cachedHttp = new CachedHTTP(call, httpCache);
				byte[] bytes = cachedHttp.GetBytes(Uri.ToString());
				httpCache.AddEntry(Uri, bytes); // todo: fix function to allow updating
			}*/
		}

		private Dictionary<string, Entry> cache = new Dictionary<string, Entry>();
		public string BasePath { get; set; }
		public long Size => dataStream.Length;
		private string indexPath;
		private string dataPath;
		private Stream indexStream;
		private Stream dataStream;
		private object entryLock = new object();

		public HttpCache(string basePath, bool writeable)
		{
			BasePath = basePath;
			Directory.CreateDirectory(basePath);

			indexPath = Paths.Combine(basePath, "http.index");
			dataPath = Paths.Combine(basePath, "http.data");

			FileAccess fileAccess = writeable ? FileAccess.ReadWrite : FileAccess.Read;
			indexStream = new FileStream(indexPath, FileMode.OpenOrCreate, fileAccess, FileShare.Read);
			dataStream = new FileStream(dataPath, FileMode.OpenOrCreate, fileAccess, FileShare.Read);

			if (indexStream.Length == 0)
				SaveHeader();
			
			LoadIndex();
		}

		public void Dispose()
		{
			indexStream.Dispose();
			dataStream.Dispose();
		}

		public override string ToString() => BasePath;

		private void LoadHeader(BinaryReader indexReader)
		{
			currentVersion = indexReader.ReadUInt32();
		}

		private void SaveHeader()
		{
			using (BinaryWriter indexWriter = new BinaryWriter(indexStream, Encoding.Default, true))
			{
				indexWriter.Write(latestVersion);
			}
		}

		private void LoadIndex()
		{
			indexStream.Seek(0, SeekOrigin.Begin);
			using (var indexReader = new BinaryReader(indexStream, Encoding.Default, true))
			{
				LoadHeader(indexReader);
				while (indexReader.PeekChar() >= 0)
				{
					var entry = new Entry();
					entry.Uri = indexReader.ReadString();
					entry.Offset = indexReader.ReadInt64();
					entry.Size = indexReader.ReadInt32();
					long ticks = indexReader.ReadInt64();
					entry.Downloaded = new DateTime(ticks);
					cache[entry.Uri] = entry;
				}
			}
		}

		public List<Entry> Entries
		{
			get
			{
				var entries = new List<Entry>();
				foreach (Entry entry in cache.Values)
					entries.Add(entry);
				return entries;
			}
		}

		public List<LoadableEntry> LoadableEntries
		{
			get
			{
				var entries = new List<LoadableEntry>();
				foreach (Entry entry in cache.Values)
				{
					var loadableEntry = new LoadableEntry()
					{
						Uri = entry.Uri,
						Size = entry.Size,
						Offset = entry.Offset,
						Downloaded = entry.Downloaded,
						httpCache = this
					};
					entries.Add(loadableEntry);
				}
				return entries;
			}
		}

		public void AddEntry(string uri, byte[] bytes)
		{
			lock (entryLock)
			{
				// todo: add support for updating entries
				if (cache.TryGetValue(uri, out Entry entry))
					return;

				entry = new Entry()
				{
					Uri = uri,
					Size = bytes.Length,
					Downloaded = DateTime.Now,
				};

				// todo: seek to last entry instead since the last entry might be incomplete
				using (var dataWriter = new BinaryWriter(dataStream, Encoding.Default, true))
				{
					dataWriter.Seek(0, SeekOrigin.End);
					entry.Offset = dataStream.Position;
					dataWriter.Write(bytes);
				}

				using (var indexWriter = new BinaryWriter(indexStream, Encoding.Default, true))
				{
					indexWriter.Seek(0, SeekOrigin.End);
					indexWriter.Write(entry.Uri);
					indexWriter.Write(entry.Offset);
					indexWriter.Write(entry.Size);
					indexWriter.Write(entry.Downloaded.Ticks);
				}
				cache[uri] = entry;
			}
		}

		public bool Contains(string uri)
		{
			return cache.ContainsKey(uri);
		}

		public byte[] GetBytes(string uri)
		{
			lock (entryLock)
			{
				if (!cache.TryGetValue(uri, out Entry entry))
					return null;

				dataStream.Position = entry.Offset;
				using (var dataReader = new BinaryReader(dataStream, Encoding.Default, true))
				{
					byte[] data = dataReader.ReadBytes(entry.Size);
					return data;
				}
			}
		}

		public string GetString(string uri)
		{
			byte[] bytes = GetBytes(uri);
			string text = Encoding.ASCII.GetString(bytes);
			return text;
		}
	}

	// will take too long to serialize each time
	// could work if we only re-serialized dictionary
	/*public class HttpCacheSerialized
	{
		private Dictionary<string, byte[]> cache;
	}*/
}
/*
Serialize Cache for HTTP
	append only cache?
		how to remove items from cache?
	just a Dictionary<string, string>
		Dictionary<key,entry>
			Entry
				offset
				size
			Have to write out entire dictionary again if modified
			cleanup very rarely?
			write out rarely?
			List<Entry>
		unused entries?
		crash protection
			write out new files and replace originals
	where to store indices?
		separate file?
			.idx/.log
		header area at beginning of file?

	need to store nodes
	nosql database?
	Header
		URI
		offset
		size
	
	compress

Use Serializer?
	Current serialize everything
		Load cache loads everything
			(uses too much memory)
				Change serializer to only load header
					Useful for analyzing .atlas files
					How to only load specific objects
						New wrapper class
							Reference<T>
						Only load a subset of TypeRepo entries
			Make sure to unload
	Future
		Smart Serializer that uses references to detect changes
*/
