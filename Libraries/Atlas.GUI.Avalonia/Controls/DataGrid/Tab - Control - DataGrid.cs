using Atlas.Core;
using Atlas.Extensions;
using Atlas.GUI.Avalonia.View;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;


namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlDataGrid : Grid, IDisposable, ITabSelector, ILayoutable
	{
		private static int MaxColumnWidth = 600;

		private TabModel tabModel;
		private TabInstance tabInstance;
		public TabDataSettings tabDataSettings;
		public IList iList;
		private Type elementType;

		public DataGrid dataGrid;
		public TextBox textBoxSearch;

		//private HashSet<int> pinnedItems = new HashSet<int>();
		private DataGridCollectionView collectionView;
		private Dictionary<string, DataGridColumn> columnObjects = new Dictionary<string, DataGridColumn>();
		private Dictionary<DataGridColumn, string> columnNames = new Dictionary<DataGridColumn, string>();
		private List<PropertyInfo> columnProperties = new List<PropertyInfo>(); // makes filtering faster, could change other Dictionaries strings to PropertyInfo

		public event EventHandler<EventArgs> OnSelectionChanged;

		public bool autoSelectFirst = true;
		private bool autoSelectNew = true;
		private bool autoGenerateColumns = true;

		private int disableSaving = 0; // enables saving if > 0
		private int isAutoSelecting = 0; // enables saving if > 0

		private DispatcherTimer dispatcherTimer;  // delays auto selection to throttle updates
		private object autoSelectItem = null;

		public bool AutoLoad { get; internal set; }

		private Filter filter;

		private Stopwatch stopwatch = new Stopwatch();

		private TabControlDataGrid()
		{
			this.Initialize();
		}

		public TabControlDataGrid(TabInstance tabInstance, IList iList, bool autoGenerateColumns, TabDataSettings tabDataSettings = null)
		{
			this.tabInstance = tabInstance;
			this.tabModel = tabInstance.tabModel;
			this.AutoLoad = tabModel.AutoLoad;
			this.iList = iList;
			this.autoGenerateColumns = autoGenerateColumns;
			this.tabDataSettings = tabDataSettings ?? new TabDataSettings();
			Debug.Assert(iList != null);
			ColumnDefinitions = new ColumnDefinitions("*");
			RowDefinitions = new RowDefinitions("Auto,*");
			Initialize();
		}

		public override string ToString()
		{
			return tabModel.Name;
		}

		// Before MaxWidth applies
		private bool finishedLoading = false;
		protected override Size MeasureOverride(Size availableSize)
		{
			//Size testSize = LayoutHelper.ApplyLayoutConstraints(this, availableSize);
			/*{
				double width = (control.Width > 0) ? control.Width : constraints.Width;
				double height = (control.Height > 0) ? control.Height : constraints.Height;
				width = Math.Min(width, control.MaxWidth);
				width = Math.Max(width, control.MinWidth);
				height = Math.Min(height, control.MaxHeight);
				height = Math.Max(height, control.MinHeight);
				return new Size(width, height);
			}*/
			//base.Measure(availableSize);
			//dataGrid.Measure(availableSize);
			//Size desiredSize = dataGrid.DesiredSize;
			Size size = base.MeasureOverride(availableSize);
			if (!finishedLoading)
			{
				tabInstance.SetEndLoad();
				disableSaving--;
				finishedLoading = true;
				if (selectionModified)
					tabInstance.SaveTabSettings(); // selection has probably changed
			}
			return size;
		}

		// Before MaxWidth applies
		protected override Size MeasureCore(Size availableSize)
		{
			return base.MeasureCore(availableSize);
		}

		protected override void OnMeasureInvalidated()
		{
			dataGrid.InvalidateMeasure();
			base.OnMeasureInvalidated();
			if (Parent != null)
				Parent.InvalidateMeasure();
		}

		// real DesiredSize doesn't work because of HorizontalAlign = Stretch?
		public new Size DesiredSize
		{
			get
			{
				double columnWidths = GetTotalColumnWidths();
				Size desiredSize = new Size(columnWidths, 0);
				foreach (var control in Children)
				{
					Size childDesiredSize = control.DesiredSize;
					desiredSize = new Size(Math.Max(desiredSize.Width, childDesiredSize.Width), Math.Max(desiredSize.Height, childDesiredSize.Height));
				}
				return desiredSize;
			}
		}

		public double GetTotalColumnWidths()
		{
			double total = 0;
			foreach (var dataColumn in dataGrid.Columns)
				total += dataColumn.ActualWidth;
			return total;
		}

		private void Initialize()
		{
			disableSaving++;
			/*ValueToBrushConverter brushConverter = (ValueToBrushConverter)dataGrid.Resources["colorConverter"];
			brushConverter.HasChildrenBrush = (SolidColorBrush)Resources[Keys.HasChildrenBrush];

			if (tabModel.Editing == true)
			{
				dataGrid.IsReadOnly = false;
				brushConverter.Editable = true;
				brushConverter.EditableBrush = (SolidColorBrush)Resources[Keys.EditableBrush];
			}*/

			Type listType = iList.GetType();
			elementType = listType.GetElementTypeForAll();

			InitializeControls();

			INotifyCollectionChanged iNotifyCollectionChanged = iList as INotifyCollectionChanged;
			if (AutoLoad && iNotifyCollectionChanged != null)
			{
				// DataGrid must exist before adding this
				iNotifyCollectionChanged.CollectionChanged += INotifyCollectionChanged_CollectionChanged;

				// Invoking was happening at bad times in the data binding
				if (dispatcherTimer == null)
				{
					dispatcherTimer = new DispatcherTimer();
					dispatcherTimer.Tick += DispatcherTimer_Tick;
					dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
					dispatcherTimer.Start();
				}
			}

			//Debug.Assert(dataGrid.Columns.Count > 0); // make sure something is databindable, not all lists have a property, add a ListToString wrapper around ToString()?
		}

		private void InitializeControls()
		{
			//this.Background = new SolidColorBrush(Theme.BackgroundColor);
			this.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.VerticalAlignment = VerticalAlignment.Stretch;
			this.Focusable = true;
			//this.BorderThickness = new Thickness(0);

			MaxWidth = 4000;
			MaxHeight = 4000;
			AddDataGrid();

			textBoxSearch = new TextBox();
			textBoxSearch.IsVisible = false;
			//textBoxSearch.TextInput += TextBoxSearch_TextInput; // doesn't work
			textBoxSearch.KeyDown += TextBoxSearch_KeyDown;
			textBoxSearch.KeyUp += TextBoxSearch_KeyUp;

			/*Grid containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("*"),
				RowDefinitions = new RowDefinitions("Auto,*"), // textBoxSearch, dataGrid
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
				//MaxWidth = 4000,
			};

			containerGrid.Children.Add(textBoxSearch);
			containerGrid.Children.Add(dataGrid);*/
			//containerGrid.Children.Add(scrollViewer);

			LoadSettings();

			//Grid.SetRow(dataGrid, 0);
			//Children.Add(containerGrid);
			//Content = containerGrid;
			Children.Add(textBoxSearch);
		}

		private void AddDataGrid()
		{
			dataGrid = new DataGrid()
			{
				SelectionMode = DataGridSelectionMode.Extended, // No MultiSelect support :( (use right click for copy/paste)

				CanUserResizeColumns = true,
				CanUserReorderColumns = true,
				CanUserSortColumns = true,

				//Foreground = new SolidColorBrush(Colors.White),
				HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black),
				VerticalGridLinesBrush = new SolidColorBrush(Colors.Black),

				Background = new SolidColorBrush(Colors.White),
				RowBackground = new SolidColorBrush(Colors.White),
				//BorderBrush = new SolidColorBrush(Colors.Black),
				AlternatingRowBackground = new SolidColorBrush(Colors.White),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch, // doesn't work
				HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, // todo: can't get working
				//HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, // Use scrollviewer instead for now 
				//BorderThickness = new Thickness(0), // DataGrid bug, setting this breaks the background OnFocus, but fixes the extra border
				//Padding = new Thickness(0),
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1),
				IsReadOnly = !tabModel.Editing,
				GridLinesVisibility = DataGridGridLinesVisibility.All,
				MaxWidth = 4000,
				MaxHeight = this.MaxHeight,
				[Grid.RowProperty] = 1,
			};
			// Add a style for selected & focused here?
			var styles = dataGrid.Styles;

			//dataGrid.AutoGenerateColumns = true;
			if (autoGenerateColumns)
			{
				AddMethodsAsColumns();
				AddPropertiesAsColumns();
			}

			collectionView = new DataGridCollectionView(iList);
			dataGrid.Items = collectionView;
			dataGrid.SelectedItem = null;
			dataGrid.InvalidateMeasure();

			dataGrid.SelectionChanged += DataGrid_SelectionChanged;

			//dataGrid.PointerPressed += DataGrid_PointerPressed; // doesn't trigger (only implemented for column headers)
			//dataGrid.PointerReleased += DataGrid_PointerReleased; // does trigger, but after selection changes
			//dataGrid.CellPointerPressed += DataGrid_CellPointerPressed; // only triggers some of the time

			dataGrid.Tapped += DataGrid_Tapped;
			dataGrid.Initialized += DataGrid_Initialized;
			dataGrid.ColumnReordered += DataGrid_ColumnReordered;
			dataGrid.PointerEnter += DataGrid_PointerEnter;
			//this.GotFocus += TabDataGrid_GotFocus;
			//this.LostFocus += TabDataGrid_LostFocus;

			//var keymap = AvaloniaLocator.Current.GetService<PlatformHotkeyConfiguration>();
			//AddContextMenu();

			Children.Add(dataGrid);
		}

		private void AddContextMenu()
		{
			var list = new AvaloniaList<object>();

			MenuItem menuItemCopy = new MenuItem() { Header = "Copy - _DataGrid" };
			menuItemCopy.Click += delegate
			{
				string text = DataGridUtils.DataGridToStringTable(dataGrid);
				if (text != null)
					((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(text);
			};
			list.Add(menuItemCopy);

			//list.Add(new Separator());

			ContextMenu contextMenu = new ContextMenu();
			contextMenu.Items = list;

			dataGrid.ContextMenu = contextMenu;
		}

		public IList Items
		{
			get
			{
				return iList;
			}
			set
			{
				iList = value;
				/*if (collectionView != null && iList is ICollection)
				{
					var collection = (ICollection)iList;
					collectionView.DeferRefresh();
					collection.Clear();

					collectionView.Refresh();
				}
				else*/
				{
					collectionView = new DataGridCollectionView(iList);
					dataGrid.Items = collectionView;
				}
				//dataGrid.SelectedItem = null;
				//dataGrid.InvalidateMeasure();
				//dataGrid.InvalidateArrange();
			}
		}

		// Add column stretch property instead?
		private void DataGrid_PointerEnter(object sender, PointerEventArgs e)
		{
			dataGrid.PointerEnter -= DataGrid_PointerEnter; // todo? still need to Dispose
			foreach (DataGridColumn column in dataGrid.Columns)
			{
				// don't let columns autosize later
				if (column.ActualWidth >= column.MaxWidth)
					column.Width = new DataGridLength(column.ActualWidth);
				column.MaxWidth = 2000;
			}
		}

		private void DispatcherTimer_Tick(object sender, EventArgs e)
		{
			if (autoSelectItem != null)
			{
				disableSaving++;
				SelectedItem = autoSelectItem;
				disableSaving--;
				autoSelectItem = null;
			}
		}

		private bool selectionModified = false;

		private void INotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				// Group up any new items after the 1st one
				//if (SelectedRows.Count == 0 || (dataGrid.SelectedCells.Count == 1 && dataGrid.CurrentCell.Item == dataGrid.Items[dataGrid.Items.Count - 1]))
				// autoSelectNew not exposed
				if (autoSelectFirst && autoSelectNew && (textBoxSearch.Text == null || textBoxSearch.Text.Length == 0))
				{
					//CancellationTokenSource tokenSource = new CancellationTokenSource();
					//this.Dispatcher.Invoke(() => SelectedItem = e.NewItems[0], System.Windows.Threading.DispatcherPriority.SystemIdle, tokenSource.Token, TimeSpan.FromSeconds(1));

					// don't update the selection too often or we'll slow things down
					if (!stopwatch.IsRunning || stopwatch.ElapsedMilliseconds > 1000)
					{
						//disableSaving++;
						isAutoSelecting++;
						// change to dispatch here?
						autoSelectItem = null;
						selectionModified = true;
						//SelectedItem = e.NewItems[0];
						SelectedItem = iList[iList.Count - 1];
						//disableSaving--;
						isAutoSelecting--;
						stopwatch.Reset();
						stopwatch.Start();
						//collectionView.Refresh();
					}
					else
					{
						autoSelectItem = iList[iList.Count - 1];
					}
				}
				// causing Invalid thread issues when removing items, remove completely?
				dataGrid.InvalidateArrange(); // not resizing when adding new item, not needed?
				dataGrid.InvalidateMeasure(); // not resizing when adding new item, not needed?
			}
		}

		private void DataGrid_Tapped(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
		}

		private void DataGrid_ColumnReordered(object sender, DataGridColumnEventArgs e)
		{
			SortedDictionary<int, string> orderedColumns = new SortedDictionary<int, string>();
			foreach (DataGridColumn column in dataGrid.Columns)
				orderedColumns[column.DisplayIndex] = columnNames[column];

			tabDataSettings.ColumnNameOrder.Clear();
			tabDataSettings.ColumnNameOrder.AddRange(orderedColumns.Values);

			tabInstance.SaveTabSettings();
		}

		private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Bookmark bookmark = null;
			if (disableSaving == 0)
			{
				bookmark = tabInstance.CreateNavigatorBookmark();
			}

			UpdateSelection();

			if (disableSaving == 0)
			{
				if (isAutoSelecting == 0)
					autoSelectNew = (dataGrid.SelectedItems.Count == 0);
				tabInstance.SaveTabSettings(); // selection has probably changed
			}
			if (bookmark != null)
				bookmark.Changed = String.Join(",", tabDataSettings.SelectedRows);
		}

		private void DataGrid_Initialized(object sender, EventArgs e)
		{
			//enableSaving = true;
		}

		private DataGridRow GetControlRow(object obj, int depth)
		{
			Control control = obj as Control;
			if (control == null)
				return null;
			DataGridRow row = control as DataGridRow;
			if (row != null)
				return row;
			if (depth == 0)
				return null;
			return GetControlRow(control.Parent, depth - 1);
		}

		private bool unselectOnRelease = false;
		private void DataGrid_CellPointerPressed(object sender, DataGridCellPointerPressedEventArgs e)
		{
			DataGridRow row = e.Row;

			//if (dataGridCell.Column is DataGridCheckBoxColumn)
			//	return;
			unselectOnRelease = false;
			if (row != null && dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count == 1)
			{
				if (dataGrid.SelectedItems.Contains(row.DataContext))
				{
					unselectOnRelease = true;
					//dataGrid.SelectedItems.Clear();
					//e.PointerPressedEventArgs.Handled = true;
				}
			}
		}

		// happens too late to deselect
		private void DataGrid_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e)
		{
			if (unselectOnRelease)
			{
				unselectOnRelease = false;
				dataGrid.SelectedItems.Clear();
				e.Handled = true;
			}
		}

		// unselect cell if already selected, this never gets triggered
		private void DataGrid_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e)
		{
			DataGridRow row = GetControlRow(e.Source, 3);

			//if (dataGridCell.Column is DataGridCheckBoxColumn)
			//	return;

			if (row != null && dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count == 1)
			{
				if (dataGrid.SelectedItems.Contains(row.DataContext))
				{
					dataGrid.SelectedItems.Clear();
					e.Handled = true;
				}
			}
		}

		private void TextBoxSearch_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				textBoxSearch.IsVisible = false;
				textBoxSearch.Text = "";
				FilterText = "";
				tabInstance.SaveTabSettings();
			}
		}

		private void TextBoxSearch_KeyUp(object sender, global::Avalonia.Input.KeyEventArgs e)
		{
			FilterText = textBoxSearch.Text;
			AutoSelect();
			if (disableSaving == 0)
				tabInstance.SaveTabSettings();
		}

		private void AddMethodsAsColumns()
		{
			List<TabDataSettings.MethodColumn> methodColumns = TabDataSettings.GetButtonMethods(elementType);

			foreach (TabDataSettings.MethodColumn methodColumn in methodColumns)
			{
				AddButtonColumn(methodColumn);
			}
		}

		private void AddPropertiesAsColumns()
		{
			columnObjects = new Dictionary<string, DataGridColumn>();
			columnNames = new Dictionary<DataGridColumn, string>();
			columnProperties = new List<PropertyInfo>();

			List<TabDataSettings.PropertyColumn> propertyColumns = tabDataSettings.GetPropertiesAsColumns(elementType);
			if (propertyColumns.Count == 0)
				return;

			if (iList is INamedItemCollection itemCollection && itemCollection.ColumnName != null)
			{
				propertyColumns[0].label = itemCollection.ColumnName;
			}

			foreach (TabDataSettings.PropertyColumn propertyColumn in propertyColumns)
			{
				AddColumn(propertyColumn.label, propertyColumn.propertyInfo);
			}
		}

		public void AddColumn(string label, string propertyName)
		{
			PropertyInfo propertyInfo = elementType.GetProperty(propertyName);
			AddColumn(label, propertyInfo);
		}

		public void AddColumn(string label, PropertyInfo propertyInfo)
		{
			bool propertyEditable = (propertyInfo.GetCustomAttribute(typeof(EditingAttribute)) != null);
			ColumnMaxWidthAttribute attributeColumnMaxWidth = propertyInfo.GetCustomAttribute<ColumnMaxWidthAttribute>();
			bool isReadOnly = (tabModel.Editing == false || propertyEditable == false || !propertyInfo.CanWrite);

			//DataGridBoundColumn column;
			/*if (tabModel.Editing == false)
			{
			  //DataGridTemplateColumn templateColumn = new DataGridTemplateColumn();
			  FormattedTemplateColumn templateColumn = new FormattedTemplateColumn();
			  templateColumn.Header = label;
			  templateColumn.MaxWidth = 500;
			  dataGrid.Columns.Add(templateColumn);
			  return;
			}
			else*/
			{
				/*if (propertyInfo.PropertyType == typeof(bool))
				{
					FormattedCheckBoxColumn checkBoxColumn = new FormattedCheckBoxColumn(propertyInfo);
					if (!isReadOnly)
						checkBoxColumn.OnModified += Item_OnModified;
					column = checkBoxColumn;
				}
				else
					column = new FormattedTextColumn(propertyInfo);*/
			}


			//AvaloniaProperty avaloniaProperty = new AvaloniaProperty(propertyInfo.Name, propertyInfo.PropertyType, propertyInfo.DeclaringType, );

			//MethodInfo methodInfo = typeof(AvaloniaProperty).GetMethod("DirectProperty", new Type[] { propertyInfo.DeclaringType,  });
			//MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(propertyInfo.PropertyType);
			//textBoxCell.Binding = (IIndirectBinding<T>)genericMethodInfo.Invoke(null, new object[] { propertyInfo.Name });

			//object obj = Activator.CreateInstance(type, true);
			//textBoxCell.Binding = Binding.Property<string>((obj) => BindingProperty(obj));
			//actions.Add(new TaskAction("Add 5 Items", new Action(() => AddItems(5)), false)); // Foreground task so we can modify collection
			// public static IndirectBinding<TValue> Property<T, TValue>(Expression<Func<T, TValue>> propertyExpression);
			//textBoxCell.Binding = Binding.Property<string>(propertyInfo.Name); // TextBoxCell requires a string cast, but property value might not be castable to a string (like a class)

			//AvaloniaProperty avaloniaProperty = AvaloniaProperty.DirectProperty<propertyInfo.DeclaringType, propertyInfo.PropertyType> ()
			
			DataGridBoundColumn column;
			/*if (tabModel.Editing == false)
			{
				//DataGridTemplateColumn templateColumn = new DataGridTemplateColumn();
				FormattedTemplateColumn templateColumn = new FormattedTemplateColumn();
				templateColumn.Header = label;
				templateColumn.MaxWidth = 500;
				dataGrid.Columns.Add(templateColumn);
				return;
			}
			else*/
			{
				if (propertyInfo.PropertyType == typeof(bool))
				{
					DataGridCheckBoxColumn checkBoxColumn = new DataGridCheckBoxColumn();
					//checkBoxColumn.PropertyChanged
					//if (!isReadOnly)
					//	checkBoxColumn.OnModified += Item_OnModified;
					column = checkBoxColumn;
					column.Binding = new Binding(propertyInfo.Name);
				}
				else
				{
					//if (isReadOnly)
						column = new DataGridPropertyTextColumn(dataGrid, propertyInfo, isReadOnly);
					//else
					//	column = new DataGridTextColumn();
				}
			}
			//DataGridTextColumn column = new DataGridTextColumn();
			column.Header = label;
			column.IsReadOnly = isReadOnly;
			//column.Bind(avaloniaProperty, iList);
			//column.Width = new DataGridLength(200);// new DataGridLength(1, DataGridLengthUnitType.Star);
			column.MaxWidth = attributeColumnMaxWidth != null ? attributeColumnMaxWidth.MaxWidth : MaxColumnWidth;
			//column.HeaderCell.AreSeparatorsVisible = true;
			//column.HeaderCell.SeparatorBrush = new SolidColorBrush(Colors.Black); // Header Cell styles aren't implemented yet :(

			/*Binding binding = new Binding(propertyInfo.Name);
			if (column.IsReadOnly)
			{
				if (typeof(INotifyPropertyChanged).IsAssignableFrom(elementType))
					binding.Mode = BindingMode.OneWay; // leaks memory without INotifyPropertyChanged
				else
					binding.Mode = BindingMode.OneTime;
			}
			else
			{
				if (typeof(INotifyPropertyChanged).IsAssignableFrom(elementType))
					binding.Mode = BindingMode.TwoWay;
				else
					binding.Mode = BindingMode.OneTime;
				binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			}
			column.Binding = binding;*/

			dataGrid.Columns.Add(column);
			columnObjects[propertyInfo.Name] = column;
			columnNames[column] = propertyInfo.Name;
			columnProperties.Add(propertyInfo);
		}

		public void AddButtonColumn(TabDataSettings.MethodColumn methodColumn)
		{
			/*DataGridTemplateColumn column = new DataGridTemplateColumn();
			//column.CellTemplate.

			//column.Binding = new Binding(methodName);
			//column.Sortable = true;

			column.Header = label;
			dataGrid.Columns.Add(column);*/

			// databound
			//DataGridCheckBoxColumn checkBoxColumn = new DataGridCheckBoxColumn()
			DataGridButtonColumn column = new DataGridButtonColumn(methodColumn.methodInfo, methodColumn.label);
			//column.Header = methodColumn.methodInfo.Name;
			dataGrid.Columns.Add(column);
		}

		public void LoadSettings()
		{
			if (tabInstance.project.projectSettings.AutoLoad)
			{
				SortSavedColumn();
				if (tabDataSettings.Filter != null && tabDataSettings.Filter.Length > 0)
				{
					textBoxSearch.Text = tabDataSettings.Filter;
					FilterText = textBoxSearch.Text; // change to databinding?
					textBoxSearch.IsVisible = true;
				}
				else
				{
					textBoxSearch.Text = "";
					//FilterText = textBoxSearch.Text;
					textBoxSearch.IsVisible = false;
				}
				if (!SelectSavedItems()) // sorting must happen before this
					AutoSelect();
				//UpdateSelection(); // datagrid not fully loaded yet
			}
			OnSelectionChanged?.Invoke(this, null);
		}

		public List<object> GetMatchingRowObjects()
		{
			List<object> rowObjects = new List<object>();
			if (tabDataSettings.SelectedRows.Count == 0)
				return rowObjects;

			Dictionary<string, object> keys = new Dictionary<string, object>(); // todo: change to unordered?
			foreach (object listItem in collectionView) // collectionView takes filters into account
			{
				if (listItem == null)
					continue;
				string id = listItem.ObjectToUniqueString();
				if (id != null)
					keys[id] = listItem;
			}
			foreach (SelectedRow selectedRow in tabDataSettings.SelectedRows)
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
					if (selectedRow.rowIndex < 0 || selectedRow.rowIndex >= iList.Count) // some items might be filtered or have changed
						continue;
					listItem = iList[selectedRow.rowIndex];
				}
				if (tabInstance.IsOwnerObject(listItem.GetInnerValue())) // stops self referencing loops
					continue;

				/*if (item.pinned)
				{
					pinnedItems.Add(rowIndex);
				}*/
				rowObjects.Add(listItem);
			}

			return rowObjects;
		}

		public bool SelectSavedItems()
		{
			if (tabDataSettings.SelectionType == SelectionType.None)
				return false;

			if (tabDataSettings.SelectedRows.Count == 0)
				return true; // clear too?

			List<object> matchingItems = GetMatchingRowObjects();
			if (matchingItems.Count > 0)
			{
				SelectedItems = matchingItems;
				return true;
			}
			return false;
		}

		private object GetAutoSelectValue()
		{
			object firstValidObject = null;
			foreach (object obj in collectionView)
			{
				object value = obj;
				if (value == null)
					continue;
				value = value.GetInnerValue();
				if (value == null)
					continue;

				//ListItem listItem = obj as ListItem;
				//if (listItem != null)
				if (obj is ListItem listItem)
				{
					if (listItem.autoLoad == false)
						continue;
				}
				if (obj is ListMember listMember)
				{
					if (listMember.autoLoad == false)
						continue;
				}

				if (value is TabView tabView)
				{
					if (tabView.tabModel.AutoLoad == false)
						continue;
				}
				if (firstValidObject == null)
					firstValidObject = obj;

				Type type = value.GetType();
				if (TabModel.ObjectHasChildren(value) && type.IsEnum == false)
				{
					// make sure there's something present
					if (typeof(ICollection).IsAssignableFrom(type))
					{
						if (((ICollection)value).Count == 0)
							continue;
					}
					/*else if (typeof(IEnumerable).IsAssignableFrom(type))
					  {
						  if (!((IEnumerable)value).GetEnumerator().MoveNext())
							  continue;
					  }*/

					if (tabInstance.IsOwnerObject(obj.GetInnerValue())) // stops self referencing loops
						return null;

					SelectedItem = obj;
					break;
				}
			}
			return firstValidObject;
		}

		private void AutoSelect()
		{
			if (autoSelectFirst == false)
				return;

			object firstValidObject = GetAutoSelectValue();
			if (firstValidObject != null && dataGrid.SelectedItems.Count == 0)
				SelectedItem = firstValidObject;
			//SaveSelectedItems();
			if (firstValidObject != null)
				UpdateSelection();
		}

		public IList SelectedItems
		{
			get
			{
				return dataGrid.SelectedItems;
			}
			set
			{
				var dataGridSelectedItems = dataGrid.SelectedItems;
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
				disableSaving++;
				// datagrid has a bug and doesn't reselect cleared records correctly
				// Could try only removing removed items, and adding new items, need to check SelectedItems order is correct after
				dataGrid.SelectedItems.Clear();
				dataGrid.SelectedItem = null; // need both of these
				//foreach (object obj in dataGrid.SelectedItems)
				// remove all items so the we have to worry about this order changing?
				//while (dataGrid.SelectedItems.Count > 0)
				//dataGrid.SelectedItems.RemoveAt(0);
				foreach (object obj in value)
					dataGrid.SelectedItems.Add(obj);
				//dataGrid.Render(); //Can't get data grid to flush this correctly, see DataGrid.FlushSelectionChanged()
				dataGrid.InvalidateVisual(); // required for autoselection to work
				//Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);
				//dataGrid.Flush(); //Can't get data grid to flush this correctly,
				//if (value.Count > 0)
				//	dataGrid.ScrollIntoView(value[0], null);
				disableSaving--;
			}
			/*get
			{
				SortedDictionary<int, object> orderedRows = new SortedDictionary<int, object>();

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

				HashSet<object> idxSelected = new HashSet<object>();
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
			get
			{
				return dataGrid.SelectedIndex;
			}
			set
			{
				dataGrid.SelectedIndex = value;
			}
		}

		public object SelectedItem
		{
			get
			{
				return dataGrid.SelectedItem;
			}
			set
			{
				// don't reselect if already selected				
				if (dataGrid.SelectedItems.Count != 1 || dataGrid.SelectedItems[0] != value)
				{
					dataGrid.SelectedItems.Clear();
					dataGrid.SelectedItem = null; // need both of these
					if (value != null)
						//dataGrid.SelectedItems.Add(value);
						dataGrid.SelectedItem = value;
				}
				if (value != null && dataGrid.IsEffectivelyVisible)
				{
					try
					{
						//if (collectionView.Contains(value))
							dataGrid.ScrollIntoView(value, dataGrid.CurrentColumn);
					}
					catch (Exception e)
					{
						// {System.ArgumentOutOfRangeException: Specified argument was out of the range of valid values.
						//Parameter name: index
						//   at Avalonia.Collections.DataGridCollectionView.GetItemAt(Int32 index) in D:\a\1\s\src\Avalonia.Controls.DataGrid\Collections\DataGridCollectionView.cs:line 1957
					}
				}
			}
		}

		private void UpdateSelection()
		{
			//SelectPinnedItems();
			tabDataSettings.SelectedRows = SelectedRows;
			tabDataSettings.SelectionType = SelectionType.User; // todo: place earlier with more accurate type

			OnSelectionChanged?.Invoke(this, null);

			tabInstance.UpdateNavigator();
		}

		public HashSet<SelectedRow> SelectedRows
		{
			get
			{
				// todo: cell selection not supported yet
				HashSet<SelectedRow> selectedRows = new HashSet<SelectedRow>();
				/*Dictionary<object, List<DataGridCellInfo>> orderedRows = new Dictionary<object, List<DataGridCellInfo>>();
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
					SelectedRow selectedItem = new SelectedRow();
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
					SelectedRow selectedItem = new SelectedRow();
					selectedItem.label = obj.ObjectToUniqueString();
					//selectedItem.index = dataGrid.Items.IndexOf(obj);
					if (selectedItem.label == type.FullName)
					{
						selectedItem.label = null;
					}
					tabDataConfiguration.selectedRows.Add(selectedItem);
				}*/
				foreach (object obj in dataGrid.SelectedItems)
				{
					if (obj == null)
						continue;
					Type type = obj.GetType();
					SelectedRow selectedRow = new SelectedRow();
					selectedRow.label = obj.ObjectToUniqueString();
					selectedRow.rowIndex = iList.IndexOf(obj);
					if (selectedRow.label == type.FullName)
					{
						selectedRow.label = null;
					}
					// Fill in the DataKey/DataValue pair if found
					var keyProperties = type.GetPropertiesWithAttribute<DataKeyAttribute>();
					var keyFields = type.GetFieldsWithAttribute<DataKeyAttribute>();
					if (keyProperties.Count > 0)
					{
						selectedRow.dataKey = keyProperties[0].GetValue(obj).ToString();
					}
					else if (keyFields.Count > 0)
					{
						selectedRow.dataKey = keyFields[0].GetValue(obj).ToString();
					}

					if (selectedRow.dataKey != null)
					{
						var valueProperties = type.GetPropertiesWithAttribute<DataValueAttribute>();
						var valueFields = type.GetFieldsWithAttribute<DataValueAttribute>();
						if (valueProperties.Count > 0)
						{
							selectedRow.dataValue = valueProperties[0].GetValue(obj);
						}
						else if (valueFields.Count > 0)
						{
							selectedRow.dataValue = valueFields[0].GetValue(obj);
						}
						else
						{
							selectedRow.dataValue = obj;
						}
					}

					selectedRows.Add(selectedRow);
				}
				return selectedRows;
			}
		}

		private string FilterText
		{
			set
			{
				tabDataSettings.Filter = value;
				filter = new Filter(value);

				if (filter.filterExpressions.Count > 0)
				{
					if (filter.depth > 0)
					{
						// create a new collection because this one might have multiple lists
						TabModel tabModel = TabModel.Create(this.tabModel.Name, iList);
						TabBookmark bookmarkNode = tabModel.FindMatches(filter, filter.depth);
						tabInstance.filterBookmarkNode = bookmarkNode;
						collectionView.Filter = FilterPredicate;
						tabInstance.SelectBookmark(bookmarkNode);
					}
					else
					{
						collectionView.Filter = FilterPredicate;
						collectionView.Refresh();
					}
				}
				else
				{
					collectionView.Filter = null;
				}
			}
		}

		private bool FilterPredicate(object obj)
		{
			if (tabInstance.filterBookmarkNode != null)
			{
				return tabInstance.filterBookmarkNode.selectedObjects.Contains(obj);
			}
			else
			{
				return filter.Matches(obj, columnProperties);
			}
		}

		private void SortSavedColumn()
		{
			//collectionView.SortDescriptions
			/*ListCollectionView listCollectionView = collectionView as ListCollectionView;
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
			}*/
		}

		public void Dispose()
		{
			if (dispatcherTimer != null)
			{
				dispatcherTimer.Stop();
				dispatcherTimer.Tick -= DispatcherTimer_Tick;
				dispatcherTimer = null;
			}
			stopwatch.Stop();

			dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
			dataGrid.PointerPressed -= DataGrid_PointerPressed;
			dataGrid.Tapped -= DataGrid_Tapped;
			dataGrid.Initialized -= DataGrid_Initialized;
			dataGrid.ColumnReordered -= DataGrid_ColumnReordered;
			dataGrid.PointerEnter -= DataGrid_PointerEnter;

			dataGrid.Items = null;

			textBoxSearch.KeyDown -= TextBoxSearch_KeyDown;
			textBoxSearch.KeyUp -= TextBoxSearch_KeyUp;

			//KeyDown -= UserControl_KeyDown;

			INotifyCollectionChanged iNotifyCollectionChanged = iList as INotifyCollectionChanged;
			if (AutoLoad && iNotifyCollectionChanged != null)
				iNotifyCollectionChanged.CollectionChanged -= INotifyCollectionChanged_CollectionChanged;

			iList = null;
			collectionView = null;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			//if (e.Key == Key.F5)
			//	SelectSavedItems();

			if (e.Modifiers == InputModifiers.Control)
			{
				if (e.Key == Key.F)
				{
					textBoxSearch.IsVisible = !textBoxSearch.IsVisible;
					if (textBoxSearch.IsVisible)
						textBoxSearch.Focus();
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
				textBoxSearch.IsVisible = false;
				textBoxSearch.Text = "";
				FilterText = "";
				tabInstance.SaveTabSettings();
			}
		}
	}
}

/* From Atlas.GUI.Wpf

private const double MaxDefaultWidth = 1500;
// Params

// 


private void Item_OnModified(object sender, EventArgs e)
{
	tabInstance.ItemModified();
}

private void dataGrid_Loaded(object sender, RoutedEventArgs e)
{
	enableSaving = false;
	if (tabInstance.tabConfiguration.SplitterDistance == null)
	{
		if (dataGrid.ActualWidth > MaxDefaultWidth)
			dataGrid.Width = MaxDefaultWidth;
	}
	if (AutoLoad)
	{
		LoadSavedSettings();
		if (tabDataConfiguration.selected.Count == 0 || (autoSelectNew && dataGrid.SelectedCells.Count == 0))
			AutoSelect();
	}

	/*foreach (var column in dataGrid.Columns)
{
  var starSize = column.ActualWidth / dataGrid.ActualWidth;
  column.Width = new DataGridLength(starSize, DataGridLengthUnitType.Star);
}*//*

	dataGrid.Loaded -= dataGrid_Loaded;
	//dataGrid.SelectionChanged += DataGrid_SelectionChanged; // doesn't catch cell selection, only row selections
	//dataGrid.CurrentCellChanged += DataGrid_CurrentCellChanged; // happens before selection changes
	dataGrid.SelectedCellsChanged += DataGrid_SelectedCellsChanged;
	dataGrid.PreviewMouseLeftButtonDown += DataGrid_PreviewMouseLeftButtonDown;
	enableSaving = true;
}

/*
public int SelectedIndex
{
set
{
  dataGrid.SelectedCells.Clear();
  if (dataGrid.Items.Count == 0) // todo: find out why the databinding falls behind, rows don't get created when there's no columns?
	  return;
  dataGrid.CurrentCell = new DataGridCellInfo(value, dataGrid.Columns[0]);
  var row = dataGrid.Rows[value];
  //row.Selected = true;
  row.Cells[0].Selected = true;
  dataGrid.FirstDisplayedScrollingRowIndex = value;
  SaveSelectedItems();
}
}*//*


/*private void EnsureVisibleRow(int rowIndex)
{
if (rowIndex >= 0 && rowIndex < dataGrid.RowCount)
{
  int countVisible = dataGrid.DisplayedRowCount(false);
  int firstVisible = dataGrid.FirstDisplayedScrollingRowIndex;
  if (rowIndex < firstVisible)
  {
	  dataGrid.FirstDisplayedScrollingRowIndex = rowIndex;
  }
  else if (rowIndex >= firstVisible + countVisible)
  {
	  dataGrid.FirstDisplayedScrollingRowIndex = rowIndex - countVisible / 2 + 1;
  }
}
}*//*

public List<DataGridCellInfo> GetMatchingCellInfos()
{
	List<DataGridCellInfo> cellInfos = new List<DataGridCellInfo>();
	Dictionary<string, object> keys = new Dictionary<string, object>(); // todo: change to unordered?
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

		/*if (item.pinned)
  {
	  pinnedItems.Add(rowIndex);
  }*//*
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
	List<DataGridCellInfo> matchingCellInfos = new List<DataGridCellInfo>();
	List<DataGridCellInfo> removedCellInfos = new List<DataGridCellInfo>();
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
  }*//*

	//ResumeLayout();
}

/*

// Travel up the list and select all the references from matching items
private void UpdateSelected(TabView tabView)
{
/*if (tabView != this)
{

}
if (Parent is TabView)
{
  ((TabView)Parent).UpdateSelected(tabView);
  foreach (int index in tabView.dataGrid.SelectedIndices)
  {
	  dataGrid.SetSelected(index, true);
  }
}*//*
}

private void SelectPinnedItems()
{
foreach (int rowIndex in pinnedItems)
{
	dataGrid.Rows[rowIndex].Selected = true;
}
}*//*

private void CopyCellData()
{
	ApplicationCommands.Copy.Execute(null, dataGrid);

	var oldData = Clipboard.GetDataObject();
	var newData = new DataObject();

	foreach (string format in oldData.GetFormats())
	{
		if (format.Equals("UnicodeText") || format.Equals("Text"))
		{
			newData.SetData(format, Regex.Replace(((String)oldData.GetData(format)), "\r\n$", ""));
		}
		else
		{
			newData.SetData(format, oldData.GetData(format));
		}
	}

	Clipboard.SetDataObject(newData);
}

private void textBoxSearch_PreviewKeyDown(object sender, KeyEventArgs e)
{
	if (e.Key == Key.Enter || e.Key == Key.Tab)
	{
		FilterText = textBoxSearch.Text;
		AutoSelect();
		if (enableSaving)
			tabInstance.SaveConfiguration();
		//e.Handled = true;
		return;
	}
}

/*private ICommand searchCommand;
public ICommand SearchCommand
{
get
{
  return searchCommand
	  ?? (searchCommand = new ActionCommand(() =>
	  {
		  FilterText = textBoxSearch.Text;
		  if (enableSaving)
			  tabInstance.SaveConfiguration();
	  }));
}
}*//*

/*
private void dataGrid_MouseDown(object sender, MouseEventArgs e)
{
if (e.Button == MouseButtons.Right)
{
  // pin or unpin a row
  DataGridView.HitTestInfo hit = dataGrid.HitTest(e.X, e.Y);
  if (hit.RowIndex >= 0)
  {
	  if (pinnedItems.Contains(hit.RowIndex))
	  {
		  pinnedItems.Remove(hit.RowIndex);
		  dataGrid.Rows[hit.RowIndex].Selected = false;
	  }
	  else
	  {
		  pinnedItems.Add(hit.RowIndex);
		  dataGrid.Rows[hit.RowIndex].Selected = true;
	  }
  }
}
}

private void dataGrid_DoubleClick(object sender, EventArgs e)
{
// get rid of all the other neighbors
/*if (call.parent != null)
{
  call.parent.dataGrid.BeginUpdate();
  List<int> selected = call.parent.dataGrid.SelectedIndices.Cast<int>().ToList();
  foreach (int index in selected)
  {
	  object listItem = call.parent.dataGrid.Items[index];
	  if (listItem != this.listItem)
		  call.parent.dataGrid.SetSelected(index, false); // probably triggering event each time
  }
  call.parent.dataGrid.EndUpdate();
}*//*
}*//*

// don't append an extra newline when copying cells, really annoying when copying a single cell value
private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
{
	if (e.Key == Key.Enter)
	{
		dataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
		e.Handled = true;
		return;
	}
	if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
	{
		if (e.Key == Key.C)
		{
			CopyCellData();
			e.Handled = true;
			return;
		}
	}
}

private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
{
	UpdateSelection();

	if (enableSaving)
	{
		autoSelectNew = (dataGrid.SelectedCells.Count == 0);
		tabInstance.SaveConfiguration(); // selection has probably changed
	}
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

// use ObjectToString() formatting instead of default
private void dataGrid_CopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
{
	var rowContents = e.ClipboardRowContent.ToList(); // create a copy before clearing
	e.ClipboardRowContent.Clear();
	foreach (var cellContent in rowContents)
	{
		object content = cellContent.Content;
		if (content != null)
		{
			Type type = content.GetType();
			if (!type.IsNumeric())
				content = content.ObjectToString();
		}

		e.ClipboardRowContent.Add(new DataGridClipboardCellContent(cellContent.Item, cellContent.Column, content));
	}
}


/*public class DynamicTemplateSelector : DataTemplateSelector
{
	public DataTemplate CheckboxTemplate { get; set; }

	public override DataTemplate DynamicTemplateSelector(object item, DependencyObject container)
	{
		MyObject obj = item as MyObject;

		if (obj != null)
		{
			// custom logic to select appropriate data template and return
		}
		else
		{
			return base.SelectTemplate(item, container);
		}
	}
}*/

/*public class ActionCommand : ICommand
{
	private readonly Action _action;

	public ActionCommand(Action action)
	{
		_action = action;
	}

	public void Execute(object parameter)
	{
		_action();
	}

	public bool CanExecute(object parameter)
	{
		return true;
	}

	public event EventHandler CanExecuteChanged;
}*/
