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

## Serializer
- See [Serializer](Serializer.md) for Permissions
