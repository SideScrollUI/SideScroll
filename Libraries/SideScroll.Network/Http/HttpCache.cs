using SideScroll.Attributes;
using System.Text;

namespace SideScroll.Network.Http;

/// <summary>
/// An append-only file-backed HTTP response cache that stores raw bytes indexed by URI,
/// using a separate binary index file and a data file for efficient lookups.
/// </summary>
public class HttpCache : IDisposable
{
	/// <summary>Gets the current file format version written to new cache files.</summary>
	public const uint LatestVersion = 1;

	/// <summary>Gets or sets the file format version read from an existing cache.</summary>
	public uint CurrentVersion = 1;

	/// <summary>Metadata for a single cached HTTP response entry.</summary>
	public class Entry
	{
		/// <summary>Gets or sets the URI of the cached resource.</summary>
		public string? Uri { get; set; }

		/// <summary>Gets or sets the byte offset of this entry's data in the data file.</summary>
		public long Offset { get; set; }

		/// <summary>Gets or sets the size in bytes of this entry's data.</summary>
		public int Size { get; set; }

		/// <summary>Gets or sets the time this entry was downloaded.</summary>
		public DateTime Downloaded { get; set; }

		/// <summary>Returns the entry's <see cref="Uri"/>.</summary>
		public override string? ToString() => Uri;
	}

	/// <summary>An <see cref="Entry"/> that also holds a reference to its owning <see cref="HttpCache"/> so its content can be lazily loaded.</summary>
	public class LoadableEntry : Entry
	{
		/// <summary>Gets or sets the cache that owns this entry.</summary>
		public HttpCache? Cache { get; set; }

		/// <summary>Gets the cached response body decoded as ASCII text.</summary>
		[HiddenColumn]
		public string? Text => Cache?.GetString(Uri!);

		/*public void Download(Call call)
		{
			var cachedHttp = new CachedHTTP(call, httpCache);
			byte[] bytes = cachedHttp.GetBytes(Uri.ToString());
			httpCache.AddEntry(Uri, bytes); // todo: fix function to allow updating
		}*/
	}

	/// <summary>Gets the base directory path where the index and data files are stored.</summary>
	public string BasePath { get; }

	/// <summary>Gets the current total size in bytes of the data file.</summary>
	public long Size => _dataStream.Length;

	private readonly Dictionary<string, Entry> _cache = [];

	private readonly string _indexPath;
	private readonly string _dataPath;

	private readonly Stream _indexStream;
	private readonly Stream _dataStream;

	private readonly object _entryLock = new();
	private bool _disposed;

	/// <summary>Returns the cache's <see cref="BasePath"/>.</summary>
	public override string ToString() => BasePath;

	/// <summary>Opens or creates the cache files at <paramref name="basePath"/>, loading any existing index entries.</summary>
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

	/// <summary>Releases the index and data file streams.</summary>
	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			// Dispose managed resources
			_indexStream.Dispose();
			_dataStream.Dispose();

			// Clear collections
			_cache.Clear();
		}

		_disposed = true;
	}

	/// <summary>Releases the index and data file streams.</summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
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

	/// <summary>Gets a snapshot list of all cached entries.</summary>
	public List<Entry> Entries => _cache.Values.ToList();

	/// <summary>Gets a snapshot list of all entries as <see cref="LoadableEntry"/> instances with a reference back to this cache.</summary>
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

	/// <summary>Appends the response bytes for <paramref name="uri"/> to the cache, ignoring the call if the URI is already cached.</summary>
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

	/// <summary>Returns <c>true</c> if a response for <paramref name="uri"/> exists in the cache.</summary>
	public bool ContainsKey(string uri)
	{
		return _cache.ContainsKey(uri);
	}

	/// <summary>Reads and returns the raw bytes for the cached response for <paramref name="uri"/>, or <c>null</c> if not found.</summary>
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

	/// <summary>Returns the cached response for <paramref name="uri"/> decoded as an ASCII string.</summary>
	public string GetString(string uri)
	{
		byte[] bytes = GetBytes(uri)!;
		string text = Encoding.ASCII.GetString(bytes);
		return text;
	}
}
