# Setup

- It's recommended to start with a default [Avalonia .NET App](https://avaloniaui.net/gettingstarted#installation)
- Make sure to update the Avalonia version to match SideScroll

## Add Nuget Packages
- Add the Avalonia NuGet packages
  - `Avalonia.Desktop`
  - `Avalonia.Fonts.Inter`
  - `Avalonia.Themes.Fluent`
- Add the NuGet package `SideScroll.UI.Avalonia`
- Add any optional SideScroll packages
  - `SideScroll.UI.Avalonia.Charts.LiveCharts`
  - `SideScroll.UI.Avalonia.ScreenCapture`

## App.xaml
```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.Start.App"
             RequestedThemeVariant="Default">
  <Application.Styles>
    <FluentTheme/>
    <StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>
    <StyleInclude Source="avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml"/>
    <StyleInclude Source="avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml"/>
    <StyleInclude Source="avares://SideScroll.UI.Avalonia/Themes/Fluent/Fluent.xaml"/>
  </Application.Styles>

  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceInclude Source="avares://SideScroll.UI.Avalonia/Themes/Controls/ControlThemes.xaml"/>
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

## Create a new MainWindow
- Replace the `MainWindow.cs` with a non-`.xaml` version
- Alternatively, a TabViewer can also be added to an Avalonia Control (see `BaseWindow.cs` for an example)

```csharp
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using SideScroll.UI.Avalonia;
// using SideScroll.UI.Avalonia.Charts.LiveCharts;
// using SideScroll.UI.Avalonia.ScreenCapture;

namespace MyApp.Start;

public class SideScrollWindow : BaseWindow
{
	public SideScrollWindow() : base(new Project(Settings))
	{
		AddTab(new TabSample());

		// LiveChartCreator.Register();
		// ScreenCapture.AddControlTo(TabViewer);
		TabViewer.Toolbar?.AddVersion();
	}

	public static ProjectSettings Settings => new()
	{
		Name = "<AppName>",
		LinkType = "<LinkPrefix>",
		Version = new Version(0, 1),
		DataVersion = new Version(0, 1),
	};
}
```

#### Sample Tab
```csharp
namespace SideScroll.Tabs.Samples;

public class TabSample : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>()
			{
				new("Tab 1", new TabSample()),
				new("Tab 2", new TabSample()),
			};

			model.Actions = new List<TaskCreator>()
			{
				new TaskDelegate("Log this", LogThis, true, true),
			};
		}

		private void LogThis(Call call)
		{
			call.Log.Add("I've been logged");
		}
	}
}
```

See [Adding Tabs](AddingTabs.md) for more