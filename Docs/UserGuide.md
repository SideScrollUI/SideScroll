
# User Guide
---

## GUI
---

### Selecting Items
* Use Ctrl or Shift to select multiple items
* Any time you select an item, Atlas will remember it and will select that exact same item the next time that list is shown.

### Filtering

* Press Ctrl-F on any Data Grid to add a filter (You can click anywhere on a tab to focus it)
* You can use | or & to restrict searches
* Examples:
  - Search for the exact string "ABC" or anything containing 123
    - "ABC" | 123
* Recursive searches will eventually be supported

### Hints
* To copy the cell data to the clipboard, you can double click on the cell, or right click and select `Copy`
* You can clear the local disk cache & settings by going to `Test` -> `Data Repos` and clicking `Delete`

### Bookmarks

You can save a bookmark for any location in the GUI. Multiple bookmarks can be selected at once and the merged results will be shown.

![Bookmarks](/../Images/Screenshots/bookmarks.png)

### Widescreen monitor recommended

Atlas automatically displays all entries in a recursive Parent->Child->Sub-Child...and will keep going until it hits the edge of your screen or an object that can't be expanded. Because of this, entries can easily get to be 10-20 levels deep. Atlas does include some autoscrolling capability, but it really shines with a widescreen monitor. It's much easier to use when everything is visible at once.

![Widescreen](/../Images/Screenshots/widescreen.png)

### Web Browser (Windows/WPF Only)

By default Windows will use IE 8 for it's embedded web browser, which doesn't work well on a lot of websites. To enable IE 11 mode for Atlas, you can start Atlas in Administrator mode (right click and `Run as adminstrator`) and it will add the registry entry for you. You can restart it afterwards and all future sessions with the same application name will have it enabled. You can also enable it manually by running `regedit` and adding an entry here:

* `Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION`
  - Type = `DWORD`
  - Name = `Atlas.Start.Wpf.exe`
  - Value = `2af9`
