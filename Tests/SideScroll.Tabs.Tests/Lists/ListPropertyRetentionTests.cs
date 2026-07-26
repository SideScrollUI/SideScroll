using NUnit.Framework;
using SideScroll.Tabs.Lists;
using System.Runtime.CompilerServices;
// Aliased, importing System.ComponentModel makes Category/Description ambiguous with NUnit's
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;
using PropertyChangedEventHandler = System.ComponentModel.PropertyChangedEventHandler;

namespace SideScroll.Tabs.Tests;

/// <summary>
/// A ListProperty subscribes to its source object, so the source holds the ListProperty, and the
/// ListProperty holds everything subscribed to it. Toolbar bindings pair a long lived source
/// (a NodeView owned by the parent tab's model) with a short lived subscriber that references the
/// tab, so anything subscribing has to unsubscribe or the tab is never collected
/// </summary>
[Category("Tabs")]
public class ListPropertyRetentionTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ListPropertyRetention");
	}

	/// <summary>Stands in for a NodeView, owned by the parent tab and outliving the tab bound to it.</summary>
	public class Source : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private bool _favorite;
		public bool Favorite
		{
			get => _favorite;
			set
			{
				_favorite = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Favorite)));
			}
		}
	}

	/// <summary>
	/// Stands in for ToolbarToggleButton: subscribes to the ListProperty and holds the TabInstance.
	/// Dispose() mirrors what that control does, unsubscribe and release the binding
	/// </summary>
	private class Subscriber
	{
		public TabInstance TabInstance { get; } = new();

		private readonly ListProperty _listProperty;

		public Subscriber(ListProperty listProperty)
		{
			_listProperty = listProperty;
			_listProperty.PropertyChanged += ListProperty_PropertyChanged;
		}

		public void Dispose()
		{
			_listProperty.PropertyChanged -= ListProperty_PropertyChanged;
			_listProperty.Dispose();
		}

		private void ListProperty_PropertyChanged(object? sender, PropertyChangedEventArgs e) { }
	}

	// Kept out of the test method so the locals aren't still rooted while collecting
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static (WeakReference Subscriber, WeakReference TabInstance, WeakReference ListProperty) Bind(
		Source source, bool dispose)
	{
		var listProperty = new ListProperty(source, nameof(Source.Favorite));
		var subscriber = new Subscriber(listProperty);

		if (dispose)
		{
			subscriber.Dispose();
		}

		return (new WeakReference(subscriber), new WeakReference(subscriber.TabInstance), new WeakReference(listProperty));
	}

	private static void Collect()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}

	[Test, Description(
		"Establishes the leak: a subscriber that never unsubscribes stays reachable through the source " +
		"object for as long as the source lives, dragging the TabInstance with it.")]
	public void Subscriber_WithoutUnsubscribing_IsHeldBySource()
	{
		var source = new Source();

		(WeakReference subscriber, WeakReference tabInstance, WeakReference listProperty) = Bind(source, dispose: false);

		Collect();

		Assert.That(subscriber.IsAlive, Is.True,
			"The source object's PropertyChanged holds the ListProperty, which holds the subscriber.");
		Assert.That(tabInstance.IsAlive, Is.True,
			"So the TabInstance the subscriber references leaks too, once per navigation.");
		Assert.That(listProperty.IsAlive, Is.True);

		GC.KeepAlive(source);
	}

	[Test, Description(
		"Disposing is what releases the tab. ToolbarToggleButton subscribes in its constructor and " +
		"owns the binding, so it has to undo both when disposed.")]
	public void Subscriber_AfterDisposing_ReleasesTheWholeChain()
	{
		var source = new Source();

		(WeakReference subscriber, WeakReference tabInstance, WeakReference listProperty) = Bind(source, dispose: true);

		Collect();

		Assert.That(subscriber.IsAlive, Is.False,
			"Unsubscribing should leave nothing reachable from the source object.");
		Assert.That(tabInstance.IsAlive, Is.False,
			"And the TabInstance should be collectable.");
		Assert.That(listProperty.IsAlive, Is.False,
			"Disposing the binding releases it from the source object too, so nothing accumulates.");

		GC.KeepAlive(source);
	}

	[Test, Description(
		"Disposing the ListProperty releases it from the source, which frees its subscribers too. " +
		"This is the other end of the same chain.")]
	public void ListProperty_AfterDispose_IsReleasedBySource()
	{
		var source = new Source();

		WeakReference listProperty = BindAndDisposeProperty(source);

		Collect();

		Assert.That(listProperty.IsAlive, Is.False,
			"Dispose() unsubscribes from the source, so nothing holds the ListProperty.");

		GC.KeepAlive(source);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static WeakReference BindAndDisposeProperty(Source source)
	{
		var listProperty = new ListProperty(source, nameof(Source.Favorite));
		listProperty.Dispose();
		return new WeakReference(listProperty);
	}

	[Test, Description("Without Dispose() the source keeps the ListProperty alive")]
	public void ListProperty_WithoutDispose_IsHeldBySource()
	{
		var source = new Source();

		WeakReference listProperty = BindProperty(source);

		Collect();

		Assert.That(listProperty.IsAlive, Is.True);

		GC.KeepAlive(source);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static WeakReference BindProperty(Source source)
	{
		return new WeakReference(new ListProperty(source, nameof(Source.Favorite)));
	}
}
