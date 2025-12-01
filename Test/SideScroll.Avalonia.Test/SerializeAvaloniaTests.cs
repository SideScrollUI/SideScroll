using Avalonia.Media;
using NUnit.Framework;
using SideScroll.Serialize;
using SideScroll.Serialize.Atlas;
using SideScroll.Tabs.Bookmarks;

namespace SideScroll.Avalonia.Test;

[Category("Serialize")]
public class SerializeAvaloniaTests : BaseTest
{
	private SerializerMemory? _serializer;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeUI");
	}

	[SetUp]
	public void Setup()
	{
		_serializer = new SerializerMemoryAtlas();
	}

	[Test]
	public void SerializeBookmark()
	{
		Bookmark input = new()
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
		_serializer!.Save(Call, input);
		Bookmark output = _serializer.Load<Bookmark>(Call);

		Assert.That(output, Is.Not.Null);
		Assert.That(output.TabBookmark, Is.Not.Null);
		Assert.That(output.TabBookmark.ChildBookmarks, Is.Not.Null);
		Assert.That(output.TabBookmark.ChildBookmarks, Has.Exactly(1).Items);
	}

	[Test]
	public void SerializeColor()
	{
		var input = new Color(1, 2, 3, 4);
		_serializer!.Save(Call, input);
		Color output = _serializer.Load<Color>(Call);

		Assert.That(output, Is.EqualTo(input));
	}
}
