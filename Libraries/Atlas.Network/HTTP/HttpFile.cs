using Atlas.Core;
using System;
using System.Threading.Tasks;

namespace Atlas.Network;

public class HttpFile
{
	public Uri? Uri { get; set; }
	public int? Size { get; set; }

	public override string? ToString() => Uri?.Query;

	public async Task DownloadAsync(Call call, HttpCache httpCache)
	{
		var cachedHttp = new HttpCachedCall(call, httpCache);
		byte[] bytes = await cachedHttp.GetBytesAsync(Uri!.ToString());
		Size = bytes.Length;
	}
}
