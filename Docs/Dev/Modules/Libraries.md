# Libraries

- All libraries use .NET 8.0

## Core Libraries

* Atlas.Core
  - Tab Interfaces
  -	Logging
  - Utilities - (Extensions, Attributes, etc)

* Atlas.Serialize
  - Serializes objects to and from a local DataRepo
  - Supports lazy loading and circular references
  - [Serializer.md]

* Atlas.Network
  - HTTP - (downloading files)
  
## Tab Libraries

* Atlas.Tabs
  - Defines the UI models
  - Contains all the Tab interface logic for everything
  - Shared across UI frameworks
    - Currently only AvaloniaUI
  
* Atlas.Tabs.Tools
  - File Browser
  
* Atlas.Tabs.Samples
  - Used for testing UI features
  - `* Samples` tabs that show up in debug mode

## User Interface Libraries

* Atlas.UI.Avalonia
  - Cross Platform UI Controls
  - Works on Windows, MacOS, and Linux

* Atlas.UI.Avalonia.Charts
  - Base chart interface

* Atlas.UI.Avalonia.LiveCharts
  - LiveCharts version of the Charts package

* Atlas.UI.Avalonia.ScreenCapture
  - Screenshot control for Windows and MacOS
