using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Atlas.Start.Avalonia.Tabs
{
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
			//HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Top;
			ColumnDefinitions = new ColumnDefinitions("Auto");
			RowDefinitions = new RowDefinitions("Auto");

			var controlParams = new TabControlParams(TabInstance, MyParams, false)
			{
				[Grid.RowProperty] = 0,
			};
			controlParams.AddPropertyRow(nameof(MyParams.Name));
			controlParams.AddPropertyRow(nameof(MyParams.Amount));
			Children.Add(controlParams);
		}
	}
}
