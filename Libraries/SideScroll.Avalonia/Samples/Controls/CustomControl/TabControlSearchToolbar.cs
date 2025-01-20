using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Avalonia.Themes;
using SideScroll.Resources;
using SideScroll.Tabs;

namespace SideScroll.Avalonia.Samples.Controls.CustomControl;

public class TabControlSearchToolbar : TabControlToolbar
{
	protected override Type StyleKeyOverride => typeof(TabControlToolbar);

	public ToolbarButton ButtonAdd;

	public ToolbarButton ButtonSearch;
	public ToolbarButton ButtonLoadNext;

	public ToolbarButton ButtonCopyClipBoard;

	public TabControlSearch Search;

	public TextBox TextBoxLimit;

	public ToolbarRadioButton RadioButtonAscending;
	public ToolbarRadioButton RadioButtonDescending;

	public TextBox TextBoxStatus;

	public TabControlSearchToolbar(TabInstance tabInstance) : base(tabInstance)
	{
		ButtonAdd = AddButton("Add", Icons.Svg.Add, "Add");
		AddButton("Save", Icons.Svg.Save);

		AddSeparator();
		ButtonSearch = AddButton("Search (Ctrl + S)", Icons.Svg.Search);
		ButtonSearch.HotKey = new KeyGesture(Key.S, KeyModifiers.Control);

		ButtonLoadNext = AddButton("Next", Icons.Svg.RightArrow);
		ButtonLoadNext.IsEnabled = false;

		AddSeparator();
		ButtonCopyClipBoard = AddButton("Copy to Clipboard", ImageColorView.CreateAlternate(Icons.Svg.PadNote));

		AddSeparator();
		Search = new TabControlSearch
		{
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness(6, 0),
		};
		AddControl(Search);

		AddSeparator();
		AddLabel("Limit");
		TextBoxLimit = AddText("10", 60);
		ToolTip.SetTip(TextBoxLimit, "1 - 100");

		AddSeparator();
		RadioButtonAscending = AddRadioButton("Ascending");
		RadioButtonDescending = AddRadioButton("Descending");
		RadioButtonAscending.IsChecked = true;

		AddSeparator();
		TextBoxStatus = AddLabelText("Status");

		AddSeparator();
		AddToggleButton("Favorite", Icons.Svg.StarFilled, Icons.Svg.Star, false);
	}
}
