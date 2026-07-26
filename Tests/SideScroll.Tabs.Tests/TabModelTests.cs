using NUnit.Framework;
using SideScroll.Tabs.Lists;
using System.Collections;
// Aliased, importing System.ComponentModel makes Category/Description ambiguous with NUnit's
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;
using PropertyChangedEventHandler = System.ComponentModel.PropertyChangedEventHandler;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class TabModelTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TabModel");
	}

	/// <summary>A caller owned row that would be broken if the tab disposed it when closing.</summary>
	public class DisposableRow : IDisposable
	{
		public string Name { get; set; } = "Row";

		public bool Disposed { get; private set; }

		public void Dispose() => Disposed = true;
	}

	[Test, Description(
		"Item lists hold the caller's own objects, so closing a tab must not dispose them. " +
		"Only the rows the model created are owned here.")]
	public void Clear_CallerItems_AreNotDisposed()
	{
		var rows = new List<DisposableRow> { new(), new() };

		var model = new TabModel();
		model.AddItems(rows);

		Assert.That(model.ItemLists.Single(), Is.SameAs(rows),
			"The caller's list should be added as-is, otherwise this isn't testing the right path.");

		model.Clear();

		Assert.That(rows.Select(row => row.Disposed), Has.All.False,
			"The caller still owns these objects after the tab closes.");
	}

	/// <summary>Source object whose property changes a <see cref="ListProperty"/> row subscribes to.</summary>
	public class NotifyingSource : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private string _value = "a";
		public string Value
		{
			get => _value;
			set
			{
				_value = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
			}
		}
	}

	[Test, Description(
		"ListProperty rows subscribe to their source object's PropertyChanged, so Clear() must still " +
		"dispose them or the source keeps the closed tab's rows alive.")]
	public void Clear_ListPropertyRows_UnsubscribeFromSource()
	{
		var source = new NotifyingSource();

		var model = new TabModel();
		model.AddItems(source);

		ListProperty listProperty = model.ItemLists
			.SelectMany(list => list.Cast<object>())
			.OfType<ListProperty>()
			.Single(property => property.PropertyInfo.Name == nameof(NotifyingSource.Value));

		int changes = 0;
		listProperty.PropertyChanged += (_, _) => changes++;

		source.Value = "b";
		Assert.That(changes, Is.GreaterThan(0),
			"The row should forward source changes while the tab is open.");

		int changesWhileOpen = changes;
		model.Clear();
		source.Value = "c";

		Assert.That(changes, Is.EqualTo(changesWhileOpen),
			"After Clear() the row should no longer be subscribed to the source.");
	}

	/// <summary>A list that throws while enumerating, e.g. one computed lazily from disposed state.</summary>
	public class ThrowingList : IList
	{
		public IEnumerator GetEnumerator() => throw new InvalidOperationException("Enumeration failed");

		public int Count => 1;
		public bool IsReadOnly => true;
		public bool IsFixedSize => true;
		public bool IsSynchronized => false;
		public object SyncRoot => this;
		public object? this[int index] { get => null; set { } }

		public int Add(object? value) => 0;
		public void Clear() { }
		public bool Contains(object? value) => false;
		public int IndexOf(object? value) => -1;
		public void Insert(int index, object? value) { }
		public void Remove(object? value) { }
		public void RemoveAt(int index) { }
		public void CopyTo(Array array, int index) { }
	}

	[Test, Description(
		"Clear() runs from TabInstance.Dispose(), so a list that throws while enumerating must not " +
		"stop the model from being cleared (or the caller's Dispose from finishing).")]
	public void Clear_ThrowingList_StillClearsModel()
	{
		var model = new TabModel();
		model.ItemLists.Add(new ThrowingList());
		model.AddObject("data");

		Assert.DoesNotThrow(() => model.Clear());

		Assert.That(model.ItemLists, Is.Empty);
		Assert.That(model.Objects, Is.Empty);
	}
}
