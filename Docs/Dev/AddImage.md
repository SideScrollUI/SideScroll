# Adding new Image Resources

- Location
  - [Icons](../../Images/Libraries/Atlas.Resources/Icons/)

* Find new icon:
  - [Icon Icons](https://icon-icons.com/)
    - Select outline after searching
    - Download svg and convert to 24x24 png (see below)
  - [FlatIcon](https://www.flaticon.com/)
    - (no longer offers svg downloads without premium membership)
    - Click color picker next to "Add to Collection"
    - Paste in color `#006df0`
    - Click Download
    - Select PNG, 24 pixels
* Colors:
  - Dark Blue: `#006df0` (currently used)
  - Light Blue: `#759eeb` (not used anymore)
  - (bad) Toolbar Buttons: `#8888FF`
  - search_right_light_16.png: `#9090a6`

## Adding a new file:
- Use 24 x 24 png files
- Add in the Assets folder
- Add a copy of a svg if possible so it can be updated in the future
- Embed the image in the project (Alt-Enter on file, select Embedded)
  - Properties
  - Build Action
	  - Embedded
- Add with git
- Add an entry in the [Credits](../Credits.md)

## Converting svg to png
- [Vectr](https://vectr.com/)
  - Make sure image fills width or height
  - Fills
    - Set color `#006df0`
  - Export (top right button)
    - Set to png
    - Set size to 24 x 24
    - Download

## Converting original svg to compatible svg
- Avalonia can only import svg's in a certain format
- [Vectr](https://vectr.com/) is one of the only compatible svg editors
- Import svg into Vectr
  - Vect uses a default size of 640x640
  - Increase Height of image to 600-640 and center so there's just a slim border
  - Export, set resolution to 24x24
    - Avalonia can't resize svg yet?