using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Atlas.Resources
{
	public class Assets
	{
		public const string Hourglass = "hourglass64.png";
		public const string Shutter = "shutter64.png";

		public class Streams
		{
			public static Stream Hourglass => Get(Assets.Hourglass);
			public static Stream Shutter => Get(Assets.Shutter);

			public static Stream Get(string resourceName)
			{
				var assembly = Assembly.GetExecutingAssembly();
				return assembly.GetManifestResourceStream("Atlas.Resources.Assets." + resourceName);
			}

			public static List<Stream> All { get; set; } = new List<Stream>()
			{
				Hourglass,
				Shutter,
			};
		}
	}

}
