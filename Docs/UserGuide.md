# User Guide

## Selecting Items
* Use Ctrl or Shift to select multiple items
* Previous selections are remembered, and used to select future options when loading the same or similar tabs

## Filtering

* Press Ctrl-F on any Data Grid to show the filter (You can click anywhere on a tab to focus it)
* You can use | or & to restrict searches
* Examples:
  - Search for the exact string "ABC" or anything containing 123
    - "ABC" | 123
* Recursive searches will eventually be supported

## Hints
* To copy the cell data to the clipboard, you can right click and select `Copy`
* You can clear the local disk cache & settings by going to `Samples` -> `Data Repos` and clicking `Delete`

## Links

You can save a link for any location in the UI. Override the `Linker` class to add new methods that add and retrieve links.

![Bookmarks](/../Images/Screenshots/bookmarks.png)

## Widescreen monitor recommended

Atlas automatically displays all entries in a recursive Parent->Child->Sub-Child...and will keep going until it hits the edge of your screen or an object that can't be expanded. Because of this, entries can easily get to be 10-20 levels deep. Atlas does include some autoscrolling capability, but it really shines with a widescreen monitor. It's much easier to use when everything is visible at once.

![Widescreen](/../Images/Screenshots/widescreen.png)
