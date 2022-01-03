# Setup

## Clone Atlas
- `git clone https://github.com/garyhertel/Atlas.git`
- `cd Atlas`
- Restore SubModules
  - `git submodule update --init --recursive`
- (If Required) Updating sub-module git repos
  - `git submodule sync --recursive`
  - `git submodule update --init --recursive --remote`
- Pulling afterwards
  - `git pull --recurse-submodules`

## Console
- Build
  - `dotnet build Atlas.Avalonia.sln`
- Run
  - `dotnet run --project Programs/Atlas.Start.Avalonia/Atlas.Start.Avalonia.csproj`

## IDE
- Open `Atlas.sln`
- Operating system
  - Windows
    - Visual Studio 2022 - (recommended) (community edition or higher)
    - JetBrains Rider
  - Mac
    - JetBrains Rider
  - Linux
    - JetBrains Rider
    - Visual Studio Code
- Start Atlas in Debug Mode
  - Set the start project to one of the programs
  - `Programs / Atlas.Start.Avalonia`
    - .NET Core
      - Cross platform (Windows, Linux, Mac)
    
