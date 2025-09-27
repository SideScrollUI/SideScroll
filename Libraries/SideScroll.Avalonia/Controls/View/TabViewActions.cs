using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using SideScroll.Tasks;
using SideScroll.Tabs;

namespace SideScroll.Avalonia.Controls.View;

public class TabViewActions : UserControl
{
	public TabInstance TabInstance { get; }
	public TabModel TabModel => TabInstance.Model;

	private readonly Dictionary<Button, TaskCreator> _taskCreators = [];

	public TabViewActions(TabInstance tabInstance)
	{
		TabInstance = tabInstance;

		if (TabModel.Actions!.Count == 0) return;

		var containerGrid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitions("Auto,*"),
			RowDefinitions = new RowDefinitions("Auto"),
			Margin = new Thickness(8),
		};

		int rowIndex = 0;
		foreach (TaskCreator taskCreator in TabModel.Actions)
		{
			var rowDefinition = new RowDefinition
			{
				Height = new GridLength(1, GridUnitType.Auto),
			};
			containerGrid.RowDefinitions.Add(rowDefinition);

			var button = new TabTextButton(taskCreator.Label, taskCreator.AccentType)
			{
				Margin = new Thickness(4, 2),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				HorizontalContentAlignment = HorizontalAlignment.Center,
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
		if (taskCreator.Flyout is ConfirmationFlyoutConfig config)
		{
			var flyout = new ConfirmationFlyout(() => InvokeTask(taskCreator), config.Text, config.ConfirmText, config.CancelText);
			flyout.ShowAt(this); // Using button will inherit button theme accent overrides
		}
		else
		{
			TabInstance.StartTask(taskCreator, taskCreator.ShowTask);
		}
	}

	public void InvokeTask(TaskCreator taskCreator)
	{
		TabInstance.StartTask(taskCreator, taskCreator.ShowTask);
	}
}
