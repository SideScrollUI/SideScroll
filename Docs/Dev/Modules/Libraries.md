# Libraries

- All libraries use .NET 8.0

## Core Libraries

* SideScroll.Core
  - Tab Interfaces
  -	Logging
  - Utilities - (Extensions, Attributes, etc)

* SideScroll.Serialize
  - Serializes objects to and from a local DataRepo
  - Supports lazy loading and circular references
  - [Serializer.md]

* SideScroll.Network
  - HTTP - (downloading files)
  
## Tab Libraries

* SideScroll.Tabs
  - Defines the UI models
  - Contains all the Tab interface logic for everything
  - Shared across UI frameworks
    - Currently only AvaloniaUI
  
* SideScroll.Tabs.Tools
  - File Browser
  
* SideScroll.Tabs.Samples
  - Used for testing UI features
  - `* Samples` tabs that show up in debug mode

## User Interface Libraries

* SideScroll.UI.Avalonia
  - Cross Platform UI Controls
  - Works on Windows, MacOS, and Linux

* SideScroll.UI.Avalonia.Charts
  - Base chart interface

* SideScroll.UI.Avalonia.LiveCharts
  - LiveCharts version of the Charts package

* SideScroll.UI.Avalonia.ScreenCapture
  - Screenshot control for Windows and MacOS
