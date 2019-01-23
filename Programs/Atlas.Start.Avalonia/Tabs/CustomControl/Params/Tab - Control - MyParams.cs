﻿using System;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls;
using Atlas.GUI.Avalonia.Controls;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabControlMyParams : Grid
	{
		private TabInstance tabInstance;
		private MyParams myParams;

		public event EventHandler<EventArgs> OnSelectionChanged;

		public TabControlMyParams(TabInstance tabInstance, MyParams myParams)
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
			controlParams.AddPropertyRow(myParams.GetType().GetProperty(nameof(myParams.Name)));
			controlParams.AddPropertyRow(myParams.GetType().GetProperty(nameof(myParams.Amount)));
			this.Children.Add(controlParams);
		}
	}
}
