using Atlas.Core;
using Atlas.Serialize;
using Atlas.Tabs;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Atlas.UI.Avalonia.Test
{
	[Category("Serialize")]
	public class TestSerializeAvaloniaUI : TestBase
	{
		private SerializerMemory serializer;

		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("SerializeUI");
		}

		[SetUp]
		public void Setup()
		{
			serializer = new SerializerMemoryAtlas();
		}

		[Test]
		public void SerializeBookmark()
		{
			var input = new Bookmark()
			{
				TabBookmark = new TabBookmark()
				{
					ChildBookmarks = new Dictionary<string, TabBookmark>()
					{
						{ "test", new TabBookmark() }
					}
				},
			};
			input.TabBookmark.Bookmark = input;
			serializer.Save(Call, input);
			Bookmark output = serializer.Load<Bookmark>(Call);

			Assert.NotNull(output);
			Assert.NotNull(output.TabBookmark);
			Assert.NotNull(output.TabBookmark.ChildBookmarks);
			Assert.AreEqual(1, output.TabBookmark.ChildBookmarks.Count);
		}
	}
}
