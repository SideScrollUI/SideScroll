using Atlas.Core;
using Atlas.Extensions;
using Atlas.GUI.Avalonia.Controls;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Atlas.GUI.Avalonia
{
	public class TabContainer : Panel//, IDisposable
	{
		private TabViewSettings TabViewSettings
		{
			get
			{
				return tabInstance.tabViewSettings;
			}
			set
			{
				tabInstance.tabViewSettings = value;
			}
		}
		private TabViewSettings TabSettings { get; set; } = new TabViewSettings();
		public TabInstance tabInstance;
		public TabModel tabModel;
		public string Label
		{
			get
			{
				return tabModel.Name;
			}
			set
			{
				tabModel.Name = value;
				tabTitle.Text = value;
			}
		}

		// Layout Controls
		public TabControlSplitContainer splitControls;
		private TabControlTitle tabTitle;

		//private bool allowAutoScrolling = false; // stop new controls from triggering the ScrollView automatically
		protected virtual bool focusTab { get; set; } = true; // use TabViewSettings or TabInstance instead?

		public override string ToString() => tabModel.Name;


		private TabContainer()
		{
			this.tabInstance = new TabInstance();
			Initialize();
		}

		public TabContainer(TabInstance tabInstance)
		{
			this.tabInstance = tabInstance;
			Initialize();
		}

		public void Initialize()
		{
			tabModel = tabInstance.tabModel;

			Load();
			//InitializeControls();
			//AddListeners();

			// Have return ListCollection?
			//if (tabInstance.CanLoad) // 
			//	tabModel.ItemList.Clear();
			//tabInstance.Load(); // Creates a new ListCollection and initializes class

			//ReloadControls();
		}

		private void AddListeners()
		{
			//tabInstance.OnReload += TabInstance_OnReload;
			tabInstance.OnLoadBookmark += TabInstance_OnLoadBookmark;
			//tabInstance.OnClearSelection += TabInstance_OnClearSelection;
			//tabInstance.OnSelectItem += TabInstance_OnSelectItem;
		}

		// Gets called multiple times when re-initializing
		private void InitializeControls()
		{
			Background = new SolidColorBrush(Theme.BackgroundColor); // doesn't do anything
			//Background = new SolidColorBrush(Colors.Blue); // doesn't do anything
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;
			if (focusTab)
			{
				Focusable = true;
				GotFocus += TabContainer_GotFocus;
				LostFocus += TabContainer_LostFocus;
			}
			AddListeners();
			
			// not filling the height vertically? splitter inside isn't
			splitControls = new TabControlSplitContainer()
			{
				ColumnDefinitions = new ColumnDefinitions("*"),
				//RowDefinitions = new RowDefinitions("Auto,*"), // Header, Body
				//RowDefinitions = new RowDefinitions("Auto"), // Header, Body
				//Background = new SolidColorBrush(Theme.BackgroundColor),
			};
			// StackPanel doesn't translate layouts, and we want splitters if we want multiple children?
			tabTitle = new TabControlTitle(tabInstance, tabModel.Name);

			//splitControls.Children.Add(tabTitle);
			splitControls.AddControl(tabTitle, false, SeparatorType.Spacer);

			//AddContextMenu();

			Children.Add(splitControls);
		}

		private Control content;
		public Control Content
		{
			set
			{
				if (content != null)
					splitControls.Children.Remove(content);

				// Add support for multiple, or use TabView for everything?
				content = value;
				//Grid.SetRow(content, 1);
				//splitControls.Children.Add(content);
				splitControls.AddControl(content, true, SeparatorType.Spacer);
			}
		}

		private void TabContainer_GotFocus(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Background = new SolidColorBrush(Theme.BackgroundFocusedColor);
		}

		private void TabContainer_LostFocus(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Background = new SolidColorBrush(Theme.BackgroundColor);
		}

		public void ReloadControls()
		{
			ClearControls();

			InitializeControls();
			//AddBookmarks();
		}


		public void Load()
		{
			LoadSettings();

			ReloadControls();
			//Dispatcher.BeginInvoke((Action)(() => { allowAutoScrolling = true; }));
		}

		public void LoadSettings()
		{
			//allowAutoScrolling = false;

			if (tabInstance.project.userSettings.AutoLoad)
				LoadDefaultTabSettings();
		}

		private void LoadDefaultTabSettings()
		{
			TabSettings = tabInstance.LoadDefaultTabSettings();
		}

		private void ClearControls()
		{			
			//LogicalChildren.Clear();
			//Content = null;
			Children.Clear();
		}

		private void TabInstance_OnLoadBookmark(object sender, EventArgs e)
		{
			LoadBookmark();
		}

		private void LoadBookmark()
		{
			TabBookmark bookmarkNode = tabInstance.tabBookmark;
			tabInstance.tabViewSettings = bookmarkNode.tabViewSettings;

			//LoadTabSettings();
		}
	}
}

/*
replace this with the TabView?
*/