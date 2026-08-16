using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using SideScroll.Avalonia.Tests;

[assembly: AvaloniaTestApplication(typeof(HeadlessAppBuilder))]

namespace SideScroll.Avalonia.Tests;

/// <summary>
/// Runs an Avalonia <see cref="Application"/> for the tests that need one, so controls can be
/// constructed and driven rather than reached through an extracted helper
/// </summary>
/// <remarks>
/// Mark a test with <c>[AvaloniaTest]</c> to run it on the UI thread with this application
/// available. Plain <c>[Test]</c> methods still run without one, which is what the tests written
/// before this harness rely on
/// </remarks>
public static class HeadlessAppBuilder
{
	/// <summary>Builds the headless application, discovered through <see cref="AvaloniaTestApplicationAttribute"/>.</summary>
	public static AppBuilder BuildAvaloniaApp()
	{
		return AppBuilder.Configure<HeadlessApp>()
			.UseSkia()
			.UseHeadless(new AvaloniaHeadlessPlatformOptions
			{
				// Real drawing, so a control that only fails while rendering still fails here
				UseHeadlessDrawing = false,
			});
	}
}

/// <summary>
/// Helpers for driving a window far enough that what's being measured is actually true of it
/// </summary>
public static class HeadlessWindow
{
	/// <summary>
	/// Shows a window and settles its layout, including anything applied while rendering
	/// </summary>
	/// <remarks>
	/// <see cref="Layoutable.Measure"/> and <see cref="Layoutable.Arrange"/> alone aren't enough.
	/// A <see cref="ScrollViewer"/>'s offset isn't realized until a frame is rendered, so a control
	/// scrolled out of view still reports its unscrolled position and a test reads the layout it
	/// asked for rather than the one on screen
	/// </remarks>
	public static void ShowAndSettle(Window window)
	{
		window.Show();
		Settle(window);
	}

	/// <summary>
	/// Settles a shown window's layout after a change, such as assigning a scroll offset
	/// </summary>
	public static void Settle(Window window)
	{
		window.Measure(new Size(window.Width, window.Height));
		window.Arrange(new Rect(0, 0, window.Width, window.Height));

		AvaloniaHeadlessPlatform.ForceRenderTimerTick();
		Dispatcher.UIThread.RunJobs();
	}
}

/// <summary>
/// The styles the sample app declares in XAML, added in code so the test project doesn't need
/// Avalonia's XAML build targets for one file
/// </summary>
public class HeadlessApp : Application
{
	/// <inheritdoc/>
	public override void Initialize()
	{
		Styles.Add(new FluentTheme());

		foreach (string source in new[]
		{
			"avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml",
			"avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml",
			"avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml",
			"avares://SideScroll.Avalonia/Themes/Fluent/Fluent.xaml",
		})
		{
			Styles.Add(new StyleInclude(new Uri("avares://SideScroll.Avalonia.Tests/"))
			{
				Source = new Uri(source),
			});
		}

		foreach (string source in new[]
		{
			"avares://SideScroll.Avalonia/Themes/Controls/ControlThemes.xaml",
			"avares://SideScroll.Avalonia.Charts.LiveCharts/Themes/ControlThemes.xaml",
		})
		{
			Resources.MergedDictionaries.Add(new ResourceInclude(new Uri("avares://SideScroll.Avalonia.Tests/"))
			{
				Source = new Uri(source),
			});
		}
	}
}
