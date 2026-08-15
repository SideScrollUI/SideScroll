# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Fixed
- Fixed every custom struct loading as its default. A value type is copied into whatever holds it, and the loader registers an object before reading its data, so the holder took a copy while every member was still unset and the values were read into the boxed original nothing referred to. A `struct Point { X = 1, Y = 2 }` reloaded as `0, 0` from a property, a field, a list, or an array, with only an `object` member working, since that holds the box. A value type is read before the copy is taken now
- Fixed a `ReadOnlyCollection` saving its contents and reloading empty. It implements the non-generic `IList`, so `TypeRepoList` claimed it, and loading failed while constructing it since it has no parameterless constructor. The new `TypeRepoReadOnlyCollection` creates it around the list it wraps and reads the elements into that, which is also what leaves the collection in place before its elements exist for one of them to reference it back
- Fixed `SortedSet`, `Queue`, `Stack`, and `LinkedList` saving their contents and reloading empty. None of them is an `IList`, an `IDictionary`, or a `HashSet`, so none had a repo of its own and each was left to `TypeRepoObject`, which reaches a type's contents through its properties, where a collection doesn't expose its elements. Nothing reported it either way. The new `TypeRepoCollection` loads them through the method each one adds with, and a `Stack` is filled in reverse since it enumerates from the top
- Fixed a deserialized dictionary keeping only its first entry when its keys hash on their own members, which comparing by an id and every `record` key does. `TypeRepoDictionary` added each key before reading it, so every key of that type hashed as an empty one and collided, and the entry that survived was stored under that hash and couldn't be found by an equal key afterwards. It preloads its keys now, the way `TypeRepoHashSet` already did
- Fixed a failure part way through loading an object being discarded without a trace, so the object came back partly filled and indistinguishable from a complete one. It's reported to the load's log now, with the type and object index. Loading still returns what it read rather than failing a file that partly works

### Changed

## [0.24] - 2026-08-10

### Added
- Added `DataItem.Refresh()`, which discards the cached `FileInfo` so `ModifiedUtc` picks up the file's current state. It stays cached by default because it renders as a grid column, where refreshing on read would put a stat syscall on the render path for every visible row
- Added `HttpUtils.DecodeString()` and `DefaultEncoding` (UTF-8) for decoding response bodies as text
- Added `IDisposable` to both `DataViewCollection` classes, so a discarded collection can stop mirroring a repository view that outlives it
- Added `TextHighlighter` and `TabAvaloniaEdit.Highlighters` for highlighting formats beyond JSON and XML, matched against the file extension or by probing the text. Registering one at startup highlights that format everywhere it's shown, including generic string and file tabs, and `TextHighlighting.Register()` and `Load()` load an AvaloniaEdit `.xshd` definition from an embedded resource
- Added `ProcessUtils.StartDotnetProcess(IReadOnlyList<string>)`, which passes each argument through `ArgumentList` so values containing spaces or quotes reach the child process intact. The `string` overload leaves every value for the caller to quote, where an unquoted path under a directory like `C:\Users\First Last` splits into two arguments before the child process sees it

### Fixed
- Fixed a reused `TabAvaloniaEdit` keeping the previous call's syntax highlighting. `SetFormatted()` left `TextType` on the format it last detected when the new text was neither JSON nor XML, and both `SelectHighlighter()` and `UpdateTheme()` read it, so showing JSON and then plain text colored the plain text as JSON
- Fixed `TabAvaloniaEdit` discarding a caller's own syntax highlighting. It was cleared for every `TextType` that isn't JSON, XML, or a registered `TextHighlighter`, from a method that also runs on every theme change, so switching between light and dark removed it. Only a highlighting the editor applied itself is cleared now
- Fixed two distinct objects that compare equal being saved and cloned as one. `TypeRepo.IdxObjectToIndex` and `Serializer.Clones` used the default comparer, so a type overriding `Equals()` to compare part of itself, which comparing by an id and every `record` does, made the second object a reference to the first and replaced whatever else differed between them. Both compare by reference now through the new `SerializerObjectComparer`, which keeps immutable reference types and all value types matched by value so repeated strings are still stored once and boxed values still match across reads
- Fixed the restrictions on what reaches public JSON applying only while writing. `[PrivateData]` members, `[Unserialized]` members, and the non-`[PublicData]` members of a `[ProtectedData]` type set `ShouldSerialize`, which `System.Text.Json` consults only when writing, so json handed back could populate every member an export deliberately omits. A `[PrivateData]` member overrides the `[PublicData]` type around it in both directions now. A member bound to a constructor parameter is assigned by the constructor, which no `JsonPropertyInfo` governs, and is still read
- Fixed a deserialized list's `Capacity` discarding the elements that followed it. `TypeRepoList` preallocates as an optimization, and both finding the property and setting it now fall back to loading without it: a `Capacity` hidden by one of a different type left `GetProperty()` unable to choose and threw before the list was read, and a setter that refused the value, whether it's fixed size or already holds more elements than were saved, abandoned every element with nothing logged
- Fixed enabling `XBinSize` on a chart series containing gaps throwing an `InvalidOperationException`. `GetDataPoints()` converts a NaN Y to null so the chart breaks the line, which is what `TimeRangeValue.FillAndMerge()` produces, and binning dereferenced it. A gap contributes nothing to its bin now, and a bin holding only gaps stays one
- Fixed HTTP requests ignoring task cancellation. `HttpCall` and `HttpUtils` never passed the call's cancel token to `SendAsync()`, `GetAsync()`, the content reads, or the delay between retries, so cancelling a task left its requests running and then worked through every remaining attempt. A cancelled call now stops at the attempt it's on
- Fixed `ViewHttpResponse` never releasing the `HttpResponseMessage` it owns. `GetBytesAsync()` deliberately transfers ownership so the headers stay readable, and nothing disposed it afterwards. It's `IDisposable` now and `GetStringAsync()` disposes it
- Fixed `HttpCachedCall` sharing one cache entry across different `Accept` headers, so a request for one representation could be served the body cached for another. Each header keys its own entry, and requests without one still key on the uri so existing caches match
- Fixed a network read failure aborting an HTTP request instead of retrying it, and the final failure discarding the cause it had already logged, which is now the inner exception
- Fixed a data grid opening on an unrelated row when its default selected item couldn't be identified. `ToUniqueString()` returns null when nothing readable identifies an object, and the null was compared against every row, so it matched the first row that was also unidentifiable. No row is selected now
- Fixed `TabDataGrid.Dispose()` leaving its tunnelled key handler attached. Every other subscription was removed there, but this one was registered through `AddHandler()` rather than `+=` and had no matching `RemoveHandler()`, so the grid kept the control reachable
- Fixed a serialized multidimensional array being created from its stored dimensions without checking them. Only the element count was validated against the data size, so a corrupt or crafted file could name dimensions whose product exhausted memory in `Array.CreateInstance()`, or build an array of a different size than the elements that follow and read past them. The dimensions must be non-negative and their product must match the stored count now, and the lengths header counts against the available bytes instead of being skipped after the check
- Fixed Atlas object sizes and type member counts being used as read. A negative object size walked the read offsets backwards so later objects read over earlier data, and a member count now has to fit in the bytes that remain rather than being looped on as given
- Fixed one item with a throwing `ToString()`, `[DataKey]`, or `[DataValue]` failing an entire list. `ListToString.Create()` builds every row through the constructor, so the exception escaped `TabModel.AddList()` and left the tab unrendered rather than affecting that row. The text shows the exception now, and an unreadable key or value leaves the item unidentified
- Fixed `ObjectExtensions.ToUniqueString()` propagating an exception from a property or field getter. `ObjectUtils.GetObjectId()` builds on it, and bookmarking and row identity build on that, so one throwing member took down rendering for the whole row instead of falling through to the next one
- Fixed `ListEnumValue.Create()` showing a zero-valued flag as selected for every value, since `HasFlag(0)` is always true, so a conventional `None = 0` row appeared selected alongside whichever flag was actually set
- Fixed `ListProperty(object, string)` null-forgiving an unknown property name, which surfaced as an `ArgumentNullException` for a parameter named `key` from inside `ReflectionCache`. It throws an `ArgumentException` naming the type and member now
- Fixed `NumberExtensions.RoundToSignificantFigures(double)` returning `NaN` for subnormal values below about `1e-308`, where the scaling factor reaches infinity and destroys the value instead of rounding it
- Fixed `SerializerFile.LoadHeader()` reading its header path without checking it exists, which threw a bare `FileNotFoundException` naming a path the caller never chose instead of a `SerializerException`
- Fixed a search filter silently dropping everything after a `)` that has no `(` open, so `500) failed` searched for `500` alone and matched more rows than were asked for, with nothing to indicate the rest was ignored. A `)` only closes a subexpression when one is open now, and is otherwise an ordinary character in the term. Grouping, negation, and an unmatched `(` are unchanged
- Fixed `AllIndexesOf()` and `AllIndexesOfYield()` rejecting a whitespace search value, so looking for a space threw. Only an empty value loops forever, and that's still rejected
- Fixed `SvgUtils.TryGetSvgImage()` and `TabFileImage` comparing the `.svg` extension through `ToLower().EndsWith()`, which the current culture treats as ignoring zero width and soft hyphen characters, so a file the OS reads as a different extension was loaded as SVG. The comparison is ordinal now, matching `FileTypeDetector` and `TabFile.ExtensionTypes`
- Fixed a DataGrid's copy and CSV exports keeping the original column order after a column was moved. `DataGrid.Columns` stays in its insertion order and `DisplayIndex` is the only record of what's on screen, and the export grouped columns into a plain `Dictionary` keyed by `DisplayIndex`, which enumerates by insertion instead
- Fixed `DataItemCollection.OrderBy()` and `OrderByDescending()` reporting an unknown member as a `NullReferenceException` thrown later from inside `OrderBy()`, naming neither the type nor the member. They throw an `ArgumentException` identifying both now. This is what `DataRepo.LoadView(..., orderByMemberName)` passes through
- Fixed `FileTypeDetector.RegisterProbe()` re-sorting its whole probe list on every registration with `List.Sort()`, which is unstable above 16 elements, so probes sharing a priority could run in a different order between builds and a different one could claim a file. Probes are inserted in priority order now, keeping their registration order within a priority
- Fixed `ListSeries` accepting a null list. The `List` property is non-nullable, but the constructors assigned one without checking and `LoadList()` returned early behind a `[MemberNotNull]` that suppressed the warnings, so readers failed somewhere unrelated. They throw `ArgumentNullException` now
- Fixed reusing a `SerializerMemory` instance still returning the object saved first. `Save()` wrote at the stream's current position while every reader rewinds to the start, so a second save appended a payload nothing could read. `Save()` and `LoadBase64String()` reset the stream now, so each one replaces what came before
- Fixed `DataRepoIndex.Save()` opening the primary index with `FileMode.Create` before writing anything, so a failure part way through truncated the last valid index and left a partial one to rebuild from headers, losing the original insertion order. It writes to a temp file and moves it into place now, matching the serializers
- Fixed a `+N` search prefix accepting any depth, so a large one recursing through a searchable object graph that references itself ran until the stack overflowed, which can't be caught. `FindMatches()` has no cycle detection and relies on the depth to terminate, so it's capped at the new `Filter.MaxDepth` (32)
- Fixed `TabModel.AddItems()` copying a generic enumerable with no bound, so displaying an infinite sequence never returned and a large generated one could exhaust memory before the tab appeared. It stops at the new `TabModel.MaxItems` (200,000), matching the cap the string list branch already applied
- Fixed a failed save destroying the previous file and reporting success. `SerializerFileAtlas` and `SerializerFileJson` opened the destination with `FileMode.Create` before serializing, so any failure truncated the existing data, and exhausting every retry returned normally with the error only logged at `Info`. They serialize to a temp file and move it into place now, retries log a warning, and the final failure reaches the caller
- Fixed `SerializerFileAtlas.SaveAttemptsMax` and `SerializerFileJson.SaveAttemptsMax` accepting zero or negative values, which skipped the save loop entirely and reported success without writing anything
- Fixed lazy loading value type properties (`DateTime`, `Guid`, `TimeSpan`, and numbers) throwing an `InvalidProgramException` when read, which terminated the process. The generated getter passed the boxed value straight to the setter instead of unboxing it
- Fixed a `LazyClass` race where a property could return an uninitialized default (`null` or `0`) to a concurrent reader, since the generated getter and setter marked the property loaded before assigning its value
- Fixed lazy loading emitting a new dynamic assembly for every load, which can never be unloaded. Generated types are now cached and reused per type and property set
- Fixed lazy loaded properties throwing a `NullReferenceException` when read after a partial load left them without a `TypeRef`. They now return their current value instead
- Fixed `HttpCache` failing to open after an interrupted write. It now keeps every complete entry and drops the partial one, truncating the orphaned bytes left in the data file
- Fixed `HttpCache` failing to open, or truncating live data, when a corrupt index entry parsed with a garbage offset. Truncation now uses the furthest entry and ignores offsets outside the data file
- Fixed `HttpCache.Entries`, `LoadableEntries`, `ContainsKey()`, and `Size` reading the index and data stream without the lock that `AddEntry()` writes under, which could throw while entries were being added. `HttpCacheManager` is now locked too
- Fixed `HttpCache` throwing when opening a cache that doesn't exist read only, since it tried to write the header
- Fixed `HttpCache.GetString()` decoding cached bytes as ASCII, which replaced every byte over 0x7F with a `?`. It now uses the new `HttpUtils.DefaultEncoding` (UTF-8) and skips a leading byte order mark, and returns `null` instead of throwing when the entry's bytes can't be retrieved
- Fixed `Call.RunAsync()` not observing cancellation while waiting for a rate limiter slot. The cancel token wasn't passed into the wait, so work that never finished held every slot and the cancellation was never noticed
- Fixed `Call.RunAsync()` returning empty placeholder results for items that never ran after cancelling, which callers couldn't tell apart from real results
- Fixed `ConcurrentRateLimiter` discarding fractional refill tokens each cycle, which made the effective rate drift below the configured requests per second, and allowing bursts above that rate after an idle period
- Fixed `ConcurrentRateLimiter` stranding a concurrency slot when cancelled while waiting for a rate token, and disposing while waiters are in flight breaking active lease cleanup. `WaitAsync()` now throws `ObjectDisposedException` once the limiter is disposed
- Fixed `CallTimer.Stop()` logging the duration and finishing the task again when called before `Dispose()`, which also calls it
- Fixed non-ASCII HTTP response text being decoded as ASCII by `HttpUtils`, `HttpCall`, `HttpCachedCall`, and `ViewHttpResponse`, replacing every byte above `0x7F` with `?`
- Fixed HTTP retry request messages not being disposed after each attempt
- Fixed non-positive HTTP attempt counts silently disabling requests, negative retry delays failing during a retry, and non-positive streaming buffer sizes truncating downloads or throwing during allocation
- Fixed `HttpCall`'s retry delay wrapping negative for a large `SleepMilliseconds`, where `Task.Delay(-1)` waits forever. The delay is calculated as a `long` and clamped now
- Fixed `HttpUtils.GetBytesAsync()` abandoning the `HttpResponseMessage` when reading its content failed, so every retry after a successful request leaked one
- Fixed the first HTTP retry waiting four times the configured `BaseRetryDelay` instead of the delay itself, so the backoff now starts at the base delay and doubles from there as documented
- Fixed `DataPageView` accepting zero and negative page sizes, which caused division-by-zero errors or broken pagination
- Fixed `DataPageView.PageSize` changes not raising `PropertyChanged`, so bound views kept showing the old page size, page count, and next page state
- Fixed `DataPageView.PageSize` increases leaving `PageIndex` past the last page, which showed an empty page with no next page to advance to. The index is clamped to the last page now
- Fixed `ChartView.AddDimensions()` turning a missing dimension property into a delayed `NullReferenceException`; it now reports the missing property immediately
- Fixed one corrupt repository item or header aborting the entire `DataRepo` scan and preventing the remaining valid ones from loading
- Fixed deleting through a `DataViewCollection` deleting the repository item twice and raising `OnDelete` twice. Removing the item from the repository raises a change notification, and handling it called back into the same delete that was being reported
- Fixed `DataRepoView` loads and sorts replacing the public `Items` collection instance, which left every subscriber attached to the abandoned one. A `DataViewCollection` built from a view stopped showing saves and deletes after any reload
- Fixed a repository reset leaving a `DataViewCollection`'s lookups holding view items it no longer displays, so a later delete could resolve one of them. It rebuilds from the repository's current contents now
- Fixed a corrupt archive destroying the data it was meant to replace. `CompressionUtils.Decompress()` deleted the existing directory before extracting a zip, and truncated the existing file before decompressing a gzip, so a read that failed part way through left nothing behind. Both extract to a temp copy that's swapped in only once it succeeds now, and `Compress()` writes its archive the same way
- Fixed a memory leak where `LiveChartSeries` subscribed to its source list's `CollectionChanged` with an anonymous handler it had no way to remove. The list outlives the chart, so every reload leaked the chart it replaced and each dead one kept refreshing on every data update. It's `IDisposable` now and `ClearSeries()` disposes each series
- Fixed `TabForm.Dispose()` releasing the generated property controls but leaving the `ObjectChanged` and `OnFocus` subscriptions on the `TabFormObject`, which lives in the `TabModel` and outlives the control, so `Update()` also reloaded every form it had already replaced
- Fixed `BaseWindow` starting a cache cleanup `DispatcherTimer` and never stopping it. A running timer is rooted by the dispatcher and holds its `Tick` handler, so closing a window leaked the window, its `TabViewer`, and the whole `Project`. The new `OnClosed()` override stops it and disposes the viewer
- Fixed `TabViewer` retaining every tab column ever opened. It tracks the `TabView`s it's waiting on to complete `ChildrenLoadedAsync`, adding each new child as the user navigates, but only dropped them on the next `LoadTab()`. They're untracked through the new `TabView.OnDisposed` now, and one disposed before it finished loading no longer leaves the load waiting on it forever
- Fixed `TabViewer.LoadTab()` replacing the root `TabView` without disposing it, leaking the previous `TabInstance` tree along with its running tasks and every subscription its controls made. This is what `AvaloniaHeadlessCapture.RenderTab()` does per capture
- Fixed the static `TabViewer.Instance` pinning the last viewer for the life of the process. `Dispose()` clears it, but only when it still points at the viewer being disposed, so closing one window doesn't clear another's
- Fixed `HttpClientManager` caching a client per distinct configuration with no limit. `HttpClientConfig.Timeout` is part of the key and comes from the caller, so a computed one added a client permanently. Configurations past the new `MaxClients` (32) still get a working client, they just aren't cached, since evicting one would hand a second client to callers still using the first
- Fixed `TabDataGrid.Dispose()` leaving `DataGrid.Sorting` subscribed, so a sort could still queue a column resize on a disposed grid
- Fixed `TabImageButton` disposing recolored icons that `SvgUtils` caches and shares with every other button using the same resource and color. Only the images a button creates for itself are disposed now, and `SetImage()` releases the one it replaces instead of dropping it, along with the resource stream the non-SVG path never closed
- Fixed `TabImageButton.SetImage()` leaving a non-SVG button showing its previous icon. `UpdateImage()` returned early for anything that wasn't a recolored SVG, so the new resource was never decoded
- Fixed `DataRepoViewCollection` holding every group it loads for the life of the collection, each with all of that group's items. Views are held weakly now, so a group is released once nothing references it. They can't be evicted while in use without a later `Load()` returning a second view mirroring the same repository into its own `Items`
- Fixed `TabProcessMonitor` sampling forever after its tab went away. `Load()` starts a `Timer` that only the Stop button and a reload disposed, and the runtime roots a running timer whose callback holds the tab instance, so navigating away left it sampling into a tab nothing displays. It's stopped when the instance is disposed now, along with the `Process` handle that was never released and leaked another on every reload
- Fixed closing a project leaving it held by `ThemeManager.Instance`, `LinkManager.Instance`, and the static `UserSettings.Themes`, so clearing `TabViewer.Instance` alone didn't release it. `ThemeManager.Release()` and `LinkManager.Release()` clear them when the singleton still belongs to the project being torn down, and `TabViewer.Dispose()` calls both
- Fixed `TabViewer.ClearContent()` dropping the overlay control without disposing it. The toolbar stays visible over an overlay, so taking a second snapshot before closing the first replaced it through `SetContent()` and left its viewport sized bitmaps to the finalizer. It disposes the control it removes now, and `TabViewer.Dispose()` clears any overlay and disposes its toolbars
- Fixed `TabViewerToolbar` polling `VisualRoot` every 50ms in an `async void` loop with no timeout while it waited to be attached to a window, which never ended for a toolbar that never attached and held the toolbar for the life of the process. It waits for `AttachedToVisualTree` instead, and the window state subscription is released when the toolbar is disposed
- Fixed `TaskInstance` never being disposed, so the `CancellationTokenSource` each root task owns was left to the collector, and one given a deadline through `CancelAfter()` kept a runtime timer for the full duration after its work had finished. `TabInstance.Dispose()` disposes its tasks now, and a task still running releases the source when it completes rather than out from under work that would then throw from `Token` or `Register()`. Sub-tasks are disposed with the root, so cancelling one afterwards no longer throws on the shared source
- Fixed `ProcessUtils.OpenBrowser()` and `OpenFolder()` discarding the `Process` returned by every `Process.Start()` call, leaving a handle to each launched process for the finalizer to release
- Fixed `TabAvaloniaEdit.SetFormatted()` assigning `TextType` after `Text`, so the JSON and XML highlighting wasn't applied along with the text and had to wait for the theme change that fires when the editor is attached. A failure while formatting also left the previous type applied to the plain text it fell back to
- Fixed `TabAvaloniaEdit` keeping the syntax highlighting from its previous contents once the text stopped matching it, since nothing cleared `SyntaxHighlighting` when the type fell back to `Default`
- Fixed negative and overflowing repository page indexes returning the first or another incorrect page; invalid negative requests are rejected and oversized offsets now return an empty page without overflowing
- Fixed recursive directory deletion accepting filesystem roots, and paths that aren't fully qualified, from corrupt or crafted repository settings. `"C:"` resolves to the working directory and a leading separator rebases onto the current drive, so both read as absolute without being so. A directory outside SideScroll that is otherwise valid is still deleted
- Fixed Atlas stream corruption when skipping an object reference containing a derived primitive value
- Fixed lazy-property detection throwing for writeable properties without a public getter
- Fixed the theme creator registering its `HasName` styled property for the wrong Avalonia owner type
- Fixed theme creation leaving the application on a temporary theme variant when resource loading failed
- Fixed corrected form values retaining stale validation errors
- Fixed one unavailable drive preventing the Drives tab from loading any drives
- Fixed a corrupt repository index count being accepted as an empty index or driving an excessive read loop. The index is derived from the data directories, so an unreadable one is now rebuilt from the headers rather than failing the load, matching what already happened when the file was missing. A truncated index recovers the same way
- Fixed aggregate exception log entries retaining the outer exception instead of their corresponding inner exception
- Fixed async tab-load failures not contributing to the task's logged failure state
- Fixed theme JSON containing `null` causing a `NullReferenceException` instead of a clear invalid-theme error
- Fixed compression utilities treating uppercase `.GZ` and `.ZIP` extensions differently from their lowercase forms
- Fixed numeric values bypassing `Formatted()` validation for a negative maximum length
- Fixed `Tag.MaxValueLength`, `ObjectExtensions.DefaultMaxFormattedLength`, and `DataGridExtensions.MaxValueLength` accepting negative values, which reached `Formatted()` while rendering a tag, formatting any value, or exporting a grid, and threw there instead of at the assignment. Zero is still allowed and truncates to empty
- Fixed failed text probing leaving a seekable stream at a changed position
- Fixed non-recursive directory copying accepting identical source and destination paths
- Fixed `ShallowClone()` invoking indexer properties without their required arguments
- Fixed short decimal formatting appending a tera suffix to positive and negative infinity
- Fixed SVG detection consuming or rewinding caller-owned streams
- Fixed XML formatting dropping the document declaration
- Fixed `Serializer.SaveObjects` allocating massive `byte[]` arrays on the Large Object Heap for large object graphs by streaming directly to the underlying `BinaryWriter` stream instead of using `MemoryStream.ToArray()`
- Fixed clicking the screen capture overlay without dragging recopying the previous selection rectangle by clearing it on pointer press
- Fixed `ScreenCapture.MinClipboardSize` allowing zero or negative values, which caused a `PixelSize` crash on empty selections
- Fixed a `[Watermark]` member name that can't be read aborting the control it decorates. A mistyped name threw, and so did a getter that failed, taking down the form rather than the one hint. A name that resolves to nothing falls back to the attribute's own text now, or names the missing member when there's no text to fall back on
- Fixed a `[Watermark]` naming a field that a subclass redeclares with `new` being reported as ambiguous. `GetMember()` returns both declarations for a field, though not for a property, and the derived one is what the compiler resolves to

### Changed
- `TypeExtensions.GetPropertiesWithAttribute()` and `GetFieldsWithAttribute()` cache their results per type and attribute instead of re-running the search on every call, and return `IReadOnlyList<T>` rather than `List<T>` now that the result is shared. `ObjectUtils.GetDataKey()` and `GetDataValue()` call them per row, where the repeated reflection was most of the cost of building a list: 200,000 rows through `ListToString.Create()` went from ~250 ms and 183 MB to ~55 ms and 43 MB

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
