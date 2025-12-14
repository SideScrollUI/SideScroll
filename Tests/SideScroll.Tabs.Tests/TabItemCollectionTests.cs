using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Tabs.Settings;
using System.Collections;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class TabItemCollectionTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TabItemCollection");
	}

	private static void TestSelected(IList list, params object[] selectedObjects)
	{
		TabItemCollection collection = new(list);

		HashSet<SelectedRow> selectedRows = selectedObjects
			.Select(s => new SelectedRow(s)
			{
				Object = null, // No cheating
			})
			.ToHashSet();

		TestSame(list, selectedObjects, collection, selectedRows);
		TestSwapping(list, selectedObjects, collection, selectedRows);
	}

	private static void TestSame(IList list, object[] selectedObjects, TabItemCollection collection, HashSet<SelectedRow> selectedRows)
	{
		List<object> foundObjects = collection.GetSelectedObjects(selectedRows);

		Assert.That(foundObjects, Has.Exactly(selectedObjects.Length).Items);

		for (int i = 0; i < selectedObjects.Length; i++)
		{
			Assert.That(foundObjects[i], Is.EqualTo(selectedObjects[i]));
		}

		TestSwapping(list, selectedObjects, collection, selectedRows);
	}

	// Swap indices 0 and 1
	private static void TestSwapping(IList list, object[] selectedObjects, TabItemCollection collection, HashSet<SelectedRow> selectedRows)
	{
		object obj = list[0]!;
		list[0] = list[1];
		list[1] = obj;

		List<object> offsetObjects = collection.GetSelectedObjects(selectedRows);

		Assert.That(offsetObjects, Has.Exactly(selectedObjects.Length).Items);

		for (int i = 0; i < selectedObjects.Length; i++)
		{
			Assert.That(offsetObjects[i], Is.EqualTo(selectedObjects[i]));
		}
	}

	public class ToStringClass(int id)
	{
		public int Id { get; set; } = id;

		public override string ToString() => Id.ToString();
	}

	[Test]
	public void SelectToString()
	{
		List<ToStringClass> Items =
		[
			new(1),
			new(2),
			new(3),
		];

		TestSelected(Items, Items[1]);
	}

	public class DataKeyClass(int id)
	{
		[DataKey]
		public int Id { get; set; } = id;
	}

	[Test]
	public void SelectDataKey()
	{
		List<DataKeyClass> items =
		[
			new(1),
			new(2),
			new(3),
		];

		TestSelected(items, items[1]);
	}

	public class DataValueClass(int id)
	{
		[DataValue]
		public DataKeyClass DataKeyClass { get; set; } = new(id);
	}

	[Test]
	public void SelectDataValue()
	{
		List<DataValueClass> items =
		[
			new DataValueClass(1),
			new DataValueClass(2),
			new DataValueClass(3),
		];

		TestSelected(items, items[1]);
	}
}
