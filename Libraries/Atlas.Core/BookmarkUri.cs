using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Atlas.Core
{
	// <prefix>://<type>/v<version>/<id>
	public class BookmarkUri
	{
		public string Prefix { get; set; }
		public string Type { get; set; }
		public Version Version { get; set; }
		public string Id { get; set; }
		public string Url { get; set; }

		public static BookmarkUri Parse(string url)
		{
			Regex regex = new Regex(@"(?<prefix>[a-zA-Z]+)\:\/\/(?<type>[-0-9a-zA-Z]+)\/v(?<version>[\d\.]+)\/(?<id>.+)");

			Match match = regex.Match(url);
			if (!match.Success)
				return null;

			var uri = new BookmarkUri()
			{
				Url = url,
				Prefix = match.Groups["prefix"].Value.ToLower(),
				Type = match.Groups["type"].Value.ToLower(),
				Version = ParseVersion(match.Groups["version"].Value),
				Id = match.Groups["id"].Value,
			};
			return uri;
		}

		private static Version ParseVersion(string version)
		{
			List<string> parts = version.Split('.').ToList();
			while (parts.Count < 2)
				parts.Add("0");

			return new Version(string.Join(".", parts));
		}
	}
}
