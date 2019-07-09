using Atlas.Core;
using Atlas.Tabs;
using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace Atlas.GUI.Avalonia.Controls
{
	public partial class TabControlBookmarkSettings : UserControl
	{
		public Project project;
		public TabInstance tabInstance;
		public TabModel tabModel;

		private Grid containerGrid;
		//private TabControlDataGrid tabControlDataGrid;

		private Grid gridAddBookmark;
		private TextBox textBoxName;

		public TabControlBookmarkSettings(TabInstance tabInstance)
		{
			this.tabInstance = tabInstance;
			this.project = tabInstance.project;
			this.tabModel = tabInstance.tabModel;

			//InitializeControls();
		}

		public override string ToString()
		{
			return tabModel.Name;
		}

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
			if (containerGrid != null)
				return;

			//this.IsVisible = false;
			//this.Background = new SolidColorBrush(Theme.BackgroundColor);
			this.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.VerticalAlignment = VerticalAlignment.Stretch;
			//this.Width = 1000;
			//this.Height = 1000;


			containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto"),
				RowDefinitions = new RowDefinitions("Auto,Auto,Auto"), // Header, Body
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
			};

			//containerGrid.Children.Add(tabControlDataGrid);

			AddNewPanel();

			this.Content = containerGrid;

			this.Focusable = true;
		}

		public void ShowBookmark(Bookmark bookmark)
		{
			this.IsVisible = true;
			InitializeControls();
			textBoxName.Focus();
		}

		private void AddNewPanel()
		{
			gridAddBookmark = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto,Auto"),
				RowDefinitions = new RowDefinitions("Auto,Auto,Auto"), // Name Label, Name Value, Buttons
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

			Button buttonSave = TabControlButton.Create("Save");
			Grid.SetRow(buttonSave, 2);
			Grid.SetColumn(buttonSave, 0);
			buttonSave.Click += ButtonSave_Click;

			Button buttonCancel = TabControlButton.Create("Cancel");
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
			this.IsVisible = false;
		}
	}
}
