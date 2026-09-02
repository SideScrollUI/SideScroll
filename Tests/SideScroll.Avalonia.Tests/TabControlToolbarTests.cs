using Avalonia.Headless.NUnit;
using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Avalonia.Tests;

/// <summary>
/// LoadToolbar() is public and can be called again to refresh or swap a toolbar, so it has to
/// replace what it built rather than add to it. String labels and separators are used here, the
/// button controls need an Avalonia Application for their icons
/// </summary>
public class TabControlToolbarTests
{
	private class LabelToolbar : TabToolbar
	{
		public string First { get; set; } = "First";

		[Separator]
		public string Second { get; set; } = "Second";
	}

	[AvaloniaTest, Description(
		"Nothing was cleared, so a second call appended a duplicate of every control, column, " +
		"hotkey, and event handler instead of rebuilding")]
	public void LoadingTwiceReplacesTheControls()
	{
		var toolbar = new TabControlToolbar();
		var model = new LabelToolbar();

		toolbar.LoadToolbar(model);
		int children = toolbar.Children.Count;
		int columns = toolbar.ColumnDefinitions.Count;

		Assert.That(children, Is.GreaterThan(0), "precondition: it built something");

		toolbar.LoadToolbar(model);

		Assert.That(toolbar.Children, Has.Count.EqualTo(children));
		Assert.That(toolbar.ColumnDefinitions, Has.Count.EqualTo(columns));
	}

	[AvaloniaTest, Description("A different model replaces the previous one rather than being appended to it")]
	public void LoadingASecondToolbarReplacesTheFirst()
	{
		var toolbar = new TabControlToolbar();

		toolbar.LoadToolbar(new LabelToolbar());
		int children = toolbar.Children.Count;

		toolbar.LoadToolbar(new TabToolbar());

		Assert.That(toolbar.Children, Has.Count.LessThan(children), "the empty model leaves fewer controls");
	}

	private class ThrowingToolbar : TabToolbar
	{
		public string First { get; set; } = "First";

		public string Throws => throw new InvalidOperationException("toolbar property unavailable");

		public string Last { get; set; } = "Last";
	}

	[AvaloniaTest, Description(
		"Every property getter is called before it's known whether it's a control, so one that " +
		"threw stopped the toolbar rendering at that point and lost every control after it")]
	public void AThrowingPropertyDoesNotStopTheRest()
	{
		var toolbar = new TabControlToolbar();

		Assert.DoesNotThrow(() => toolbar.LoadToolbar(new ThrowingToolbar()));

		// The two labels either side of it, the throwing one contributes nothing
		Assert.That(toolbar.Children, Has.Count.EqualTo(2));
	}
}
