# Adding new Image Resources

* Find new icon:
  - FlatIcon
    - https://www.flaticon.com/
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