using NUnit.Framework;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Samples.Forms;
using SideScroll.Time;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class BookmarkJsonTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("BookmarkJson");
	}

	[Test, Description("Test simple Bookmark JSON serialization")]
	public void SerializeSimpleBookmark()
	{
		var bookmark = new Bookmark
		{
			Name = "Test Bookmark",
			BookmarkType = BookmarkType.Default,
			CreatedTime = DateTime.Now,
		};

		string json = bookmark.ToJson();
		
		Assert.That(json, Is.Not.Null);
		Assert.That(json, Contains.Substring("Test Bookmark"));

		bool success = Bookmark.TryParseJson(json, out Bookmark? deserialized);

		Assert.That(success, Is.True);
		Assert.That(deserialized, Is.Not.Null);
		Assert.That(deserialized!.Name, Is.EqualTo("Test Bookmark"));
		Assert.That(deserialized.BookmarkType, Is.EqualTo(BookmarkType.Default));
	}

	[Test, Description("Test Bookmark with invalid JSON")]
	public void DeserializeInvalidJson()
	{
		string invalidJson = "{ invalid json }";

		bool success = Bookmark.TryParseJson(invalidJson, out Bookmark? bookmark);

		Assert.That(success, Is.False);
		Assert.That(bookmark, Is.Null);
	}

	[Test, Description("Test Bookmark JSON roundtrip preserves structure")]
	public void BookmarkJsonRoundtrip()
	{
		var original = new Bookmark
		{
			Name = "Roundtrip Test",
			BookmarkType = BookmarkType.Full,
			Imported = false,
			CreatedTime = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
			TabBookmark = new TabBookmark
			{
				Width = 500.0,
				TabDatas =
				[
					new TabDataBookmark
					{
						DataRepoGroupId = "TestGroup",
						Filter = "test filter",
						SelectedRows =
						[
							new SelectedRowView(
								new SelectedRow
								{
									Label = "Test Row",
									RowIndex = 5,
									DataKey = "key1"
								}
							)
						]
					}
				]
			}
		};

		// Serialize
		string json = original.ToJson();

		// Deserialize
		bool success = Bookmark.TryParseJson(json, out Bookmark? deserialized);

		// Verify
		Assert.That(success, Is.True);
		Assert.That(deserialized, Is.Not.Null);
		Assert.That(deserialized!.Name, Is.EqualTo(original.Name));
		Assert.That(deserialized.BookmarkType, Is.EqualTo(original.BookmarkType));
		Assert.That(deserialized.Imported, Is.EqualTo(original.Imported));
		Assert.That(deserialized.TabBookmark.Width, Is.EqualTo(original.TabBookmark.Width));
		
		var originalTabData = original.TabBookmark.TabDatas[0];
		var deserializedTabData = deserialized.TabBookmark.TabDatas[0];
		
		Assert.That(deserializedTabData.DataRepoGroupId, Is.EqualTo(originalTabData.DataRepoGroupId));
		Assert.That(deserializedTabData.Filter, Is.EqualTo(originalTabData.Filter));
		Assert.That(deserializedTabData.SelectedRows[0].SelectedRow.Label, 
			Is.EqualTo(originalTabData.SelectedRows[0].SelectedRow.Label));
	}

	[Test, Description("Test Bookmark JSON serialization with nested structure")]
	public void SerializeBookmarkWithNestedData()
	{
		// Create a bookmark matching the structure from the JSON
		var bookmark = new Bookmark
		{
			Name = "Data Repos",
			BookmarkType = BookmarkType.Leaf,
			Imported = true,
			CreatedTime = new DateTime(2026, 2, 7, 22, 39, 21, 13, DateTimeKind.Local).AddTicks(6976),
			TabBookmark = new TabBookmark
			{
				TabDatas =
				[
					new TabDataBookmark
					{
						DataRepoGroupId = "SampleParams",
						DataRepoType = typeof(SampleItem),
						SelectedRows =
						[
							new SelectedRowView(
								new SelectedRow
								{
									Label = "Item 3",
									RowIndex = 3,
									DataKey = "Item 3",
									DataValue = new SampleItem
									{
										Name = "Item 3",
										Description = "Describe all the things",
										Amount = 30,
										EnumAttributeTargets = (AttributeTargets)512,
										ListItem = new SampleListItem("Two", 2),
										DateTime = new DateTime(2026, 2, 7, 15, 53, 19, 721, DateTimeKind.Local).AddTicks(7632),
										TimeZone = new TimeZoneView("PST / PDT", "Pacific Time", TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"))
									}
								},
								new TabBookmark
								{
									TabDatas =
									[
										new TabDataBookmark
										{
											SelectedRows =
											[
												new SelectedRowView(
													new SelectedRow
													{
														Label = "List Items",
														RowIndex = 0
													},
													new TabBookmark
													{
														TabDatas =
														[
															new TabDataBookmark
															{
																SelectedRows =
																[
																	new SelectedRowView(
																		new SelectedRow
																		{
																			Label = "One",
																			RowIndex = 0
																		},
																		new TabBookmark
																		{
																			TabDatas =
																			[
																				new TabDataBookmark
																				{
																					SelectedRows =
																					[
																						new SelectedRowView(
																							new SelectedRow
																							{
																								Label = "Name",
																								RowIndex = 0
																							},
																							new TabBookmark
																							{
																								TabDatas = []
																							}
																						)
																					]
																				}
																			]
																		}
																	)
																]
															}
														]
													}
												)
											]
										}
									]
								}
							)
						]
					}
				]
			}
		};

		// Serialize to JSON
		string json = bookmark.ToJson();

		// Verify the JSON can be parsed
		Assert.That(json, Is.Not.Null);
		Assert.That(json, Is.Not.Empty);

		// Deserialize back
		bool success = Bookmark.TryParseJson(json, out Bookmark? deserialized);

		// Verify deserialization succeeded
		Assert.That(success, Is.True);
		Assert.That(deserialized, Is.Not.Null);

		// Verify basic properties
		Assert.That(deserialized!.Name, Is.EqualTo("Data Repos"));
		Assert.That(deserialized.BookmarkType, Is.EqualTo(BookmarkType.Leaf));
		Assert.That(deserialized.Imported, Is.EqualTo(true));

		// Verify nested structure exists
		Assert.That(deserialized.TabBookmark, Is.Not.Null);
		Assert.That(deserialized.TabBookmark.TabDatas, Is.Not.Empty);

		var firstTabData = deserialized.TabBookmark.TabDatas[0];
		Assert.That(firstTabData.DataRepoGroupId, Is.EqualTo("SampleParams"));
		Assert.That(firstTabData.SelectedRows, Is.Not.Empty);

		var firstSelectedRow = firstTabData.SelectedRows[0];
		Assert.That(firstSelectedRow.SelectedRow.Label, Is.EqualTo("Item 3"));
		Assert.That(firstSelectedRow.SelectedRow.RowIndex, Is.EqualTo(3));
		Assert.That(firstSelectedRow.SelectedRow.DataKey, Is.EqualTo("Item 3"));

		// Verify DataValue is properly serialized and deserialized
		Assert.That(firstSelectedRow.SelectedRow.DataValue, Is.Not.Null);
		Assert.That(firstSelectedRow.SelectedRow.DataValue, Is.InstanceOf<SampleItem>());
		
		var dataValueItem = (SampleItem)firstSelectedRow.SelectedRow.DataValue!;
		Assert.That(dataValueItem.Name, Is.EqualTo("Item 3"));
		Assert.That(dataValueItem.Description, Is.EqualTo("Describe all the things"));
		Assert.That(dataValueItem.Amount, Is.EqualTo(30));
		Assert.That(dataValueItem.EnumAttributeTargets, Is.EqualTo((AttributeTargets)512));
		Assert.That(dataValueItem.ListItem, Is.Not.Null);
		Assert.That(dataValueItem.ListItem!.Name, Is.EqualTo("Two"));
		Assert.That(dataValueItem.ListItem.Value, Is.EqualTo(2));
		Assert.That(dataValueItem.TimeZone, Is.Not.Null);
		Assert.That(dataValueItem.TimeZone!.Name, Is.EqualTo("Pacific Time"));

		// Verify nested TabBookmark structure
		Assert.That(firstSelectedRow.TabBookmark, Is.Not.Null);
		Assert.That(firstSelectedRow.TabBookmark.TabDatas, Is.Not.Empty);

		var nestedTabData = firstSelectedRow.TabBookmark.TabDatas[0];
		Assert.That(nestedTabData.SelectedRows, Is.Not.Empty);

		var nestedSelectedRow = nestedTabData.SelectedRows[0];
		Assert.That(nestedSelectedRow.SelectedRow.Label, Is.EqualTo("List Items"));
		Assert.That(nestedSelectedRow.SelectedRow.RowIndex, Is.EqualTo(0));

		// Verify deeply nested structure exists (3 levels deep)
		Assert.That(nestedSelectedRow.TabBookmark.TabDatas, Is.Not.Empty);
		Assert.That(nestedSelectedRow.TabBookmark.TabDatas[0].SelectedRows, Is.Not.Empty);

		var deeplyNestedRow = nestedSelectedRow.TabBookmark.TabDatas[0].SelectedRows[0];
		Assert.That(deeplyNestedRow.SelectedRow.Label, Is.EqualTo("One"));

		// Verify the deepest level (4 levels deep)
		Assert.That(deeplyNestedRow.TabBookmark.TabDatas, Is.Not.Empty);
		Assert.That(deeplyNestedRow.TabBookmark.TabDatas[0].SelectedRows, Is.Not.Empty);

		var deepestRow = deeplyNestedRow.TabBookmark.TabDatas[0].SelectedRows[0];
		Assert.That(deepestRow.SelectedRow.Label, Is.EqualTo("Name"));

		// Verify the empty TabDatas at the deepest level
		Assert.That(deepestRow.TabBookmark.TabDatas, Is.Empty);
	}

	// Unregistered type for testing
	public class UnregisteredDataType
	{
		public string? Data { get; set; }
		public int Value { get; set; }
	}

	[Test, Description("Test Bookmark with unregistered type in DataValue is blocked")]
	public void SerializeBookmarkWithUnregisteredDataValue()
	{
		// Create a bookmark with an unregistered type in DataValue
		var bookmark = new Bookmark
		{
			Name = "Unregistered Type Test",
			BookmarkType = BookmarkType.Leaf,
			CreatedTime = DateTime.Now,
			TabBookmark = new TabBookmark
			{
				TabDatas =
				[
					new TabDataBookmark
					{
						DataRepoGroupId = "TestGroup",
						DataRepoType = typeof(UnregisteredDataType),
						SelectedRows =
						[
							new SelectedRowView(
								new SelectedRow
								{
									Label = "Test Item",
									RowIndex = 0,
									DataKey = "test",
									DataValue = new UnregisteredDataType
									{
										Data = "should not serialize",
										Value = 42
									}
								}
							)
						]
					}
				]
			}
		};

		// Serialize to JSON
		string json = bookmark.ToJson();

		// Deserialize back
		bool success = Bookmark.TryParseJson(json, out Bookmark? deserialized);

		// Verify deserialization succeeded
		Assert.That(success, Is.True);
		Assert.That(deserialized, Is.Not.Null);

		// Verify the unregistered type in DataValue was blocked (serialized as null)
		var firstTabData = deserialized!.TabBookmark.TabDatas[0];
		var firstSelectedRow = firstTabData.SelectedRows[0];
		
		Assert.That(firstSelectedRow.SelectedRow.Label, Is.EqualTo("Test Item"));
		Assert.That(firstSelectedRow.SelectedRow.DataKey, Is.EqualTo("test"));
		
		// Unregistered types should be blocked from serialization (result in null)
		Assert.That(firstSelectedRow.SelectedRow.DataValue, Is.Null);
	}
}
