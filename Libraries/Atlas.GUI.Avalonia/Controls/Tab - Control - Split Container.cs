using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Atlas.GUI.Avalonia.Controls
{
	public enum SeparatorType
	{
		None,
		Splitter,
		Spacer,
	}

	// Grid wrapper that allows multiple children and optional splitters in between each
	// Only updates controls that change
	// Vertical only right now
	public class TabControlSplitContainer : Grid
	{
		public Dictionary<object, Control> gridControls = new Dictionary<object, Control>();
		//public Dictionary<int, GridSplitter> gridSplitters = new Dictionary<int, GridSplitter>(); // reattach each time controls change
		public List<GridSplitter> gridSplitters = new List<GridSplitter>(); // reattach each time controls change
		public double MaxDesiredWidth = double.MaxValue;

		public List<Item> gridItems = new List<Item>();


		public class Item
		{
			public object Object { get; set; }
			public Control Control { get; set; }
			public bool fill { get; set; }
		}

		public TabControlSplitContainer()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch; // not taking up maximum
			//ColumnDefinition columnDefinition = new ColumnDefinition(1, GridUnitType.Star);
			//ColumnDefinitions.Add(columnDefinition);
			Focusable = true;

			GotFocus += TabView_GotFocus;
			LostFocus += TabView_LostFocus;
		}

		// real DesiredSize doesn't work because of HorizontalAlign = Stretch?
		public new Size DesiredSize
		{
			get
			{
				Size desiredSize = new Size();
				foreach (var control in Children)
				{
					Size childDesiredSize = control.DesiredSize;
					desiredSize = new Size(Math.Max(desiredSize.Width, childDesiredSize.Width), Math.Max(desiredSize.Height, childDesiredSize.Height));
				}
				return desiredSize;
			}
		}

		// can't override DesiredSize
		protected override Size MeasureCore(Size availableSize)
		{
			if (MaxDesiredWidth != double.MaxValue)
				availableSize = new Size(Math.Min(MaxDesiredWidth, availableSize.Width), availableSize.Height);
			Size measured = base.MeasureCore(availableSize);
			Size maxSize = new Size(Math.Min(MaxDesiredWidth, measured.Width), measured.Height);
			return maxSize;
		}

		public Size arrangeOverrideFinalSize;
		protected override Size ArrangeOverride(Size finalSize)
		{
			arrangeOverrideFinalSize = finalSize;
			return base.ArrangeOverride(finalSize);
		}

		private void TabView_GotFocus(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Background = new SolidColorBrush(Theme.BackgroundFocusedColor);
		}

		private void TabView_LostFocus(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Background = new SolidColorBrush(Theme.BackgroundColor);
		}

		protected override void OnPointerEnter(PointerEventArgs e)
		{
			base.OnPointerEnter(e);
			Background = new SolidColorBrush(Theme.BackgroundFocusedColor);
		}

		protected override void OnPointerLeave(PointerEventArgs e)
		{
			base.OnPointerLeave(e);
			Background = new SolidColorBrush(Theme.BackgroundColor);
		}

		public void AddSplitter()
		{

		}

		public void AddControl(Control control, bool fill, SeparatorType separatorType)
		{
			Item item = new Item()
			{
				Control = control,
				fill = fill,
			};
			gridItems.Add(item);
			AddSeparatorRowDefinition(RowDefinitions.Count);
			/*if (separatorType == SeparatorType.Splitter)
				AddRowSplitter(RowDefinitions.Count);
			else if (separatorType == SeparatorType.Splitter)
				AddRowSpacer(RowDefinitions.Count);*/

			RowDefinition rowDefinition = new RowDefinition();
			if (fill)
				rowDefinition.Height = new GridLength(1, GridUnitType.Star);
			else
				rowDefinition.Height = GridLength.Auto;
			RowDefinitions.Add(rowDefinition);

			SetRow(control, RowDefinitions.Count - 1);
			//SetRow(control, Children.Count);
			Children.Add(control);

			//gridControls[oldChild.Key] = control;
			InvalidateMeasure();
		}

		// always show splitters if their is a fill before or after?
		// Do we allow changing an auto to a fill?
		// always add a RowDefinition before and after
		public void InsertControl(Control control, bool fill, int index)
		{
			AddSeparatorRowDefinition(index - 1);

			RowDefinition rowDefinition = new RowDefinition();
			if (fill)
				rowDefinition.Height = new GridLength(1, GridUnitType.Star);
			else
				rowDefinition.Height = GridLength.Auto;
			RowDefinitions.Insert(index, rowDefinition);

			SetRow(control, index);
			//Children.Insert(index, control);
			Children.Add(control);
		}

		// Avalonia GridSplitter hardcodes neighbors in it's OnAttachedToVisualTree
		// Reattach them whenever we change neighbors
		private void ReattachSplitters()
		{
			foreach (var gridSplitter in gridSplitters)
			{
				Children.Remove(gridSplitter);
			}
			gridSplitters.Clear();

			Debug.Assert(gridItems.Count * 2 == RowDefinitions.Count);
			int index = 0;
			//int prevStretch = -1; // todo: we can figure out splitters vs spacers automatically via fill property
			foreach (var gridItem in gridItems)
			{
				if (index > 0)
					AddGridSplitter(index);
				index++;
				// separator
				RowDefinition rowDefinition = RowDefinitions[index];
				if (gridItem.fill)
					//rowDefinition.Height = new GridLength(1000);
					RowDefinitions[index].Height = new GridLength(1, GridUnitType.Star);
				else
					RowDefinitions[index].Height = GridLength.Auto;

				SetRow(gridItem.Control, index);
				//Children.Insert(index, gridSplitter.Value);
				//Children.Add(index, gridSplitter.Value);
				index++;
			}
		}

		private void AddSeparatorRowDefinition(int index)
		{
			RowDefinition rowDefinition = new RowDefinition();
			rowDefinition.Height = GridLength.Auto;
			RowDefinitions.Insert(index, rowDefinition);
		}

		private void AddRowSplitter(int index)
		{
			/*if (Children.Count <= 1)
				return;

			RowDefinition rowDefinition = new RowDefinition();
			rowDefinition.Height = new GridLength(6);
			RowDefinitions.Insert(index, rowDefinition);*/

			GridSplitter gridSplitter = new GridSplitter()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Background = Brushes.Black,
				//ShowsPreview = true,
				//HorizontalAlignment.Stretch,
				//VerticalAlignment = VerticalAlignment.Center,
				Height = 6,
			};
			gridSplitters.Add(gridSplitter);
			//gridSplitter.DragCompleted += verticalGridSplitter_DragCompleted;
			SetRow(gridSplitter, index);
			Children.Insert(index, gridSplitter);
		}

		private void AddGridSplitter(int index)
		{
			/*RowDefinition rowDefinition = new RowDefinition();
			rowDefinition.Height = new GridLength(6);
			RowDefinitions.Insert(index, rowDefinition);*/

			GridSplitter gridSplitter = new GridSplitter()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Background = Brushes.Black,
				//ShowsPreview = true,
				//HorizontalAlignment.Stretch,
				//VerticalAlignment = VerticalAlignment.Center,
				Height = 6,
			};
			gridSplitters.Add(gridSplitter);
			//gridSplitter.DragCompleted += verticalGridSplitter_DragCompleted;
			SetRow(gridSplitter, index);
			//Children.Insert(index, gridSplitter);
			Children.Add(gridSplitter);
		}

		private void AddRowSpacer(int index)
		{
			//if (Children.Count <= 1)
			//	return;

			/*RowDefinition rowDefinition = new RowDefinition();
			rowDefinition.Height = new GridLength(5);
			RowDefinitions.Add(rowDefinition);*/

			Border border = new Border
			{
				//Width = 100,
				Height = 6,
				[Grid.RowProperty] = index
			};
			// Add a dummy panel so the children count equals the rowdefinition count, otherwise we need to track which rowdefinitions belong to which control
			//Bounds border = new Bounds();
			//SetRow(border, index);
			Children.Add(border);
		}

		private void RemoveControls(Dictionary<object, Control> controls)
		{
			HashSet<Control> hashedControls = new HashSet<Control>(); // one line linq?
			foreach (var pair in gridControls)
			{
				hashedControls.Add(pair.Value);
			}
			// Remove any children not in use anymore
			foreach (var oldChild in controls)
			{
				if (hashedControls.Contains(oldChild.Value))
					continue;

				RowDefinitions.RemoveAt(RowDefinitions.Count - 1);
				RowDefinitions.RemoveAt(RowDefinitions.Count - 1);

				Children.Remove(oldChild.Value);

				IDisposable iDisposable = oldChild.Value as IDisposable;
				if (iDisposable != null)
					iDisposable.Dispose();
			}
		}

		private List<Control> prevOrderedControls;
		private void AddControls(Dictionary<object, Control> oldControls, List<Control> orderedControls)
		{
			//RowDefinitions.Clear();
			gridItems.Clear();
			int newIndex = 1;
			foreach (Control control in orderedControls)
			{
				Item item = new Item()
				{
					Control = control,
					fill = true,
				};
				gridItems.Add(item);

				bool fill = !(control is TabNotes); // don't show for notes, needs to be configurable
				if (!Children.Contains(control))
				{
					// Add a new control
					InsertControl(control, fill, newIndex);
				}
				newIndex += 2; // leave spot for splitters
			}
			prevOrderedControls = orderedControls;
		}

		// Only used for child controls right now
		public void SetControls(Dictionary<object, Control> newControls, List<Control> orderedControls)
		{
			//if (prevOrderedControls != null && orderedControls.SequenceEqual(prevOrderedControls))
			//	return;

			Dictionary<object, Control> oldControls = gridControls;

			// don't clear old controls so we invalidate container as little as possible when we resize the remaining

			this.gridControls = newControls;

			BeginInit();

			RemoveControls(oldControls);
			AddControls(oldControls, orderedControls);

			ReattachSplitters();

			EndInit();

			// Add all child controls to the view
			InvalidateMeasure();
			InvalidateArrange();
			//UpdateSelectedTabInstances();
		}

		internal void Clear()
		{
			foreach (object obj in Children)
			{
				if (obj is IDisposable disposable)
					disposable.Dispose(); // does Children.Clear() already handle this?
			}
			RowDefinitions.Clear();
			Children.Clear();
		}
	}
}
