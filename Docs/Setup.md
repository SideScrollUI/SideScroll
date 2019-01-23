

# Setup
---
1. Clone Atlas
    - `git clone https://github.com/garyhertel/Atlas.git`
    - `cd Atlas`
    - Restore SubModules for AvaloniaUI (the nuget packages aren't recent enough)
    - `git submodule update --init --recursive`
    - Apply AvaloniaUI patches to update version dependencies
      - `git apply AvaloniaUI.diff`
2. Console
    - `dotnet build Atlas.Avalonia.sln`
    - `dotnet run --project Programs/Atlas.Start.Avalonia/Atlas.Start.Avalonia.csproj`
3. IDE
  -Tested
    - Windows
      - Visual Studio 2017 - (recommended) (community edition or +)
    - Linux
        - Visual Studio Code
  - Open `Atlas.sln` in an IDE
  - Start Atlas in Debug Mode
    - At this stage, it's recommended to always run Atlas in debug mode to catch exceptions
4. Running
  - Configure Paths
    - After starting, Select `Settings` to change any of the default locations (not currently enabled)
    
