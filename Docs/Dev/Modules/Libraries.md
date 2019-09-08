# Libraries

---

## Core Libraries

* Atlas.Core (.NET Standard)

  - GUI Tab Interface
  -	Logging
  - Utilities - (Extensions, Attributes, etc)

* Atlas.Serialize (.NET Standard)
  - Serializes all objects into a customer data store
  - Supports lazy loading and circular references
  - This is growing so big it should probably be it's own library)
  - [Serializer.md]
* Atlas.Network (.NET Standard)
  - HTTP - (downloading files)
  - FTP - (downloading files)
  
## Tab Libraries

* Atlas.Tabs (.NET Standard)
  - Defines the GUI menus
  - Contains all the Tab interface logic for everything
  - Shared across GUIs
  
* Atlas.Tabs.Tools (.NET Standard)
  - File Browser
  
* Atlas.Tabs.Test (.NET Standard)
  - File Browser

## GUI Libraries

* Atlas.GUI.WPF (.NET Framework)
  - Windows only GUI Controls
  - Furthest along of the GUI interfaces
  - Has a lot better layout controls than WinForms
  - Ideally would eventually get deprecated for a cross platform solution
  
* Atlas.GUI.Avalonia (.NET Standard)
  - Cross Platform GUI Controls
  - Works well on Windows, Linux?, Mac?
  - Still early beta

     
