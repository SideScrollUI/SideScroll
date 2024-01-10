using Atlas.Extensions;
using System.Text.RegularExpressions;

namespace Atlas.Core;

// <prefix>://<type>/[v<version>/]<path>[?<query>]
public class LinkUri
{
	public string? Prefix { get; set; }
	public string? Type { get; set; }
	public Version? Version { get; set; }
	public string? Path { get; set; }
	public string? Query { get; set; }

	public string? Url { get; set; }

	public override string ToString() => Url ?? ToUri();

	public string ToUri()
	{
		string uri = @$"{Prefix}://{Type}/";

		if (Version != null)
		{
			uri += $"v{Version?.Formatted()}/";
		}

		uri += Path;

		if (Query != null)
		{
			uri += '?' + Query;
		}

		return uri;
	}

	public static LinkUri? Parse(string url)
	{
		Regex regex = new(@"(?<prefix>[a-zA-Z]+)\:\/\/(?<type>[-0-9a-zA-Z]+)\/(v(?<version>[\d\.]+)\/)?(?<path>[^\?]+)(\?(?<query>.+))?");

		Match match = regex.Match(url);
		if (!match.Success) return null;

		LinkUri uri = new()
		{
			Url = url,
			Prefix = match.Groups["prefix"].Value.ToLower(),
			Type = match.Groups["type"].Value.ToLower(),
			Version = ParseVersion(match.Groups["version"].Value),
			Path = match.Groups["path"].Value,
			Query = match.Groups["query"].Value,
		};
		return uri;
	}

	private static Version? ParseVersion(string? version)
	{
		if (version.IsNullOrEmpty()) return null;

		List<string> parts = version!.Split('.').ToList();
		while (parts.Count < 2)
			parts.Add("0");

		return new Version(string.Join(".", parts));
	}
}
