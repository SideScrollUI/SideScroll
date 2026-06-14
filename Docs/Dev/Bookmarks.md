# Bookmarks

- Bookmarks allow exporting and importing the current path and settings in a `sidescroll://link/<v0.1.2.3>/<base64>` link
  - You can override the Linker to wrap these links in another format
  - You can use a TinyURL like service to create shorter links
- Each Tab in a bookmark exports these settings
  - Selected Rows
  - Custom Tab Data
- Bookmarks will only export classes and members that set `[PublicData]` or `[ProtectedData]`

## Bookmark Types

- Full Path
  - (Link button in top Toolbar)
  - Restores the full path
- Tab Root
  - (Can override above)
  - Add `[TabRoot]` to the ITab
  - Tabs that specify a `[TabRoot]` will create a link starting from that Tab if there's not another `[TabRoot]` present in the path
  - Default constructor required
- Tab Link
  - Click the link icon to the left of each Tab Title
  - Creates a link directly to that Tab
  - Add `[PublicData]` to the ITab
  - Default constructor required

## Saving and Restoring Bookmark Data

```csharp
private class Instance : TabInstance, ITabAsync
{
  public bool TestFlag { get; set; }

  public class TestSettings
  {
    public string Value { get; set; }
  }
  private TestSettings _settings = new TestSettings { Value = "Test"; };

  // Add Data to Bookmark
  public override void GetBookmark(TabBookmark tabBookmark)
  {
    base.GetBookmark(tabBookmark);
    tabBookmark.SetData(TestFlag); // Uses default key if none provided
    tabBookmark.SetData("Key", _settings);
  }

  // Retrieve Data from Bookmark
  public async Task LoadAsync(Call call, TabModel model)
  {
    TestFlag = GetBookmarkData<bool>();
    _settings = GetBookmarkData<TestSettings>("Key");
  }
}
```

## Saving and Restoring Selected Items

- If a List is showing a DataRepo, you can set the `TabInstance.DataRepoInstance` to that DataRepo to automatically export selected items in the bookmark
```csharp
private class Instance : TabInstance
{
  private DataRepoView<ParamTestItem> _dataRepoParams;

  private void LoadSavedItems(Call call, TabModel model)
  {
    _dataRepoParams = DataApp.LoadView<ParamTestItem>(call, "CollectionTest");
    DataRepoInstance = _dataRepoParams; // Enable links to pass selected DataItem

    var dataCollection = new DataCollection<ParamTestItem, TabParamItem>(_dataRepoParams);
    model.Items = dataCollection.Items;
  }
}
```

## JSON Schema

A bookmark can be serialized to JSON with `Bookmark.ToJson()` and parsed back with
`Bookmark.TryParseJson(...)` (both use the public serializer, so only `[PublicData]` /
`[ProtectedData]` members are written). This is the same selection tree the
[Tab Schema](TabSchema.md) uses to drive bookmark-guided traversal.

The structure is recursive: a `Bookmark` holds a root `TabBookmark`, each `TabBookmark` holds
`TabDatas`, each `TabData` holds `SelectedRows`, and each selected row holds a child `TabBookmark`
— forming the navigation path. Default/null values are omitted.

```json
{
  "Name": "Example",
  "CreatedTime": "2026-06-13T00:00:00Z",
  "TabBookmark": {
    "TabDatas": [
      {
        "SelectedRows": [
          {
            "SelectedRow": { "Label": "Samples" },
            "TabBookmark": {
              "TabDatas": [
                {
                  "SelectedRows": [
                    {
                      "SelectedRow": { "Label": "Controls" },
                      "TabBookmark": { "TabDatas": [] }
                    }
                  ]
                }
              ]
            }
          }
        ]
      }
    ]
  }
}
```

### Fields

| Object | Field | Notes |
| --- | --- | --- |
| `Bookmark` | `Name` | Optional display name. |
| | `BookmarkType` | `Default` \| `Full` \| `Leaf` \| `Tab` (string enum). Omitted when `Default`. |
| | `TabType` | Assembly-qualified short type name, when set. |
| | `CreatedTime` | UTC timestamp, optional. |
| | `TabBookmark` | The root node of the selection tree. |
| `TabBookmark` | `Tab` | Serialized `ITab` for a `[TabRoot]`/`[PublicData]` root, when present. |
| | `Width` | Tab width, optional. |
| | `TabDatas` | One entry per data grid/control on the tab (usually one). |
| | `BookmarkData` | Custom per-tab data (`SetData`/`GetBookmarkData`), keyed by name. |
| `TabData` | `SelectedRows` | The rows selected on this tab; each carries a child `TabBookmark`. |
| | `DataRepoGroupId` / `DataRepoType` | Set when the list is backed by a DataRepo. |
| | `Filter` | Search filter text, when set. |
| `SelectedRowView` | `SelectedRow` | Identifies the row (see below). |
| | `TabBookmark` | The nested bookmark for navigating into that row. |
| `SelectedRow` | `Label` | The row's display text (`ToString`). |
| | `RowIndex` | Zero-based index in the unfiltered list; disambiguates duplicate labels. |
| | `DataKey` | Stable key when the row type has a `[DataKey]`; overrides `Label` for matching. |
| | `DataValue` | The row value when it has a `[DataValue]`; imported into the DataRepo on restore. |

> A leaf node is a `TabBookmark` with an empty `TabDatas` (nothing further selected). The Tab
> Schema's bookmark-guided traversal follows these selections and stops at such leaves, matching
> rows on the full `SelectedRow` identity (`Label` + `DataKey`/`DataValue` + `RowIndex`).

Richer row example:

```json
"SelectedRow": {
  "Label": "Mercury",
  "RowIndex": 0,
  "DataKey": "Mercury",
  "DataValue": "AU=0.39"
}
```

## Serializer
- See [Serializer](Serializer.md) for Permissions
