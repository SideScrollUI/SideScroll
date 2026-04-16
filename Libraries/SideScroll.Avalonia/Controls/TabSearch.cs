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

/// <summary>
/// A search bar grid that combines a text box and a clear button, raising a text-changed event as the user types.
/// </summary>
public class TabSearch : Grid
{
	/// <summary>Gets the text box that accepts the search input.</summary>
	public TextBox TextBoxSearch { get; protected set; }

	/// <summary>Gets or sets the current search text.</summary>
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
			Padding = new Thickness(5, 2, 25, 1),
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
			Margin = new Thickness(7, 0),
			HorizontalAlignment = HorizontalAlignment.Right,
		};
		Children.Add(image);
	}

	protected override void OnGotFocus(GotFocusEventArgs e)
	{
		TextBoxSearch.Focus();
	}
}
