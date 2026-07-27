# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Fixed
- Fixed `DateTime.Ceil()` rounding to seconds instead of the passed tick interval
- Fixed `TimeSpan.FormattedShort()` dropping the larger units for negative durations (e.g. -90 seconds showed as `-30` instead of `-1:30`)
- Fixed `TimeSpan.FormattedShort()` throwing an `OverflowException` for `TimeSpan.MinValue`, which `Formatted()` reaches for every `TimeSpan` shown in a row or cell
- Fixed `TimeSpan.FormattedShort()` dropping a unit for durations past ~4,000 years, since the totals were cast to `int` to test them and wrapped (`TimeSpan.MaxValue` rendered without its minutes)
- Fixed `DateTime.Ceil()` throwing an `ArgumentOutOfRangeException` for values in the final rounding interval before `DateTime.MaxValue`, which has nothing above it to round up to. It saturates at `MaxValue` now
- Fixed `LinkUri.TryParse()` throwing instead of returning false for a malformed version like `v1..2`, `v1.2.3.4.5`, or one too large for an `int`
- Fixed `ByteFormatter.Format()` recursing until the process died with a `StackOverflowException` for `long.MinValue`, which negates back onto itself
- Fixed `TimeRangeValue.FillAndMerge()` breaking a chart line inside time that was already covered. A shorter range nested inside a longer one moved the running end time backwards, so a later point inserted a `NaN` gap into the longer range's span
- Fixed `ObjectUtils.AreEqual()` throwing for values that can't be converted to each other's type (an Enum against a string, two different Enums, a `Guid` against a string). It's used to evaluate `[Hide]`, `[HideRow]`, and `[HideColumn]`, so an unconvertible pair broke rendering instead of comparing unequal
- Fixed `FileUtils.DirectoryCopy()` recursing into its own destination when copying into a subdirectory of the source. The destination was created before the source subdirectories were listed, so it copied itself into itself until the stack overflowed, leaving behind a directory tree too deep for `rmdir` to remove. Copying into the source is now rejected
- Fixed `ByteFormatter.Format()` ignoring the passed `decimalPlaces` for negative values
- Removed the unused `FileUtils.chmod()` interop declaration, which marshaled paths as UTF-16 instead of the UTF-8 that libc expects
- Removed the unused `TypeRepo.LoadObjectRef(byte[], ref int)` overload, which read the null flag and field order incorrectly
- Fixed `string.CamelCased()` throwing on an empty string
- Fixed `ConcurrentRateLimiter` discarding fractional refill tokens each cycle, which made the effective rate drift below the configured requests per second
- Fixed `CustomComparer` returning inconsistent results for mixed-type comparisons, which could make sorting a DataGrid column with mixed value types throw. Different types are now grouped together by type name
- Fixed `DataRepo.CleanupCache()` deleting all items in JSON repos regardless of age, since it always checked the Atlas data filename and missing files return a year 1601 timestamp
- Updated `DataRepoIndex.Load()` to drop entries whose data no longer exists (e.g. removed by `CleanupCache()`), so stale entries no longer count against `MaxItems` or accumulate in the index file
- Fixed `DataRepoView.LoadAllIndexed()` ignoring the `ascending` parameter when falling back to `LoadAll()` for unindexed views
- Fixed `DataRepoIndex.Load()` not repairing `NextIndex` when it equals an existing item's index, which could assign a duplicate index on the next save
- Fixed `TabInstance.IsOwnerObject()` comparing `[DataKey]` values by reference instead of by value, so parent/child loop detection now works for equal string and boxed value keys
- Fixed `TabModel` dictionaries never sorting by key, since the comparable check was made against the dictionary instead of the keys
- Fixed `GetInnerValue()` overflowing the stack when `[InnerValue]` members form a cycle. It now stops after `MaxInnerValueDepth` (16) levels and returns the value reached
- Fixed `Log.Level` and `Entries` no longer updating once a Log reached `MaxLogItems`, which hid later errors from the Tasks that check the Log Level
- Fixed `LogSettings.Clone()` not copying the `Context`, so Logs added entries on the calling thread instead of the UI thread after calling `Call.DebugLogAll()` or `Log.SetLogLevel()`
- Fixed `CompressionUtils.Compress()` logging the compressed size before the `GZipStream` finished writing
- Renamed the `CompressionUtils.Decompress()` size tags, the decompressed size was labeled as the compressed size
- Fixed lazy loading emitting a new dynamic assembly for every load, which could never be unloaded. Generated types are now cached and reused per type and property set
- Fixed lazy loaded properties throwing a `NullReferenceException` when read after a partial load left them without a `TypeRef`. They now return their current value instead
- Fixed lazy loading value type properties (`DateTime`, `Guid`, `TimeSpan`, ...) throwing an `InvalidProgramException`, the generated setter call was missing the unboxing conversion
- Fixed serializing enums with a non `int` underlying type (`enum Status : byte`) throwing an `InvalidCastException`. They now save using their underlying type, `int` backed enums are unchanged
- Fixed `DateTimeOffset` losing its offset when serialized, only the UTC instant was stored. Existing data without an offset still loads as UTC
- Fixed `Serializer.Clone()` throwing for `Uri` and `TimeZoneInfo`, and cloning `Version` as an empty `0.0`. These are immutable so the instance is now shared
- Fixed multi dimensional arrays (`int[,]`) deserializing as null, their dimensions are now stored and restored
- Fixed `TypeRepoArray` and `TypeRepoArrayBytes` validating the available bytes before the reader was positioned in their data, which could reject valid arrays
- Updated `TypeRepoArrayBytes` to use `ReadExactly()`, a short `Read()` is allowed and isn't an error
- Removed unused `LoadObject()` overrides from the `DateTime`, `DateTimeOffset`, and `TimeSpan` type repos that were copied from `TypeRepoEnum` and would have read the wrong number of bytes
- Fixed `TimeRangeValue.FillAndMerge()` extending the `EndTime` of the values passed in, so charting the same series twice kept widening its time ranges
- Fixed `TimeRangeValue.FillAndMerge()` converting inserted gaps to UTC while leaving the surrounding values unconverted, which mixed `DateTimeKind`s within a series
- Fixed HTTP requests never retrying, the retry loops caught `WebException` which `HttpClient` doesn't throw. They now catch `HttpRequestException` and timeouts, so `HttpUtils` returns null after all attempts instead of throwing on the first
- Fixed `LazyClass` IL generation race condition where multi-threaded access to a lazy-loaded property could return uninitialized defaults (like `null` or `0`) because the property was marked as loaded before its value was assigned.
- Fixed HTTP responses decoding as ASCII, which replaced every byte over 0x7F with a '?'. They now use `HttpUtils.DefaultEncoding` (UTF-8) and skip a leading byte order mark
- Fixed binned Charts drawing a line straight through gaps instead of breaking it, only a leading gap ever added the `NaN` that breaks the line
- Fixed binned Charts treating a bin whose values sum to zero as a gap, and misaligning bins for negative X values
- Fixed `HttpCall` returning error responses as content, which let `HttpCachedCall` cache a 404 or 500 permanently. Non success status codes now throw
- Fixed `HttpCachedCall` ignoring the `accept` parameter and caching every Accept header under the same key
- Fixed `HttpCache` throwing when opening a cache that doesn't exist read only
- Fixed `HttpCache` failing to open after an interrupted write. It now keeps every complete entry and drops the partial one
- Fixed the `HttpUtils` retry delay being 4x longer than intended on the first retry
- Fixed `HttpCache.Entries`, `LoadableEntries`, `ContainsKey()`, and `Size` reading without the lock that `AddEntry()` writes under, which could throw while entries were being added. `HttpCacheManager` is now locked too
- Fixed `HttpUtils.GetHeadAsync()` using its own `HttpClient` that followed redirects, unlike the client the GET requests use
- Fixed `HttpMemoryCache` throwing an `InvalidCastException` when a uri was already cached as a different type. It's now treated as a cache miss
- Fixed `HttpGetProgress.Percent` returning `NaN` when the total length is zero
- Fixed `TypeRepoType` aborting the rest of an object's members when a `Type`'s assembly is missing, instead of just that member
- Fixed `TaskInstance.SetFinished()` completing twice when called again before its posted `OnFinished()` ran
- Fixed `LinkUri.ToUri()` adding a trailing '?' for parsed uris that had no query
- Fixed `Filter` searching nested lists without a depth limit, which could overflow the stack for self referencing values
- Fixed `DataRepoInstance` and `DataRepoView` using a null key when an item has no `[DataKey]`, which failed later with an unrelated error
- Fixed `Call.RunAsync()` returning empty placeholder results for items that never ran after cancelling
- Fixed `CallTimer.Stop()` logging the duration and finishing the task again when called before `Dispose()`
- Fixed the Json and LocalStorage serializers finishing a `TaskInstance` they didn't own, before deserializing. They now report progress like the Atlas serializer does
- Fixed `Linker.AddLinkAsync()` only measuring the encoded bookmark against `MaxLength`, which allowed creating links that were too large for `GetLinkAsync()` to open
- Fixed `[Inline]` members expanding without a depth limit, which could overflow the stack for self referencing values
- Fixed `TypeSchema` recomputing `HasSubType` from the current type when loading instead of using the saved value. Sealing a class could misparse object references in files saved before it
- Fixed `SkipObjectRef()` throwing a `NullReferenceException` for a type that no longer exists
- Fixed `TypeSchema.TypeHasEmptyConstructor()` only checking public constructors, so a type with a single non public constructor that takes parameters looked constructible and then silently failed to load
- Fixed `ItemQueueCollection.MaxCount` and `TaskInstanceCollection.MaxTasks` not applying to items added through a base class reference or `AddRange()`
- Fixed `ProcessUtils.OpenFolder()` opening a file explorer at a default location when the path doesn't exist
- Updated `Serializer.Clone()` to report types it can't create an instance of instead of throwing a `MissingMethodException`
- Fixed `DataRepoIndexLocalStorage.Load()` not validating the index, so entries whose data was removed still counted against `MaxItems` and a bad `NextIndex` could repeat an index
- Fixed `DataRepoIndexLocalStorage.Save()` ignoring failed localStorage writes, which silently stopped recording items once the quota was reached
- Fixed `ObjectExtensions.ToUniqueString` throwing exceptions on write-only properties and properties with index parameters
- Fixed `ConcurrentRateLimiter` allowing bursts exceeding the configured RPS after idle periods
- Fixed `ProcessUtils.OpenFolder` throwing `FileNotFoundException` when called on non-existent paths
- Fixed `ProcessUtils.GetDotnetRuntimes` failing to parse .NET runtime lists containing preview version suffixes
- Fixed `HeadlessTabView` treating scalar rows that aren't `IsPrimitive` (`DateTime`, `TimeSpan`, `decimal`) as navigable, so they spent the child exploration budget on tabs that always loaded empty. It now uses `TabUtils.ObjectHasLinks()`, the same rule `TabModel.AddItems()` gates on
- Fixed `HeadlessTabOptions.TabFilter` not being applied to `ILoadAsync` rows, so a `[PrivateData]` loader was resolved and added to the public schema
- Fixed `HeadlessTabView` resolving item list element types without `GetElementTypeForAll()`, so arrays and non-generic list subclasses fell back to `object` and missed their element type's `[Explorable]` attribute and allowlist entry
- Fixed `HeadlessTabView` not flagging a list as truncated when rows were dropped by `TabFilter` or left unlisted by cancellation, which made the exported schema claim the list was complete
- Fixed `FileUtils.IsTextStream(Stream)` unintentionally disposing the underlying stream and failing to reset its position
- Fixed `ProcessUtils.OpenFolder` silently failing on Linux by adding `xdg-open` support
- Fixed `TypeExtensions.GetAssemblyQualifiedShortName` incorrectly leaving fully qualified assembly names for generic arguments within an array (e.g. `List<int>[]`)
- Fixed `ObjectExtensions.ToUniqueString` throwing a `StackOverflowException` when identifying object models with circular references
- Fixed `DateTimeUtils.FormatTimeRange` duration being offset when the time range crosses a Daylight Saving Time (DST) transition or when mixing time zones (e.g. UTC and Local)
- Fixed `StringExtensions.Range` throwing an `ArgumentOutOfRangeException` when provided a negative `start` index
- Fixed `NumberExtensions.RoundToSignificantFigures` returning `NaN` for extremely small doubles and throwing an `OverflowException` for decimals due to the scaling magnitude exceeding the type's limits
- Fixed `SerializerMemory.ValidateBase64` incorrectly passing parameter names instead of values to `ArgumentNullException.ThrowIfNull`, causing it to never throw on null strings
- Fixed `SerializerMemoryAtlas.TryLoad` failing to catch serialization exceptions for invalid data, causing uncaught errors instead of safely returning false
- Fixed `TypeRepoDictionary` throwing a `NullReferenceException` when deserializing or cloning explicitly-implemented dictionaries (like `ConcurrentDictionary`) by replacing reflection with a direct interface cast
- Fixed `Atlas.Serializer` crashing silently when deserializing non-generic collections like `Hashtable` and `ArrayList`
- Fixed `Atlas.Serializer` failing to resolve generic arguments for custom collections inheriting from `HashSet<T>` and `IEnumerable<T>`
- Fixed `TypeRepo` throwing `ArgumentOutOfRangeException` or exhausting memory on maliciously crafted large or negative object counts by enforcing `ValidateDataSize` checks across all collection repos
- SideScroll.Tabs: Fixed a memory leak where `ListProperty` items did not unsubscribe from `INotifyPropertyChanged.PropertyChanged` events, preventing source objects and tab collections from being garbage-collected when tabs closed.
- SideScroll.Tabs: Fixed a bug in the `SearchFilter` text parser where search terms immediately preceding an open parenthesis (e.g. `Method(Param)`) were silently dropped from the search query.
- SideScroll.Tabs: Fixed `TabUtils.ObjectHasLinks` throwing a `NullReferenceException` when evaluating an `IListItem` with a null value.
- SideScroll.Tabs: Fixed `TabCreatorAsync.LoadUI` throwing a `NullReferenceException` when its underlying async creator returns null.
- SideScroll.Tabs: Fixed `LazyJsonNode.Create` throwing an `InvalidOperationException` when trying to wrap a `JsonValue` that isn't backed by a `JsonElement` (e.g., dynamically created nodes).
- SideScroll.Tabs: Fixed `SelectedRow.GetHashCode()` violating the `Equals` contract by omitting `RowIndex`, which is treated as a wildcard when null.
- SideScroll.Tabs: Fixed an off-by-one error in `BookmarkNavigator.TrimHistory()` that allowed the bookmark history to exceed `MaxHistorySize` by one.
- SideScroll.Tabs: Fixed `TabDataColumns.GetPropertyColumns` scrambling the natural order of remaining properties after applying the user-specified `ColumnNameOrder` due to unordered `Dictionary.Values` iteration.
- SideScroll.Tabs: Fixed `ListToString.Create()` returning `limit + 1` items due to an off-by-one error.
- SideScroll.Tabs: Fixed a logical error in `ReflectionCache.ComputeFieldDisplayName` where `[DebugOnly]` markers were only applied if *both* the field and its type had the attribute, rather than either of them.
- SideScroll.Tabs: Fixed `ListField.Value` setter throwing `InvalidCastException` on nullable types or null assignments by adopting the same robust type conversion logic used in `ListProperty`.
- SideScroll.Tabs: Fixed `ListEnumValue.Create` throwing an `OverflowException` for `ulong` enums with high bits set by using `Enum.Format` instead of `Convert.ToInt64`.
- SideScroll.Tabs: Fixed `ListMethod` throwing a `RuntimeBinderException` when invoking methods returning non-generic or internal Task types (like `async Task`) due to unsafe `dynamic` binding, by correctly awaiting the inner task and using reflection for the result.
- SideScroll.Network: Fixed `HttpClientManager` using an inefficient string allocation as the dictionary key when pooling `HttpClient` instances.
- SideScroll.Network: Fixed `HttpClientManager` passing the shared `HttpClientHandler` into the default client without `disposeHandler: false`, preventing a shared handler disposal crash.
- SideScroll.Network: Fixed `HttpCall` completely failing on the first attempt when it receives a transient server error (e.g., 502, 503) instead of properly retrying, by checking `HttpUtils.IsTransient`.
- SideScroll.Network: Fixed `HttpCache.LoadIndex` failing to truncate orphaned bytes from the data stream when a crash leaves an incomplete trailing entry, resolving a long-standing `todo`.
- SideScroll.Network: Fixed `HttpCache.GetString` potentially throwing a `NullReferenceException` if the cache entry bytes could not be retrieved, now gracefully returning `null`.
- SideScroll.Tabs.Tools: Fixed `TabUserSettings.Reset` mutating the global `DefaultUserSettings` template by incorrectly performing a reference assignment instead of a deep clone.
- SideScroll.Tabs.Tools: Fixed `TabFileSerialized` incorrectly defaulting to loading `Data.atlas` when a different `.atlas` file (e.g. `Settings.atlas`) was opened in the File Viewer.
- Fixed `TabModel.Clear()` disposing every `IDisposable` in its item lists, which disposed the caller's own objects when a tab closed. Only the `ListMember` rows the model created are disposed now, and a list that throws while enumerating no longer stops `TabInstance.Dispose()`
- Fixed `HttpCache` failing to open, or truncating live data, when a corrupt index entry parsed with a garbage offset. Truncation now uses the furthest entry and ignores offsets outside the data file
- Fixed `TypeRepoEnumerable` resolving the element type from the first generic ancestor's first type argument, which deserialized nothing for collections whose type argument isn't the element type (e.g. `class Cache<TKey> : HashSet<string>`). It now reads `IEnumerable<T>` first
- Fixed `HttpCall` throwing a plain `Exception` after retrying a transient error, losing the `HttpRequestException.StatusCode` that callers use to tell a 503 apart from a network failure
- Fixed `SerializerFileAtlas.CreateForFile()` leaving an empty `BasePath` for a filename with no directory, which made saving throw before writing anything
- Fixed `ListToString.Create()` still creating one item when passed a limit of zero or less
- Fixed `SelectedRow.Equals()` treating a missing `RowIndex` as a wildcard, which made it intransitive. A `HashSet<SelectedRow>` dropped a selected row depending on the order rows were added, and `DeepClone()` aliased two distinct rows into one instance and lost their `RowIndex` (bookmarks are deep cloned every time a link is opened). Lookups that need the wildcard now call the new `SelectedRow.Matches()`
- Fixed `SelectedRow.Equals()` comparing `DataValue` by reference while `GetHashCode()` used its value, so equal rows could disagree and a deserialized row never matched a live one
- SideScroll.Tabs.Tools: Fixed `TabDirectory` Delete building its path by combining the directory with the selected row's display label, which could resolve outside the directory being viewed (`..` or an absolute path) and recursively delete it. It now uses the row's `[DataKey]` path and skips anything that isn't inside. One failed delete no longer skips the rest
- SideScroll.Tabs.Tools: Fixed `TabZipFile` showing an empty tab instead of an error for a corrupt or unreadable archive
- SideScroll.Tabs.Tools: Fixed `.atlas` files only opening in the serialized viewer when the extension was lowercase, the file system is case insensitive on Windows and macOS
- Fixed `TabDataBookmark.ToDataSettings()` sharing its `ColumnNameOrder` list with the settings it returns, so dragging a column rewrote the column order stored in the bookmark it was opened from, including the ones held in the navigation history
- Fixed `Filter.Matches(IList)` passing the list itself to the single item overload instead of iterating it, so it matched against the list's type name rather than its contents, and threw an `IndexOutOfRangeException` for arrays
- Fixed `SearchFilter.IsMatch()` and `FindMatches()` throwing a `NullReferenceException` for values with nothing to show in a tab (`DateTime`, `int`, `string`). Scalars now match on their own text
- Fixed DataGrid Search collecting text from nested lists without any limit, which searched every item of every inner list for every row on each keystroke. Capped by the new `Filter.MaxSearchTextValues` (1,000)
- Fixed a DataGrid Search depth prefix with too many digits (e.g. `+99999999999`) throwing an `OverflowException` while typing
- Fixed DataGrid Search uppercasing with the current culture before comparing ordinally, so case insensitive search stopped working for any term containing an `i` in cultures where it doesn't uppercase to `I` (e.g. searching `ibm` didn't match `IBM` in tr-TR)
- Fixed `ToolbarToggleButton` never releasing the `ListProperty` it binds to, which leaked the tab. The bound object holds the `ListProperty`, which held the button and its `TabInstance`, and toolbars bind to objects owned by the parent tab (like the file viewer's Favorite star), so a tab was retained for every time it was opened. Disposing the button now unsubscribes and disposes the binding
- SideScroll.Tabs.Tools: Fixed file type detection lowercasing extensions with the current culture, so an extension containing an `I` (like `.ZIP`) became `.zıp` and matched nothing in cultures where `I` doesn't lowercase to `i`. `TabFile.ExtensionTypes` now ignores case, and the directory file extension filter compares ordinally
- Fixed `ObjectExtensions.EnumerableToString` throwing a `NullReferenceException` when formatting collections containing `null` items.
- Fixed `Extensions.Merge(object, object)` throwing a `TargetException` when attempting to merge properties from an object of a different type or `null` by adding type validation.
- Fixed `StringExtensions.Reverse` corrupting text containing surrogate pairs or combining characters (like emojis) by using `StringInfo` to reverse grapheme clusters rather than raw 16-bit characters.
- Fixed `TypeExtensions.GetElementTypeForAll` resolving the element type from the first generic ancestor's first type argument, which returned the wrong element type for collections like `Dictionary<TKey, TValue>` (returning `TKey` instead of `KeyValuePair<TKey, TValue>`) or custom non-generic collections. It now searches for `IEnumerable<T>` first.
- Fixed `ObjectExtensions.ToUniqueString` throwing a `NullReferenceException` when evaluating a default `DictionaryEntry` struct with a `null` key.
- SideScroll.Avalonia: Fixed `DataGridExtensions.ColumnToStringTable` and `SelectedColumnToString` throwing a `NullReferenceException` when the DataGrid has no data (a null `ItemsSource` or `SelectedItems`).
- Fixed `DateTime.Max()` and `Min()` comparing `Ticks`, which are wall clock readings that aren't comparable across `DateTimeKind`s, and labeling the result with the first value's `Kind` regardless of which one won. Charts combining a UTC series with a Local one got a time window shifted by the UTC offset
- Fixed `DateTimeOffset.Trim()` discarding the offset and returning the value as UTC, unlike `DateTime.Trim()` which keeps its `Kind`
- Fixed `DateTime.FormatId()` formatting with the current culture, so the same instant produced a different identifier in cultures whose default calendar isn't Gregorian (`th-TH` is Buddhist, `ar-SA` is Hijri)
- Fixed cancelling `Call.RunAsync()` while it was waiting for a concurrency or rate-limit slot, which could leave cancellation blocked behind work that had not finished
- Fixed `ReflectorUtils.FollowPropertyPath()` throwing an `InvalidOperationException` when a property path segment does not exist instead of returning `null`
- Fixed `TimeZoneView.ConvertTimeToUtc()` interpreting times from custom time zones as if they came from the machine's local time zone
- Fixed `TimeZoneView` returning different hash codes for equal instances and sorting its values in reverse order
- Fixed `LogWriterText` writing the root log's creation time on every line instead of each entry's timestamp
- Fixed `ObjectExtensions.ToUniqueString()` rounding distinct floating-point values to the same identifier and formatting numeric identifiers differently across cultures
- Fixed the first dynamically added `TaskInstance` sub-task having a progress maximum of zero, which prevented it from reporting progress to its parent
- Fixed disposing a `LogTimer` more than once logging multiple `Finished` entries
- Fixed `Paths.Combine()` allowing a Windows-style leading backslash in a later segment to discard the accumulated base path
- HTTP retries now dispose transient responses and observe task cancellation
- HTTP caches now discard entries whose data ranges are invalid
- HTTP memory cache lookups no longer report JSON `null` as a successful result
- `HttpCall` requests and retry delays now observe task cancellation
- `ConcurrentRateLimiter` leases can now be safely disposed more than once
- Exception logs created within the same second no longer overwrite each other
- Disposing a `ConcurrentRateLimiter` now cancels pending waits without breaking active lease cleanup
- Posted `ItemCollectionUI` bulk operations now snapshot their input before returning
- Negative `TaskInstanceCollection.MaxTasks` values are now treated as zero
- Synchronous task failures now finish their `TaskInstance` and are recorded in its log
- `ItemCollection.AddRange()` now snapshots input before mutating the collection
- Negative `ItemQueueCollection.MaxCount` values are now treated as zero
- `TimeSpan.PeriodDuration()` now rejects non-positive period counts explicitly
- Count-based `ListSeries` now group time periods by item count instead of average value
- `ReflectorUtils.FollowPropertyPath()` now returns `null` for unresolved indexed paths
- `Log.Throw()` now preserves the original exception stack trace
- `LogEntry` now raises property changes directly when no synchronization context is configured
- `ListSeries.CalculateTotal()` now retains exact fractional totals above 50
- Time-range min/max totals now include point values at the window start
- Time-range tags are now deduplicated by exact value instead of substring
- Zero-retention logs no longer remain subscribed to child logs they immediately remove
- Time-range tag consolidation now retains distinct non-string values
- `ProcessUtils.GetDotnetRuntimes()` now returns usable paths without display brackets
- `TimeZoneView` identity now uses time-zone IDs instead of display names
- Unix timestamp parsing now supports signed values and dates beyond the `uint` seconds range
- `ObjectExtensions.ToUniqueString()` now handles a `DictionaryEntry` with a null key
- Memory cache wrappers can now be disposed deterministically
- Moved the `Debug.Fail()` out of `LogUtils.Save()` to its caller. Asserting inside the logging utility made it unusable from Debug builds that call it directly, including tests, where the test host translates the assert into a thrown exception
- `ViewHttpResponse` can now be disposed to release its owned HTTP response
- `LinkUri` normalization is now culture invariant
- Date and time rounding helpers now reject zero and negative intervals
- `StringExtensions.Range()` now handles `int.MaxValue` as its inclusive end index
- `HttpUtils.GetStringAsync()` now disposes its HTTP response after decoding the body
- Time-series conversion now skips points whose Y value is null
- `LogWriterText` now accepts filenames without a directory component
- Significant-figure rounding now rejects zero and negative precision
- Empty `ViewHttpResponse` instances now expose an empty body instead of throwing
- Fixed `Call.RunAsync()` not observing cancellation while waiting for a rate limiter slot. The cancel token wasn't passed into the wait, so work that never finished held every slot and kept the cancellation from being noticed
- Empty `LinkUri` query strings no longer produce an unparseable trailing question mark
- `ListSeries` now infers element types from non-generic lists
- Object formatting now rejects negative maximum lengths explicitly
- `FileUtils.IsTextStream(StreamReader)` now preserves seekable reader positions
- Memory cache wrappers now reject non-positive expiration durations during construction
- Posted `ItemCollectionUI` removals now follow the intended item when its index changes
- HTTP downloads now retry when the connection fails while reading the response body
- Visible-property discovery now excludes write-only properties
- Logs now remove every excess item after their retention limit is lowered
- Default `FilePath` values now expose an empty path instead of null
- Synchronous `TaskCreator.Run()` calls no longer dereference a missing background task
- Background task failures are now observed and recorded in their task log
- Subtask calls now reference the child task for correct nested progress attribution
- Completed zero-item tasks now report 100% progress
- Root tasks can now dispose their owned cancellation source without subtasks disposing the shared source
- Completed task contexts can now create later timers without encountering a disposed cancellation source
- `ConcurrentRateLimiter` now releases its cancellation and semaphore resources when disposed
- Unconfigured `TimeZoneView` instances no longer break date conversion or formatting
- Text-file extension detection is now case insensitive
- Removed `SerializerFile.TestWrite()`, whose purported writability test truncated existing serialized data
- Fixed public JSON serialization allowing generic collections whose concrete element types were not approved for public export
- Fixed `DataRepo.LoadAll()` and `LoadHeaders()` looking for Atlas files when the repository uses JSON
- Added JSON repository key headers and preserved index keys during bulk and paged loading
- Added validation preventing negative `DataRepoIndex.MaxItems` retention limits from crashing pruning

### Changed
- SideScroll.Serialize: Made the `name` parameter in `SerializerFile` and its subclasses (`SerializerFileAtlas`, `SerializerFileJson`) nullable, and updated the `Name` property to accurately support `null` names.
- SideScroll.Serialize: Added a static `CreateForFile` method to `SerializerFileAtlas` to simplify instantiation for specific file paths, and updated `TabFileSerialized` to use it.

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
