using Atlas.Tabs;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Core.Test;

[Category("Core")]
public class TestTabItemCollection : TestBase
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TabItemCollection");
	}

	public void TestSelected(IList list, params object[] selectedObjects)
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

		Assert.AreEqual(selectedObjects.Length, foundObjects.Count);

		for (int i = 0; i < selectedObjects.Length; i++)
		{
			Assert.AreEqual(selectedObjects[i], foundObjects[i]);
		}

		TestSwapping(list, selectedObjects, collection, selectedRows);
	}

	// Swap indices 0 and 1
	private static void TestSwapping(IList list, object[] selectedObjects, TabItemCollection collection, HashSet<SelectedRow> selectedRows)
	{
		object obj = list[0];
		list[0] = list[1];
		list[1] = obj;

		List<object> offsetObjects = collection.GetSelectedObjects(selectedRows);

		Assert.AreEqual(selectedObjects.Length, offsetObjects.Count);

		for (int i = 0; i < selectedObjects.Length; i++)
		{
			Assert.AreEqual(selectedObjects[i], offsetObjects[i]);
		}
	}

	public class ToStringClass
	{
		public int Id { get; set; }

		public override string ToString() => Id.ToString();

		public ToStringClass(int id)
		{
			Id = id;
		}
	}

	[Test]
	public void TestToString()
	{
		List<ToStringClass> Items = new()
		{
			new(1),
			new(2),
			new(3),
		};

		TestSelected(Items, Items[1]);
	}

	public class DataKeyClass
	{
		[DataKey]
		public int Id { get; set; }

		public DataKeyClass(int id)
		{
			Id = id;
		}
	}

	[Test]
	public void TestDataKey()
	{
		List<DataKeyClass> Items = new()
		{
			new(1),
			new(2),
			new(3),
		};

		TestSelected(Items, Items[1]);
	}

	public class DataValueClass
	{
		[DataValue]
		public DataKeyClass DataKeyClass { get; set; }

		public DataValueClass(int id)
		{
			DataKeyClass = new DataKeyClass(id);
		}
	}

	[Test]
	public void TestDataValue()
	{
		List<DataValueClass> Items = new()
		{
			new DataValueClass(1),
			new DataValueClass(2),
			new DataValueClass(3),
		};

		TestSelected(Items, Items[1]);
	}
}
