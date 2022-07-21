using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using System.Collections.Generic;

namespace Atlas.UI.Avalonia.Controls;

public class TabControlActions : UserControl
{
	public TabInstance TabInstance;
	public TabModel TabModel;

	// public bool GridInitialized { get; private set; }

	private readonly Dictionary<Button, TaskCreator> _taskCreators = new();

	public TabControlActions(TabInstance tabInstance, TabModel tabModel)
	{
		TabInstance = tabInstance;
		TabModel = tabModel;

		InitializeControls();
	}

	private void InitializeControls()
	{
		var containerGrid = new Grid()
		{
			ColumnDefinitions = new ColumnDefinitions("Auto,*"),
			RowDefinitions = new RowDefinitions("Auto"),
		};

		int rowIndex = 0;
		foreach (TaskCreator taskCreator in TabModel.Actions!)
		{
			var rowDefinition = new RowDefinition()
			{
				Height = new GridLength(1, GridUnitType.Auto),
			};
			containerGrid.RowDefinitions.Add(rowDefinition);

			var button = new TabControlButton(taskCreator.Label)
			{
				Margin = new Thickness(4, 2),
			};
			button.Click += Button_Click;
			_taskCreators[button] = taskCreator;
			Grid.SetRow(button, rowIndex++);
			containerGrid.Children.Add(button);
		}

		Content = containerGrid;
	}

	private void Button_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
	{
		Button button = (Button)sender!;
		TaskCreator taskCreator = _taskCreators[button];
		TabInstance.StartTask(taskCreator, taskCreator.ShowTask);
	}
}

/*
This could be converted to a DataGrid now that there's a button column
*/
