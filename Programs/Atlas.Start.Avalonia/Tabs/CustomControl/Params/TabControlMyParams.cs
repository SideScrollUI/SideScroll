using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Atlas.Start.Avalonia.Tabs;

public class TabControlMyParams : Grid
{
	public TabInstance TabInstance;
	public MyParams MyParams;

	//public event EventHandler<EventArgs> OnSelectionChanged;

	public TabControlMyParams(TabInstance tabInstance, MyParams myParams)
	{
		TabInstance = tabInstance;
		MyParams = myParams;

		InitializeControls();
	}

	private void InitializeControls()
	{
		VerticalAlignment = VerticalAlignment.Top;
		ColumnDefinitions = new ColumnDefinitions("Auto");
		RowDefinitions = new RowDefinitions("Auto");

		var controlParams = new TabControlParams(MyParams, false)
		{
			[Grid.RowProperty] = 0,
		};
		controlParams.AddPropertyControl(nameof(MyParams.Name));
		controlParams.AddPropertyControl(nameof(MyParams.Amount));
		Children.Add(controlParams);
	}
}
