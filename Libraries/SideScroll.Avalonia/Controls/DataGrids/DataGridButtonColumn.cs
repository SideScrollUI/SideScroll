using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SideScroll.Attributes;
using System.Diagnostics;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.DataGrids;

/// <summary>A data-grid column that renders a <see cref="DataGridButton"/> in each cell and invokes a reflected method on the row's data object when clicked.</summary>
public class DataGridButtonColumn : DataGridBoundColumn
{
	/// <summary>Gets the method invoked when the button in a cell is clicked, or <c>null</c> when <see cref="ClickAction"/> is used instead.</summary>
	public MethodInfo? MethodInfo { get; }

	/// <summary>
	/// Gets or sets an optional delegate to invoke when the button is clicked.
	/// When set, takes precedence over <see cref="MethodInfo"/>; the row's data object is passed as the argument.
	/// </summary>
	public Action<object>? ClickAction { get; set; }

	/// <summary>Gets or sets the button label text.</summary>
	public string ButtonText { get; set; }

	/// <summary>Gets or sets the name of a boolean property on the row object that controls button visibility, or <c>null</c> to always show.</summary>
	public string? VisiblePropertyName { get; set; }

	/// <summary>Creates a column that invokes <paramref name="methodInfo"/> on the row object when clicked.</summary>
	public DataGridButtonColumn(MethodInfo methodInfo, string buttonText)
	{
		MethodInfo = methodInfo;
		VisiblePropertyName = methodInfo.GetCustomAttribute<ButtonColumnAttribute>()?.VisiblePropertyName;
		ButtonText = buttonText;
		CanUserSort = false;
		Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
	}

	/// <summary>Creates a column that invokes <paramref name="clickAction"/> with the row's data object when clicked.</summary>
	public DataGridButtonColumn(string buttonText, Action<object> clickAction)
	{
		ClickAction = clickAction;
		ButtonText = buttonText;
		CanUserSort = false;
		Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
	}

	// This doesn't get called when reusing cells
	protected override Control GenerateElement(DataGridCell cell, object dataItem)
	{
		return GenerateEditingElementDirect(cell, dataItem);
	}

	protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
	{
		var button = new DataGridButton(ButtonText);
		button.Resources.Add("ButtonPadding", new Thickness(2, 5));
		if (VisiblePropertyName != null)
		{
			button.BindVisible(VisiblePropertyName);
		}
		button.Click += Button_Click;
		return button;
	}

	private void Button_Click(object? sender, RoutedEventArgs e)
	{
		try
		{
			Button button = (Button)sender!;
			if (ClickAction != null)
			{
				ClickAction(button.DataContext!);
			}
			else
			{
				MethodInfo!.Invoke(button.DataContext, []);
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}

	protected override object PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
	{
		return string.Empty;
	}
}
