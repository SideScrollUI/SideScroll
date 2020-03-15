using System;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabControlMyParams : Grid
	{
		private TabInstance tabInstance;
		private MyParams myParams;

		//public event EventHandler<EventArgs> OnSelectionChanged;

		public TabControlMyParams(TabInstance tabInstance, MyParams myParams)
		{
			this.tabInstance = tabInstance;
			this.myParams = myParams;

			InitializeControls();
		}

		private void InitializeControls()
		{
			//HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Top;
			ColumnDefinitions = new ColumnDefinitions("Auto");
			RowDefinitions = new RowDefinitions("Auto");

			var controlParams = new TabControlParams(tabInstance, myParams, false)
			{
				[Grid.RowProperty] = 0,
			};
			controlParams.AddPropertyRow(nameof(myParams.Name));
			controlParams.AddPropertyRow(nameof(myParams.Amount));
			Children.Add(controlParams);
		}
	}
}
