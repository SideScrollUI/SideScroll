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

	public ToolbarButton ButtonAdd { get; protected set; }

	public ToolbarButton ButtonSearch { get; protected set; }
	public ToolbarButton ButtonLoadNext { get; protected set; }

	public ToolbarButton ButtonCopyClipBoard { get; protected set; }

	public TabControlSearch Search { get; protected set; }

	public TextBox TextBoxLimit { get; protected set; }

	public ToolbarRadioButton RadioButtonAscending { get; protected set; }
	public ToolbarRadioButton RadioButtonDescending { get; protected set; }

	public TextBox TextBoxStatus { get; protected set; }

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
