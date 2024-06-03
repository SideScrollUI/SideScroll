# Adding new Image Resources

- Location
  - [Icons](../../Images/Libraries/Atlas.Resources/Icons/)

* Find new icon:
  - [Icon Icons](https://icon-icons.com/)
    - Select outline after searching
    - Download svg and convert to compatible svg (see below)

## Adding a new file:
- Add in the Assets folder
- Add a copy of a svg if possible so it can be updated in the future
- Embed the image in the project (Alt-Enter on file, select Embedded)
  - Properties
  - Build Action
	  - Embedded
- Add with git
- Add an entry in the [Credits](../Credits.md)

## Converting original svg to compatible svg
- Avalonia can only import svg's in a certain format
- [Vectr](https://vectr.com/) is one of the only compatible svg editors
- Import svg into Vectr
  - Vect uses a default size of 640x640
  - Increase Height of image to 600-640 and center so there's just a slim border
  - Export, set resolution to 24x24
    - Avalonia can't resize svg yet?
- Make sure the color in the `.svg` is also set to solid black so it gets updated with the theme
  - `rgb(0,0,0)`