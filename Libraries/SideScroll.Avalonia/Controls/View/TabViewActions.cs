using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using SideScroll.Tasks;
using SideScroll.Tabs;
using SideScroll.Avalonia.Controls.Flyouts;

namespace SideScroll.Avalonia.Controls.View;

/// <summary>
/// A panel that renders the clickable action buttons defined by a <see cref="TaskCreator"/> list,
/// each wired to invoke a <see cref="TaskCreator"/> on the owning tab instance.
/// </summary>
public class TabViewActions : UserControl
{
	/// <summary>Gets the tab instance whose actions this panel renders.</summary>
	public TabInstance TabInstance { get; }

	/// <summary>Gets the tab model from the owning tab instance.</summary>
	public TabModel TabModel => TabInstance.Model;

	private readonly Dictionary<Button, TaskCreator> _taskCreators = [];

	/// <summary>Initializes the actions panel for <paramref name="tabInstance"/>, rendering a button for each of the given actions.</summary>
	/// <param name="tabInstance">The tab instance the actions are invoked on.</param>
	/// <param name="actions">The actions to render as buttons.</param>
	public TabViewActions(TabInstance tabInstance, IReadOnlyList<TaskCreator> actions)
	{
		TabInstance = tabInstance;

		if (actions.Count == 0) return;

		var containerGrid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitions("Auto,*"),
			RowDefinitions = new RowDefinitions("Auto"),
			Margin = new Thickness(8),
		};

		int rowIndex = 0;
		foreach (TaskCreator taskCreator in actions)
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
				[ToolTip.TipProperty] = taskCreator.Description,
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

	private void InvokeTask(TaskCreator taskCreator)
	{
		TabInstance.StartTask(taskCreator, taskCreator.ShowTask);
	}
}
