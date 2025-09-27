using SideScroll.Attributes;
using System.Text;

namespace SideScroll.Network.Http;

public class HttpCache : IDisposable
{
	public const uint LatestVersion = 1;

	public uint CurrentVersion = 1;

	public class Entry
	{
		public string? Uri { get; set; }

		public long Offset { get; set; }
		public int Size { get; set; }

		public DateTime Downloaded { get; set; }

		public override string? ToString() => Uri;
	}

	public class LoadableEntry : Entry
	{
		public HttpCache? Cache { get; set; }

		[HiddenColumn]
		public string? Text => Cache?.GetString(Uri!);

		/*public void Download(Call call)
		{
			var cachedHttp = new CachedHTTP(call, httpCache);
			byte[] bytes = cachedHttp.GetBytes(Uri.ToString());
			httpCache.AddEntry(Uri, bytes); // todo: fix function to allow updating
		}*/
	}

	public string BasePath { get; }
	public long Size => _dataStream.Length;

	private readonly Dictionary<string, Entry> _cache = [];

	private readonly string _indexPath;
	private readonly string _dataPath;

	private readonly Stream _indexStream;
	private readonly Stream _dataStream;

	private readonly object _entryLock = new();

	public override string ToString() => BasePath;

	public HttpCache(string basePath, bool writeable)
	{
		BasePath = basePath;
		Directory.CreateDirectory(basePath);

		_indexPath = Paths.Combine(basePath, "http.index");
		_dataPath = Paths.Combine(basePath, "http.data");

		FileAccess fileAccess = writeable ? FileAccess.ReadWrite : FileAccess.Read;
		_indexStream = new FileStream(_indexPath, FileMode.OpenOrCreate, fileAccess, FileShare.Read);
		_dataStream = new FileStream(_dataPath, FileMode.OpenOrCreate, fileAccess, FileShare.Read);

		if (_indexStream.Length == 0)
		{
			SaveHeader();
		}

		LoadIndex();
	}

	public void Dispose()
	{
		_indexStream.Dispose();
		_dataStream.Dispose();
	}

	private void LoadHeader(BinaryReader indexReader)
	{
		CurrentVersion = indexReader.ReadUInt32();
	}

	private void SaveHeader()
	{
		using BinaryWriter indexWriter = new(_indexStream, Encoding.Default, true);

		indexWriter.Write(LatestVersion);
	}

	private void LoadIndex()
	{
		_indexStream.Seek(0, SeekOrigin.Begin);
		using var indexReader = new BinaryReader(_indexStream, Encoding.Default, true);

		LoadHeader(indexReader);
		while (indexReader.PeekChar() >= 0)
		{
			var entry = new Entry
			{
				Uri = indexReader.ReadString(),
				Offset = indexReader.ReadInt64(),
				Size = indexReader.ReadInt32()
			};
			long ticks = indexReader.ReadInt64();
			entry.Downloaded = new DateTime(ticks);
			_cache[entry.Uri] = entry;
		}
	}

	public List<Entry> Entries => _cache.Values.ToList();

	public List<LoadableEntry> LoadableEntries =>
		_cache.Values.Select(entry => new LoadableEntry
		{
			Uri = entry.Uri,
			Size = entry.Size,
			Offset = entry.Offset,
			Downloaded = entry.Downloaded,
			Cache = this
		})
			.ToList();

	public void AddEntry(string uri, byte[] bytes)
	{
		lock (_entryLock)
		{
			// todo: add support for updating entries
			if (_cache.TryGetValue(uri, out Entry? entry))
				return;

			entry = new Entry
			{
				Uri = uri,
				Size = bytes.Length,
				Downloaded = DateTime.Now,
			};

			// todo: seek to last entry instead since the last entry might be incomplete
			using (var dataWriter = new BinaryWriter(_dataStream, Encoding.Default, true))
			{
				dataWriter.Seek(0, SeekOrigin.End);
				entry.Offset = _dataStream.Position;
				dataWriter.Write(bytes);
			}

			using (var indexWriter = new BinaryWriter(_indexStream, Encoding.Default, true))
			{
				indexWriter.Seek(0, SeekOrigin.End);
				indexWriter.Write(entry.Uri);
				indexWriter.Write(entry.Offset);
				indexWriter.Write(entry.Size);
				indexWriter.Write(entry.Downloaded.Ticks);
			}
			_cache[uri] = entry;
		}
	}

	public bool Contains(string uri)
	{
		return _cache.ContainsKey(uri);
	}

	public byte[]? GetBytes(string uri)
	{
		lock (_entryLock)
		{
			if (!_cache.TryGetValue(uri, out Entry? entry))
				return null;

			_dataStream.Position = entry.Offset;
			using var dataReader = new BinaryReader(_dataStream, Encoding.Default, true);

			byte[] data = dataReader.ReadBytes(entry.Size);
			return data;
		}
	}

	public string GetString(string uri)
	{
		byte[] bytes = GetBytes(uri)!;
		string text = Encoding.ASCII.GetString(bytes);
		return text;
	}
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
					Useful for analyzing .sidescroll files
					How to only load specific objects
						New wrapper class
							Reference<T>
						Only load a subset of TypeRepo entries
			Make sure to unload
	Future
		Smart Serializer that uses references to detect changes
*/
