using Avalonia.Headless.NUnit;
using NUnit.Framework;
using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using System.ComponentModel;

namespace SideScroll.Avalonia.Tests;

/// <summary>
/// The toolbar model owns the <see cref="ToolToggleButton.ListProperty"/> bindings its buttons
/// hold. A control rendering one releases only its own subscription, since the same model can be
/// rendered again
/// </summary>
public class ToolbarBindingOwnershipTests
{
	public class BoundModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private bool _enabled;
		public bool Enabled
		{
			get => _enabled;
			set
			{
				_enabled = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
			}
		}
	}

	private class ToggleToolbar : TabToolbar
	{
		public ToolToggleButton Toggle { get; set; }

		public ToggleToolbar(BoundModel model)
		{
			Toggle = new ToolToggleButton("Toggle", Icons.Svg.Refresh, Icons.Svg.Refresh, false)
			{
				ListProperty = new ListProperty(model, typeof(BoundModel).GetProperty(nameof(BoundModel.Enabled))!),
			};
		}
	}

	private static int CountNotifications(ListProperty listProperty, BoundModel model, bool value)
	{
		int notifications = 0;
		void Handler(object? sender, PropertyChangedEventArgs e) => notifications++;

		listProperty.PropertyChanged += Handler;
		model.Enabled = value;
		listProperty.PropertyChanged -= Handler;

		return notifications;
	}

	[AvaloniaTest, NUnit.Framework.Description(
		"Reloading disposes the controls it replaces, and the model's binding has to survive that " +
		"because the new controls bind to the same ListProperty instance")]
	public void ReloadingKeepsTheModelBindingAlive()
	{
		var model = new BoundModel();
		var toolbarModel = new ToggleToolbar(model);
		ListProperty listProperty = toolbarModel.Toggle.ListProperty!;

		var toolbar = new TabControlToolbar();
		toolbar.LoadToolbar(toolbarModel);

		Assert.That(CountNotifications(listProperty, model, true), Is.EqualTo(1), "bound after the first load");

		toolbar.LoadToolbar(toolbarModel);

		Assert.That(CountNotifications(listProperty, model, false), Is.EqualTo(1), "still bound after a reload");
	}

	[AvaloniaTest, NUnit.Framework.Description(
		"Replaced controls are disposed, so a control from an earlier load no longer holds a " +
		"subscription to a model that outlives it")]
	public void ReloadingReleasesTheReplacedControls()
	{
		var toolbarModel = new ToggleToolbar(new BoundModel());
		var toolbar = new TabControlToolbar();

		toolbar.LoadToolbar(toolbarModel);
		ToolbarToggleButton first = toolbar.Children.OfType<ToolbarToggleButton>().Single();

		toolbar.LoadToolbar(toolbarModel);
		ToolbarToggleButton second = toolbar.Children.OfType<ToolbarToggleButton>().Single();

		Assert.That(second, Is.Not.SameAs(first), "a new control was built");
		Assert.That(toolbar.Children.OfType<ToolbarToggleButton>().Count(), Is.EqualTo(1), "the old one was replaced");
	}

	[AvaloniaTest, NUnit.Framework.Description("Disposing the toolbar control leaves the model's binding for the model to release")]
	public void DisposingTheControlKeepsTheModelBinding()
	{
		var model = new BoundModel();
		var toolbarModel = new ToggleToolbar(model);
		ListProperty listProperty = toolbarModel.Toggle.ListProperty!;

		var toolbar = new TabControlToolbar();
		toolbar.LoadToolbar(toolbarModel);
		toolbar.Dispose();

		Assert.That(CountNotifications(listProperty, model, true), Is.EqualTo(1));
	}

	[Test, NUnit.Framework.Description("The model releases the binding, which is what nothing else was doing")]
	public void DisposingTheToolbarModelReleasesTheBinding()
	{
		var model = new BoundModel();
		var toolbarModel = new ToggleToolbar(model);
		ListProperty listProperty = toolbarModel.Toggle.ListProperty!;

		Assert.That(CountNotifications(listProperty, model, true), Is.EqualTo(1), "bound to start with");

		toolbarModel.Dispose();

		Assert.That(CountNotifications(listProperty, model, false), Is.EqualTo(0), "released");
		Assert.That(toolbarModel.Toggle.ListProperty, Is.Null);
	}

	[Test, NUnit.Framework.Description("Clearing a model releases the toolbars in it, which is how a closing tab reaches them")]
	public void ClearingATabModelDisposesItsToolbars()
	{
		var model = new BoundModel();
		var toolbarModel = new ToggleToolbar(model);
		ListProperty listProperty = toolbarModel.Toggle.ListProperty!;

		var tabModel = new TabModel();
		tabModel.AddObject(toolbarModel);

		tabModel.Clear();

		Assert.That(CountNotifications(listProperty, model, true), Is.EqualTo(0));
	}
}
