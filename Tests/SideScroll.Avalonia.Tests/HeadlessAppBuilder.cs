using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SideScroll.Avalonia.Tests;
using SkiaSharp;

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
	/// <summary>The embedded family the tests draw with, rather than one the machine happens to have.</summary>
	private const string InterFontFamily = "avares://Avalonia.Fonts.Inter/Assets#Inter";

	/// <summary>The regular face of that family, for a consumer that wants a typeface rather than a family name.</summary>
	internal const string InterRegularAsset = "avares://Avalonia.Fonts.Inter/Assets/Inter-Regular.ttf";

	/// <summary>Builds the headless application, discovered through <see cref="AvaloniaTestApplicationAttribute"/>.</summary>
	public static AppBuilder BuildAvaloniaApp()
	{
		return AppBuilder.Configure<HeadlessApp>()
			// Text is measured with whatever font the machine provides otherwise, and the tests that
			// measure it then depend on which machine is running them. WithInterFont() registers an
			// embedded family but doesn't name a default one, and FontManager resolves that name
			// from the platform before the collection it registered can help, so it's named here
			.WithInterFont()
			.With(new FontManagerOptions
			{
				DefaultFamilyName = InterFontFamily,
			})
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
	/// Shows a window and settles its layout
	/// </summary>
	public static void ShowAndSettle(Window window)
	{
		window.Show();
		Settle(window);
	}

	/// <summary>
	/// Settles a shown window's layout after a change, such as assigning a scroll offset
	/// </summary>
	/// <remarks>
	/// Draining the dispatcher is what does this. <see cref="Layoutable.Measure"/> and
	/// <see cref="Layoutable.Arrange"/> aren't enough on their own — a <see cref="ScrollViewer"/>'s
	/// offset isn't applied until the queued work runs, so a control scrolled out of view still
	/// reports its unscrolled position and a test reads the layout it asked for rather than the one
	/// on screen. Measuring and arranging first only sizes a window that hasn't been laid out yet
	/// </remarks>
	public static void Settle(Window window)
	{
		window.Measure(new Size(window.Width, window.Height));
		window.Arrange(new Rect(0, 0, window.Width, window.Height));

		Dispatcher.UIThread.RunJobs();
	}
}

/// <summary>
/// The styles the sample app declares in XAML, added in code so the test project doesn't need
/// Avalonia's XAML build targets for one file, and the font LiveCharts shapes its labels with
/// </summary>
public class HeadlessApp : Application
{
	/// <summary>
	/// The face LiveCharts shapes with, held for the process because a disposed typeface leaves the
	/// shaper back where it started
	/// </summary>
	private static SKTypeface? _liveChartsTypeface;

	/// <inheritdoc/>
	public override void Initialize()
	{
		ConfigureLiveChartsFont();

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

	/// <summary>
	/// Gives LiveCharts the embedded font as well
	/// </summary>
	/// <remarks>
	/// LiveCharts shapes its labels through an <see cref="SKTypeface"/> of its own rather than
	/// Avalonia's font manager, so naming a default family for the latter doesn't reach it and its
	/// axis labels are still measured with a face off the machine. Loading the same one it draws
	/// everything else with keeps a chart's text measurement as reproducible as the rest, and it's
	/// set globally because the charts under test don't choose a font
	/// </remarks>
	private static void ConfigureLiveChartsFont()
	{
		if (_liveChartsTypeface == null)
		{
			using Stream stream = AssetLoader.Open(new Uri(HeadlessAppBuilder.InterRegularAsset));
			_liveChartsTypeface = SKTypeface.FromStream(stream);
		}

		LiveCharts.Configure(settings => settings.HasTextSettings(new TextSettings
		{
			DefaultTypeface = _liveChartsTypeface,
		}));
	}
}
