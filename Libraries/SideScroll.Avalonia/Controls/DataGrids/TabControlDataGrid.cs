using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SideScroll.Attributes;
using SideScroll.Avalonia.Utilities;
using SideScroll.Avalonia.View;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Tabs;
using SideScroll.Tabs.Bookmarks;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Settings;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.DataGrids;

public class TabControlDataGrid : Grid, ITabSelector, ITabItemSelector, ITabDataSelector
{
	private const int ColumnPercentBased = 150;
	private const int MaxMinColumnWidth = 200;
	private const int MaxAutoSizeMinColumnWidth = 250;

	public int MaxColumnWidth = 600;

	public TabModel TabModel;
	public TabInstance TabInstance;
	public TabDataSettings TabDataSettings { get; set; }
	public IList? List;
	private Type _elementType;

	public bool AutoGenerateColumns = true;

	public DataGrid DataGrid;
	public TabControlSearch? SearchControl;

	public DataGridCollectionView? CollectionView;

	public event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;

	private Dictionary<string, DataGridColumn> _columnObjects = [];
	private Dictionary<DataGridColumn, string> _columnNames = [];
	private List<PropertyInfo> _columnProperties = []; // makes filtering faster, could change other Dictionaries strings to PropertyInfo

	private int _disableSaving; // enables saving if > 0
	private bool _ignoreSelectionChanged;

	private readonly Stopwatch _notifyItemChangedStopwatch = new();
	private DispatcherTimer? _dispatcherTimer;  // delays auto selection to throttle updates
	private object? _autoSelectItem;
	private NotifyCollectionChangedAction? _autoSelectAction;

	private Filter? _filter;

	public IList? Items
	{
		get => List;
		set
		{
			List = value;
			/*if (collectionView != null && iList is ICollection)
			{
				var collection = (ICollection)iList;
				collectionView.DeferRefresh();
				collection.Clear();

				collectionView.Refresh();
			}
			else*/
			if (DataGrid != null)
			{
				CollectionView = new DataGridCollectionView(List);

				DataGrid.ItemsSource = CollectionView; // DataGrid autoselects on assignment :(

				if (TabModel.AutoSelectSaved == AutoSelectType.None && !TabModel.AutoSelectDefault)
					ClearSelection();
				else
					LoadSettings();

				Dispatcher.UIThread.Post(AutoSizeColumns, DispatcherPriority.Background);
			}
		}
	}

	public override string ToString() => TabModel.Name;

	public TabControlDataGrid(TabInstance tabInstance, IList iList, bool autoGenerateColumns, TabDataSettings? tabDataSettings = null, TabModel? model = null)
	{
		TabInstance = tabInstance;
		TabModel = model ?? TabInstance.Model;
		List = iList;
		AutoGenerateColumns = autoGenerateColumns;
		TabDataSettings = tabDataSettings ?? new TabDataSettings();
		Debug.Assert(iList != null);

		ColumnDefinitions = new ColumnDefinitions("*");
		RowDefinitions = new RowDefinitions("Auto,*");

		Initialize();
	}

	static TabControlDataGrid()
	{
		// DataGridRow event triggers before DataGridCell :(
		PointerPressedEvent.AddClassHandler<DataGridRow>(DataGridRow_PointerPressed, RoutingStrategies.Tunnel, true);
	}

	[MemberNotNull(nameof(_elementType), nameof(DataGrid))]
	private void Initialize()
	{
		_disableSaving++;

		Type listType = List!.GetType();
		_elementType = listType.GetElementTypeForAll()!;

		InitializeControls();
		AddListUpdatedDispatcher();

		Dispatcher.UIThread.Post(() =>
		{
			TabInstance.SetEndLoad();
			_disableSaving--;
			if (_selectionModified)
				TabInstance.SaveTabSettings(); // selection has probably changed
		}, DispatcherPriority.Background);
	}

	[MemberNotNull(nameof(DataGrid))]
	private void InitializeControls()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;

		MaxWidth = 4000;
		MaxHeight = 4000;

		Focusable = true;

		AddSearch();
		AddDataGrid();

		LoadSettings();
	}

	private void AddSearch()
	{
		SearchControl = new TabControlSearch
		{
			IsVisible = false,
		};

		SearchControl.KeyDown += SearchControl_KeyDown;
		SearchControl.KeyUp += SearchControl_KeyUp;

		Children.Add(SearchControl);
	}

	[MemberNotNull(nameof(DataGrid))]
	private void AddDataGrid()
	{
		DataGrid = new DataGrid
		{
			SelectionMode = DataGridSelectionMode.Extended, // No MultiSelect support :( (use right click for copy/paste)

			CanUserResizeColumns = true,
			CanUserReorderColumns = true,
			CanUserSortColumns = true,

			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
			//BorderThickness = new Thickness(0), // DataGrid bug, fixes the extra border below the scrollbar, but then there's no border
			//Padding = new Thickness(0),

			BorderThickness = new Thickness(0, 1),
			IsReadOnly = !TabModel.Editing,
			GridLinesVisibility = DataGridGridLinesVisibility.None,
			MaxWidth = 4000,
			MaxHeight = this.MaxHeight,
			[Grid.RowProperty] = 1,
		};

		if (AutoGenerateColumns)
			AddColumns();

		CollectionView = new DataGridCollectionView(List);
		DataGrid.ItemsSource = CollectionView;
		DataGrid.SelectedItem = null;

		DataGrid.SelectionChanged += DataGrid_SelectionChanged;

		DataGrid.CellPointerPressed += DataGrid_CellPointerPressed; // Add one click deselection
		DataGrid.ColumnReordered += DataGrid_ColumnReordered;

		DataGrid.EffectiveViewportChanged += DataGrid_EffectiveViewportChanged;

		DataGrid.AddHandler(KeyDownEvent, DataGrid_KeyDown, RoutingStrategies.Tunnel);

		Dispatcher.UIThread.Post(AutoSizeColumns, DispatcherPriority.Background);

		DataGrid.ContextMenu = new DataGridContextMenu(DataGrid);

		Children.Add(DataGrid);
	}

	private void DataGrid_KeyDown(object? sender, KeyEventArgs e)
	{
		// These keys are used for navigating in the TabViewer
		if (e.Key is Key.Left or Key.Right)
		{
			RaiseEvent(e);
		}
	}

	private void AddListUpdatedDispatcher()
	{
		if (List is INotifyCollectionChanged iNotifyCollectionChanged) // AutoLoad
		{
			// DataGrid must exist before adding this
			iNotifyCollectionChanged.CollectionChanged += INotifyCollectionChanged_CollectionChanged;

			// Invoking was happening at bad times in the data binding
			if (_dispatcherTimer == null)
			{
				_dispatcherTimer = new DispatcherTimer
				{
					Interval = TimeSpan.FromSeconds(1), // Tick event doesn't fire if set to < 1 second
				};
				_dispatcherTimer.Tick += DispatcherTimer_Tick;
				_dispatcherTimer.Start();
			}
		}
	}

	private double? _maxDesiredWidth = null;

	private void AutoSizeColumns()
	{
		// Only works with Stretch right now
		if (DataGrid == null || HorizontalAlignment != HorizontalAlignment.Stretch)
			return;

		var autoSizeColumns = DataGrid.Columns
			.Where(c => c.IsVisible)
			.Where(c => c is DataGridTextColumn or DataGridCheckBoxColumn)
			.ToList();

		// The star column widths will change as other column widths are changed
		var originalWidths = new Dictionary<DataGridColumn, DataGridLength>();
		foreach (DataGridColumn column in autoSizeColumns)
		{
			originalWidths[column] = column.Width;
			column.Width = new DataGridLength(column.ActualWidth, DataGridLengthUnitType.Auto); // remove Star sizing so columns don't interfere with each other
		}

		foreach (DataGridColumn column in autoSizeColumns)
		{
			DataGridLength originalWidth = originalWidths[column];
			double originalDesiredWidth = double.IsNaN(originalWidth.DesiredValue) ? 0 : originalWidth.DesiredValue;

			column.MaxWidth = 2000;

			if (column.MinWidth == 0)
				column.MinWidth = Math.Min(MaxMinColumnWidth, originalDesiredWidth);
			else
				column.MinWidth = Math.Max(column.MinWidth, Math.Min(100, originalDesiredWidth));

			double desiredWidth = Math.Max(column.MinWidth, originalDesiredWidth);
			if (column is DataGridPropertyTextColumn textColumn)
			{
				desiredWidth = Math.Max(desiredWidth, textColumn.MinDesiredWidth);
				desiredWidth = Math.Min(desiredWidth, textColumn.MaxDesiredWidth);
				column.MinWidth = Math.Min(column.MinWidth, textColumn.MaxDesiredWidth);

				if (textColumn.AutoSize)
				{
					column.MinWidth = Math.Max(column.MinWidth, Math.Min(MaxAutoSizeMinColumnWidth, desiredWidth));
					//column.Width = new DataGridLength(desiredWidth, DataGridLengthUnitType.Auto);
					column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto, desiredWidth, double.NaN);
					continue;
				}
				else if (desiredWidth >= textColumn.MaxDesiredWidth)
				{
					column.Width = new DataGridLength(column.ActualWidth, DataGridLengthUnitType.Star, desiredWidth, double.NaN);
				}
			}

			if (desiredWidth >= ColumnPercentBased)
			{
				// Changes ActualWidth
				column.Width = new DataGridLength(desiredWidth, DataGridLengthUnitType.Star);
			}
		}
		_maxDesiredWidth = autoSizeColumns.Sum(c => c.ActualWidth);

		//dataGrid.MinColumnWidth = 40; // doesn't do anything
		// If 1 or 2 columns, make the last column stretch
		if (DataGrid.Columns.Count == 1)
		{
			DataGrid.Columns[0].Width = new DataGridLength(DataGrid.Columns[0].ActualWidth, DataGridLengthUnitType.Star);
		}
		else if (DataGrid.Columns.Count == 2)
		{
			DataGrid.Columns[1].Width = new DataGridLength(DataGrid.Columns[1].ActualWidth, DataGridLengthUnitType.Star);
		}
	}

	protected override Size MeasureOverride(Size constraint)
	{
		Size size = base.MeasureOverride(constraint);
		if (_maxDesiredWidth != null && size.Width > _maxDesiredWidth)
		{
			size = size.WithWidth(_maxDesiredWidth.Value);
		}
		return size;
	}

	private bool _selectionModified;

	private void INotifyCollectionChanged_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (List == null) // reloading detaches list temporarily?
			return;

		if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace)
		{
			// Group up any new items after the 1st one
			if (TabModel.AutoSelectNew && SearchControl!.Text.IsNullOrEmpty())
			{
				_selectItemEnabled = true;
				object? item = e.NewItems![0];
				//object item = List[List.Count - 1];
				// don't update the selection too often or we'll slow things down
				if (!_notifyItemChangedStopwatch.IsRunning || _notifyItemChangedStopwatch.ElapsedMilliseconds > 1000)
				{
					// change to dispatch here?
					_autoSelectItem = null;
					_selectionModified = true;
					Dispatcher.UIThread.Post(() => SetSelectedItem(item, e.Action), DispatcherPriority.Background);
					_notifyItemChangedStopwatch.Reset();
					_notifyItemChangedStopwatch.Start();
				}
				else
				{
					_autoSelectItem = item;
					_autoSelectAction = e.Action;
				}
			}
			Dispatcher.UIThread.Post(AutoSizeColumns, DispatcherPriority.Background);
		}
		else if (e.Action == NotifyCollectionChangedAction.Reset) // Clear() will trigger this
		{
			// doesn't work
			//collectionView.Refresh();
		}
	}

	private void DispatcherTimer_Tick(object? sender, EventArgs e)
	{
		object? selectItem = _autoSelectItem;
		NotifyCollectionChangedAction? autoSelectAction = _autoSelectAction;
		if (selectItem == null) return;

		Dispatcher.UIThread.Post(() => SetSelectedItem(selectItem, autoSelectAction), DispatcherPriority.Background);
		_autoSelectItem = null;
		_autoSelectAction = null;
	}

	private bool _selectItemEnabled;

	private void SetSelectedItem(object? selectedItem, NotifyCollectionChangedAction? action)
	{
		if (!_selectItemEnabled) return;

		if (action != NotifyCollectionChangedAction.Replace)
		{
			_disableSaving++;
			SelectedItem = selectedItem;
			_disableSaving--;
		}
		else
		{
			SelectedItem = selectedItem;
		}
	}

	private void DataGrid_ColumnReordered(object? sender, DataGridColumnEventArgs e)
	{
		var orderedColumns = new SortedDictionary<int, string>();
		foreach (DataGridColumn column in DataGrid.Columns)
		{
			orderedColumns[column.DisplayIndex] = _columnNames[column];
		}

		TabDataSettings.ColumnNameOrder.Clear();
		TabDataSettings.ColumnNameOrder.AddRange(orderedColumns.Values);

		TabInstance.SaveTabSettings();
	}

	private void DataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		// ignore if clearing selection before setting
		if (_ignoreSelectionChanged)
			return;

		Bookmark? bookmark = null;
		if (_disableSaving == 0)
		{
			bookmark = TabInstance.CreateNavigatorBookmark();
		}

		UpdateSelection();

		if (_disableSaving == 0)
		{
			TabInstance.SaveTabSettings(); // selection has probably changed
		}
		if (bookmark != null)
		{
			bookmark.Changed = string.Join(",", TabDataSettings.SelectedRows);
		}
	}

	private static DataGridRow? GetControlRow(object? obj, int depth)
	{
		if (depth == 0)
			return null;

		return obj switch
		{
			DataGridRow row => row,
			Control control => GetControlRow(control.Parent, depth - 1),
			_ => null
		};
	}

	// Single click deselect
	private void DataGrid_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
	{
		var pointer = e.PointerPressedEventArgs.GetCurrentPoint(this);
		if (pointer.Properties.IsLeftButtonPressed &&
			e.Row != null &&
			DataGrid.SelectedItems != null &&
			DataGrid.SelectedItems.Count == 1 &&
			e.Cell.Content != null)
		{
			Type type = e.Cell.Content!.GetType();
			if (typeof(CheckBox).IsAssignableFrom(type) ||
				typeof(Button).IsAssignableFrom(type))
				return;

			if (DataGrid.SelectedItems.Contains(e.Row.DataContext))
			{
				Dispatcher.UIThread.Post(ClearSelection, DispatcherPriority.Background);
			}
		}
	}

	private static bool IsControlSelectable(IInputElement? inputElement)
	{
		if (inputElement == null)
			return false;

		Type type = inputElement.GetType();

		return
			typeof(CheckBox).IsAssignableFrom(type) ||
			typeof(Button).IsAssignableFrom(type) ||
			(inputElement is Visual visual && IsControlSelectable(visual.GetVisualParent() as IInputElement));
	}

	private static DataGrid? GetOwningDataGrid(StyledElement? control)
	{
		return control switch
		{
			null => null,
			DataGrid dataGrid => dataGrid,
			_ => GetOwningDataGrid(control.Parent)
		};
	}

	// Single click deselect (cells don't always occupy their entire contents)
	private static void DataGridRow_PointerPressed(DataGridRow row, PointerPressedEventArgs e)
	{
		PointerPoint point = e.GetCurrentPoint(row);

		if (!point.Properties.IsLeftButtonPressed)
			return;

		DataGrid? dataGrid = GetOwningDataGrid(row);
		// Can't access row.OwningGrid, so we have to do this the hard way
		if (dataGrid != null)
		{
			if (dataGrid.SelectedItems == null || dataGrid.SelectedItems.Count != 1)
				return;

			// Ignore if toggling CheckBox or clicking Button
			// ReadOnly CheckBoxes will return the Cell Grid instead of a child Border control
			IInputElement? input = row.InputHitTest(point.Position);
			if (IsControlSelectable(input))
				return;

			if (dataGrid.SelectedItems.Contains(row.DataContext))
			{
				Dispatcher.UIThread.Post(() => ClearSelection(dataGrid), DispatcherPriority.Background);
				e.Handled = true;
			}
		}
	}

	private void ClearSelection()
	{
		ClearSelection(DataGrid);
	}

	private static void ClearSelection(DataGrid dataGrid)
	{
		dataGrid.SelectedItems.Clear();
		dataGrid.SelectedItem = null;
	}

	private void ClearSearch()
	{
		if (!TabModel.ShowSearch)
		{
			SearchControl!.IsVisible = false;
		}

		SearchControl!.Text = "";
		FilterText = "";
		Focus();

		TabInstance.SaveTabSettings();
	}

	private void SearchControl_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			ClearSearch();
		}
	}

	private void SearchControl_KeyUp(object? sender, KeyEventArgs e)
	{
		FilterText = SearchControl!.Text;

		SelectDefaultItems();

		if (_disableSaving == 0)
		{
			TabInstance.SaveTabSettings();
		}
	}

	private void AddColumns()
	{
		_columnObjects = [];
		_columnNames = [];
		_columnProperties = [];

		List<TabDataSettings.MethodColumn> methodColumns = TabDataSettings.GetButtonMethods(_elementType);
		foreach (TabDataSettings.MethodColumn methodColumn in methodColumns)
		{
			AddButtonColumn(methodColumn);
		}

		List<TabDataSettings.PropertyColumn> propertyColumns = TabDataSettings.GetPropertiesAsColumns(_elementType);

		// Filter [Hide(null)]
		propertyColumns = propertyColumns
			.Where(p => p.IsVisible(List!))
			.ToList();

		if (propertyColumns.Count == 0)
			return;

		// 2 columns need headers for resizing first column?
		// For visual color separation due to HasLinks background color being too close to title
		bool showHeader = propertyColumns.Count != 1 && !typeof(IListPair).IsAssignableFrom(_elementType);
		if (List is IItemCollection itemCollection)
		{
			if (itemCollection.ColumnName is string columnName)
			{
				propertyColumns[0].Label = columnName;
			}
			showHeader = itemCollection.ShowHeader ?? showHeader;
		}

		if (!showHeader)
		{
			DataGrid.HeadersVisibility = DataGridHeadersVisibility.None;
		}

		bool styleCells = methodColumns.Count > 0 ||
			propertyColumns
			.Select(p => p.IsStyled())
			.Max();

		foreach (TabDataSettings.PropertyColumn propertyColumn in propertyColumns)
		{
			AddColumn(propertyColumn.Label, propertyColumn.PropertyInfo, styleCells);
		}
	}

	public void AddColumn(string label, string propertyName, bool styleCells = false)
	{
		PropertyInfo propertyInfo = _elementType.GetProperty(propertyName)!;
		AddColumn(label, propertyInfo, styleCells);
	}

	public void AddColumn(string label, PropertyInfo propertyInfo, bool styleCells = false)
	{
		MinWidthAttribute? attributeMinWidth = propertyInfo.GetCustomAttribute<MinWidthAttribute>();
		MaxWidthAttribute? attributeMaxWidth = propertyInfo.GetCustomAttribute<MaxWidthAttribute>();
		AutoSizeAttribute? attributeAutoSize = propertyInfo.GetCustomAttribute<AutoSizeAttribute>();

		bool isReadOnly = true;

		int maxDesiredWidth = attributeMaxWidth?.MaxWidth ?? MaxColumnWidth;
		DataGridBoundColumn column;
		/*if (tabModel.Editing == false)
		{
			//var templateColumn = new DataGridTemplateColumn();
			var templateColumn = new FormattedTemplateColumn();
			templateColumn.Header = label;
			templateColumn.MaxWidth = 500;
			dataGrid.Columns.Add(templateColumn);
			return;
		}
		else*/
		{
			if (propertyInfo.PropertyType == typeof(bool))
			{
				isReadOnly = (propertyInfo.GetCustomAttribute<EditingAttribute>() == null);
				var checkBoxColumn = new DataGridPropertyCheckBoxColumn(propertyInfo, isReadOnly)
				{
					StyleCells = styleCells,
				};
				column = checkBoxColumn;
				column.Binding = new Binding(propertyInfo.Name);
			}
			else
			{
				var textColumn = new DataGridPropertyTextColumn(DataGrid, propertyInfo, isReadOnly, maxDesiredWidth)
				{
					StyleCells = styleCells,
				};

				if (attributeMinWidth != null)
				{
					textColumn.MinDesiredWidth = attributeMinWidth.MinWidth;
				}

				if (attributeAutoSize != null)
				{
					textColumn.AutoSize = true;
				}

				textColumn.ScanItemAttributes(List!);

				column = textColumn;
			}
		}
		column.Header = label;
		column.IsReadOnly = isReadOnly;
		column.MaxWidth = attributeMaxWidth?.MaxWidth ?? MaxColumnWidth;
		if (attributeMinWidth != null)
		{
			column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto, attributeMinWidth.MinWidth, double.NaN);
		}

		DataGrid.Columns.Add(column);
		_columnObjects[propertyInfo.Name] = column;
		_columnNames[column] = propertyInfo.Name;
		_columnProperties.Add(propertyInfo);

		DataGrid.Sorting += DataGrid_Sorting;
	}

	private void DataGrid_Sorting(object? sender, DataGridColumnEventArgs e)
	{
		Dispatcher.UIThread.Post(AutoSizeColumns, DispatcherPriority.Background);
	}

	public void AddButtonColumn(string methodName)
	{
		MethodInfo methodInfo = _elementType.GetMethod(methodName)!;
		AddButtonColumn(new TabDataSettings.MethodColumn(methodInfo));
	}

	public void AddButtonColumn(TabDataSettings.MethodColumn methodColumn)
	{
		var column = new DataGridButtonColumn(methodColumn.MethodInfo, methodColumn.Label);
		DataGrid.Columns.Add(column);
		DataGrid.IsReadOnly = false; // Requires double clicking otherwise
		_columnNames[column] = methodColumn.Label;
	}

	public void LoadSettings()
	{
		if (List == null) return;

		if (TabInstance.Project.UserSettings.AutoLoad)
		{
			// SortSavedColumn(); // Not supported yet
			LoadSearch();

			if (!SelectSavedItems()) // sorting must happen before this
				SelectDefaultItems();

			//UpdateSelection(); // datagrid not fully loaded yet

			OnSelectionChanged?.Invoke(this, new TabSelectionChangedEventArgs());
		}
	}

	private void LoadSearch()
	{
		if (SearchControl == null)
			return;

		if (TabModel.ShowSearch || (TabDataSettings.Filter != null && TabDataSettings.Filter.Length > 0))
		{
			SearchControl.Text = TabDataSettings.Filter;
			FilterText = SearchControl.Text; // change to databinding?
			SearchControl.IsVisible = true;
		}
		else
		{
			SearchControl.Text = "";
			//FilterText = textBoxSearch.Text;
			SearchControl.IsVisible = false;
		}
	}

	public List<object> GetMatchingRowObjects()
	{
		var rowObjects = new List<object>();
		if (TabDataSettings.SelectedRows.Count == 0)
			return rowObjects;

		TabItemCollection tabItemCollection = new(List!, CollectionView);

		List<object> matchingObjects = tabItemCollection.GetSelectedObjects(TabDataSettings.SelectedRows);

		foreach (object matchingObject in matchingObjects)
		{
			if (TabDataSettings.SelectionType != SelectionType.User &&
				TabInstance.IsOwnerObject(matchingObject.GetInnerValue())) // stops self referencing loops
				continue;

			rowObjects.Add(matchingObject);
		}

		if (TabInstance.TabBookmark?.Bookmark?.Imported == true && rowObjects.Count != TabDataSettings.SelectedRows.Count)
		{
			// Replace with call and CallDebugLogger?
			Debug.Print("Failed to find all bookmarked rows, Selected: [" + string.Join(", ", TabDataSettings.SelectedRows) + "], Found: [" + string.Join(", ", rowObjects) + "]");
			Debug.Print("Possible Causes: Object ToString() changed. Try adding [DataKey] to object field/property");
		}

		return rowObjects;
	}

	public bool SelectSavedItems()
	{
		if (TabDataSettings.SelectionType == SelectionType.None)
			return false;

		if (TabModel.AutoSelectSaved == AutoSelectType.None)
			return false;

		if (List == null || List.Count == 0)
			return false;

		// Select new log items automatically
		if (TabInstance.TaskInstance.TaskStatus == TaskStatus.Running)
		{
			SelectedItem = List[List.Count - 1];
			return true;
		}

		if (TabDataSettings.SelectedRows.Count == 0)
			return TabModel.AutoSelectSaved == AutoSelectType.Any; // clear too?

		List<object> matchingItems = GetMatchingRowObjects();
		if (matchingItems.Count > 0)
		{
			SelectedItems = matchingItems;
			return true;
		}
		return false;
	}

	private object? GetDefaultSelectedItem()
	{
		string defaultItemText;
		if (List is IItemCollection itemCollection && itemCollection.DefaultSelectedItem is object defaultItem)
		{
			defaultItemText = defaultItem.ToUniqueString()!;
		}
		else if (TabModel.DefaultSelectedItem is object defaultModelItem)
		{
			defaultItemText = defaultModelItem.ToUniqueString()!;
		}
		else
		{
			return null;
		}

		foreach (object obj in CollectionView!)
		{
			if (obj.ToUniqueString() == defaultItemText)
				return obj;
		}
		return null;
	}

	private object? GetAutoSelectValue()
	{
		object? firstValidObject = null;
		foreach (object obj in CollectionView!)
		{
			object? value = obj;
			if (value == null)
				continue;

			value = value.GetInnerValue();
			if (value == null)
				continue;

			if (obj is ListItem listItem)
			{
				if (listItem.AutoLoad == false)
					continue;
			}

			if (obj is ListMember listMember)
			{
				if (listMember.AutoLoad == false)
					continue;
			}

			if (value is TabView tabView)
			{
				if (tabView.Model.AutoSelectSaved == AutoSelectType.None && !tabView.Model.AutoSelectDefault) continue;
			}

			firstValidObject ??= obj;

			Type type = value.GetType();
			if (TabUtils.ObjectHasLinks(value, true) && type.IsEnum == false)
			{
				// make sure there's something present
				if (value is ICollection collection && collection.Count == 0)
					continue;

				if (TabInstance.IsOwnerObject(obj.GetInnerValue())) // stops self referencing loops
					return null;

				SelectedItem = obj;
				break;
			}
		}
		return firstValidObject;
	}

	private void SelectDefaultItems()
	{
		if (!TabModel.AutoSelectDefault)
			return;

		object? firstValidObject = GetDefaultSelectedItem() ?? GetAutoSelectValue();
		if (firstValidObject != null && DataGrid.SelectedItems.Count == 0)
			SelectedItem = firstValidObject;

		//SaveSelectedItems();

		if (firstValidObject != null)
			UpdateSelection();
	}

	public IList SelectedItems
	{
		get => DataGrid.SelectedItems;
		set
		{
			var dataGridSelectedItems = DataGrid.SelectedItems;
			if (value.Count == dataGridSelectedItems.Count)
			{
				bool match = true;
				for (int i = 0; i < value.Count; i++)
				{
					if (value[i] != dataGridSelectedItems[i])
						match = false;
				}
				if (match)
					return;
			}

			_disableSaving++;

			// datagrid has a bug and doesn't reselect cleared records correctly
			// Could try only removing removed items, and adding new items, need to check SelectedItems order is correct after
			if (value.Count > 0)
				_ignoreSelectionChanged = true;

			DataGrid.SelectedItems.Clear();
			DataGrid.SelectedItem = null; // need both of these

			_ignoreSelectionChanged = false;

			foreach (object obj in value)
			{
				if (List!.Contains(obj))
					DataGrid.SelectedItems.Add(obj);
			}
			DataGrid.InvalidateVisual(); // required for autoselection to work
			if (value.Count > 0)
			{
				Dispatcher.UIThread.Post(() => ScrollIntoView(value[0]), DispatcherPriority.ApplicationIdle);
			}
			_disableSaving--;
		}
		// save for future cell selection support?
		/*get
		{
			var orderedRows = new SortedDictionary<int, object>();

			foreach (DataGridCellInfo cellInfo in dataGrid.SelectedCells)
			{
				orderedRows[dataGrid.Items.IndexOf(cellInfo.Item)] = cellInfo.Item;
			}
			return orderedRows.Values.ToList();
		}
		set
		{
			foreach (object obj in value)
				dataGrid.SelectedItems.Add(obj);

			var idxSelected = new HashSet<object>();
			foreach (object obj in value)
				idxSelected.Add(obj);

			dataGrid.SelectedItems.Clear();
			foreach (DataGridViewRow row in dataGrid.Rows)
			{
				if (idxSelected.Contains(row.DataBoundItem))
					row.Cells[0].Selected = true;
			}
		}*/
	}

	public int SelectedIndex
	{
		get => DataGrid.SelectedIndex;
		set => DataGrid.SelectedIndex = value;
	}

	public object? SelectedItem
	{
		get => DataGrid.SelectedItem;
		set
		{
			//autoSelectItem = null;
			if (value == null)
				_selectItemEnabled = false;

			// don't reselect if already selected				
			if (DataGrid.SelectedItems.Count != 1 || DataGrid.SelectedItems[0] != value)
			{
				DataGrid.SelectedItems.Clear();
				DataGrid.SelectedItem = null; // need both of these
				if (value != null)
					DataGrid.SelectedItem = value;
			}
			ScrollIntoView(value);
		}
	}

	private void ScrollIntoView(object? value)
	{
		// DataGrid.IsInitialized is unreliable and can still be false while showing
		if (value == null || DataGrid == null || !DataGrid.IsEffectivelyVisible)
			return;

		try
		{
			//if (collectionView.Contains(value))
			DataGrid.ScrollIntoView(value, DataGrid.CurrentColumn);
		}
		catch (Exception)
		{
			// {System.ArgumentOutOfRangeException: Specified argument was out of the range of valid values.
			//Parameter name: index
			//   at Avalonia.Collections.DataGridCollectionView.GetItemAt(Int32 index) in D:\a\1\s\src\Avalonia.Controls.DataGrid\Collections\DataGridCollectionView.cs:line 1957
		}
	}

	private void UpdateSelection(bool recreate = false)
	{
		TabDataSettings.SelectedRows = SelectedRows;
		TabDataSettings.SelectionType = SelectionType.User; // todo: place earlier with more accurate type

		OnSelectionChanged?.Invoke(this, new TabSelectionChangedEventArgs(recreate));
	}

	public HashSet<SelectedRow> SelectedRows
	{
		get
		{
			// todo: cell selection not supported yet
			var selectedRows = new HashSet<SelectedRow>();
			/*var orderedRows = new Dictionary<object, List<DataGridCellInfo>>();
			foreach (DataGridCellInfo cellInfo in dataGrid.SelectedCells)
			{
				if (cellInfo.Column == null)
					continue; // this shouldn't happen, but it does (WPF only?)
				if (!orderedRows.ContainsKey(cellInfo.Item))
					orderedRows[cellInfo.Item] = new List<DataGridCellInfo>();
				orderedRows[cellInfo.Item].Add(cellInfo);
			}
			foreach (var selectedRow in orderedRows)
			{
				object obj = selectedRow.Key;
				List<DataGridCellInfo> cellsInfos = selectedRow.Value;
				Type type = obj.GetType();
				var selectedItem = new SelectedRow();
				selectedItem.label = obj.ObjectToUniqueString();
				selectedItem.index = dataGrid.Items.IndexOf(obj);
				if (selectedItem.label == type.FullName)
				{
					selectedItem.label = null;
				}
				foreach (DataGridCellInfo cellInfo in cellsInfos)
				{
					selectedItem.columns.Add(columnNames[cellInfo.Column]);
				}
				//selectedItem.pinned = pinnedItems.Contains(row.Index);
				tabDataConfiguration.selected.Add(selectedItem);
			}*/
			/*object obj = dataGrid.SelectedItem;
			if (obj != null)
			{
				Type type = obj.GetType();
				var selectedItem = new SelectedRow();
				selectedItem.label = obj.ObjectToUniqueString();
				//selectedItem.index = dataGrid.Items.IndexOf(obj);
				if (selectedItem.label == type.FullName)
				{
					selectedItem.label = null;
				}
				tabDataConfiguration.selectedRows.Add(selectedItem);
			}*/

			try
			{
				foreach (object obj in DataGrid.SelectedItems)
				{
					if (obj == null)
						continue;

					var selectedRow = new SelectedRow(obj)
					{
						RowIndex = List!.IndexOf(obj),
					};
					selectedRows.Add(selectedRow);
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			return selectedRows;
		}
	}

	private string? FilterText
	{
		set
		{
			TabDataSettings.Filter = value;
			_filter = new Filter(value);
			if (TabModel.SearchFilter != null)
			{
				TabModel.SearchFilter.Filter = _filter;
			}

			if (_filter.FilterExpressions.Count > 0)
			{
				if (_filter.Depth > 0)
				{
					// create a new collection because this one might have multiple lists
					TabModel tabModel = TabModel.Create(this.TabModel.Name, List!)!;
					TabBookmark bookmarkNode = tabModel.FindMatches(_filter, _filter.Depth);
					TabInstance.FilterBookmarkNode = bookmarkNode;
					CollectionView!.Filter = FilterPredicate;
					TabInstance.SelectBookmark(bookmarkNode);
				}
				else
				{
					CollectionView!.Filter = FilterPredicate;
					CollectionView.Refresh();
				}
			}
			else
			{
				CollectionView!.Filter = null;
			}

			if (TabModel.SearchFilter != null && TabInstance.IsLoaded)
			{
				// Update Child Controls in case children use same search filter
				UpdateSelection(true);
			}
		}
	}

	private bool FilterPredicate(object obj)
	{
		if (TabInstance.FilterBookmarkNode != null)
		{
			return TabInstance.FilterBookmarkNode.SelectedObjects.Contains(obj);
		}
		else
		{
			return _filter!.Matches(obj, _columnProperties);
		}
	}

	// Not possible with current DataGrid yet?
	/*private void SortSavedColumn()
	{
		//collectionView.SortDescriptions
		ListCollectionView listCollectionView = collectionView as ListCollectionView;
		if (listCollectionView != null && tabDataSettings.SortColumnName != null)
		{
			DataGridColumn matchingColumn = null;
			foreach (DataGridColumn column in dataGrid.Columns)
			{
				string propertyName = columnNames[column];
				if (propertyName == tabDataSettings.SortColumnName)
				{
					column.SortDirection = tabDataSettings.SortDirection;
					matchingColumn = column;
					break;
				}
			}
			if (matchingColumn != null) // property might have been renamed/removed
			{
				PropertyInfo propertyInfo = listElementType.GetProperty(tabDataSettings.SortColumnName);
				Debug.Assert(propertyInfo != null);
				listCollectionView.CustomSort = new DataGridSortComparer(propertyInfo, tabDataSettings.SortDirection);
			}
		}
	}*/

	public void Dispose()
	{
		if (_dispatcherTimer != null)
		{
			_dispatcherTimer.Stop();
			_dispatcherTimer.Tick -= DispatcherTimer_Tick;
			_dispatcherTimer = null;
		}
		_notifyItemChangedStopwatch.Stop();

		if (DataGrid != null)
		{
			DataGrid.SelectionChanged -= DataGrid_SelectionChanged;
			DataGrid.CellPointerPressed -= DataGrid_CellPointerPressed;
			DataGrid.ColumnReordered -= DataGrid_ColumnReordered;
			DataGrid.EffectiveViewportChanged -= DataGrid_EffectiveViewportChanged;

			DataGrid.ItemsSource = null;

			if (DataGrid.ContextMenu is IDisposable contextMenu)
			{
				contextMenu.Dispose();
			}
			DataGrid.ContextMenu = null;
		}

		if (SearchControl != null)
		{
			SearchControl.KeyDown -= SearchControl_KeyDown;
			SearchControl.KeyUp -= SearchControl_KeyUp;
			SearchControl = null;
		}

		if (List is INotifyCollectionChanged iNotifyCollectionChanged) // as AutoLoad
			iNotifyCollectionChanged.CollectionChanged -= INotifyCollectionChanged_CollectionChanged;

		List = null;
		CollectionView = null;

		Children.Clear();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (e.KeyModifiers == KeyModifiers.Control)
		{
			if (e.Key == Key.F)
			{
				SearchControl!.IsVisible = !SearchControl.IsVisible;
				if (SearchControl.IsVisible)
					SearchControl.Focus();
				return;
			}

			/*if (keyData == Keys.F2)
			{
				//dataGrid.SelectedCells;
				dataGrid.BeginEdit(true);
				return true;
			}
			*/
		}
		else if (e.Key == Key.Escape)
		{
			ClearSearch();
		}
	}

	private void DataGrid_EffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
	{
		UpdateVisible();

		//Dispatcher.UIThread.Post(UpdateVisible, DispatcherPriority.ContextIdle);
	}

	// Hide control when offscreen
	private void UpdateVisible()
	{
		if (DataGrid == null || !IsLoaded) return;

		bool visible = AvaloniaUtils.IsControlVisible(this);
		if (visible != DataGrid.IsVisible)
		{
			if (!visible && DataGrid.IsFocused)
			{
				// Key events will be lost if DataGrid is invisible and still has focus
				Focus();
			}
			DataGrid.IsVisible = visible;
			//DataGrid.InvalidateArrange();
		}
	}
}

/* From SideScroll.Wpf

public List<DataGridCellInfo> GetMatchingCellInfos()
{
	var cellInfos = new List<DataGridCellInfo>();
	var keys = new Dictionary<string, object>(); // todo: change to unordered?
	foreach (object listItem in iList)
	{
		if (listItem == null)
			continue;

		string id = listItem.ObjectToUniqueString();
		if (id != null)
			keys[id] = listItem;
	}
	foreach (SelectedRow selectedRow in tabDataConfiguration.selected)
	{
		object listItem;
		if (selectedRow.obj != null)
		{
			listItem = selectedRow.obj;
		}
		else if (selectedRow.label != null)
		{
			if (!keys.TryGetValue(selectedRow.label, out listItem))
				continue;
		}
		else
		{
			if (selectedRow.index < 0 || selectedRow.index >= dataGrid.Items.Count) // some items might be filtered or have changed
				continue;
			listItem = dataGrid.Items[selectedRow.index];
		}
		if (tabInstance.IsOwnerObject(listItem.GetInnerValue())) // stops self referencing loops
			continue;


		if (selectedRow.columns.Count == 0)
		{
			// select all columns
			foreach (DataGridColumn dataGridColumn in columnObjects.Values)
			{
				DataGridCellInfo cellInfo = new DataGridCellInfo(listItem, dataGridColumn);
				cellInfos.Add(cellInfo);
				break;// break for bug
			}
		}
		else
		{
			foreach (var columnName in selectedRow.columns)
			{
				DataGridColumn dataGridColumn;
				if (columnObjects.TryGetValue(columnName, out dataGridColumn)) // column might have been renamed/removed
				{
					DataGridCellInfo cellInfo = new DataGridCellInfo(listItem, dataGridColumn);
					cellInfos.Add(cellInfo);
					break; // avoid DataGrid bug when selecting 2 cells in the same row
				}
			}
		}
	}

	return cellInfos;
}

// don't clear cells and then reselect if you can help it (although we could disable updates while updating these)
public void SelectSavedItems()
{
	List<DataGridCellInfo> cellInfos = GetMatchingCellInfos();

	//SuspendLayout();
	//ClearSelection();
	var matchingCellInfos = new List<DataGridCellInfo>();
	var removedCellInfos = new List<DataGridCellInfo>();
	foreach (DataGridCellInfo cellInfo in dataGrid.SelectedCells)
	{
		if (cellInfo.Column == null)
		{
			dataGrid.SelectedCells.Clear();
			break;
			//continue;
		}
		DataGridCellInfo? matchingCellInfo = null;
		foreach (DataGridCellInfo newCellInfo in cellInfos)
		{
			if (cellInfo.Item == newCellInfo.Item && cellInfo.Column == newCellInfo.Column)
			{
				matchingCellInfo = newCellInfo;
				//cellInfos.Remove(cellInfo);
				matchingCellInfos.Add(newCellInfo);
				break;
			}
		}
		if (matchingCellInfo == null)
		{
			removedCellInfos.Add(cellInfo);
		}
	}

	foreach (DataGridCellInfo cellInfo in matchingCellInfos)
	{
		cellInfos.Remove(cellInfo);
	}

	foreach (DataGridCellInfo cellInfo in removedCellInfos)
	{
		dataGrid.SelectedCells.Remove(cellInfo);
	}

	foreach (DataGridCellInfo newCellInfo in cellInfos)
	{
		dataGrid.SelectedCells.Add(newCellInfo);
	}

	if (dataGrid.SelectedCells.Count > 0)
	{
		DataGridCellInfo cellInfo = dataGrid.SelectedCells[0];
		dataGrid.CurrentCell = cellInfo;
		dataGrid.ScrollIntoView(cellInfo);
	}

	/*dataGrid.SelectedCells.Clear();

if (cellInfos.Count > 0)
{
  foreach (DataGridCellInfo cellInfo in cellInfos)
  {
	  dataGrid.SelectedCells.Add(cellInfo);
  }

  dataGrid.CurrentCell = cellInfos[0];
  dataGrid.ScrollIntoView(cellInfos[0].Item);
  }

	//ResumeLayout();
}

private void dataGrid_Sorting(object sender, DataGridSortingEventArgs e)
{
	// could possibly use "DataGridSort.Comparer.Set(Column, comparer)" instead
	ListCollectionView listCollectionView = collectionView as ListCollectionView;
	if (e.Column != null && e.Column.CanUserSort == true && listCollectionView != null)
	{
		if (e.Column.SortDirection == ListSortDirection.Ascending)
		{
			e.Column.SortDirection = ListSortDirection.Descending;
		}
		else
		{
			e.Column.SortDirection = ListSortDirection.Ascending;
		}
		tabDataConfiguration.SortColumn = columnNames[e.Column];
		tabDataConfiguration.SortDirection = (ListSortDirection)e.Column.SortDirection;
		dataGrid.SelectedCellsChanged -= DataGrid_SelectedCellsChanged;
		SortSavedColumn();
		dataGrid.SelectedCellsChanged += DataGrid_SelectedCellsChanged;
		//SelectSavedLabels(); // sorting selects different item
		tabInstance.SaveConfiguration();
		//Dispatcher.Invoke(SelectSavedLabels);
		//CancellationTokenSource tokenSource = new CancellationTokenSource();
		//this.Dispatcher.Invoke(() => SelectSavedLabels(), DispatcherPriority.SystemIdle, tokenSource.Token, TimeSpan.FromSeconds(1));
		e.Handled = true;
	}
}

private void dataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
{
	object obj = e.Row.DataContext;
	if ((obj is ListProperty) && !e.Column.IsReadOnly)
	{
		if (!((ListProperty)obj).Editable)
			e.Cancel = true;
	}
}
*/
