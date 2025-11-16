using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SideScroll.Attributes;
using System.Diagnostics;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.DataGrids;

public class DataGridButtonColumn : DataGridBoundColumn
{
	public MethodInfo MethodInfo { get; set; }
	public string ButtonText { get; set; }
	public string? VisiblePropertyName { get; set; }

	public DataGridButtonColumn(MethodInfo methodInfo, string buttonText)
	{
		MethodInfo = methodInfo;
		VisiblePropertyName = methodInfo.GetCustomAttribute<ButtonColumnAttribute>()?.VisiblePropertyName;
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
			MethodInfo.Invoke(button.DataContext, []);
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
