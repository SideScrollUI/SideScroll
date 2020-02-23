using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabTreeView : TabControl //, IDisposable
	{
		public TabDataSettings tabDataConfiguration;
		private IList iList;
		private Type elementType;


		//private HashSet<int> pinnedItems = new HashSet<int>();
		//private ICollectionView collectionView;
		private List<PropertyInfo> columnProperties = new List<PropertyInfo>(); // makes filtering faster, could change other Dictionaries strings to PropertyInfo

		public event EventHandler<EventArgs> OnSelectionChanged;
		//private bool autoSelectNew = true;
		//private DispatcherTimer dispatcherTimer;

		public bool AutoLoad { get; internal set; }

		//private Filter filter;

		private Stopwatch stopwatch = new Stopwatch();
		//private object autoSelectItem = null;

		public TabTreeView(TabInstance tabInstance, TabDataSettings tabDataConfiguration, IList iList) : base(tabInstance)
		{
			this.tabDataConfiguration = tabDataConfiguration;
			this.iList = iList;
			Debug.Assert(tabDataConfiguration != null);
			Initialize();
		}

		public override string ToString() => tabInstance.ToString();

		private void Initialize()
		{
			/*ValueToBrushConverter brushConverter = (ValueToBrushConverter)dataGrid.Resources["colorConverter"];
			brushConverter.HasChildrenBrush = (SolidColorBrush)Resources[Keys.HasChildrenBrush];

			if (tabModel.Editing == true)
			{
				dataGrid.IsReadOnly = false;
				brushConverter.Editable = true;
				brushConverter.EditableBrush = (SolidColorBrush)Resources[Keys.EditableBrush];
			}*/

			Type listType = iList.GetType();
			elementType = listType.GenericTypeArguments[0];

			/*collectionView = CollectionViewSource.GetDefaultView(iList); // This fails if an object's ToString() has a -> in it
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

			AddPropertiesAsColumns();*/

			InitializeControls();

			//Debug.Assert(dataGrid.Columns.Count > 0); // make sure something is databindable, not all lists have a property, add a ListToString wrapper around ToString()?
		}

		protected override void OnMeasureInvalidated()
		{
			base.OnMeasureInvalidated();
		}

		private TreeView treeView;
		private void InitializeControls()
		{
			Grid grid = new Grid()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				RowDefinitions = new RowDefinitions("*"),
				ColumnDefinitions = new ColumnDefinitions("*"),
			};

			//Background = new SolidColorBrush(Colors.Blue);
			//Background = new SolidColorBrush(Colors.Orange);
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;
			//Orientation = Orientation.Vertical;
			//Width = 1000;
			//Height = 700; // works

			treeView = new TreeView()
			{
				Background = new SolidColorBrush(Colors.White),
				//Width = 50,
				//Height = 500, // does work
				Items = iList,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch, // doesn't work
				//Items = new Bind
			};

			Grid.SetRow(treeView, 0);
			//treeView.PointerPressed += TreeView_PointerPressed; // only triggers when non-cells are clicked
			treeView.PointerReleased += TreeView_PointerReleased;
			//Content = treeView;
			grid.Children.Add(treeView);
			//Content = grid;
		}
		
		private void TreeView_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e)
		{
			UpdateSelection();
		}

		public IList SelectedItems
		{
			get
			{
				return new List<object>() { treeView.SelectedItem };
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

		private void UpdateSelection()
		{
			//SelectPinnedItems();
			SaveSelectedItems();

			OnSelectionChanged?.Invoke(this, null);
		}

		public void SaveSelectedItems()
		{
			tabDataConfiguration.SelectedRows.Clear();
			/*Dictionary<object, List<DataGridCellInfo>> orderedRows = new Dictionary<object, List<DataGridCellInfo>>();
			foreach (DataGridCellInfo cellInfo in dataGrid.SelectedCells)
			{
				if (cellInfo.Column == null)
					continue; // this shouldn't happen, but it does
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
			object obj = treeView.SelectedItem;
			if (obj != null)
			{
				Type type = obj.GetType();
				SelectedRow selectedItem = new SelectedRow();
				selectedItem.label = obj.ToUniqueString();
				//selectedItem.index = dataGrid.Items.IndexOf(obj);
				if (selectedItem.label == type.FullName)
				{
					selectedItem.label = null;
				}
				tabDataConfiguration.SelectedRows.Add(selectedItem);
			}
		}
	}
}
