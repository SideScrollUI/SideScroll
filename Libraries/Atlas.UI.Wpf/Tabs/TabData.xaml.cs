using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Atlas.UI.Wpf
{
	public partial class TabData : UserControl, IDisposable
	{
		private const double MaxDefaultWidth = 1500;
		// Params
		private TabModel tabModel;
		private TabInstance tabInstance;
		public TabDataSettings tabDataSettings;
		private IList iList;

		// 
		//private HashSet<int> pinnedItems = new HashSet<int>();
		private ICollectionView collectionView;
		private Dictionary<string, DataGridColumn> columnObjects = new Dictionary<string, DataGridColumn>();
		private Dictionary<DataGridColumn, string> columnNames = new Dictionary<DataGridColumn, string>();
		private List<PropertyInfo> columnProperties = new List<PropertyInfo>(); // makes filtering faster, could change other Dictionaries strings to PropertyInfo

		public event EventHandler<EventArgs> OnSelectionChanged;
		public bool autoSelectFirst = true;
		private bool autoSelectNew = true;
		private int disableSaving = 0; // enables saving if 0
		private DispatcherTimer dispatcherTimer;

		//public bool AutoLoad { get; internal set; }

		private Filter filter;
		private Type listElementType;

		private Stopwatch stopwatch = new Stopwatch();
		private object autoSelectItem = null;

		public TabData(TabInstance tabInstance, IList iList, TabDataSettings tabDataSettings = null)
		{
			this.tabInstance = tabInstance;
			this.tabModel = tabInstance.Model;
			this.tabDataSettings = tabDataSettings ?? new TabDataSettings();
			this.iList = iList;
			Debug.Assert(tabDataSettings != null);
			InitializeComponent();
		}

		public override string ToString()
		{
			return tabModel.Name;
		}

		public void Initialize()
		{
			this.Focusable = true;
			this.GotFocus += TabData_GotFocus;
			this.LostFocus += TabData_LostFocus;

			ValueToBrushConverter brushConverter = (ValueToBrushConverter)dataGrid.Resources["colorConverter"];
			brushConverter.HasChildrenBrush = (SolidColorBrush)Resources[Keys.HasChildrenBrush];

			if (tabModel.Editing == true)
			{
				dataGrid.IsReadOnly = false;
				brushConverter.Editable = true;
				brushConverter.EditableBrush = (SolidColorBrush)Resources[Keys.EditableBrush];
			}

			Type listType = iList.GetType();
			listElementType = listType.GenericTypeArguments[0];

			collectionView = CollectionViewSource.GetDefaultView(iList); // This fails if an object's ToString() has a -> in it
			dataGrid.ItemsSource = collectionView;

			INotifyCollectionChanged iNotifyCollectionChanged = iList as INotifyCollectionChanged;
			if (iNotifyCollectionChanged != null)
			{
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

			AddPropertiesAsColumns();

			//Debug.Assert(dataGrid.Columns.Count > 0); // make sure something is databindable, not all lists have a property, add a ListToString wrapper around ToString()?
		}

		private void TabData_GotFocus(object sender, RoutedEventArgs e)
		{
			Background = (SolidColorBrush)Resources[Keys.BackgroundFocusedBrush];
		}

		private void TabData_LostFocus(object sender, RoutedEventArgs e)
		{
			Background = (SolidColorBrush)Resources[Keys.BackgroundBrush];
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

		private void AddPropertiesAsColumns()
		{
			columnObjects = new Dictionary<string, DataGridColumn>();
			columnNames = new Dictionary<DataGridColumn, string>();
			columnProperties = new List<PropertyInfo>();

			List<TabDataSettings.PropertyColumn> propertyColumns = tabDataSettings.GetPropertiesAsColumns(listElementType);

			foreach (TabDataSettings.PropertyColumn propertyColumn in propertyColumns)
			{
				AddColumn(propertyColumn.label, propertyColumn.propertyInfo);
			}
		}

		private void AddColumn(string label, PropertyInfo propertyInfo)
		{
			bool propertyEditable = (propertyInfo.GetCustomAttribute(typeof(EditingAttribute)) != null);
			bool isReadOnly = (tabModel.Editing == false || propertyEditable == false || !propertyInfo.CanWrite);

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
					FormattedCheckBoxColumn checkBoxColumn = new FormattedCheckBoxColumn(propertyInfo);
					if (!isReadOnly)
						checkBoxColumn.OnModified += Item_OnModified;
					column = checkBoxColumn;
				}
				else
					column = new FormattedTextColumn(propertyInfo);
			}
			column.IsReadOnly = isReadOnly;
			Binding binding = new Binding(propertyInfo.Name);
			if (column.IsReadOnly)
			{
				if (typeof(INotifyPropertyChanged).IsAssignableFrom(listElementType))
					binding.Mode = BindingMode.OneWay; // leaks memory without INotifyPropertyChanged
				else
					binding.Mode = BindingMode.OneTime;
			}
			else
			{
				if (typeof(INotifyPropertyChanged).IsAssignableFrom(listElementType))
					binding.Mode = BindingMode.TwoWay;
				else
					binding.Mode = BindingMode.OneTime;
				binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			}
			column.Binding = binding;
			column.CanUserSort = true;
			column.Header = label;
			column.MaxWidth = 500;

			dataGrid.Columns.Add(column);
			columnObjects[propertyInfo.Name] = column;
			columnNames[column] = propertyInfo.Name;
			columnProperties.Add(propertyInfo);
		}

		private void Item_OnModified(object sender, EventArgs e)
		{
			tabInstance.ItemModified();
		}

		private void dataGrid_Loaded(object sender, RoutedEventArgs e)
		{
			disableSaving++;
			if (tabInstance.tabViewSettings.SplitterDistance == null)
			{
				if (dataGrid.ActualWidth > MaxDefaultWidth)
					dataGrid.Width = MaxDefaultWidth;
			}
			if (tabModel.AutoLoad)
			{
				LoadSettings();
				if (autoSelectFirst)
				{
					if (tabDataSettings.SelectedRows.Count == 0 || (autoSelectNew && dataGrid.SelectedCells.Count == 0))
						SelectFirstValue();
				}
			}

			/*foreach (var column in dataGrid.Columns)
			{
				var starSize = column.ActualWidth / dataGrid.ActualWidth;
				column.Width = new DataGridLength(starSize, DataGridLengthUnitType.Star);
			}*/

			dataGrid.Loaded -= dataGrid_Loaded;
			//dataGrid.SelectionChanged += DataGrid_SelectionChanged; // doesn't catch cell selection, only row selections
			//dataGrid.CurrentCellChanged += DataGrid_CurrentCellChanged; // happens before selection changes
			dataGrid.SelectedCellsChanged += DataGrid_SelectedCellsChanged;
			dataGrid.PreviewMouseLeftButtonDown += DataGrid_PreviewMouseLeftButtonDown;
			disableSaving--;
		}

		public void LoadSettings()
		{
			if (dataGrid.IsLoaded == false)
				return;
			disableSaving++;
			SortSavedColumn();
			if (tabDataSettings.Filter != null && tabDataSettings.Filter.Length > 0)
			{
				textBoxSearch.Text = tabDataSettings.Filter;
				FilterText = textBoxSearch.Text; // change to databinding?
				textBoxSearch.Visibility = Visibility.Visible;
			}
			/*else
			{
				textBoxSearch.Text = "";
				//FilterText = textBoxSearch.Text;
				textBoxSearch.Visibility = Visibility.Hidden;
			}*/
			SelectSavedItems(); // sorting must happen before this
			//UpdateSelection(); // datagrid not fully loaded yet
			OnSelectionChanged?.Invoke(this, null);
			disableSaving--;
		}

		// AutoSelect any new item that get added to the list, with a throttle
		private void INotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				//if (SelectedRows.Count == 0 || (dataGrid.SelectedCells.Count == 1 && dataGrid.CurrentCell.Item == dataGrid.Items[dataGrid.Items.Count - 1]))
				if (autoSelectFirst && autoSelectNew && textBoxSearch.Text.Length == 0)
				{
					//CancellationTokenSource tokenSource = new CancellationTokenSource();
					//this.Dispatcher.Invoke(() => SelectedItem = e.NewItems[0], System.Windows.Threading.DispatcherPriority.SystemIdle, tokenSource.Token, TimeSpan.FromSeconds(1));
					
					// don't update the selection too often or we'll slow things down
					if (!stopwatch.IsRunning || stopwatch.ElapsedMilliseconds > 1000)
					{
						disableSaving++;
						SelectedItem = iList[iList.Count - 1];
						disableSaving--;
						autoSelectItem = null;
						stopwatch.Reset();
						stopwatch.Start();
					}
					else
					{
						autoSelectItem = iList[iList.Count - 1];
					}
				}
			}
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
		}*/

		public object SelectedItem
		{
			set
			{
				if (dataGrid.SelectedCells.Count != 1 || dataGrid.SelectedCells[0].Item != value)
				{
					dataGrid.SelectedCells.Clear();
					dataGrid.SelectedItem = value;
				}
				if (value != null)
					dataGrid.ScrollIntoView(value);
			}
		}
		
		// Returns list of items from selected rows
		public IList SelectedItems
		{
			get
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
				/*
				HashSet<object> idxSelected = new HashSet<object>();
				foreach (object obj in value)
					idxSelected.Add(obj);

				dataGrid.SelectedItems.Clear();
				foreach (DataGridViewRow row in dataGrid.Rows)
				{
					if (idxSelected.Contains(row.DataBoundItem))
						row.Cells[0].Selected = true;
				}*/
			}
		}

		public HashSet<SelectedRow> SelectedRows
		{
			get
			{
				HashSet<SelectedRow> selectedRows = new HashSet<SelectedRow>();
				var orderedRowCells = new Dictionary<object, List<DataGridCellInfo>>();
				foreach (DataGridCellInfo cellInfo in dataGrid.SelectedCells)
				{
					if (cellInfo.Column == null)
						continue; // this shouldn't happen, but it does
					if (!orderedRowCells.ContainsKey(cellInfo.Item))
						orderedRowCells[cellInfo.Item] = new List<DataGridCellInfo>();
					orderedRowCells[cellInfo.Item].Add(cellInfo);
				}
				foreach (var rowCells in orderedRowCells)
				{
					object obj = rowCells.Key;
					List<DataGridCellInfo> cellsInfos = rowCells.Value;
					Type type = obj.GetType();
					SelectedRow selectedItem = new SelectedRow();
					selectedItem.label = obj.ToUniqueString();
					selectedItem.rowIndex = dataGrid.Items.IndexOf(obj);
					if (selectedItem.label == type.FullName)
					{
						selectedItem.label = null;
					}
					foreach (DataGridCellInfo cellInfo in cellsInfos)
					{
						selectedItem.selectedColumns.Add(columnNames[cellInfo.Column]);
					}
					//selectedItem.pinned = pinnedItems.Contains(row.Index);
					selectedRows.Add(selectedItem);
				}
				return selectedRows;
			}
		}

		private void SelectFirstValue()
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

				Atlas.Tabs.ListItem listItem = obj as Atlas.Tabs.ListItem;
				if (listItem != null)
				{
					if (listItem.autoLoad == false)
						continue;
				}

				if (value is TabView)
				{
					TabView tabView = (TabView)value;
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
						continue;

					SelectedItem = obj;
					break;
				}
			}
			if (firstValidObject != null && dataGrid.SelectedItems.Count == 0)
				SelectedItem = firstValidObject;
			//SaveSelectedItems();
			if (dataGrid.SelectedItems.Count > 0)
				UpdateSelection();
		}

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
		}*/

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
				}*/

			//ResumeLayout();
		}

		public List<DataGridCellInfo> GetMatchingCellInfos()
		{
			var cellInfos = new List<DataGridCellInfo>();
			var keys = new Dictionary<string, object>(); // todo: change to unordered?
			foreach (object listItem in iList)
			{
				if (listItem == null)
					continue;
				string id = listItem.ToUniqueString();
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
					if (selectedRow.rowIndex < 0 || selectedRow.rowIndex >= dataGrid.Items.Count) // some items might be filtered or have changed
						continue;
					listItem = dataGrid.Items[selectedRow.rowIndex];
				}
				if (tabInstance.IsOwnerObject(listItem.GetInnerValue())) // stops self referencing loops
					continue;

				/*if (item.pinned)
				{
					pinnedItems.Add(rowIndex);
				}*/
				if (selectedRow.selectedColumns.Count == 0)
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
					foreach (var columnName in selectedRow.selectedColumns)
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
		}*/

		private void UpdateSelection()
		{
			//SelectPinnedItems();
			tabDataSettings.SelectedRows = SelectedRows;

			OnSelectionChanged?.Invoke(this, null);

			tabInstance.UpdateNavigator();
		}

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
		}

		private void textBoxSearch_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Tab)
			{
				FilterText = textBoxSearch.Text;
				SelectFirstValue();
				if (disableSaving == 0)
					tabInstance.SaveTabSettings();
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
		}*/

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
		}*/

		// unselect cell if already selected
		private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DependencyObject dependency = (DependencyObject)e.OriginalSource;

			DataGridRow dataGridRow = ItemsControl.ContainerFromElement(dataGrid, dependency) as DataGridRow;
			if (dataGridRow == null)
				return;

			FrameworkElement frameworkElement = dependency as FrameworkElement;
			if (frameworkElement == null)
				return;

			DataGridCell dataGridCell = ((FrameworkElement)dependency).Parent as DataGridCell;
			if (dataGridCell == null)
				return;

			if (dataGridCell.Column is DataGridCheckBoxColumn)
				return;

			if (dataGrid.SelectedCells != null && dataGrid.SelectedCells.Count == 1)
			{
				//DataGridRow dataGridRow = dataGrid.ItemContainerGenerator.ContainerFromItem(dataGrid.SelectedItem) as DataGridRow;
				if (dataGridCell.IsSelected)
				{
					dataGridCell.IsSelected = false;
					e.Handled = true;
				}
			}
		}

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
			Bookmark bookmark = null;
			if (disableSaving == 0)
			{
				bookmark = tabInstance.CreateNavigatorBookmark();
			}

			UpdateSelection();

			if (disableSaving == 0)
			{
				autoSelectNew = (dataGrid.SelectedCells.Count == 0); // start autoselecting again if the user unselects everything
				tabInstance.SaveTabSettings(); // selection has probably changed
			}
			if (bookmark != null)
				bookmark.Changed = String.Join(",", tabDataSettings.SelectedRows);
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
				tabDataSettings.SortColumnName = columnNames[e.Column];
				tabDataSettings.SortDirection = (ListSortDirection)e.Column.SortDirection;
				dataGrid.SelectedCellsChanged -= DataGrid_SelectedCellsChanged;
				SortSavedColumn();
				dataGrid.SelectedCellsChanged += DataGrid_SelectedCellsChanged;
				//SelectSavedLabels(); // sorting selects different item
				tabInstance.SaveTabSettings();
				//Dispatcher.Invoke(SelectSavedLabels);
				//CancellationTokenSource tokenSource = new CancellationTokenSource();
				//this.Dispatcher.Invoke(() => SelectSavedLabels(), DispatcherPriority.SystemIdle, tokenSource.Token, TimeSpan.FromSeconds(1));
				e.Handled = true;
			}
		}

		private void dataGrid_ColumnReordered(object sender, DataGridColumnEventArgs e)
		{
			SortedDictionary<int, string> orderedColumns = new SortedDictionary<int, string>();
			foreach (DataGridColumn column in dataGrid.Columns)
				orderedColumns[column.DisplayIndex] = columnNames[column];
			
			tabDataSettings.ColumnNameOrder.Clear();
			tabDataSettings.ColumnNameOrder.AddRange(orderedColumns.Values);

			tabInstance.SaveTabSettings();
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

		// use Formatted() formatting instead of default
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
						content = content.Formatted();
				}

				e.ClipboardRowContent.Add(new DataGridClipboardCellContent(cellContent.Item, cellContent.Column, content));
			}
		}

		private void UserControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F5)
				SelectSavedItems();
			if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				if (e.Key == Key.F)
				{
					if (textBoxSearch.Visibility != Visibility.Visible)
					{
						textBoxSearch.Visibility = Visibility.Visible;
						textBoxSearch.Focus();
					}
					else
					{
						textBoxSearch.Visibility = Visibility.Collapsed;
					}

					return;
				}
				/*
				if (keyData == Keys.F2)
				{
					//dataGrid.SelectedCells;
					dataGrid.BeginEdit(true);
					return true;
				}
				*/
			}
			else if (e.Key == Key.Escape)
			{
				textBoxSearch.Visibility = Visibility.Collapsed;
				textBoxSearch.Text = "";
				FilterText = "";
				tabInstance.SaveTabSettings();
			}
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

			dataGrid.SelectedCellsChanged -= DataGrid_SelectedCellsChanged;
			dataGrid.PreviewMouseLeftButtonDown -= DataGrid_PreviewMouseLeftButtonDown;
			dataGrid.BeginningEdit -= dataGrid_BeginningEdit;
			dataGrid.CopyingRowClipboardContent -= dataGrid_CopyingRowClipboardContent;
			dataGrid.ColumnReordered -= dataGrid_ColumnReordered;
			dataGrid.ItemsSource = null;

			textBoxSearch.PreviewKeyDown -= textBoxSearch_PreviewKeyDown;

			KeyDown -= UserControl_KeyDown;

			INotifyCollectionChanged iNotifyCollectionChanged = iList as INotifyCollectionChanged;
			if (iNotifyCollectionChanged != null)
				iNotifyCollectionChanged.CollectionChanged -= INotifyCollectionChanged_CollectionChanged;

			iList = null;
			collectionView = null;
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
}
