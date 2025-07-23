using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Avalonia.Extensions;
using SideScroll.Avalonia.Themes;
using SideScroll.Avalonia.Utilities;
using SideScroll.Resources;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Avalonia.Controls;

public class TabSearch : Grid
{
	public TextBox TextBoxSearch { get; set; }

	public string? Text
	{
		get => TextBoxSearch.Text;
		set => TextBoxSearch.Text = value;
	}

	public TabSearch()
	{
		ColumnDefinitions = new ColumnDefinitions("*");
		RowDefinitions = new RowDefinitions("Auto");

		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Top;

		Focusable = true;

		AddTextBox();
		AddIcon();
	}

	[MemberNotNull(nameof(TextBoxSearch))]
	private void AddTextBox()
	{
		TextBoxSearch = new ToolbarTextBox
		{
			VerticalContentAlignment = VerticalAlignment.Center,
			Padding = new Thickness(5, 3, 25, 3),
			Watermark = "Search",
		};

		Children.Add(TextBoxSearch);
	}

	private void AddIcon()
	{
		var coloredImage = SvgUtils.TryGetSvgColorImage(Icons.Svg.SearchRight, SideScrollTheme.ToolbarTextForeground.Color.WithAlpha(128));

		var image = new Image
		{
			Width = 16,
			Height = 16,
			Source = coloredImage,
			Margin = new Thickness(7, 4),
			HorizontalAlignment = HorizontalAlignment.Right,
		};
		Children.Add(image);
	}

	protected override void OnGotFocus(GotFocusEventArgs e)
	{
		TextBoxSearch.Focus();
	}
}
