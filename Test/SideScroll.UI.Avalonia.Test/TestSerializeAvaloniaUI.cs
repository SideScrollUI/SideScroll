using Avalonia.Media;
using NUnit.Framework;
using SideScroll.Serialize;
using SideScroll.Serialize.Atlas;
using SideScroll.Tabs;

namespace SideScroll.UI.Avalonia.Test;

[Category("Serialize")]
public class TestSerializeAvaloniaUI : TestBase
{
	private SerializerMemory? serializer;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeUI");
	}

	[SetUp]
	public void Setup()
	{
		serializer = new SerializerMemorySideScroll();
	}

	[Test]
	public void SerializeBookmark()
	{
		var input = new Bookmark
		{
			TabBookmark = new TabBookmark
			{
				ChildBookmarks = new Dictionary<string, TabBookmark>
				{
					{ "test", new TabBookmark() }
				}
			},
		};
		input.TabBookmark.Bookmark = input;
		serializer!.Save(Call, input);
		Bookmark output = serializer.Load<Bookmark>(Call);

		Assert.NotNull(output);
		Assert.NotNull(output.TabBookmark);
		Assert.NotNull(output.TabBookmark.ChildBookmarks);
		Assert.AreEqual(1, output.TabBookmark.ChildBookmarks.Count);
	}

	[Test]
	public void SerializeColor()
	{
		var input = new Color(1, 2, 3, 4);
		serializer!.Save(Call, input);
		Color output = serializer.Load<Color>(Call);

		Assert.AreEqual(input, output);
	}
}
