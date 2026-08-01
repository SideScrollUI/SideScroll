using NUnit.Framework;
using SideScroll.Collections;

// Importing the namespace collides with NUnit's Category and Description attributes
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;

namespace SideScroll.Tests.Collections;

[Category("Collections")]
public class ItemCollectionTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ItemCollection");
	}

	[Test, Description(
		"AddRange() adds to the backing list directly, so it has to raise the notifications " +
		"InsertItem() would have. Without them a binding to Count never updates")]
	public void AddRangeNotifiesCountChanged()
	{
		ItemCollection<int> items = [];

		List<string?> propertyNames = [];
		((INotifyPropertyChanged)items).PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName);

		items.AddRange([1, 2, 3]);

		Assert.That(propertyNames, Does.Contain(nameof(ItemCollection<int>.Count)));
		Assert.That(propertyNames, Does.Contain("Item[]"));
		Assert.That(items, Has.Count.EqualTo(3));
	}

	[Test, Description("The collection change is still raised, and after the property changes")]
	public void AddRangeStillNotifiesCollectionChanged()
	{
		ItemCollection<int> items = [];

		int collectionChanges = 0;
		items.CollectionChanged += (_, _) => collectionChanges++;

		items.AddRange([1, 2]);

		Assert.That(collectionChanges, Is.EqualTo(1), "One Reset, not one per item.");
	}
}
