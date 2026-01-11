using Avalonia.Media;
using NUnit.Framework;
using SideScroll.Serialize;
using SideScroll.Serialize.Atlas;
using SideScroll.Tabs.Bookmarks.Models;

namespace SideScroll.Avalonia.Tests;

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
			TabBookmark = new()
			{
				TabDatas =
				[
					new()
					{
						SelectedRows =
						[
							new("Label", new TabBookmark()
							{
								Width = 99,
							}),
						],
					}
				],
			},
			CreatedTime = DateTime.Now,
		};
		_serializer!.Save(Call, input);
		Bookmark output = _serializer.Load<Bookmark>(Call);

		Assert.That(output, Is.Not.Null);
		Assert.That(output.TabBookmark, Is.Not.Null);
		Assert.That(output.TabBookmark.TabDatas, Has.Exactly(1).Items);
		Assert.That(output.TabBookmark.TabDatas[0].SelectedRows, Has.Exactly(1).Items);
		Assert.That(output.TabBookmark.TabDatas[0].SelectedRows[0].TabBookmark.Width, Is.EqualTo(99));
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
