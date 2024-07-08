# Build

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
