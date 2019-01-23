using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Atlas.Core;
using NUnit.Framework;

namespace Atlas.Network.Test
{
	[Category("HTTP")]
	public class TestHttp : TestBase
	{
		//private string HttpdCachePath;
		private const int intCount = 10000;

		/*[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("HTTP");

			HttpdCachePath = Paths.Combine(TestPath, "HttpCache");

			if (Directory.Exists(HttpdCachePath))
				Directory.Delete(HttpdCachePath, true);
		}

		[Test, Description("MultipleInstancesReadWrite")]
		public void MultipleInstancesReadWrite()
		{
			string uri = "abc";
			byte[] input = new byte[4] { 1, 2, 3, 4 };
			HttpCache httpCache = project.OpenCache(HttpdCachePath);
			httpCache.AddEntry(uri, input);
			byte[] output = httpCache.GetBytes(uri);

			Assert.AreEqual(input, output);
		}*/

		/*[Test, Description("ReadWrite")]
		public void ReadWrite()
		{
			string uri = "abc";
			byte[] input = new byte[4] { 1, 2, 3, 4 };
			using (HttpCache httpCache = new HttpCache(HttpdCachePath))
			{
				httpCache.AddEntry(uri, input);
				byte[] output = httpCache.GetBytes(uri);

				Assert.AreEqual(input, output);
			}
		}

		[Test, Description("OpenClose")]
		public void OpenClose()
		{
			string uri = "abc";
			byte[] input = new byte[4] { 1, 2, 3, 4 };
			using (HttpCache httpCache = new HttpCache(HttpdCachePath))
			{
				httpCache.AddEntry(uri, input);
			}

			using (HttpCache httpCache = new HttpCache(HttpdCachePath))
			{
				byte[] output = httpCache.GetBytes(uri);

				Assert.AreEqual(input, output);
			}
		}*/
	}
}