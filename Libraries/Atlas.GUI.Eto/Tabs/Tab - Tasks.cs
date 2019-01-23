using System;
using System.Collections.Generic;
using Atlas.Core;
using Eto.Forms;
using Atlas.Tabs;
using System.Collections;
using System.Linq;

namespace Atlas.GUI.Eto
{
	public class TabTasks : Panel
	{
		private TabModel tabModel;

		public GridView gridView;

		public IList SelectedItems { get { return gridView.SelectedItems.ToList(); } }

		public bool GridInitialized { get; private set; } = false;

		public event EventHandler<EventArgs> OnSelectionChanged;

		public TabTasks(TabModel tabModel)
		{
			this.tabModel = tabModel;
			
			InitializeControls();
			//this.AutoSize = true;
		}

		private void InitializeControls()
		{
			gridView = new GridView();
			gridView.AllowMultipleSelection = true;
			/*bindingSource = new SyncBindingSource();
			bindingSource.ListChanged += BindingSource_ListChanged;
			bindingSource.DataSource = iList;
			*/
			//InitializeCellStyles();
			//dataGridView.AutoGenerateColumns = false;
			//bindingSource.Position = -1;

			//if (tabModel.Editing == true)
			//	dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
			gridView.GridLines = GridLines.Both;
			gridView.BackgroundColor = Theme.BackgroundColor;

			Type listType = tabModel.Tasks.GetType();

			IList iList = tabModel.Tasks;
			Type genericType = null;
			if (iList is Array)
				genericType = typeof(FilterCollection<>).MakeGenericType(listType.GetElementType());
			else if (listType.GenericTypeArguments.Length > 0)
				genericType = typeof(FilterCollection<>).MakeGenericType(listType.GenericTypeArguments);

			var filtered = Activator.CreateInstance(genericType, new object[] { iList });
			gridView.DataStore = (IEnumerable<object>)filtered;

			//ListCollection.CreateList(typeof(FilterCollection<>), iList);
			//DataStore = (IEnumerable<object>)iList;

			AddColumn("Task", nameof(TaskInstance.Label));
			AddColumn("   %   ", nameof(TaskInstance.Percent));
			AddColumn("Status", nameof(TaskInstance.Status));

			/*if (iList.Count > 0 && dataGridView.Columns.Count == 0)
			{
				bindingSource.DataSource = ListToString.Create(iList);
				AddPropertiesAsColumns(typeof(ListToString));
			}*/
			//Debug.Assert(dataGridView.Columns.Count > 0); // make sure something is databindable, not all lists have a property, add a ListToString wrapper around ToString()?

			//Debug.Assert(dataGridView.Rows.Count == listItems.Count);

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

			gridView.SelectedItemsChanged += GridView_SelectedItemsChanged;
			gridView.SelectionChanged += GridView_SelectionChanged;
		}

		private void AddColumn(string label, string propertyName)
		{
			GridColumn column = new GridColumn();
			//if (type == typeof(bool))
			//	column.DataCell = new DataGridViewCheckBoxColumn();
			//else
			TextBoxCell textBoxCell = new TextBoxCell();
			textBoxCell.Binding = Binding.Property<string>(propertyName);
			column.DataCell = textBoxCell;

			//textBoxCell.Binding = Binding.Property<MyPoco, string>(r => r.Text);
			column.HeaderText = label;
			//column.DataPropertyName = propertyName;
			//if (tabModel.Editing == false)
			//	column.ReadOnly = true;
			//column.ValueType = type;
			//column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; // can't resize columns if this is set, we already get the correct size later with AutoResizeColumns() 
			/*if (type.IsNumeric())
			{
				column.DefaultCellStyle = cellStyleNumber;
			}*/
			gridView.Columns.Add(column);
		}

		private void GridView_SelectionChanged(object sender, EventArgs e)
		{
			UpdateSelection();
		}

		private void GridView_SelectedItemsChanged(object sender, EventArgs e)
		{
			//throw new NotImplementedException();
		}

		private void UpdateSelection()
		{
			OnSelectionChanged?.Invoke(this, null);
		}

		/*private void InitializeGrid()
		{
			if (!IsHandleCreated || GridInitialized)
				return;

			GridInitialized = true;

			//if (dataGridView.Columns.Count > 1)
			//	dataGridView.Columns[dataGridView.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			foreach (DataGridViewColumn column in dataGridView.Columns)
			{
				column.MinimumWidth = column.GetPreferredWidth(DataGridViewAutoSizeColumnMode.ColumnHeader, true);
			}

			dataGridView.ClearSelection();

			dataGridView.SelectionChanged += dataGridView_SelectionChanged;
		}

		public override Size GetPreferredSize(Size proposedSize)
		{
			Size inputSize = new Size(Width, Height);
			Size gridSize = dataGridView.GetPreferredSize(proposedSize);
			Size size = base.GetPreferredSize(proposedSize);
			size.Height = Math.Max(gridSize.Height, size.Height);

			size.Width = 4;
			foreach (DataGridViewColumn column in dataGridView.Columns)
			{
				size.Width += column.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
			}
			//size.Width = Math.Min(size.Width, MaxGridWidth);

			foreach (var scroll in dataGridView.Controls.OfType<VScrollBar>())
			{
				size.Width += scroll.Width;
				break;
			}

			//size.Height = 150;
			return size;
		}

		public IList SelectedItemsOrdered
		{
			get
			{
				SortedDictionary<int, object> orderedItems = new SortedDictionary<int, object>();
				foreach (DataGridViewCell cell in dataGridView.SelectedCells)
				{
					orderedItems.Add(cell.RowIndex, cell.OwningRow.DataBoundItem);
				}
				return orderedItems.Values.ToList();
			}
		}

		private void dataGridView_SelectionChanged(object sender, EventArgs e)
		{
			OnSelectionChanged?.Invoke(this, null);
		}
		
		*/
		/*
		private void dataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex == 0)
			{
				DataGridViewRow row = dataGridView.Rows[e.RowIndex];
				TaskCreator taskCreator = row.DataBoundItem as TaskCreator;
				taskCreator.Start(call);
			}
		}*/
	}
}

/*
*/
