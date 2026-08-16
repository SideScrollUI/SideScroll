using Avalonia.Headless.NUnit;
using NUnit.Framework;
using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Avalonia.Tests;

/// <summary>
/// Covers the headless application harness itself, so a break in it is reported here rather than
/// as an unrelated failure in whatever test needed it
/// </summary>
public class HeadlessHarnessTests
{
	private class ButtonToolbar : TabToolbar
	{
		public ToolButton Refresh { get; set; } = new("Refresh", Icons.Svg.Refresh);
	}

	[AvaloniaTest, Description(
		"A toolbar button builds its icon from a resource, which needs an Application. Without one " +
		"this throws inside TabImageButton.CreateDefaultImage()")]
	public void AToolbarButtonCanBeBuilt()
	{
		var toolbar = new TabControlToolbar();

		toolbar.LoadToolbar(new ButtonToolbar());

		Assert.That(toolbar.Children, Is.Not.Empty);
		Assert.That(toolbar.Children.OfType<ToolbarButton>().Count(), Is.EqualTo(1));
	}

	[AvaloniaTest, Description("Clearing and rebuilding works with real controls, not only labels")]
	public void ReloadingAToolbarOfButtonsReplacesThem()
	{
		var toolbar = new TabControlToolbar();
		var model = new ButtonToolbar();

		toolbar.LoadToolbar(model);
		int children = toolbar.Children.Count;

		toolbar.LoadToolbar(model);

		Assert.That(toolbar.Children, Has.Count.EqualTo(children));
	}

	[AvaloniaTest, Description("The application and its styles are available to a test that asks for them")]
	public void TheApplicationIsRunning()
	{
		Assert.That(global::Avalonia.Application.Current, Is.Not.Null);
		Assert.That(global::Avalonia.Application.Current!.Styles, Is.Not.Empty);
	}
}
