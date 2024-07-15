# Building

## Clone
- `git clone https://github.com/SideScrollUI/SideScroll.git`
- `cd SideScroll`

## Build
- `dotnet build`

## Run
- `dotnet run --project Programs/SideScroll.Start.Avalonia/SideScroll.Start.Avalonia.csproj`

### Pack
- Update `<Version>` and `<PackageReleaseNotes>` in [Directory.Build.props](../../Directory.Build.props)
- `dotnet pack -o Packages`
