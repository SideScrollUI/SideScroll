# Adding new Image Resources

## Current Icons

  - [Icons](../../Libraries/SideScroll.Resources/Icons.cs)

## Finding new icon

- [Icon Icons](https://icon-icons.com/) has lots of icons available for personal and commercial use
  - Select outline after searching
  - Download svg and convert to compatible svg (see below)

## Adding a new file

- Add in the Assets or Icons folder
- Use svg files whenever possible so the image can be dynamically sized and the colors themed 
- Add a copy of the original svg to the `originals` folder
- Embed the image in the project (Alt-Enter on file, select Embedded)
  - Properties
    - Build Action
	    - Embedded
- Add the new image in the `Assets.cs` or `Icons.cs`
- Test out the new icons, and verify that the colors update when changing the theme
- Add a new entry in the [Credits](../Credits.md)
- Add files with git

## Editing svg files

- If you have to edit an svg file, [Inkscape](https://inkscape.org/) is an easy and free to use editor
- If needed, try to center the image and make sure there's a slim border
- When exporting, set the resolution to 24x24 (might not be required anymore)

### Color Theming

- To allow updating the theme colors for an `svg` file, set all colors used to one of the following so they can be updated. Note that this might require manually editing the svg. Saving a file in Inkscape can also be used to convert a svg to a more compatible format.
  - `rgb(0,0,0)`
    ```
    <path fill: rgb(0,0,0); ... />
    ```
  - `currentColor`
    ```
    <svg
      xmlns="http://www.w3.org/2000/svg"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
    >
    ```

## Animated Gif Images

- For creating animated gif images, [ScreenToGif](https://www.screentogif.com/) is a good free option
