using Atlas.Core;
using Atlas.Extensions;
using Atlas.UI.Avalonia.Tabs;
using Atlas.Tabs;
using Avalonia.Controls;
using System;
using System.Collections;

namespace Atlas.UI.Avalonia.View
{
	public class TabCreator
	{
		public static Control CreateChildControl(TabInstance parentTabInstance, object obj, string label = null, ITabSelector tabControl = null)
		{
			object value = obj.GetInnerValue();
			if (value == null)
				return null;

			if (label == null)
			{
				// use object? or inner value?
				label = obj.Formatted();
				if (label == null || label.Length == 0)
					label = "(" + obj.GetType().Name + ")";
			}

			TabBookmark tabBookmark = null; // Also assigned to child TabView's, tabView.tabInstance.tabBookmark = tabBookmark;
			if (parentTabInstance.tabBookmark != null && parentTabInstance.tabBookmark.tabChildBookmarks != null)
			{
				if (parentTabInstance.tabBookmark.tabChildBookmarks.TryGetValue(label, out tabBookmark))
				{
					// FindMatches only
					if (tabBookmark.tabModel != null)
						value = tabBookmark.tabModel;
				}
				/*foreach (Bookmark.Node node in tabInstance.tabBookmark.nodes)
				{
					tabBookmark = node;
					break;
				}*/
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
			/*else if (value is Uri)
			{
				TabWebBrowser tabWebBrowser = new TabWebBrowser((Uri)value);
				value = tabWebBrowser;
			}*/

			if (value is ILoadAsync loadAsync)
			{
				TabInstance childTabInstance = new TabInstanceLoadAsync(loadAsync);
				childTabInstance.project = parentTabInstance.project;
				childTabInstance.tabBookmark = tabBookmark;
				childTabInstance.tabModel.Name = label;
				TabView tabView = new TabView(childTabInstance);
				tabView.Load();
				return tabView;
			}

			if (value is ITab iTab)
			{
				// Custom controls implement ITab
				TabInstance childTabInstance = parentTabInstance.CreateChildTab(iTab);
				childTabInstance.tabBookmark = tabBookmark;
				//childTabInstance.Reintialize(); // todo: fix, called in TabView
				childTabInstance.tabModel.Name = label;
				TabView tabView = new TabView(childTabInstance);
				//tabView.Label = label;
				tabView.Load();
				return tabView;
			}
			else if (value is TabView tabView)
			{
				tabView.tabInstance.ParentTabInstance = parentTabInstance;
				tabView.tabInstance.tabBookmark = tabBookmark ?? tabView.tabInstance.tabBookmark;
				tabView.Label = label;
				tabView.Load();
				return tabView;
			}
			else if (value is Control control)
			{
				return control;
			}
			/*else if (value is FilePath)
			{
				FilePath filePath = (FilePath)value;
				TabAvalonEdit tabAvalonEdit = new TabAvalonEdit(name);
				tabAvalonEdit.Load(filePath.path);
				return tabAvalonEdit;
			}*/
			else
			{
				if (value is Enum && parentTabInstance.tabModel.Object.GetType().IsEnum)
					return null;

				TabModel childTabModel;
				if (value is TabModel tabModel)
				{
					childTabModel = tabModel;
					childTabModel.Name = label;
				}
				else
				{
					// skip count 1, needs to be visible to users, and not autocollapse some types
					/*if (value is IList)
					{
						IList list = (IList)value;
						if (list.Count == 1)
						{
							value = list[0];
						}
					}*/
					childTabModel = TabModel.Create(label, value);
					if (childTabModel == null)
						return null;
				}
				childTabModel.Editing = parentTabInstance.tabModel.Editing;

				TabInstance childTabInstance = parentTabInstance.CreateChild(childTabModel);
				childTabInstance.Label = label;
				TabView tabModelView = new TabView(childTabInstance);
				tabModelView.Load(true);
				return tabModelView;
			}
		}
	}
}
