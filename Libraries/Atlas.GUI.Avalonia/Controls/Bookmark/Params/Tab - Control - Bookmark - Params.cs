using System;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls;
using Atlas.GUI.Avalonia.Controls;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlBookmarkParams : Grid
	{
		private TabInstance tabInstance;
		private BookmarkParams myParams;

		//public event EventHandler<EventArgs> OnSelectionChanged;

		public TabControlBookmarkParams(TabInstance tabInstance, BookmarkParams myParams)
		{
			this.tabInstance = tabInstance;
			this.myParams = myParams;

			InitializeControls();
		}

		private void InitializeControls()
		{
			//this.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.VerticalAlignment = VerticalAlignment.Top;
			this.ColumnDefinitions = new ColumnDefinitions("Auto");
			this.RowDefinitions = new RowDefinitions("Auto");

			var controlParams = new TabControlParams(tabInstance, myParams, false)
			{
				[Grid.RowProperty] = 0,
			};
			controlParams.AddPropertyRow(nameof(myParams.Name));
			controlParams.AddPropertyRow(nameof(myParams.Amount));
			this.Children.Add(controlParams);
		}
	}
}
