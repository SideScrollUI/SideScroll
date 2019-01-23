using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using Atlas.Core;
using Atlas.Extensions;
using Eto.Forms;
using Atlas.Tabs;

namespace Atlas.GUI.Eto
{
	public partial class TabData : Panel
	{
		//private int MaxWidth = 1000;

		// Params
		private TabInstance tabInstance;
		private TabDataSettings tabDataSettings;
		private TabModel tabModel;
		private IList iList;
		//public int test = new Random().Next();
		private Type elementType;
		private bool autoSelectNew = true;

		// 
		private HashSet<int> pinnedItems = new HashSet<int>();
		//private DataGridViewCellStyle cellStyleNumber = new DataGridViewCellStyle();

		public GridView gridView; // the DataGrid

		public IList SelectedItems { get { return gridView.SelectedItems.ToList(); } }

		public event EventHandler<EventArgs> OnSelectionChanged;
		//public SyncBindingSource bindingSource;

		public TabData(TabInstance tabInstance, TabDataSettings tabDataSettings, IList iList)
		{
			this.tabInstance = tabInstance;
			this.tabModel = tabInstance.tabModel;
			this.tabDataSettings = tabDataSettings;
			this.iList = iList;
			Type listType = iList.GetType();
			elementType = listType.GenericTypeArguments[0];
			Debug.Assert(tabDataSettings != null);

			InitializeControls();
		}

		/*public void Initialize(Call caller, ListCollection tabModel)
		{
			this.call = caller;
			this.tabModel = tabModel;
			
			LoadGrid();
		}*/

		private void InitializeControls()
		{
			gridView = new GridView();
			gridView.AllowMultipleSelection = true;
			/*bindingSource = new SyncBindingSource();
			bindingSource.ListChanged += BindingSource_ListChanged;
			bindingSource.DataSource = iList;
			*/
			//InitializeCellStyles();
			//gridView.AutoGenerateColumns = true;
			//bindingSource.Position = -1;

			//if (tabModel.Editing == true)
			//	dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
			gridView.GridLines = GridLines.Both;
			gridView.BackgroundColor = Theme.BackgroundColor;

			Type listType = iList.GetType();

			Type genericType = null;
			//if (elementType is IConvertible)
			//iList = GetTestItems();
			//gridView.DataStore = GetTestItems();
			gridView.DataStore = (IEnumerable<object>)iList;
			/*{
				if (iList is Array)
					genericType = typeof(FilterCollection<>).MakeGenericType(listType.GetElementType());
				else if (listType.GenericTypeArguments.Length > 0)
					genericType = typeof(FilterCollection<>).MakeGenericType(listType.GenericTypeArguments);

				var filtered = Activator.CreateInstance(genericType, new object[] { iList });
				gridView.DataStore = (IEnumerable<object>)filtered;
			}*/
			//else
			{
				//if (tabModel.Name != "Sample Call")
				//	gridView.DataStore = (IEnumerable<object>)iList;
				//else
				//	tabModel.Name = tabModel.Name;
			}

			//ListCollection.CreateList(typeof(FilterCollection<>), iList);

			AddPropertiesAsColumns();
			/*if (iList.Count > 0 && dataGridView.Columns.Count == 0)
			{
				bindingSource.DataSource = ListToString.Create(iList);
				AddPropertiesAsColumns(typeof(ListToString));
			}*/
			//Debug.Assert(dataGridView.Columns.Count > 0); // make sure something is databindable, not all lists have a property, add a ListToString wrapper around ToString()?

			//Debug.Assert(dataGridView.Rows.Count == listItems.Count);

			gridView.SelectedItemsChanged += TabData_SelectedItemsChanged;
			gridView.SelectionChanged += TabData_SelectionChanged;


			Content = new StackLayout
			{
				Orientation = Orientation.Vertical,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
				BackgroundColor = Theme.BackgroundColor,
				Items =
				{
					gridView
				}
			};

			//SelectSavedLabels();
		}

		private void TabData_SelectionChanged(object sender, EventArgs e)
		{
			UpdateSelection();

			// can't save every time due to performance 
			if (sender == this) // user initiated, check for changed instead?
			{
				this.tabInstance.SaveTabSettings();
			}
		}

		private void TabData_SelectedItemsChanged(object sender, EventArgs e)
		{
			//throw new NotImplementedException();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			//Initialize();
			InitializeGrid();
		}

		private void ListData_Paint(object sender, PaintEventArgs e)
		{
			// ugly hack, some things don't get initialized until the first paint, might be able to use Visible Changed
			InitializeGrid();
		}

		public void InitializeGrid()
		{
			if (tabModel.AutoLoad)
			{
				//LoadSavedSettings();
				SelectSavedLabels();
				//if (tabDataSettings.selected.Count == 0 || (autoSelectNew && SelectedItems.Count == 0))
				//	SelectFirstValue();
				//OnSelectionChanged?.Invoke(this, null);
			}

			//Update();

			//dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
			//if (dataGridView.Columns.Count > 0)
			//	dataGridView.Columns[dataGridView.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			/*foreach (DataGridViewColumn column in dataGridView.Columns)
			{
				column.MinimumWidth = column.GetPreferredWidth(DataGridViewAutoSizeColumnMode.ColumnHeader, true);
				int prefSize = column.GetPreferredWidth(DataGridViewAutoSizeColumnMode.DisplayedCells, true);
				if (prefSize > column.MinimumWidth)
					column.FillWeight = Math.Min(prefSize, 200);
			}

			dataGridView.ClearSelection();

			dataGridView.SelectionChanged += dataGridView_SelectionChanged;
			initializeFinished = true;*/
		}

		/*

		private void BindingSource_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemAdded)
			{
				if (SelectedRows.Count == 0 ||
					(SelectedRows.Count == 1 && SelectedRows[0].Index == dataGridView.Rows.Count - 2))
					SelectedIndex = e.NewIndex;
			}
		}*/

		private void AddPropertiesAsColumns()
		{
			List<TabDataSettings.PropertyColumn> propertyColumns = tabDataSettings.GetPropertiesAsColumns(elementType);

			foreach (TabDataSettings.PropertyColumn propertyColumn in propertyColumns)
			{
				AddColumn(propertyColumn.label, propertyColumn.propertyInfo);
			}
		}

		public delegate string BindingPropertyAction(object obj);

		/*
		
		//
		// Summary:
		//     Creates a new indirect property binding using the specified propertyExpression.
		//
		// Parameters:
		//   propertyExpression:
		//     Expression of the property to bind to.
		//
		// Type parameters:
		//   T:
		//     The type of the model.
		//
		//   TValue:
		//     The property value type.
		//
		// Remarks:
		//     This supports single and multiple levels of property accessors in the model.
		public static IndirectBinding<TValue> Property<T, TValue>(Expression<Func<T, TValue>> propertyExpression);
		*/

		/*public class PropertyColumn
		{
			public PropertyInfo PropertyInfo { get; set; }

			public PropertyColumn(PropertyInfo propertyInfo)
			{
				this.PropertyInfo = propertyInfo;
			}

			public string Text
			{
				get
				{
					return PropertyInfo.GetValue(obj).ObjectToUniqueString();
				}
			}
		}*/

		private void AddColumn(string label, PropertyInfo propertyInfo)
		{
			GridColumn gridColumn = new GridColumn();
			//if (type == typeof(bool))
			//	gridColumn.DataCell = new DataGridViewCheckBoxColumn();
			//else
			TextBoxCell textBoxCell = new TextBoxCell();
			//textBoxCell.Binding = Binding.Property<string>(propertyInfo.Name);
			//Binding.Property.MakeGenericType()

			//Type classType = typeof(Binding.Property<>);
			//Type[] typeParams = new Type[] { propertyInfo.PropertyType };
			//Type constructedType = classType.MakeGenericType(typeParams);

			//object x = Activator.CreateInstance(constructedType, new object[] { propertyInfo.Name });

			/*
			
			foreach (MethodInfo methodInfo in methodInfos)
			{
			if (methodInfo.IsPublic && methodInfo.ReturnType == null)
			{
				ParameterInfo[] parameterInfos = methodInfo.GetParameters();
				if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(Call))
				{
					methods.Add(new TaskDelegate(methodInfo.Name, (TaskDelegate.CallAction)Delegate.CreateDelegate(typeof(TaskDelegate.CallAction), methodInfo)));
				}
			}
			*/
			//var pp = new PropertyBinding<object>(p.Member.Name);

			// 
			//var propertyColumn = new PropertyColumn(propertyInfo);
			//textBoxCell.Binding = Binding.Property((propertyColumn) => c.Text));
			//textBoxCell.Binding = Binding.Property(propertyColumn, c => c.Text));
			//textBoxCell.Binding = Binding.Property(() => c.Value);.Convert(r => "Value: " + Convert.ToString(r)));

			// 
			//MethodInfo methodInfo = typeof(Binding).GetMethod("Property", new Type[] {typeof(BindingProperty), typeof(string) });
			//MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(propertyInfo.PropertyType);
			//textBoxCell.Binding = (IIndirectBinding<T>)genericMethodInfo.Invoke(null, new object[] { propertyInfo.Name });

			//object obj = Activator.CreateInstance(type, true);
			//textBoxCell.Binding = Binding.Property<string>((obj) => BindingProperty(obj));
			//actions.Add(new TaskAction("Add 5 Items", new Action(() => AddItems(5)), false)); // Foreground task so we can modify collection
			// public static IndirectBinding<TValue> Property<T, TValue>(Expression<Func<T, TValue>> propertyExpression);
			// only supports IIndirectBinding<T>, requires a string cast, but property value might not be castable to a string (like a class)
			textBoxCell.Binding = Binding.Property<string>(propertyInfo.Name); 

			//textBoxCell.Binding = Binding.Property((TestInfo t) => t.Name);
			//textBoxCell.Binding = Binding.Property<>
			//textBoxCell.Binding = Binding.Property((PropertyColumn p) => p.Text);


			//MethodInfo methodInfo = typeof(Binding).GetMethod("Property", new Type[] { typeof(string) });
			//MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(propertyInfo.PropertyType);
			//textBoxCell.Binding = genericMethodInfo.Invoke();

			//textBoxCell.Binding = Binding.Delegate<>

			gridColumn.DataCell = textBoxCell;
			gridColumn.Sortable = true;

			//textBoxCell.Binding = Binding.Property<MyPoco, string>(r => r.Text);
			gridColumn.HeaderText = label;
			gridColumn.AutoSize = true;
			//gridColumn.DataPropertyName = propertyName;
			//if (tabModel.Editing == false)
			//	gridColumn.ReadOnly = true;
			//gridColumn.ValueType = type;
			//gridColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; // can't resize columns if this is set, we already get the correct size later with AutoResizeColumns() 
			/*if (type.IsNumeric())
			{
				gridColumn.DefaultCellStyle = cellStyleNumber;
			}*/
			gridView.Columns.Add(gridColumn);
		}

		private string BindingProperty(object obj)
		{
			if (obj == null)
				return null;
			return obj.ObjectToUniqueString();
		}
		/*
		
		private string BindingProperty<T>(object obj)
		{
			if (obj == null)
				return null;
			return obj.ObjectToUniqueString();
		}
		*/
		/*
			protected override void OnResize(EventArgs e)
			{
				base.OnResize(e);
				if (initializeFinished)
				{
					dataGridView.Width = Width;
					dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
				}
			}

			public override Size GetPreferredSize(Size proposedSize)
			{
				Size originalSize = dataGridView.GetPreferredSize(Size);
				Size size = originalSize;// base.GetPreferredSize(proposedSize);

				//dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
				size.Width = 4; // border?
				foreach (DataGridViewColumn column in dataGridView.Columns)
				{
					// if no rows are visible, returns only header size (AllCells will return a different value)
					if (dataGridView.Rows.Count < 100)
						size.Width += column.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
					else
						size.Width += column.GetPreferredWidth(DataGridViewAutoSizeColumnMode.DisplayedCells, true);
				}
				size.Width = Math.Min(size.Width, MaxWidth);

				if (initialized == true)
				{
					foreach (var scroll in dataGridView.Controls.OfType<VScrollBar>())
					{
						size.Width += scroll.Width;
						break;
					}

					//	if ((dataGridView.ScrollBars & ScrollBars.Vertical) != ScrollBars.None)
					//	size.Width += 20; // add room for vertical scrollbar
				}

				//widthColumns = Math.Min(widthColumns, Width);

				//if (tabModel.Name == "Grid")
				//	;

				return size;
			}

			public int SelectedIndex
			{
				set
				{
					dataGridView.ClearSelection();
					if (dataGridView.Rows.Count == 0) // todo: find out why the databinding falls behind, rows don't get created when there's no columns?
						return;
					var row = dataGridView.Rows[value];
					//row.Selected = true;
					row.Cells[0].Selected = true;
					dataGridView.FirstDisplayedScrollingRowIndex = value;
					SaveSelectedItems();
				}
			}

			public object SelectedItem
			{
				set
				{
					SelectedIndex = iList.IndexOf(value);
				}
			}*/

		/*public List<DataGridViewRow> SelectedRows
		{
			get
			{
				SortedDictionary<int, DataGridViewRow> orderedRows = new SortedDictionary<int, DataGridViewRow>();
				foreach (DataGridViewCell cell in this.SelectedItems)
				{
					orderedRows[cell.RowIndex] = cell.OwningRow;
				}
				return orderedRows.Values.ToList();
			}
		}

		public IList SelectedItemsOrdered
		{
			get
			{
				List<object> dataBoundItems = new List<object>();
				foreach (DataGridViewRow row in SelectedRows)
				{
					dataBoundItems.Add(row.DataBoundItem);
				}
				return dataBoundItems;
			}
		}
		/*
		public IList SelectedItems
		{
			get
			{
				List<object> selectedItems = new List<object>();
				foreach (DataGridViewRow row in SelectedRows)
				{
					selectedItems.Add(row.DataBoundItem);
				}
				return selectedItems;
			}
			set
			{
				HashSet<object> idxSelected = new HashSet<object>();
				foreach (object obj in value)
					idxSelected.Add(obj);

				dataGridView.ClearSelection();
				foreach (DataGridViewRow row in dataGridView.Rows)
				{
					if (idxSelected.Contains(row.DataBoundItem))
						row.Cells[0].Selected = true;
				}
			}
		}

		public List<IndexTree> SelectedIndices
		{
			get
			{
				return null;
			}
			set
			{
				dataGridView.SelectionChanged -= dataGridView_SelectionChanged;
				dataGridView.ClearSelection();
				
				foreach (IndexTree childTree in value)
				{
					dataGridView.Rows[childTree.index].Selected = true;
				}
				dataGridView.SelectionChanged += dataGridView_SelectionChanged;
				OnSelectionChanged?.Invoke(this, null);
			}
		}

		private void SelectFirstValue()
		{
			foreach (object obj in iList)
			{
				object value = obj;
				ListItem listItem = obj as ListItem;
				if (listItem != null)
				{
					if (listItem.autoLoad == false)
						continue;
					value = listItem.Value;
				}
				if (value == null)
					continue;

				if (value is TabView)
				{
					TabView tabView = (TabView)value;
					if (tabView.tabModel.AutoLoad == false)
						continue;
				}

				Type type = value.GetType();
				if (ListCollection.ObjectHasChildren(value) && type.IsEnum == false)
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
					}

					if (call.IsOwnerObject(obj)) // stops self referencing loops
						continue;

					SelectedItem = obj;
					break;
				}
			}
			SaveSelectedItems();
		}*/

		public void SaveSelectedItems()
		{
			tabDataSettings.SelectedRows.Clear();
			foreach (object obj in gridView.SelectedItems)
			{
				Type type = obj.GetType();
				SelectedRow selectedItem = new SelectedRow();
				selectedItem.label = obj.ToString();
				//selectedItem.index = row.Index;
				if (selectedItem.label == type.FullName)
				{
					selectedItem.label = null;
					//Debug.Assert(selectedItem.label != null);
					//continue;
				}
				/*
				if (selectedItem.label == null)
				{
					//Debug.Assert(selectedItem.label != null);
					continue;
				}
				*/
				//selectedItem.pinned = pinnedItems.Contains(row.Index);
				tabDataSettings.SelectedRows.Add(selectedItem);
			}
		}

		/*private void EnsureVisibleRow(int rowIndex)
		{
			if (rowIndex >= 0 && rowIndex < dataGridView.RowCount)
			{
				int countVisible = dataGridView.DisplayedRowCount(false);
				int firstVisible = dataGridView.FirstDisplayedScrollingRowIndex;
				if (rowIndex < firstVisible)
				{
					dataGridView.FirstDisplayedScrollingRowIndex = rowIndex;
				}
				else if (rowIndex >= firstVisible + countVisible)
				{
					dataGridView.FirstDisplayedScrollingRowIndex = rowIndex - countVisible / 2 + 1;
				}
			}
		}*/

		public void SelectSavedLabels()
		{
			//if (Rows.Count == 0)
			//	return;

			SuspendLayout();
			//ClearSelection();

			// should we be using dataGridView.Rows...DataBoundItem instead?
			int rowIndex = 0;
			Dictionary<string, int> keys = new Dictionary<string, int>();
			foreach (object listItem in iList)
			{
				string id = listItem.ToString();
				if (id != null)
					keys[id] = rowIndex++;
			}

			List<int> selectedRows = new List<int>();

			//object parentValue = tabModel.Object;
			foreach (SelectedRow item in tabDataSettings.SelectedRows)
			{
				if (item.label != null)
				{
					if (!keys.TryGetValue(item.label, out rowIndex))
						continue;
				}
				else
				{
					rowIndex = item.rowIndex;
				}
				if (rowIndex >= iList.Count)
					continue;
				/*DataGridViewRow row = dataGridView.Rows[rowIndex];
				object listItem = row.DataBoundItem;
				object value = ListCollection.GetValue(listItem);
				//if (value != null && parentValue.GetType() == value.GetType())
				//	continue;
				if (value is TabView)
				{
					TabView tabView = (TabView)value;
					if (tabView.tabModel.AutoLoad == false)
						continue;
				}
				row.Cells[0].Selected = true;*/
				//EnsureVisibleRow(rowIndex);
				/*if (item.pinned)
				{
					pinnedItems.Add(rowIndex);
				}*/
				selectedRows.Add(rowIndex);
			}

			gridView.SelectedRows = selectedRows;

			ResumeLayout();
		}

		/*public void FilterText(string text, bool exactMatch = true)
		{
			bindingSource.Filter = text;
		}

		public void SelectValue(object value)
		{
			foreach (object listItem in iList)
			{
				if (listItem == value)
				{
					SelectedItem = listItem;
					break;
				}
			}
		}

		// Travel up the list and select all the references from matching items
		private void UpdateSelected(TabView controlMultiList)
		{
			/*if (controlMultiList != this)
			{

			}
			if (Parent is ControlMultiList)
			{
				((ControlMultiList)Parent).UpdateSelected(controlMultiList);
				foreach (int index in controlMultiList.dataGridView.SelectedIndices)
				{
					dataGridView.SetSelected(index, true);
				}
			}*//*
		}

		private void SelectPinnedItems()
		{
			foreach (int rowIndex in pinnedItems)
			{
				dataGridView.Rows[rowIndex].Selected = true;
			}
		}*/

		private void UpdateSelection()
		{
			//SelectPinnedItems();
			SaveSelectedItems();

			OnSelectionChanged?.Invoke(this, null);
		}

		/*

		private void dataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (e.RowIndex < 0)
				return;

			DataGridViewRow row = dataGridView.Rows[e.RowIndex];
			DataGridViewColumn column = dataGridView.Columns[e.ColumnIndex];
			//ListItem listItem = row.DataBoundItem as ListItem;
			bool hasChildren = ListCollection.ObjectHasChildren(row.DataBoundItem);
			if ((row.DataBoundItem is ListItem || row.DataBoundItem is ListMember) && e.ColumnIndex == 1)
			{
				if (hasChildren)
					e.CellStyle.BackColor = Color.Moccasin;
				else
					e.CellStyle.BackColor = Color.LightGray;
			}
			else
			{
				if (column.ReadOnly == false)
				{
					IListEditable editable = row.DataBoundItem as IListEditable;
					if (editable.Editable)
						e.CellStyle.BackColor = Color.FromArgb(198, 151, 249);
					else if (e.ColumnIndex == 1)
						e.CellStyle.BackColor = Color.LightGray;
				}
				//if (hasChildren)
				//	e.CellStyle.BackColor = Color.Moccasin;
			}
		}

		private void dataGridView_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				// pin or unpin a row
				DataGridView.HitTestInfo hit = dataGridView.HitTest(e.X, e.Y);
				if (hit.RowIndex >= 0)
				{
					if (pinnedItems.Contains(hit.RowIndex))
					{
						pinnedItems.Remove(hit.RowIndex);
						dataGridView.Rows[hit.RowIndex].Selected = false;
					}
					else
					{
						pinnedItems.Add(hit.RowIndex);
						dataGridView.Rows[hit.RowIndex].Selected = true;
					}
				}
			}
		}

		private void dataGridView_DoubleClick(object sender, EventArgs e)
		{
			// get rid of all the other neighbors
			/*if (call.parent != null)
			{
				call.parent.dataGridView.BeginUpdate();
				List<int> selected = call.parent.dataGridView.SelectedIndices.Cast<int>().ToList();
				foreach (int index in selected)
				{
					object listItem = call.parent.dataGridView.Items[index];
					if (listItem != this.listItem)
						call.parent.dataGridView.SetSelected(index, false); // probably triggering event each time
				}
				call.parent.dataGridView.EndUpdate();
			}*//*
		}

		private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			// push event?
		}

		private void dataGridView_CellStateChanged(object sender, DataGridViewCellStateChangedEventArgs e)
		{
			//e.Cell.ReadOnly = false;
		}

		private void dataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
		{
			DataGridViewCell cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
			//cell.

		}

		private void textBoxSearch_TextChanged(object sender, EventArgs e)
		{
			FilterText(textBoxSearch.Text, false);
		}

		// KeyDown event doesn't work (and might be a hack for VB programmers?)
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.F))
			{
				textBoxSearch.Visible = !textBoxSearch.Visible;
				textBoxSearch.Focus();
				return true;
			}
			
			if (keyData == Keys.F2)
			{
				//dataGridView.SelectedCells;
				dataGridView.BeginEdit(true);
				return true;
			}
			
			return base.ProcessCmdKey(ref msg, keyData);
		}*/
	}
}

/*
*/
