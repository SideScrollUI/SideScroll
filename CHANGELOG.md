# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Fixed

### Changed

## [0.23] - 2026-08-03

### Added
- Added `[Hide]` support on `[Item]` methods, so a method row can be hidden based on its return value (e.g. `[Item, Hide(null)]`). A class-level `[Hide]` now applies to its `[Item]` methods as well. Methods without a `[Hide]` are never invoked to evaluate visibility
- Added `SerializerFileAtlas.CreateForFile()` for serializing to a specific file path instead of a directory base path
- Added `SideScrollExtensions.MaxInnerValueDepth` (16) to limit how far `[InnerValue]` members are unwrapped
- Added a `Header.json` file to JSON repositories so an item's saved name is preserved across loads, instead of being reconstructed from its contents

### Fixed
- Fixed tab preloading evaluating one row past `MaxPreloadItems`, since the count was tested after the property getters ran. A maximum of zero also preloaded a row instead of disabling preloading
- Fixed `ItemCollection.AddRange()` raising only the collection change, so a binding to `Count` never updated. It now raises the `Count` and `Item[]` property changes that `ObservableCollection` pairs with every change
- Fixed DataGrid exports throwing an `InvalidCastException` for a column whose `Header` isn't a string, since the property is typed `object`
- Fixed a negative `ListByte.MaxBytes` throwing while allocating the read buffer in `Load()`, and returning nothing from `Create()`
- Fixed `ProcessUtils.OpenFolder()` throwing when the path is removed between its existence check and reading its attributes, which happened outside the `try` that swallows every other failure there
- Fixed `BookmarkNavigator` throwing from `RemoveRange()` on the next navigation when `MaxHistorySize` was zero or negative, since it asked to remove more entries than the history held
- Fixed a `Filter.MaxSearchTextValues` below one making every search report no matches, by returning before collecting even the row's own label
- Fixed bookmark addresses ending in `" / "` for a selected row with no child selection, the separator was written whether or not the child had an address
- Fixed `ResourceView.ReadText()` leaving its `StreamReader` and the manifest resource stream undisposed
- Fixed `SvgUtils.GetSvgColorImage()` closing the caller's stream, so reusing it afterwards threw an `ObjectDisposedException`. `IsSvg()` in the same class already left it open
- Fixed `TabLiveChart.MaxSeparators` throwing from `Math.Clamp()` during every Y axis update when set to zero or less
- Fixed "Copy Cell Contents" and "Copy Cell Value" throwing a `NullReferenceException` for a null cell value. Both run from `async void` handlers, so it terminated the process instead of copying an empty value
- Fixed `ListField.IsEditable` always returning true, so forms and grids offered an editor for `readonly` and `const` fields that reflection then refused to assign
- Fixed `TimeSpan.Ceil()` wrapping into a large negative duration for values in the final rounding interval before `TimeSpan.MaxValue`. It saturates at `MaxValue` now, matching `DateTime.Ceil()`
- Fixed `HttpMemoryCache` accepting a zero or negative `MaxItems`, which rejected every entry so nothing was cached and every lookup silently refetched
- Fixed `ImageUtils.LoadImage()` leaking the decoded `Bitmap` and its native memory when rejecting an image above `MaxImageSize`
- Fixed chart date axis labels starting with a space for any window under a day. `DateTimeFormat.Format()` always wrote the separator before the time, and the sub-day formats have no date part in front of it
- Fixed `TimeRangePeriod.Periods()` casting its period count to an `int`. A wide window with small periods wrapped, either going negative and silently returning no periods, or staying positive and allocating hundreds of millions of them. Counts past the new `MaxPeriods` (1,000,000) return null like the other invalid parameters
- Fixed `ProcessUtils.OpenBrowser()` passing the url into `cmd /c start` on Windows, where only `&` was escaped, so a url containing `|`, `>`, or `^` ran whatever followed it. It uses `UseShellExecute` now, which opens the default browser directly and needs no escaping
- Fixed posted `ItemCollectionUI` removals using the index captured when the removal was requested, which pointed at a different item, or past the end, by the time the callback ran on the UI thread. The item itself is passed now and its current index is looked up
- Fixed posted `ItemCollectionUI` bulk operations passing the caller's enumerable through to the callback, which ran after the caller could have mutated or disposed it. The input is snapshotted first
- Fixed `ItemCollection.AddRange()` mutating the collection while enumerating its input, so adding a collection to itself or passing a deferred enumerable could leave it partially changed without the reset notification
- Fixed `ItemQueueCollection.MaxCount` not applying to items added through a base class reference or `AddRange()`, and negative values not being treated as zero
- Fixed `Log.Level` and `Entries` no longer updating once a Log reached `MaxLogItems`, which hid later errors from the Tasks that check the Log Level
- Fixed zero-retention logs staying subscribed to child logs they removed immediately, so the child kept the parent alive
- Fixed `Log.Throw()` resetting the original exception's stack trace at the rethrow
- Fixed `LogSettings.Clone()` not copying the `Context`, so Logs added entries on the calling thread instead of the UI thread after calling `Call.DebugLogAll()` or `Log.SetLogLevel()`, and `LogEntry` now raises property changes directly when no context is configured
- Fixed `TimeZoneView.GetHashCode()` returning a reference-based hash while `Equals()` compared display names, breaking the equality contract. Both now use the time zone ID, so equal instances agree
- Fixed `TimeZoneView` sorting its values in reverse order, which reversed the order time zones are listed in
- Fixed `TimeZoneView.ConvertTimeToUtc()` interpreting times from custom time zones as if they came from the machine's local time zone, and unconfigured instances breaking date conversion and formatting
- Fixed `ListSeries` inferring element types only from the list's generic argument, so a non-generic list now falls back to the type of its first non-null item, and points whose Y value is null are skipped instead of dereferenced
- Fixed `ListSeries.CalculateTotal()` flooring totals above 50, which reported a whole number for a fractional sum. Totals are now exact at every magnitude
- Added `SeriesType.Count` support to time period grouping, so a count series groups each period by item count rather than by average value
- Fixed `LinkUri.TryParse()` throwing instead of returning false for a malformed version like `v1..2`, `v1.2.3.4.5`, or one too large for an `int`. The pattern only checks for digits and dots, so those still reach the parser
- Fixed `LinkUri.ToUri()` adding a trailing `?` for parsed uris that had no query, and normalizing the prefix and type with the current culture
- Fixed `ReflectorUtils.FollowPropertyPath()` throwing instead of returning `null` for a path segment that doesn't exist, a malformed index (`foo[`, `foo[]`, `[0]bar`), an index that's out of range or missing from a dictionary, or an index applied to something that isn't a list or dictionary
- Fixed `TypeExtensions.GetElementTypeForAll()` resolving the element type from the first generic argument, which returned `TKey` for a `Dictionary<TKey, TValue>` instead of `KeyValuePair<TKey, TValue>`, and the wrong argument for collections whose type parameter isn't the element type (e.g. `class Cache<TKey> : HashSet<string>`). It now reads `IEnumerable<T>` first
- Fixed `TypeExtensions.GetAssemblyQualifiedShortName()` leaving fully qualified assembly names for the generic arguments inside an array (e.g. `List<int>[]`)
- Fixed visible-property discovery including properties with a non-public getter
- Fixed `FileUtils.DirectoryCopy()` recursing into its own destination when copying into a subdirectory of the source. The destination was created before the source subdirectories were listed, so it copied itself into itself until the path length limit stopped it, leaving behind a directory tree too deep for `rmdir` to remove. Copying into the source is now rejected
- Fixed a default `FilePath` exposing a null path instead of an empty one, since it's a struct and its default has no path
- Fixed `TaskInstance.SetFinished()` completing twice when called again before its posted `OnFinished()` ran, since `Finished` isn't set until that runs and couldn't guard the second call
- Fixed background task failures never being observed, so an exception thrown inside the task was dropped instead of being recorded in its log, and synchronous task failures skipping `SetFinished()` entirely and leaving the task permanently unfinished
- Fixed the first dynamically added `TaskInstance` sub-task having a progress maximum of zero, which prevented it from reporting progress to its parent, and sub-task calls referencing the parent task instead of the child, which attributed nested progress to the wrong task
- Fixed completed zero-item tasks reporting no progress instead of 100%
- Fixed root tasks not disposing the cancellation source they own, and sub-tasks disposing the source shared with their parent. A completed task's context can now create later timers without hitting a disposed cancellation source
- Fixed `TaskInstanceCollection.MaxTasks` not applying to items added through a base class reference or `AddRange()`, and negative values not being treated as zero
- Fixed `SelectedRow.Equals()` treating a missing `RowIndex` as a wildcard, which made it intransitive. A `HashSet<SelectedRow>` dropped a selected row depending on the order rows were added, and `DeepClone()` aliased two distinct rows into one instance and lost their `RowIndex` (bookmarks are deep cloned every time a link is opened). Lookups that need the wildcard now call the new `SelectedRow.Matches()`
- Fixed `SelectedRow.Equals()` comparing `DataValue` by reference while `GetHashCode()` used its value, so equal rows could disagree and a deserialized row never matched a live one
- Fixed exception logs created within the same second overwriting each other, and moved the `Debug.Fail()` out of `LogUtils.Save()` to its caller. Asserting inside the logging utility made it unusable from Debug builds that call it directly, including tests, where the test host translates the assert into a thrown exception
- Fixed `LogWriterText` writing the root log's creation time on every line instead of each entry's timestamp, and failing for a filename with no directory component
- Fixed visible-property discovery including properties with a non-public getter
- Fixed synchronous `TaskCreator.Run()` calls dereferencing a missing background task
- Fixed browser localStorage keys being built by replacing every `/`, `\`, and `:` in the path, which wasn't reversible: two different paths could collide on one key, and converting a key back to a path turned any underscore in it into a directory separator. Keys are percent encoded now. This changes every stored key, so data saved by an earlier build is not found (browser storage is experimental and this only affects `SideScroll.Serialize.Browser`)
- Fixed browser repositories not recording an item's saved name, so bulk loads had to reconstruct keys from the data instead of reading them back
- Fixed browser paging and index rebuilding going through the filesystem serializers instead of localStorage, so neither worked in the browser
- Fixed `DataRepoIndexLocalStorage.Load()` not validating the index, so entries whose data the browser had evicted still counted against `MaxItems` and a bad `NextIndex` could repeat an index
- Fixed `DataRepoIndexLocalStorage.Save()` ignoring failed localStorage writes, which silently stopped recording items once the quota was reached
- Fixed `TabAvaloniaEdit`, `TabColorPicker`, `TabFormattedComboBox`, and `TabForm` keeping themselves alive after being cleared. Each subscribes to change notifications on the object it's bound to, and the bound object outlives the control, so the subscription held the control and its whole visual subtree. They're `IDisposable` now, which the existing `TabSplitGrid` and `TabControlToolbar` cleanup already calls. Reloading a `TabForm` also releases the controls it replaces
- Fixed `ToolbarToggleButton` never releasing the `ListProperty` it binds to, which leaked the tab. The bound object holds the `ListProperty`, which held the button and its `TabInstance`, and toolbars bind to objects owned by the parent tab (like the file viewer's Favorite star), so a tab was retained for every time it was opened
- Fixed a memory leak where `ListProperty` never unsubscribed from `INotifyPropertyChanged.PropertyChanged`, preventing source objects and tab collections from being garbage-collected when tabs closed
- Fixed `TabModel.Clear()` not disposing the `ListMember` rows it created, so they stayed subscribed to their source objects after the tab closed. Only the rows the model created are disposed, and a list that throws while enumerating no longer stops `TabInstance.Dispose()`
- Fixed `TabModel` dictionaries never sorting by key, since the comparable check was made against the dictionary instead of the keys, and throwing for mixed key types
- Fixed Atlas serialization failing for every type with a custom constructor in Turkish locales. Constructor parameters were matched to their members by lowercasing both with the culture-sensitive `ToLower()`, and `tr-TR` turns a member named `Id` into `ıd` while the parameter `id` stays `id`, so they never matched. Matching is ordinal now
- Fixed serializing enums with a non `int` underlying type (`enum Status : byte`) throwing an `InvalidCastException`. They now save using their underlying type, `int` backed enums are unchanged and existing data keeps loading. Saving one of these is not backwards compatible: an older client reading the file misreads the value and the members after it, without an error, so don't start using them until every client has updated
- Fixed `DateTimeOffset` losing its offset when serialized, only the UTC instant was stored. Existing data without an offset still loads as UTC
- Fixed multi dimensional arrays (`int[,]`) deserializing as null, their dimensions are now stored and restored. Single dimension arrays are unchanged. Saving one is not backwards compatible for the same reason as the enums above
- Fixed `TypeSchema` recomputing `HasSubType` from the current type when loading instead of using the saved value. Sealing a class could misparse object references in files saved before it
- Fixed `TypeRepoEnumerable` resolving the element type from the first generic ancestor's first type argument, which deserialized nothing for collections whose type argument isn't the element type (e.g. `class Cache<TKey> : HashSet<string>`). It now reads `IEnumerable<T>` first
- Fixed `TypeRepoDictionary` throwing a `NullReferenceException` when deserializing or cloning explicitly-implemented dictionaries (like `ConcurrentDictionary`) by replacing reflection with a direct interface cast
- Fixed `TypeRepoType` aborting the rest of an object's members when a `Type`'s assembly is missing, instead of just that member
- Fixed `TypeRepoArray` and `TypeRepoArrayBytes` validating the available bytes before the reader was positioned in their data, which could reject valid arrays, and updated `TypeRepoArrayBytes` to use `ReadExactly()` since a short `Read()` is allowed and isn't an error
- Fixed `TypeRepo` throwing an `ArgumentOutOfRangeException` or exhausting memory on maliciously crafted large or negative object counts and type indexes, by bounds checking indexes read from the file and enforcing `ValidateDataSize` across the collection repos
- Fixed `Serializer.Clone()` throwing for `Uri` and `TimeZoneInfo`, and cloning `Version` as an empty `0.0`. These are immutable so the instance is now shared, and types it can't create an instance of are reported instead of throwing a `MissingMethodException`
- Fixed `SerializerMemory.ValidateBase64()` passing parameter names instead of values to `ArgumentNullException.ThrowIfNull`, so it never threw for a null string, and `SerializerMemoryAtlas.TryLoad()` not catching serialization exceptions for invalid data
- Fixed CSV exports not quoting or escaping the header row, so a column name containing a comma or a quote shifted every column after it for anything reading the file
- Fixed binned Charts drawing a line straight through gaps instead of breaking it, since only a leading gap ever added the `NaN` that breaks the line, and treating a bin whose values sum to zero as a gap. Empty bins are now tracked separately from bins summing to zero
- Fixed binned Charts misaligning bins for negative X values, where the truncating cast rounded toward zero instead of down, and adding an empty bin past the last point
- Fixed screen capture retaining its bitmaps after a selection was replaced, saved, or the capture was closed
- Fixed `DataGridExtensions.ColumnToStringTable()` and `SelectedColumnToString()` throwing a `NullReferenceException` when the DataGrid has no data (a null `ItemsSource` or `SelectedItems`)
- Fixed table formatting hanging indefinitely when the maximum column width is zero
- Fixed DataGrid Search collecting text from nested lists without any limit, which searched every item of every inner list for every row on each keystroke. Capped by the new `Filter.MaxSearchTextValues` (1,000) and a nesting limit
- Fixed DataGrid Search uppercasing with the current culture before comparing ordinally, so case insensitive search stopped working for any term containing an `i` in cultures where it doesn't uppercase to `I` (e.g. searching `ibm` didn't match `IBM` in tr-TR)
- Fixed a DataGrid Search depth prefix with too many digits (e.g. `+99999999999`) throwing an `OverflowException` while typing
- Fixed `SearchFilter` silently dropping search terms immediately preceding an open parenthesis (e.g. `Method(Param)`)
- Fixed `Filter.Matches(IList)` passing the list itself to the single item overload instead of iterating it, so it matched against the list's type name rather than its contents, and threw for arrays and non-generic list subclasses
- Fixed `SearchFilter.IsMatch()` and `FindMatches()` throwing a `NullReferenceException` for values with nothing to show in a tab (`DateTime`, `int`, `string`). Scalars now match on their own text
- Fixed `TabDataBookmark.ToDataSettings()` sharing its `ColumnNameOrder` list with the settings it returns, so dragging a column rewrote the column order stored in the bookmark it was opened from, including the ones held in the navigation history
- Fixed `Linker.AddLinkAsync()` only measuring the encoded bookmark against `MaxLength`, which allowed creating links that were too large for `GetLinkAsync()` to open
- Fixed an off-by-one error in `BookmarkNavigator.TrimHistory()` that allowed the bookmark history to exceed `MaxHistorySize` by one
- Fixed `TabInstance.IsOwnerObject()` comparing `[DataKey]` values by reference instead of by value, so parent/child loop detection now works for equal string and boxed value keys
- Fixed `TabCreatorAsync.LoadUI()` throwing a `NullReferenceException` when its underlying async creator returns null
- Fixed `LazyJsonNode.Create()` throwing an `InvalidOperationException` when wrapping a `JsonValue` that isn't backed by a `JsonElement` (e.g. a dynamically created node)
- Fixed `HeadlessTabView` treating scalar rows that aren't `IsPrimitive` (`DateTime`, `TimeSpan`, `decimal`) as navigable, so they spent the child exploration budget on tabs that always loaded empty. It now uses `TabUtils.ObjectHasLinks()`, the same rule `TabModel.AddItems()` gates on
- Fixed `HeadlessTabView` resolving item list element types without `GetElementTypeForAll()`, so arrays and non-generic list subclasses fell back to `object` and missed their element type's `[Explorable]` attribute
- Fixed `HeadlessTabOptions.TabFilter` not being applied to `ILoadAsync` rows, so a `[PrivateData]` loader was resolved and added to the public schema
- Fixed `HeadlessTabView` not flagging a list as truncated when rows were dropped by `TabFilter` or left unlisted by cancellation, which made the exported schema claim the list was complete
- Fixed public JSON serialization allowing generic collections whose concrete element types were not approved for public export, so a private type could be written into a public export through an allowed collection
- Fixed `DataRepo.CleanupCache()` deleting every item in JSON repositories regardless of age, since it always checked the Atlas data filename and a missing file reports a year 1601 timestamp. Directories missing a data file are now left alone
- Fixed `DataRepo.LoadAll()` and `LoadHeaders()` looking for Atlas files when the repository uses JSON, and losing item keys during bulk and paged loading
- Fixed `DataItemCollection(IEnumerable)` leaving its key lookup empty, since the base constructor bypassed the `Add()` that populates it
- Fixed `DataRepoIndex.Load()` not repairing `NextIndex` when it equals an existing item's index, which could assign a duplicate index on the next save
- Fixed `DataRepoIndex.Load()` keeping entries whose data no longer exists (e.g. removed by `CleanupCache()`), so they no longer count against `MaxItems` or accumulate in the index file
- Fixed `DataRepoIndex.MaxItems` accepting negative retention limits, which crashed pruning
- Fixed `DataRepoInstance` and `DataRepoView` using a null key when an item has no `[DataKey]`, which failed later with an unrelated error. They now report the missing key
- Fixed `Paths.Combine()` allowing a Windows-style leading backslash in a later segment to discard the accumulated base path
- Fixed `ProcessUtils.OpenFolder()` opening a file explorer at a default location when the path doesn't exist, selecting an unrelated file when passed a rooted selection, which `Path.Combine()` uses in place of the folder, and silently failing on Linux, which now uses `xdg-open`
- Fixed `ProcessUtils.GetDotnetRuntimes()` failing to parse runtime lists containing preview version suffixes, returning paths wrapped in display brackets, and not disposing the `dotnet --list-runtimes` process
- Fixed `DateTimeUtils.FormatTimeRange()` durations being offset when the range crosses a Daylight Saving Time transition or mixes time zones, by measuring the duration before converting for display
- Fixed Unix timestamp parsing rejecting signed values and dates beyond the `uint` seconds range
- Fixed `CompressionUtils.Compress()` logging the compressed size before the `GZipStream` finished writing, and renamed the `Decompress()` size tags, the decompressed size was labeled as the compressed size
- Fixed `GetInnerValue()` overflowing the stack when `[InnerValue]` members form a cycle. It now stops after `MaxInnerValueDepth` levels and returns the value reached
- Fixed `Extensions.Merge()` throwing a `TargetException` when merging from an object of a different type or `null`, and when the target has write-only or indexed properties
- Fixed `TimeRangeValue.FillAndMerge()` breaking a chart line inside time that was already covered, extending the `EndTime` of the values passed in so charting the same series twice kept widening its ranges, and converting inserted gaps to UTC while leaving the surrounding values unconverted
- Fixed time-range tags being consolidated by substring match, which dropped distinct non-string values and merged unrelated ones
- Fixed disposing a `LogTimer` more than once logging multiple `Finished` entries
- Fixed `MemoryTypeCache` silently caching nothing for a zero or negative `maxItems`, since a `SizeLimit` of 0 makes every entry exceed the limit. It now rejects non-positive sizes and durations, and can be disposed
- Fixed `ListMethod` throwing a `RuntimeBinderException` when invoking methods returning non-generic or internal Task types (like `async Task`) due to unsafe `dynamic` binding, by awaiting the inner task and using reflection for the result
- Fixed `ListField.Value` setter throwing an `InvalidCastException` for nullable types and null assignments, by adopting the same type conversion logic `ListProperty` uses
- Fixed `ListEnumValue.Create()` throwing an `OverflowException` for `ulong` enums with high bits set, by using `Enum.Format()` instead of `Convert.ToInt64()`
- Fixed `TabDataColumns.GetPropertyColumns()` scrambling the natural order of the remaining properties after applying `ColumnNameOrder`, due to unordered `Dictionary.Values` iteration
- Fixed `ListToString.Create()` returning `limit + 1` items, and still creating one item when passed a limit of zero or less
- Fixed `[Inline]` members expanding without a depth limit, which could overflow the stack for self referencing values
- Fixed `TabUtils.ObjectHasLinks()` throwing a `NullReferenceException` when evaluating an `IListItem` with a null value
- Fixed `CustomComparer` returning inconsistent results for mixed-type comparisons, which could make sorting a DataGrid column with mixed value types throw. Different types are now grouped together by type name
- Fixed `[DebugOnly]` field markers only being applied when both the field and its type had the attribute, rather than either of them
- Fixed `TabDirectory` Delete building its path by combining the directory with the selected row's display label, which could resolve outside the directory being viewed (`..` or an absolute path) and recursively delete it. It now uses the row's `[DataKey]` path and skips anything that isn't inside. One failed delete no longer skips the rest
- Fixed `TabFileSerialized` loading `Data.atlas` instead of the `.atlas` file that was opened in the File Viewer
- Fixed `TabUserSettings.Reset()` mutating the global `DefaultUserSettings` template by assigning a reference instead of a deep clone, so a reset permanently altered the defaults for every later reset
- Fixed `TabZipFile` showing an empty tab instead of an error for a corrupt or unreadable archive
- Fixed file type detection lowercasing extensions with the current culture, so an extension containing an `I` (like `.ZIP`) became `.zıp` and matched nothing in cultures where `I` doesn't lowercase to `i`. `TabFile.ExtensionTypes` now ignores case, and the directory file extension filter compares ordinally
- Fixed `.atlas` files only opening in the serialized viewer when the extension was lowercase, the file system is case insensitive on Windows and macOS
- Fixed `DateTime.Ceil()` rounding to seconds instead of the passed tick interval, and throwing an `ArgumentOutOfRangeException` for values in the last interval before `DateTime.MaxValue`, which has nothing above it to round up to. It saturates at `MaxValue` now
- Fixed `DateTime.Max()` and `Min()` comparing `Ticks`, which are wall clock readings that aren't comparable across `DateTimeKind`s, and labeling the result with the first value's `Kind` regardless of which one won. Charts combining a UTC series with a Local one got a time window shifted by the UTC offset
- Fixed `ByteFormatter.Format()` recursing until the process died with a `StackOverflowException` for `long.MinValue`, which negates back onto itself, and ignoring the passed `decimalPlaces` for negative values
- Fixed `ObjectUtils.AreEqual()` throwing for values that can't be converted to each other's type (an Enum against a string, two different Enums, a `Guid` against a string). It's used to evaluate `[Hide]`, `[HideRow]`, and `[HideColumn]`, so an unconvertible pair broke rendering instead of comparing unequal
- Fixed `TimeSpan.FormattedShort()` throwing an `OverflowException` for `TimeSpan.MinValue`, dropping the larger units for negative durations (-90 seconds showed as `-30`), and dropping a unit for durations past ~4,000 years
- Fixed `DateTimeOffset.Trim()` discarding the offset and returning the value as UTC, unlike `DateTime.Trim()` which keeps its `Kind`
- Fixed `string.CamelCased()` and `DateTime.FormatId()` using the current culture, so Turkish locales turned a leading `i` into `İ` and non-Gregorian calendars produced a different identifier for the same instant
- Fixed `StringExtensions.Reverse()` corrupting surrogate pairs and combining characters, and `Range()` throwing for a negative start index or an `int.MaxValue` end index
- Removed `SerializerFile.TestWrite()`, whose purported writability test truncated existing serialized data

### Changed
- Date and time rounding helpers now reject zero and negative intervals, and significant-figure rounding rejects zero and negative precision
- `HttpClientManager` keys its pooled `HttpClient` instances on the `HttpClientConfig` record instead of formatting it to a string, and constructs them with `disposeHandler: false` so disposing any one client can't dispose the handler shared by all of them

## [0.22] - 2026-07-26

### Added
- Added DataGrid Search support for excluding terms with a leading `-` or `!` (e.g. `-foo`, `!(foo | bar)`)
- Added Atlas Serializer support for renaming types via `TypeSchema.RegisterDeprecatedType()`
- Added Toolbar Radio Button theming for the pressed circle fill and border, and the selected pressed circle border
- Added Context Menu theming for the pressed item background and foreground

### Fixed
- Updated Avalonia Headless Tab Loading to handle delays better
- Fixed `TaskInstance.ProgressMax` not updating for sub-tasks
- Fixed Light theme read-only CheckBox check color to stand out more than the border
- Fixed custom theme Radio Button label foregrounds showing the default variant color when pressed
- Fixed ListSeries Totals for time ranges without any entries
- Fixed TabFormattedComboBox null handling
- Fixed calling  `TabViewer.LoadTab()` multiple times
- Fixed `ListSeries.GetTimeWindow()` when there's no data points
- Fixed `ListSeries.GetTotal()` throwing when all Y values are null
- Fixed `SerializerMemoryAtlas.Load()` and `Validate()` closing the stream, which made a second `Load()` throw an `ObjectDisposedException`

### Changed
- Updated Headless Tab Viewer to no longer update the Current Bookmark
- Improved Atlas Serializer save performance (~20% faster, ~20% fewer allocations) by skipping per-object debug log tag allocations and primitive member boxing during the object graph walk
- Improved Atlas Serializer load performance (~20% faster, ~15% fewer allocations) by replacing the `dynamic` default-value comparison for primitive properties with `Equals()`
- Updated `TabInstance` to lazily create its default `Project`, so child tab creation no longer allocates a throwaway `Project` (~1.4 KB per tab load)

## [0.21] - 2026-06-29

### Added
- Added Headless Tab Viewer for basic tab navigation and tab schema generation
- Added `[Explorable]` attribute to override the tab schema element type allowlist per type
- Added Task Tab to allow copying the Task log to the clipboard as json
- Added Reflection Cache for Tab Attributes
- Added docs for Json Bookmark Schema
- Added Sample Program for `SideScroll.Demo.Avalonia.Headless`
- Added AvaloniaHeadlessCapture to allow Capturing specific Tabs
- Added `ItemCollectionUI` exception handling for `InsertItemCallback` and `RemoveItemCallback`
- Added `TabDateTimePicker` Button Flyouts
- Added Window Close Button Theming
- Added image rotation (Rotate Left / Rotate Right) toolbar buttons to TabFileImage, with RotateLeft/RotateRight icons
- Added `[RequiredGroup]` for TabForm to require at least one value to be filled in

### Fixed
- Fixed DataGrid not updating default selections for new items
- Fixed TabDateTimePicker Button Padding
- Fixed ShowTasks not showing the TabInstance's TaskInstance when required

### Changed
- Restructured the Settings tab into General / Themes / Data sub-tabs, moved Reset/Save into General, and split Data into Settings (saving) and Repositories (viewing/resetting)
- Updated `ObjectExtensions.Formatted()` TimeSpans to use `FormattedShort()`
- Updated Bookmark.Changed to `[PrivateData]`
- Renamed `TabModel.AddData()` to `AddItems()` and deprecated the old name
- Deprecated the `TabModel.ItemLists` setter — use `AddItems()` instead (the getter and list remain mutable)
- Updated to Avalonia 11.3.18 and LiveCharts 2.0.5

## [0.20] - 2026-06-08

### Added
- Added Atlas Serializer support for `Guid`, `Uri`, `DateOnly`, and `TimeOnly`
- Added TabInstance.CopyToClipboard(Call call...) variant to automatically show a Copied to Clipboard message
- Added `IDataViewItem` base interface for `DataViewCollection` — new view classes implement this instead of `IDataView`
- Added `IDeletableList` interface and `EnableDeleting` property to `DataViewCollection` for collection-level delete support
- Added `DataViewCollection.OnDelete` event raised after an item is successfully deleted
- Added automatic "Delete" button column to `TabDataGrid` when the list implements `IDeletableList.EnableDeleting`
- Added caching for environment Path variables

### Fixed
- Fixed Atlas Serializer Decimal Member Handling with other Object Members
- Fixed classes being marked as skippable if they have no innerValue

### Changed
- `TabModel.Actions` deprecated — use `model.AddActions([...])` instead, which adds the action buttons inline within the Objects list at the position they are declared
- Renamed TabModel.ItemList to ItemLists and deprecated the old name
- `IDataView` now extends `IDataViewItem` — existing view classes are fully backwards compatible; per-item `OnDelete` events are still wired automatically
- `DataGridButtonColumn` now supports an optional `ClickAction` delegate as an alternative to method reflection
- Renamed TabToolbar.Buttons to AdditionalButtons to clarify usage

## [0.19] - 2026-05-20

### Added
- Theming for ComboBoxForegroundPressed

### Fixed
- Fixed creating a leaf tab link from an existing link
- Fixed json link viewing exceptions blocking the link tab from showing
- Fixed TabModel.AddObject() fill not being passed for text controls

### Changed
- Updated to LiveChartsCore.SkiaSharpView.Avalonia 2.0.4
- Increased DataGridExtensions.MaxValueLength from 2,000 to 10,000
- Set FlyoutThemeMinWidth to 50

## [0.18] - 2026-05-12

### Added
- Summary Docs for SideScroll.Network and Avalonia Controls

### Fixed
- Fixed JSON serialization not handling primary constuctors with readonly properties
  - This fixes Links not saving or loading correctly in Browsers
- Fixed DataTable Clipboard copying

### Changed
- Updated to Avalonia 11.3.15
- Updated to LiveChartsCore.SkiaSharpView.Avalonia 2.0.2
- Updated to Microsoft.NET.Test.Sdk 18.5.1
- Updated to NUnit 4.5.1
- Migrated from old Avalonia.Svg.Skia to new Svg.Controls.Skia.Avalonia 11.3.9.5

## [0.17] - 2026-04-18

### Added
- Browser Local Storage support to allow saving data between sessions
- Desktop Link for File Viewer
- Summary docs for Serializer and Avalonia Controls
- TabSampleDataGridMixedHeights to test the mouse wheel scrolling problem

### Fixed
- Fixed SideScroll.Desktop.slnf
- Fixed TabInstance Bookmarking when there's duplicate keys

### Changed
- Updated DataGridPropertyTextColumn to check MaxHeightAttribute for all Properties
- Updated to Avalonia 11.3.14
- Updated to LiveCharts 2.0.0
- Renamed Sample App to SampleApp
- Renamed Sample MainView to SampleMainView
- Renamed Sample MainWindow to SampleMainWindow

## [0.16] - 2026-03-03

### Added
- Json links can now be imported
  - Added ProjectSettings.EnableJsonLinking
- File Viewer now supports viewing zip files
- Copy to Clipboard button for TabFileImage toolbar
- ScreenCapture Border, Icon, and Memory Improvements
- TextEditor Search Panel Theming
- FileTypeDetector to allow probing files to determine their type
- Toolbar Copy button to TabTextFile
- CHANGELOG.md to track version history
- CONTRIBUTING.md for contributor guidelines
- Enabled summary xml doc generation
- Summary docs for TabDataColumns, TabItemCollection, TabUtils, Linker, SerializerExtensions, AvaloniaExtensions, DataGridExtensions, Log, LogEntry, LogTimer, CallTimer, Tag, DataRepos, ToolButton, TabModel, ResourceView, TimeZoneView, Tab Toolbars, Tab Interfaces, TabInstance, Tab Bookmarks, Tab Settings, Filter, Project, and LazyJsonNode

### Fixed
- Fixed Bookmark JSON Serialization for enums
- Fixed TabViewSettings.Address with multiple paths
- Fixed NumberExtensions.RoundToSignificantFigures() for NaN and Infinity

### Changed
- Switched all remaining json serialization to System.Text.Json
  - Added JsonUtils.TryFormatUnescaped as an unescaped version of TryFormat to match previous behavior
- Updated Bookmark DataItem linking to use the passed object if it is a public data type when no DataValueAttribute is found
- Renamed Index.dat to Primary.sidx
- Renamed Log.Call() to Log.AddChild()
- Renamed IInnerTab to ITabContainer
- Renamed IReload to ITabReloadable
- Increased ChartView SeriesLimit default from 25 to 50

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
