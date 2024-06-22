using SideScroll;
using SideScroll.Utilities;
using SideScroll.Extensions;
using SideScroll.UI.Avalonia.Tabs;
using SideScroll.Tabs;
using Avalonia.Controls;
using System.Diagnostics;

namespace SideScroll.UI.Avalonia.View;

public static class TabCreator
{
	public static Control? CreateChildControl(TabInstance parentTabInstance, object obj, string? label = null, ITabSelector? tabControl = null)
	{
		object? value = obj.GetInnerValue();
		if (value == null || (value is bool && !parentTabInstance.Model.Skippable))
			return null;

		if (label == null)
		{
			// use object? or inner value?
			label = obj.Formatted();
			if (label.IsNullOrEmpty())
			{
				label = "(" + obj.GetType().Name + ")";
			}
		}

		string? labelOverride = null;
		if (value is Exception)
		{
		}
		else if (parentTabInstance is ITabCreator parentTabCreator)
		{
			value = parentTabCreator.CreateControl(value, out labelOverride);
		}
		else if (tabControl is ITabCreator tabCreator)
		{
			value = tabCreator.CreateControl(value, out labelOverride);
		}

		label = labelOverride ?? label; // update label before comparing bookmarks

		TabBookmark? tabBookmark = null; // Also assigned to child TabView's, tabView.tabInstance.tabBookmark = tabBookmark;
		if (parentTabInstance.TabBookmark is TabBookmark parentTabBookmark && parentTabBookmark.ChildBookmarks != null)
		{
			string dataKey = new SelectedRow(obj).ToString() ?? label;
			if (parentTabBookmark.ChildBookmarks.TryGetValue(dataKey, out tabBookmark))
			{
				// FindMatches only
				if (tabBookmark.TabModel != null)
					value = tabBookmark.TabModel;
			}
			else if (parentTabBookmark.Bookmark?.Imported == true && parentTabBookmark.ChildBookmarks.Count > 0)
			{
				Debug.WriteLine($"Failed to find imported tab bookmark for {dataKey} in [{parentTabBookmark.ChildBookmarks.Keys.CollectionToString()}]");
			}
		}
		if (value == null)
			return null;

		Type type = value.GetType();
		if (value is string || value is decimal || type.IsPrimitive)
		{
			value = new TabText(value.ToString()!); // create an ITab
		}
		/*else if (value is Uri uri)
		{
			var tabWebBrowser = new TabWebBrowser(uri);
			value = tabWebBrowser;
		}*/

		if (value is ILoadAsync loadAsync)
		{
			var childTabInstance = new TabInstanceLoadAsync(loadAsync)
			{
				Project = parentTabInstance.Project,
				TabBookmark = tabBookmark,
			};
			childTabInstance.Model.Name = label;
			value = new TabView(childTabInstance);
		}

		if (value is ITabCreatorAsync creatorAsync)
		{
			//value = new TabCreatorAsync(creatorAsync);
			// todo: move elsewhere, we shouldn't be blocking during creation
			value = Task.Run(() => creatorAsync.CreateAsync(new Call())).GetAwaiter().GetResult();
		}
		
		if (value is FilePath filePath)
		{
			value = new TabTextFile(filePath);
		}

		if (value is ITab iTab)
		{
			// Custom controls implement ITab
			TabInstance? childTabInstance = parentTabInstance.CreateChildTab(iTab);
			if (childTabInstance == null)
				return null;

			childTabInstance.TabBookmark ??= tabBookmark;
			//childTabInstance.Reinitialize(); // todo: fix, called in TabView
			childTabInstance.Model.Name = label;
			var tabView = new TabView(childTabInstance);
			tabView.Load();
			return tabView;
		}
		else if (value is TabView tabView)
		{
			tabView.Instance.ParentTabInstance = parentTabInstance;
			tabView.Instance.TabBookmark = tabBookmark ?? tabView.Instance.TabBookmark;
			tabView.Label = label;
			tabView.Load();
			return tabView;
		}
		else if (value is Control control)
		{
			return control;
		}
		else
		{
			if (value is Enum && parentTabInstance.Model.Object!.GetType().IsEnum)
				return null;

			TabModel? childTabModel;
			if (value is TabModel tabModel)
			{
				childTabModel = tabModel;
				childTabModel.Name = label;
			}
			else
			{
				childTabModel = TabModel.Create(label, value!);
				if (childTabModel == null)
					return null;
			}
			childTabModel.Editing = parentTabInstance.Model.Editing;

			TabInstance childTabInstance = parentTabInstance.CreateChild(childTabModel);
			childTabInstance.Label = label;

			var tabModelView = new TabView(childTabInstance);
			tabModelView.Load();
			return tabModelView;
		}
	}
}
