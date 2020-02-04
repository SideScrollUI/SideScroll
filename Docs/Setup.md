# Setup

## Clone Atlas
- `git clone https://github.com/garyhertel/Atlas.git`
- `cd Atlas`
- Restore SubModules for AvaloniaUI (the nuget packages aren't recent enough)
  - `git submodule update --init --recursive`
- (If Required) Updating sub-module git repos
  - `git submodule sync --recursive`
  - `git submodule update --init --recursive --remote`

## Console
- Build
  - `dotnet build Atlas.Avalonia.sln`
- Run
  - `dotnet run --project Programs/Atlas.Start.Avalonia/Atlas.Start.Avalonia.csproj`
## IDE
- Open `Atlas.sln`
- Operating system
  - Windows
    - Visual Studio 2019 - (recommended) (community edition or higher)
  - Linux
      - Visual Studio Code
      - JetBrains Rider?
  - Mac
      - Visual Studio
        - Note this is a completely different Visual Studio than the windows version
      - JetBrains Rider
- Start Atlas in Debug Mode
  - Set the start project to one of the programs
  - `Programs / Atlas.Start.Avalonia`
    - .NET Core
      - Cross platform (Windows, Linux, Mac)
    - AvaloniaUI still in alpha
      - Still pretty slow
      - Not many controls available
  - `Programs / Atlas.Start.Wpf`
    - .NET Framework
      - Windows Only
  - It's recommended to run Atlas in debug mode to catch exceptions
    
