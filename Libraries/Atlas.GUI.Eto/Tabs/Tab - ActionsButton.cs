using System;
using System.Collections.Generic;
using Eto.Forms;
using Atlas.Core;
using Atlas.Tabs;

namespace Atlas.GUI.Eto
{
	public class TabActionsButtons : StackLayout
	{
		private TabInstance tabInstance;
		private TabModel tabModel;
		
		private ItemCollection<TaskCreator> taskItems;
		public bool GridInitialized { get; private set; } = false;
		private Dictionary<Button, TaskCreator> taskCreators = new Dictionary<Button, TaskCreator>();

		public TabActionsButtons(TabInstance tabInstance, TabModel tabModel, ItemCollection<TaskCreator> taskItems)
		{
			this.tabInstance = tabInstance;
			this.tabModel = tabModel;
			this.taskItems = taskItems;

			Orientation = Orientation.Vertical;
			HorizontalContentAlignment = HorizontalAlignment.Stretch;
			VerticalContentAlignment = VerticalAlignment.Stretch;
			BackgroundColor = Theme.BackgroundColor;
			
			Reload();
		}

		private void Reload()
		{
			//AddColumn("Action", "Label");

			//GridLines = GridLines.Both;
			//DataStore = (IEnumerable<object>)tabModel.Actions;
			//this.CellClick += TabActions_CellClick;

			foreach (TaskCreator taskCreator in tabModel.Actions)
			{
				Button button = new Button();
				button.Text = taskCreator.Label;
				//button.TextColor = Theme.ButtonForegroundColor;
				//button.BackgroundColor = Theme.ButtonBackgroundColor;
				button.Click += Button_Click;
				Items.Add(button);
				taskCreators[button] = taskCreator;
			}
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
			//this.Columns.Add(column);
		}

		/*private void InitializeGrid()
		{

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

		private void Button_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			TaskCreator taskCreator = taskCreators[button];
			Call call = new Call(taskCreator.Label);
			TaskInstance taskInstance = taskCreator.Start(call);
			tabModel.Tasks.Add(taskInstance);
			//this.UnselectAll();
		}
	}
}

/*
*/
