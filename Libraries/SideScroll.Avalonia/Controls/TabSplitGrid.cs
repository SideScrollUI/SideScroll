using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;
using SideScroll.Avalonia.Controls.View;
using System.Diagnostics;

namespace SideScroll.Avalonia.Controls;

public enum SeparatorType
{
	None,
	Splitter,
	Spacer,
}

// Grid wrapper that allows multiple children and optional splitters in between each
// Only updates controls that change
// Vertical only right now
public class TabSplitGrid : Grid
{
	public double MinDesiredWidth { get; set; } = 100;
	public double MaxDesiredWidth { get; set; } = double.MaxValue;

	public Dictionary<object, Control> GridControls { get; protected set; } = [];
	public List<GridSplitter> GridSplitters { get; protected set; } = []; // reattach each time controls change

	public new bool IsArrangeValid { get; protected set; }

	private List<Item> _gridItems = [];

	public class Item
	{
		public object? Object { get; set; }
		public Control? Control { get; set; }
		public GridLength GridLength { get; set; }
	}

	public TabSplitGrid()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch; // not taking up maximum

		Focusable = true;
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		if (MaxDesiredWidth != double.MaxValue)
		{
			availableSize = availableSize.WithWidth(Math.Min(MaxDesiredWidth, availableSize.Width));
		}
		Size measured = base.MeasureOverride(availableSize);
		double desiredWidth = Math.Min(MaxDesiredWidth, measured.Width);
		desiredWidth = Math.Max(desiredWidth, MinDesiredWidth);
		Size maxSize = measured.WithWidth(desiredWidth);
		return maxSize;
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		IsArrangeValid = !double.IsNaN(finalSize.Width);
		return base.ArrangeOverride(finalSize);
	}

	public void AddControl(Control control, bool fill, SeparatorType separatorType = SeparatorType.Splitter, bool scrollable = false)
	{
		var gridLength = fill ? GridLength.Star : GridLength.Auto;
		AddControl(control, gridLength, separatorType, scrollable);
	}

	public void AddControl(Control control, GridLength gridLength, SeparatorType separatorType = SeparatorType.Splitter, bool scrollable = false)
	{
		if (scrollable)
		{
			ScrollViewer scrollViewer = new()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				Content = control,
			};
			control = scrollViewer;
		}

		Item item = new()
		{
			Control = control,
			GridLength = gridLength,
		};
		_gridItems.Add(item);

		int splitterIndex = RowDefinitions.Count;
		InsertRowDefinition(GridLength.Auto, splitterIndex);
		bool addSplitter = false;

		/*if (separatorType == SeparatorType.Splitter && fill)
		{
			// Add a Grid Splitter if there's been a single star row definition before this one
			for (int prevRowIndex = splitterIndex - 1; prevRowIndex >= 0; prevRowIndex--)
			{
				// Grid Splitter doesn't work due to the Tasks being a fixed sized between the 2 stars (bug?)
				//addSplitter |= (RowDefinitions[prevRowIndex].Height.IsStar);
			}
		}*/

		int controlIndex = splitterIndex + 1;
		RowDefinition rowDefinition = InsertRowDefinition(gridLength, controlIndex);
		rowDefinition.MaxHeight = control.MaxHeight;

		SetRow(control, controlIndex);
		Children.Add(control);

		if (addSplitter)
		{
			AddHorizontalGridSplitter(splitterIndex);
		}

		//gridControls[oldChild.Key] = control;
		InvalidateMeasure();
	}

	protected RowDefinition InsertRowDefinition(GridLength gridLength, int index)
	{
		var rowDefinition = new RowDefinition(gridLength);
		RowDefinitions.Insert(index, rowDefinition);
		return rowDefinition;
	}

	// always show splitters if their is a fill before or after?
	// Do we allow changing an auto to a fill?
	// always add a RowDefinition before and after
	public void InsertControl(Control control, GridLength gridLength, int rowIndex)
	{
		InsertRowDefinition(GridLength.Auto, rowIndex - 1); // Splitter or Spacer

		InsertRowDefinition(gridLength, rowIndex);

		SetRow(control, rowIndex);
		//Children.Insert(index, control);
		Children.Add(control);
	}

	// Avalonia GridSplitter hardcodes neighbors in it's OnAttachedToVisualTree
	// Reattach them whenever we change neighbors
	protected void ReattachSplitters()
	{
		foreach (var gridSplitter in GridSplitters)
		{
			Children.Remove(gridSplitter);
		}
		GridSplitters.Clear();

		Debug.Assert(_gridItems.Count * 2 == RowDefinitions.Count);
		int index = 0;
		//int prevStretch = -1; // todo: we can figure out splitters vs spacers automatically via fill property
		foreach (var gridItem in _gridItems)
		{
			if (index > 0)
			{
				AddHorizontalGridSplitter(index);
			}

			// separator
			index++;

			RowDefinition rowDefinition = RowDefinitions[index];
			rowDefinition.Height = gridItem.GridLength;

			SetRow(gridItem.Control!, index);
			index++;
		}
	}

	protected void AddHorizontalGridSplitter(int rowIndex)
	{
		//AddRowDefinition(false, rowIndex);

		TabSplitter gridSplitter = new()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			ResizeDirection = GridResizeDirection.Rows,
			//ShowsPreview = true,
		};
		GridSplitters.Add(gridSplitter);
		SetRow(gridSplitter, rowIndex);
		//Children.Insert(index, gridSplitter);
		Children.Add(gridSplitter);
	}

	protected void AddVerticalGridSplitter(int columnIndex)
	{
		TabSplitter gridSplitter = new()
		{
			VerticalAlignment = VerticalAlignment.Stretch,
			ResizeDirection = GridResizeDirection.Columns,
		};
		//GridSplitters.Add(gridSplitter);
		SetColumn(gridSplitter, columnIndex);
		//Children.Insert(index, gridSplitter);
		Children.Add(gridSplitter);
	}

	protected void AddRowSpacer(int rowIndex)
	{
		//if (Children.Count <= 1)
		//	return;

		Border border = new()
		{
			//Width = 100,
			Height = 6,
			[Grid.RowProperty] = rowIndex
		};
		// Add a dummy panel so the children count equals the rowdefinition count, otherwise we need to track which rowdefinitions belong to which control
		//Bounds border = new Bounds();
		//SetRow(border, index);
		Children.Add(border);
	}

	protected void RemoveControls(Dictionary<object, Control> controls)
	{
		var hashedControls = GridControls.Values.ToHashSet();

		// Remove any children not in use anymore
		foreach (var oldChild in controls)
		{
			if (hashedControls.Contains(oldChild.Value))
				continue;

			RowDefinitions.RemoveAt(RowDefinitions.Count - 1);
			RowDefinitions.RemoveAt(RowDefinitions.Count - 1);

			Children.Remove(oldChild.Value);

			DisposeControl(oldChild.Value);
		}
	}

	protected void AddControls(List<Control> orderedControls)
	{
		//RowDefinitions.Clear();
		_gridItems.Clear();
		int newIndex = 1;
		foreach (Control control in orderedControls)
		{
			Item item = new()
			{
				Control = control,
				GridLength = GridLength.Star,
			};
			_gridItems.Add(item);

			if (!Children.Contains(control))
			{
				// Add a new control
				InsertControl(control, item.GridLength, newIndex);
			}
			newIndex += 2; // leave spot for splitters
		}
	}

	// Only used for child controls right now
	public void SetControls(Dictionary<object, Control> newControls, List<Control> orderedControls)
	{
		Dictionary<object, Control> oldControls = GridControls;

		// don't clear old controls so we invalidate container as little as possible when we resize the remaining

		GridControls = newControls;

		BeginInit();

		RemoveControls(oldControls);
		AddControls(orderedControls);

		ReattachSplitters();

		EndInit();

		// Add all child controls to the view
		InvalidateMeasure();
		//UpdateSelectedTabInstances();
	}

	internal void Clear(bool dispose)
	{
		// objects might still be referenced and re-added again
		if (dispose)
		{
			foreach (Control control in Children)
			{
				// does Children.Clear() already handle this?
				DisposeControl(control);
			}
		}

		RowDefinitions.Clear();
		Children.Clear();
		GridControls.Clear();
		_gridItems.Clear();
	}

	private static void DisposeControl(Control control)
	{
		if (control is IDisposable disposable)
		{
			disposable.Dispose();
			return;
		}

		foreach (var child in control.GetVisualChildren())
		{
			if (child is IDisposable childDisposable)
			{
				childDisposable.Dispose();
			}
			else if (child is Control childControl)
			{
				DisposeControl(childControl);
			}
		}
	}
}
