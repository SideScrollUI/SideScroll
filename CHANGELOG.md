# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Json links can now be imported
  - Added ProjectSettings.EnableJsonLinking
- File Viewer now supports viewing zip files
- Copy to Clipboard button to TabFileImage toolbar
- ScreenCapture Border, Icon, and Memory Improvements
- TextEditor Search Panel Theming
- Added FileTypeDetector to allow probing files to determine their type
- Toolbar Copy button to TabTextFile
- CHANGELOG.md to track version history
- CONTRIBUTING.md for contributor guidelines
- Summary docs for TabDataColumns, TabItemCollection, TabUtils, Linker, SerializerExtensions, AvaloniaExtensions, and DataGridExtensions

### Fixed
- Fixed Bookmark JSON Serialization for enums
- Fixed TabViewSettings.Address with multiple paths

### Changed
- Switched all remaining json serialization to System.Text.Json
  - Added JsonUtils.TryFormatUnescaped as an unescaped version of TryFormat to match previous behavior
- Updated Bookmark DataItem linking to use the passed object if it is a public data type when no DataValueAttribute is found
- Renamed Index.dat to Primary.sidx
- Disabled Debug Info for Release builds

## [0.15] - 2026-01-25

### Added
- Nested DataGrid Filter Support
  - Filters now allow nesting via parenthesis
  - Filters now treat all words as required by default unless an | operator is used
- ListEnumValue for showing enums and flags
- Serializer support for custom constructors with optional parameters that can't be serialized
- MinWidth attribute support for TabTextBox
- ToolTip attribute support for Text Controls
- Theming for Border Focus Colors
- Chart Legend Context Menu for copying the name and totals to the clipboard
- Summary docs for ListSeries, ChartView, Paths, and WordSpacer

### Changed
- Simplified Bookmark Schema to group the SelectedRow and TabBookmark together
  - This new schema should also make it easier to add Json Bookmark serialization in the future
- Improved serializer handling when changing nullability of class members
- Moved SideScroll.Avalonia.ScreenCapture project into SideScroll.Avalonia
- Updated Avalonia to 11.3.11

### Fixed
- Tab Resizing not updating MaxDesiredWidth
  - This also fixes resizing DataGrid columns and missing DataGrid values
- Custom Titlebar not responding to mouse clicks near the bottom of the title bar when maximized
- Windows left positioning when the Custom Titlebar is used
- Mouse Clicks not being detected around Image and Toolbar Button Borders
- DateTimeUtils.FormatTimeRange() for UTC Time Ranges
- TimeRangePeriod Summing when TimeRangeValues aren't aligned with the TimeWindow and period
- Copying Text in TabText for Web Browsers
- MemoryTypeCache cache duration not expiring items (only size was limiting)

## [0.14] - 2025-12-09

### Added
- Custom Title Bar to show the TabViewer Toolbar in the Title Bar
  - This currently only works for Windows and macOS (no Linux support for now)
  - This can be enabled and disabled in the Settings
  - The CustomTitleIcon can be configured in the ProjectSettings
  - Added Minimize, Maximize, Restore, and Close Button Svg Icons
- DataTable support to TabModel.AddData()
- Browser Demo Project (this is still too slow for real world usage)
- Summary Documentation for Tasks and Collections

### Changed
- Improved Default Window Sizing and Position
- ImageButtons will now resize Icons to the IconSize
- Updated IsCacheable naming for ListDelegate, ListMethod, and ListProperty
- Updated Avalonia to 11.3.8

### Fixed
- Regression for ImageButton.StartTaskAsync() not passing UseBackgroundThread
  - This could cause async Tasks to not show logs while active

## [0.13] - 2025-11-09

### Added
- More theming colors for ComboBox and ScrollBar
- AcceptsReturnAttribute.AcceptsPlainEnter to support the Enter key only
- TimeRangePeriod support for milliseconds and below
- Charting support for milliseconds
- TaskCreator.StartTask() to simplify async task creation and fix an async ClipBoard copying threading issue
- OpenFolder Button to ScreenCapture
- Summary documentation for Utilities and Time classes

### Changed
- Updated Light Theme colors
- Split TabTitleButton out of TabButton and added a new Theming tab for Title
- DataRepoIndex to retain existing order when updating items instead of moving items to the end
- Switched async UI Tasks to run in the UI thread by default, and added a background thread param to TaskDelegateAsync
- Updated DataGrid so it no longer moves focus when right clicking

### Fixed
- DataGrid Clipboard export for string values that contain null characters

## [0.12] - 2025-10-15

### Added
- Theming for TextControlBorderFocused and ComboBoxDropDownBorder
- Summary documentation for Extensions and ItemCollections

### Changed
- Updated Light Theme to use Light Blue Theme and renamed old Theme to Hybrid
  - Also updated new Light Theme colors
- Split DataGridButton out of TabButton and changed to show only on selection or pointer over
- Toolbar/Image Buttons can now show Flyouts using the TaskInstance
- Toolbar/Image Buttons now show a Flyout for any errors
- Updated Avalonia to 11.3.7

### Fixed
- Removing extra UI items when deleting from DataView layer
- Native theme loading not accounting for Brush opacity
- DataRepo Indices that use restricted characters in the GroupId

## [0.11] - 2025-09-22

### Added
- Attribute Summary Documentation
- Non-nullable DeepClone() version

### Changed
- DataViewCollection now updates items when they change
- Updated Light Blue Theme Colors
- Moved Chart SeriesLimit into ChartView
- Renamed ListProperty.Editable to IsEditable
- Renamed DeepClone() to TryDeepClone()
- Renamed [Editing] attribute to [EditColumn]
- Removed [Serialized] attribute since it's no longer needed
- Updated Avalonia to 11.3.6

### Fixed
- DataGrid Theming for cells that are both selected and pointer over
- Max object limit when loading a private file

### Deprecated
- [Unit] attribute disabled until it can be implemented

## [0.10] - 2025-09-02

### Added
- TabInstance.LoadOrCreate() Methods
- TextBox support for Shift-Enter
- Confirmation Dialogs for Settings Reset Buttons
- First validated control that fails will now be focused

### Changed
- TabModel.AddForm() to return an updatable form
- Renamed TabInstance.Invoke() to Post()
- Renamed ToolButton.Default to IsDefault
- Updated Avalonia to 11.3.4

### Fixed
- Regression for serializer failing to restore all class members when removing previous members
- Regression for invalid parsing validations not showing an error

## [0.9.10] and Earlier

Previous versions were in development. See git history for details.

---

## Version Guidelines

### Added
- New features

### Changed
- Changes in existing functionality

### Deprecated
- Soon-to-be removed features

### Removed
- Removed features

### Fixed
- Bug fixes

### Security
- Vulnerability fixes
