using Atlas.Core;
using Atlas.Extensions;
using Atlas.GUI.Avalonia.Tabs;
using Atlas.Tabs;
using Avalonia.Controls;
using System;
using System.Collections;
using System.Reflection;

namespace Atlas.GUI.Avalonia.View
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
				label = obj.ObjectToString();
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
			if (parentTabInstance is ITabCreator)
			{
				value = ((ITabCreator)parentTabInstance).CreateControl(value, out labelOverride);
			}
			else if (tabControl is ITabCreator)
			{
				value = ((ITabCreator)tabControl).CreateControl(value, out labelOverride);
			}
			if (value == null)
				return null;
			if (labelOverride != null)
				label = labelOverride;
			Type type = value.GetType();


			if (value is string || value is Decimal || type.IsPrimitive)
			{
				value = new TabText(value.ToString()); // create an ITab
			}
			/*else if (value is Uri)
			{
				TabWebBrowser tabWebBrowser = new TabWebBrowser((Uri)value);
				value = tabWebBrowser;
			}*/

			if (value is ITab)
			{
				// Custom controls implement ITab
				ITab iTab = (ITab)value;
				TabInstance childTabInstance = parentTabInstance.CreateChildTab(iTab);
				childTabInstance.Reintialize(); // todo: fix, called in TabView
				childTabInstance.tabModel.Name = label;
				if (childTabInstance.tabModel.Object is TabContainer)
				{
					TabContainer tabContainer = (TabContainer)childTabInstance.tabModel.Object;
					tabContainer.Label = label;
					//tabContainer.Load();
					return tabContainer;
				}
				TabView tabView = new TabView(childTabInstance);
				//tabView.Label = label;
				tabView.Load();
				return tabView;
			}
			else if (value is TabContainer)
			{
				TabContainer tabContainer = (TabContainer)value;
				tabContainer.tabInstance.ParentTabInstance = parentTabInstance;
				tabContainer.tabInstance.tabBookmark = tabBookmark;
				tabContainer.Label = label;
				tabContainer.Load();
				return tabContainer;
			}
			else if (value is TabView)
			{
				TabView tabView = (TabView)value;
				tabView.tabInstance.ParentTabInstance = parentTabInstance;
				tabView.tabInstance.tabBookmark = tabBookmark;
				tabView.Label = label;
				tabView.Load();
				return tabView;
			}
			else if (value is Control)
			{
				Control control = (Control)value;
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
				if (value is TabModel)
				{
					childTabModel = (TabModel)value;
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
				TabView tabView = new TabView(childTabInstance);
				tabView.Load();
				return tabView;
			}
		}
	}
}
