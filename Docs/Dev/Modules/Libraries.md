# Libraries

- All libraries use .NET Standard 2.0

## Core Libraries

* Atlas.Core

  - UI Tab Interface
  -	Logging
  - Utilities - (Extensions, Attributes, etc)

* Atlas.Serialize
  - Serializes all objects into a customer data store
  - Supports lazy loading and circular references
  - This is growing so big it should probably be it's own library)
  - [Serializer.md]
* Atlas.Network
  - HTTP - (downloading files)
  - FTP - (downloading files)
  
## Tab Libraries

* Atlas.Tabs
  - Defines the UI menus
  - Contains all the Tab interface logic for everything
  - Shared across UI frameworks
    - Currently only AvaloniaUI
  
* Atlas.Tabs.Tools
  - File Browser
  
* Atlas.Tabs.Samples
  - `* Samples` tabs that show up in debug mode
  - Used for testing UI features

## User Interface Libraries
  
* Atlas.UI.Avalonia
  - Cross Platform UI Controls
  - Works on Windows, Linux, and Mac
