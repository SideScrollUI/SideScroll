using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Atlas.Resources;
using Atlas.UI.Avalonia.Tabs;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Atlas.UI.Avalonia.Controls
{
	public partial class TabControlBookmarks : UserControl
	{
		public Project project;
		public TabInstance tabInstance;
		public TabModel tabModel;

		private Grid containerGrid;
		private TabControlDataGrid tabControlDataGrid;

		private Grid gridAddBookmark;
		private TextBox textBoxName;

		public TabControlBookmarks(TabInstance tabInstance)
		{
			this.tabInstance = tabInstance;
			this.project = tabInstance.project;
			this.tabModel = tabInstance.Model;

			InitializeControls();
		}

		public override string ToString() => tabModel.Name;

		/*public void Reload()
		{
			if (tabData != null)
			{
				grid.Children.Remove(tabData);
			}
			AddListData();
		}*/

		// don't want to reload this because
		private void InitializeControls()
		{
			Background = new SolidColorBrush(Theme.BackgroundColor);
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;
			//Width = 1000;
			//Height = 1000;
			//Children.Add(border);
			//Orientation = Orientation.Vertical;

			// autogenerate columns
			//tabInstance.tabViewSettings.ChartDataSettings = tabInstance.tabViewSettings.ChartDataSettings ?? new TabDataSettings();
			tabControlDataGrid = new TabControlDataGrid(tabInstance, tabModel.Bookmarks.Items, true);//, tabInstance.tabViewSettings.);
			tabControlDataGrid.autoSelectFirst = false; // too late?
			//tabDataGrid.Initialize();
			Grid.SetRow(tabControlDataGrid, 0);

			//tabDataGrid.AddButtonColumn("<>", nameof(TaskInstance.Cancel));

			//tabDataGrid.AutoLoad = tabModel.AutoLoad;
			tabControlDataGrid.OnSelectionChanged += OnSelectedBookmarkChanged;
			//tabDataGrid.Width = 1000;
			//tabDataGrid.Height = 1000;
			//tabDataGrid.Initialize();
			//bool addSplitter = false;
			//tabParentControls.AddControl(tabDataGrid, true, false);


			/*plotView.Template = new ControlTemplate() // todo: fix
			{
				Content = new object(),
				TargetType = typeof(object),
			};*/

			// Doesn't work for Children that Stretch?

			TabControlToolbar tabToolbar = new TabControlToolbar();
			Grid.SetRow(tabToolbar, 1);
			//tabToolbar.AddButton("Add", null,
			Button buttonPin = tabToolbar.AddButton("Pin", Icons.Streams.Pin);
			buttonPin.Click += ButtonAdd_Click;
			Button buttonDelete = tabToolbar.AddButton("Delete", Icons.Streams.Delete);
			buttonDelete.Click += ButtonDelete_Click;

			//Button buttonAdd = TabButton.Create("Add");
			//buttonAdd.Click += ButtonAdd_Click;
			//Button buttonDelete = TabButton.Create("Delete");
			//buttonDelete.Click += ButtonDelete_Click;


			/*StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = Orientation.Horizontal;
			stackPanel.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch;
			//stackPanel.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
			stackPanel.Children.Add(buttonAdd);
			stackPanel.Children.Add(buttonDelete);*/

			/*Grid gridAddDeleteButtons = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("*,*"),
				RowDefinitions = new RowDefinitions("*"), // Header, Body
				HorizontalAlignment = HorizontalAlignment.Stretch,
				//VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
				[Grid.RowProperty] = 1,
				//[Grid.ColumnProperty] = 1,
			};*/
			//Grid.SetColumn(buttonDelete, 1);
			//gridAddDeleteButtons.Children.Add(buttonAdd);
			//gridAddDeleteButtons.Children.Add(buttonDelete);


			containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto"),
				RowDefinitions = new RowDefinitions("Auto,Auto,Auto"), // Header, Body
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
			};
			//containerGrid.Children.Add(borderTitle);

			containerGrid.Children.Add(tabControlDataGrid);

			// todo: add buttons
			//containerGrid.Children.Add(gridAddDeleteButtons);
			containerGrid.Children.Add(tabToolbar);

			AddNewPanel();

			//this.watch.Start();
			Content = containerGrid;

			Focusable = true;
			GotFocus += Tab_GotFocus;
			LostFocus += Tab_LostFocus;
		}

		private void AddNewPanel()
		{
			gridAddBookmark = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto,Auto"),
				RowDefinitions = new RowDefinitions("Auto,Auto,Auto"), // Header, Body
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
				[Grid.RowProperty] = 2,
			};
			//panelNew.Visibility = Visibility.Visible;
			//textBoxName.Clear();
			TextBlock textBlock = new TextBlock()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Text = "Name:",
				Foreground = new SolidColorBrush(Colors.White),
			};
			Grid.SetColumnSpan(textBlock, 2);
			gridAddBookmark.Children.Add(textBlock);


			textBoxName = new TextBox()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Text = project.Navigator.Current.Changed,
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1),
				FontSize = 14,
				MinWidth = 100,
				Padding = new Thickness(6, 2),
				//Height = 20,
				[Grid.ColumnSpanProperty] = 2,
				[Grid.RowProperty] = 1,
			};
			gridAddBookmark.Children.Add(textBoxName);


			Grid gridSaveCancelBookmark = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto,Auto"),
				RowDefinitions = new RowDefinitions("Auto,Auto"), // Header, Body
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
				[Grid.RowProperty] = 2,
			};

			Button buttonSave = new TabControlButton("Save");
			Grid.SetRow(buttonSave, 2);
			Grid.SetColumn(buttonSave, 0);
			buttonSave.Click += ButtonSave_Click;

			Button buttonCancel = new TabControlButton("Cancel");
			Grid.SetRow(buttonCancel, 2);
			Grid.SetColumn(buttonCancel, 1);
			buttonCancel.Click += ButtonCancel_Click;


			/*Grid gridSaveCancelButtons = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("*,*"),
				RowDefinitions = new RowDefinitions("*"), // Header, Body
				HorizontalAlignment = HorizontalAlignment.Stretch,
				//VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
				[Grid.RowProperty] = 1,
				//[Grid.ColumnProperty] = 1,
			};
			Grid.SetColumn(buttonCancel, 1);
			gridSaveCancelButtons.Children.Add(buttonSave);
			gridSaveCancelButtons.Children.Add(buttonCancel);*/

			gridAddBookmark.Children.Add(buttonSave);
			gridAddBookmark.Children.Add(buttonCancel);

			containerGrid.Children.Add(gridAddBookmark);

			gridAddBookmark.IsVisible = false;
		}

		private void ButtonDelete_Click(object sender, RoutedEventArgs e)
		{
			foreach (TabBookmarkItem bookmarkName in tabControlDataGrid.SelectedItems)
				project.DataApp.Delete(typeof(Bookmark), bookmarkName.Name);
			tabModel.Bookmarks.Reload();
		}

		private void ButtonAdd_Click(object sender, RoutedEventArgs e)
		{
			textBoxName.Text = project.Navigator.Current.Changed;
			gridAddBookmark.IsVisible = true;
			textBoxName.Focus();
			textBoxName.SelectionEnd = textBoxName.Text.Length;
		}

		private void ButtonSave_Click(object sender, RoutedEventArgs e)
		{
			Bookmark bookmark = tabInstance.RootInstance.CreateBookmark();
			bookmark.Name = textBoxName.Text;
			project.DataApp.Save(bookmark.Name, bookmark);

			tabModel.Bookmarks.Items.Add(new TabBookmarkItem(bookmark));
			gridAddBookmark.IsVisible = false;
		}

		private void ButtonCancel_Click(object sender, RoutedEventArgs e)
		{
			gridAddBookmark.IsVisible = false;
		}

		private void OnSelectedBookmarkChanged(object sender, EventArgs e)
		{
			List<Bookmark> bookmarks = new List<Bookmark>();
			foreach (TabBookmarkItem viewBookmark in tabControlDataGrid.SelectedItems)
			{
				Bookmark bookmark = project.DataApp.Load<Bookmark>(viewBookmark.Name, new Call(tabInstance.taskInstance.Log));
				if (bookmark != null)
					bookmarks.Add(bookmark);
			}
			if (bookmarks.Count == 0)
				return;

			// Show merged set of selected bookmarks
			Bookmark selectedBookmarks = new Bookmark();
			selectedBookmarks.MergeBookmarks(bookmarks);
			tabInstance.SelectBookmark(selectedBookmarks.tabBookmark);
		}

		/*private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (tabData != null && tabData.SelectedItems.Count > 0);
		}
		*/

		private void Tab_LostFocus(object sender, RoutedEventArgs e)
		{
			Background = new SolidColorBrush(Theme.BackgroundColor);
		}

		private void Tab_GotFocus(object sender, RoutedEventArgs e)
		{
			Background = new SolidColorBrush(Theme.BackgroundFocusedColor);
		}
	}
}


/*
WPF Implementation
<Grid HorizontalAlignment="Stretch" Name="grid">
	<Grid.RowDefinitions>
		<RowDefinition Name="gridRowBookmarks" Height="Auto" MaxHeight="500"/>
		<RowDefinition Name="gridRowToolbar" Height="Auto"/>
		<RowDefinition Name="gridRowNew" Height="Auto"/>
		<RowDefinition Name="gridRowSpacer" Height="20"/>
	</Grid.RowDefinitions>


	<!--Label Name="labelName" Grid.Row="0" Background="DarkOrange" FontSize="14"/-->
	<ToolBarTray Grid.Row="1" DockPanel.Dock="Top" ToolBarTray.IsLocked="True" Background="{DynamicResource {x:Static local:Keys.ButtonBackgroundBrush}}" >
		<ToolBar Name="toolbar" Loaded="ToolBar_Loaded" Background="{DynamicResource {x:Static local:Keys.ButtonBackgroundBrush}}" BorderBrush="Black">
			<Button Click="Button_NewClick" Content="Add" ToolBar.OverflowMode="Never" Foreground="White" />
			<Button Click="Button_DeleteClick" Content="Delete" ToolBar.OverflowMode="Never" Foreground="White"/>
		</ToolBar>
	</ToolBarTray>

	<StackPanel Grid.Row="2" Name="panelNew" Orientation="Vertical" Visibility="Collapsed">
		<Label Content="Name:" Foreground="White"/>
		<TextBox Name="textBoxName" HorizontalAlignment="Stretch" MinWidth="100" FontSize="14"/>
		<ToolBarTray DockPanel.Dock="Top" ToolBarTray.IsLocked="True" Background="{DynamicResource {x:Static local:Keys.ButtonBackgroundBrush}}" >
			<ToolBar Name="toolbarSave" Loaded="ToolBar_Loaded" Background="{DynamicResource {x:Static local:Keys.ButtonBackgroundBrush}}" BorderBrush="Black">
				<Button Click="Button_NewSave" Content="Save" ToolBar.OverflowMode="Never" Foreground="White" />
				<Button Click="Button_NewCancel" Content="Cancel" ToolBar.OverflowMode="Never" Foreground="White"/>
			</ToolBar>
		</ToolBarTray>
	</StackPanel>


</Grid>
*/
