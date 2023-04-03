using Atlas.Resources;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Avalonia.Controls;

namespace Atlas.Start.Avalonia.Tabs;

public class TabControlSearchToolbar : TabControlToolbar
{
	public ToolbarButton ButtonSearch;
	public ToolbarButton ButtonLoadAdd;
	public ToolbarButton ButtonLoadNext;
	public ToolbarButton ButtonSleep;

	public ToolbarRadioButton RadioButtonAscending;
	public ToolbarRadioButton RadioButtonDescending;

	public TextBox TextBoxLimit;

	public ToolbarButton ButtonCopyClipBoard;

	public TextBox TextBoxStatus;

	public TabControlSearchToolbar(TabInstance tabInstance) : base(tabInstance)
	{
		//project.navigator.CanSeekBackwardOb
		//CommandBinder.
		//CommandBindings.Add(commandBindingBack);

		ButtonSearch = AddButton("Search", Icons.Svg.Search);
		ButtonLoadNext = AddButton("Next", Icons.Svg.RightArrow);
		ButtonSleep = AddButton("Sleep", Icons.Svg.Refresh);
		AddSeparator();

		RadioButtonAscending = AddRadioButton("Ascending");
		RadioButtonDescending = AddRadioButton("Descending");
		RadioButtonAscending.IsChecked = true;
		AddSeparator();

		AddLabel("Limit");
		TextBoxLimit = AddText("10", 60);
		ToolTip.SetTip(TextBoxLimit, "1 - 100");
		AddSeparator();

		ButtonLoadAdd = AddButton("Add", Icons.Svg.Add);
		AddSeparator();

		AddButton("Save", Icons.Svg.Save);
		AddSeparator();

		ButtonCopyClipBoard = AddButton("Copy to Clipboard", Icons.Svg.PadNote);
		TextBoxStatus = AddLabelText("Status");
	}
}

/*
Probably convert these to IObservables? and replace RelayCommand
*/
