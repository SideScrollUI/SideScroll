# Development

## Clone
- `git clone https://github.com/SideScrollUI/SideScroll.git`
- `cd SideScroll`

## Console
- Build
  - `dotnet build SideScroll.sln`
- Run
  - `dotnet run --project Programs/SideScroll.Start.Avalonia/SideScroll.Start.Avalonia.csproj`

### Publishing
- Pack
  - Update `<SideScrollPackageReleaseNotes>` in [Directory.Build.props](../../Directory.Build.props)
  - `dotnet pack -o Packages`

## IDE
- IDE's
  - [JetBrains Rider](https://www.jetbrains.com/rider/)
  - [Visual Studio](IDEs/VisualStudio.md) (Windows Only)
  - [Visual Studio Code](IDEs/VisualStudioCode.md)
- Open `SideScroll.sln`
- Start SideScroll
  - `Programs / SideScroll.Start.Avalonia`

## Getting Started
* [Adding Tabs](AddingTabs.md)
* [Param Controls](ParamControls.md)
* [DataRepos](DataRepos.md)
* [Serializer](Serializer.md)
* [Logs](Logs.md)
* [Bookmarks](Bookmarks.md)
* [Charts](Charts.md)
* [Projects](Projects.md)
