# SideScroll
SideScroll is a cross platform Avalonia UI framework designed for quickly navigating through a tree of tabs. Every tab that shows will automatically select the next most likely tabs, and the next, until you need to start scrolling. Navigate 20 or even 50 tabs deep, and create links to save or share with others.

SideScroll is designed with speed in mind, for both development and usage. Most tabs are created in code which makes them easier to link together and refactor later. Any object can be viewed or edited by adding it to a tab, with all the controls being automatically created for you, and customized via attributes. DataRepos can be used to save and load these objects, and the selected items can be passed in links automatically.

## Features
* .NET Cross Platform [Avalonia UI](https://github.com/AvaloniaUI/Avalonia) Framework that can run on any Windows, MacOS, or Linux system
* Previous selections are used to automatically select the most likely new items
* Open multiple paths at once to easily compare or update items
* Create Links to share views with others
* Use DataRepos to manage your local saving & loading
* Load any object in a Param Control to create a form with matching Avalonia controls
* Chart line series using the [LiveCharts 2](https://github.com/beto-rodriguez/LiveCharts2) controls
* RichText document support using the [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) controls

#### VideoGamesDB
![VideoGamesDB](https://raw.githubusercontent.com/SideScrollUI/SideScroll/6b611a162f3ee741b767457f21ef08b2569fc11f/Images/Animations/SideScroll-VideoGamesDB.gif)

## Screenshots
#### Light Theme
![Light Theme](https://raw.githubusercontent.com/SideScrollUI/SideScroll/a8f4cb937e8d49db55fca4123aa92afa25e28dda/Images/Screenshots/ColumnTypes_CustomControl_Charts_Light.png)
#### Dark Theme
![Dark Theme](https://raw.githubusercontent.com/SideScrollUI/SideScroll/a8f4cb937e8d49db55fca4123aa92afa25e28dda/Images/Screenshots/ColumnTypes_CustomControl_Charts_Dark.png)
#### Custom Theming
![Custom Theme](https://raw.githubusercontent.com/SideScrollUI/SideScroll/a8f4cb937e8d49db55fca4123aa92afa25e28dda/Images/Screenshots/CustomTheme.png)
#### Rich Text Editing with AvaloniaEdit
![Rich Text Editing](https://raw.githubusercontent.com/SideScrollUI/SideScroll/9ab33ab14ebe7cfa4c8e9e8027bb1b5da96008a7/Images/Screenshots/TextEditorJsonAndXml.png)
#### Share Links
![Links](https://raw.githubusercontent.com/SideScrollUI/SideScroll/a8f4cb937e8d49db55fca4123aa92afa25e28dda/Images/Screenshots/Links.png)

![Logo](https://raw.githubusercontent.com/SideScrollUI/SideScroll/a8f4cb937e8d49db55fca4123aa92afa25e28dda/Images/Logo/png/SideScroll_40.png)

### Documentation
* [Development](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Dev/Development.md)
* [User Guide](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/UserGuide.md)
* [Credits](https://github.com/SideScrollUI/SideScroll/blob/main/Docs/Credits.md)

### Samples
* [Video Game Database](https://github.com/SideScrollUI/VideoGamesDB)
* [SideScroll Samples](https://github.com/SideScrollUI/SideScroll/blob/main/Programs/SideScroll.Start.Avalonia/MainWindow.cs)

## License
* [MIT](LICENSE)