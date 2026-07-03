namespace SideScroll.Network.Http;

/// <summary>Represents a remote file identified by a URI that can be downloaded into an <see cref="HttpCache"/>.</summary>
public class HttpFile
{
	/// <summary>Gets or sets the URI of the remote file.</summary>
	public Uri? Uri { get; set; }

	/// <summary>Gets or sets the downloaded file size in bytes, populated after a successful <see cref="DownloadAsync"/> call.</summary>
	public int? Size { get; set; }

	/// <summary>Returns the query portion of the file's <see cref="Uri"/>.</summary>
	public override string? ToString() => Uri?.Query;

	/// <summary>Downloads the file from <see cref="Uri"/> into <paramref name="httpCache"/> and records the byte count in <see cref="Size"/>.</summary>
	public async Task DownloadAsync(Call call, HttpCache httpCache)
	{
		var cachedHttp = new HttpCachedCall(call, httpCache);
		byte[] bytes = await cachedHttp.GetBytesAsync(Uri!.ToString());
		Size = bytes.Length;
	}
}
