using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Tabs;
using SideScroll.Tabs.Bookmarks;
using SideScroll.UI.Avalonia.Themes;
using SideScroll.UI.Avalonia.Utilities;
using SideScroll.UI.Avalonia.View;

namespace SideScroll.UI.Avalonia.Controls;

public class TabControlTitle : Border, IDisposable
{
	public readonly TabView TabView;
	public TabInstance TabInstance => TabView.Instance;
	public string Label { get; set; }

	public int MaxDesiredWidth { get; set; } = 50;

	public TextBlock TextBlock;
	private readonly Grid _containerGrid;

	public string Text
	{
		get => Label;
		set
		{
			Label = value;
			TextBlock.Text = value;
		}
	}

	public TabControlTitle(TabView tabView, string? label = null)
	{
		TabView = tabView;
		label ??= TabInstance.Label;
		Label = new StringReader(label).ReadLine()!; // Remove anything after first line

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
			//Margin = new Thickness(2), // Shows as black, Need Padding so Border not needed
			HorizontalAlignment = HorizontalAlignment.Stretch,
			// [ToolTip.TipProperty] = Label, // Enable?
		};
		AddContextMenu();

		var borderPaddingTitle = new Border
		{
			BorderThickness = new Thickness(5, 2, 2, 2),
			BorderBrush = Brushes.Transparent,
			Child = TextBlock,
			[Grid.ColumnProperty] = 1,
		};
		_containerGrid.Children.Add(borderPaddingTitle);

		AddLinkButton();

		Child = _containerGrid;
	}

	private void AddContextMenu()
	{
		var contextMenu = new TabViewContextMenu(TabView, TabInstance);

		// Copy Title Text to ClipBoard
		var menuItemCopy = new TabMenuItem
		{
			Header = "_Copy",
		};
		menuItemCopy.Click += delegate
		{
			ClipboardUtils.SetText(this, Label);
		};
		contextMenu.ItemList.Insert(0, menuItemCopy);

		ContextMenu = contextMenu;
	}

	private void AddLinkButton()
	{
		if (!TabInstance.IsLinkable) return;

		var linkButton = new TabControlButton
		{
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalAlignment = HorizontalAlignment.Left,
			Content = "~",
			Margin = new Thickness(0, 0, 0, 0),
			Padding = new Thickness(2, 0),
			Background = Brushes.Transparent,
			Foreground = SideScrollTheme.TitleForeground,
			BorderBrush = SideScrollTheme.TabBackgroundBorder,
			BorderThickness = new Thickness(0, 0, 1, 0),
			[Grid.ColumnProperty] = 0,
			[ToolTip.TipProperty] = "Copy Tab Link",
		};
		linkButton.Resources.Add("ThemeButtonBackgroundPointerOverBrush", SideScrollTheme.TitleButtonBackgroundPointerOver);
		linkButton.Resources.Add("ThemeButtonBackgroundPressedBrush", SideScrollTheme.TitleButtonBackgroundPointerOver);
		linkButton.Click += LinkButton_Click;
		_containerGrid.Children.Add(linkButton);
	}

	private async void LinkButton_Click(object? sender, RoutedEventArgs e)
	{
		Flyout flyout = new()
		{
			Placement = PlacementMode.BottomEdgeAlignedLeft,
		};
		AvaloniaUtils.ShowFlyout(this, flyout, "Creating Link ...");

		try
		{
			Bookmark bookmark = TabInstance.CreateBookmark();
			bookmark.BookmarkType = BookmarkType.Tab;
			string? uri = await TabInstance.Project.Linker.AddLinkAsync(new Call(), bookmark);
			if (uri == null)
				return;

			await ClipboardUtils.SetTextAsync(this, uri);
			AvaloniaUtils.ShowFlyout(this, flyout, "Link copied to clipboard");
		}
		catch (Exception ex)
		{
			AvaloniaUtils.ShowFlyout(this, flyout, ex.Message);
		}
	}

	protected override Size MeasureCore(Size availableSize)
	{
		Size measured = base.MeasureCore(availableSize);
		Size maxSize = new(Math.Min(MaxDesiredWidth, measured.Width), measured.Height);
		return maxSize;
	}

	public void Dispose()
	{
		if (ContextMenu != null)
		{
			ContextMenu.ItemsSource = null;
			ContextMenu = null;
		}
		Child = null;
	}
}
