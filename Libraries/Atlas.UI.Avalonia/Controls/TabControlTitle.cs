using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.IO;

namespace Atlas.UI.Avalonia.Controls;

public class TabControlTitle : UserControl, IDisposable
{
	public readonly TabInstance TabInstance;
	public string Label { get; set; }

	public TextBlock? TextBlock;
	//private CheckBox checkBox;
	private Grid _containerGrid;

	public string Text
	{
		get => Label;
		set
		{
			Label = value;
			TextBlock!.Text = value;
		}
	}

	public TabControlTitle(TabInstance tabInstance, string? name = null)
	{
		TabInstance = tabInstance;
		Label = name ?? tabInstance.Label;
		Label = new StringReader(Label).ReadLine()!; // Remove anything after first line

		Background = Theme.TitleBackground;

		_containerGrid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitions("Auto,*"),
			RowDefinitions = new RowDefinitions("Auto"),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
		};

		// Add a wrapper class with a border?
		// Need to make the desired size for this a constant x
		TextBlock = new TextBlock
		{
			Text = Label,
			FontSize = 15,
			//Margin = new Thickness(2), // Shows as black, Need Padding so Border not needed
			Foreground = Theme.TitleForeground,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			// [ToolTip.TipProperty] = Label, // Enable?
		};
		AvaloniaUtils.AddContextMenu(TextBlock);

		var borderPaddingTitle = new Border
		{
			BorderThickness = new Thickness(5, 2, 2, 2),
			BorderBrush = Theme.TitleBackground,
			Child = TextBlock,
			[Grid.ColumnProperty] = 1,
		};
		_containerGrid.Children.Add(borderPaddingTitle);

		// Notes
		// Add checkbox here for tabModel.Notes
		/*checkBox = new CheckBox
		{
			IsChecked = (tabInstance.tabModel.Notes != null && tabInstance.tabViewSettings.NotesVisible),
			BorderThickness = new Thickness(1),
			BorderBrush = new SolidColorBrush(Colors.White),
			Foreground = new SolidColorBrush(Theme.TitleForegroundColor),
			[Grid.ColumnProperty] = 1,
		};
		checkBox.Click += CheckBox_Click;*/

		/*if (TabInstance.Model.Notes != null && TabInstance.Model.Notes.Length > 0)
		{
			//Button button = new Button();
			Image image = AvaloniaAssets.Images.Link;
			image.Height = 20;
			Grid.SetColumn(image, 1);
			_containerGrid.Children.Add(image);
			//containerGrid.Children.Add(checkBox);
		}*/

		AddLinkButton();

		var borderContent = new Border
		{
			BorderThickness = new Thickness(1),
			BorderBrush = new SolidColorBrush(Colors.Black),
			Child = _containerGrid
		};

		Content = borderContent;
	}

	private void AddLinkButton()
	{
		if (!TabInstance.IsLinkable)
			return;

		var linkButton = new TabControlButton
		{
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalAlignment = HorizontalAlignment.Left,
			Content = "~",
			ClipToBounds = false,
			Margin = new Thickness(-7, 0, 0, 0),
			Padding = new Thickness(0),
			[Grid.ColumnProperty] = 0,
			[ToolTip.TipProperty] = "Copy Tab Link",
		};
		ClipToBounds = false;
		linkButton.Click += LinkButton_Click;
		_containerGrid.Children.Add(linkButton);
	}

	private async void LinkButton_Click(object? sender, RoutedEventArgs e)
	{
		Bookmark bookmark = TabInstance.CreateBookmark();
		bookmark.BookmarkType = BookmarkType.Tab;
		string? uri = await TabInstance.Project.Linker.GetLinkUriAsync(new Call(), bookmark);
		if (uri == null)
			return;

		await ClipBoardUtils.SetTextAsync(uri);
	}

	public void Dispose()
	{
		if (TextBlock != null)
		{
			TextBlock.ContextMenu!.Items = null;
			TextBlock.ContextMenu = null;
			TextBlock = null;
		}
		Content = null;
	}

	protected override Size MeasureCore(Size availableSize)
	{
		Size measured = base.MeasureCore(availableSize);
		Size maxSize = new(Math.Min(50, measured.Width), measured.Height);
		return maxSize;
	}

	/*private void CheckBox_Click(object sender, RoutedEventArgs e)
	{
		tabInstance.tabViewSettings.NotesVisible = (bool)checkBox.IsChecked;
		tabInstance.SaveTabSettings();
		tabInstance.Reload();
	}*/
}
