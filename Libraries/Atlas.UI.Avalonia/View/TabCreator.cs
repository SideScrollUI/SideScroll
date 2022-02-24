using Atlas.Core;
using Atlas.Extensions;
using Atlas.UI.Avalonia.Tabs;
using Atlas.Tabs;
using Avalonia.Controls;
using System;
using System.Threading.Tasks;

namespace Atlas.UI.Avalonia.View;

public static class TabCreator
{
	public static Control CreateChildControl(TabInstance parentTabInstance, object obj, string label = null, ITabSelector tabControl = null)
	{
		object value = obj.GetInnerValue();
		if (value == null || value is bool)
			return null;

		if (label == null)
		{
			// use object? or inner value?
			label = obj.Formatted();
			if (label.IsNullOrEmpty())
				label = "(" + obj.GetType().Name + ")";
		}

		TabBookmark tabBookmark = null; // Also assigned to child TabView's, tabView.tabInstance.tabBookmark = tabBookmark;
		if (parentTabInstance.TabBookmark != null && parentTabInstance.TabBookmark.ChildBookmarks != null)
		{
			if (parentTabInstance.TabBookmark.ChildBookmarks.TryGetValue(label, out tabBookmark))
			{
				// FindMatches only
				if (tabBookmark.TabModel != null)
					value = tabBookmark.TabModel;
			}
		}

		string labelOverride = null;
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
		if (value == null)
			return null;

		if (labelOverride != null)
			label = labelOverride;

		Type type = value.GetType();
		if (value is string || value is decimal || type.IsPrimitive)
		{
			value = new TabText(value.ToString()); // create an ITab
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
			TabInstance childTabInstance = parentTabInstance.CreateChildTab(iTab);
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
			if (value is Enum && parentTabInstance.Model.Object.GetType().IsEnum)
				return null;

			TabModel childTabModel;
			if (value is TabModel tabModel)
			{
				childTabModel = tabModel;
				childTabModel.Name = label;
			}
			else
			{
				childTabModel = TabModel.Create(label, value);
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
