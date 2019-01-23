using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Atlas.GUI.Eto
{
	public class TabView : Panel
	{
		public TabInstance tabInstance;
		public TabModel tabModel;
		public string Label { get { return tabModel.Name; } set { tabModel.Name = value; } }

		private TabViewSettings tabSettings;

		private TabActionsButtons tabActions;
		public TabTasks tabTasks;
		public List<TabData> tabDatas = new List<TabData>();
		//public TabChart tabChart;

		//private List<Control> listControls = new List<Control>();
		private Dictionary<object, Control> childControls = new Dictionary<object, Control>();

		private Splitter splitContainer; // Should this class use? Can't reload then?
		private StackLayout stackLayoutParentControls;
		//private TableLayout stackLayoutParentControls;
		private StackLayout stackLayoutChildControls;

		public TabView()
		{
			InitializeAll();
		}

		public TabView(TabInstance tabInstance)
		{
			this.tabInstance = tabInstance;
			InitializeAll();
		}

		public override string ToString()
		{
			return tabModel.Name;
		}

		private void InitializeAll()
		{
			tabModel = tabInstance.tabModel;
			//AddListeners();

			//this.call = call;
			// Have return ListCollection?
			//tabInterface.call = call;
			//if (tabInstance.CanLoad) // 
			//	tabModel.ItemList.Clear();
			//tabInstance.Load(); // Creates a new ListCollection and initializes class

			//tabSettings = tabInstance.LoadDefaultSettings();
		}

		private void AddContextMenu()
		{
			var buttonReload = new ButtonMenuItem() { Text = "Reload" };
			buttonReload.Click += delegate
			{
				ReloadControls();
			};
			ContextMenu = new ContextMenu(buttonReload);
		}

		public void ReloadControls()
		{
			InitializeControls();
			AddListActions();
			AddListTasks();
			//AddListChart();
			AddListData();

			//StackLayoutItem layoutItem = new StackLayoutItem(new Panel(), true);
			//parentControls.Items.Add(layoutItem);
		}

		public void InitializeControls()
		{
			//this.splitContainer.SplitterMoved += new SplitterEventHandler(this.splitContainer_SplitterMoved);
			this.BackgroundColor = Theme.TitleBackgroundColor;

			Label labelTitle = new Label();
			labelTitle.Text = tabModel.Name;
			labelTitle.TextColor = Theme.TitleForegroundColor;
			try
			{
			labelTitle.Font = new Font("Arial", 10); // doesn't work with GTK
			}
			catch (Exception)
			{

			}
			//leftControls.Items.Add

			stackLayoutParentControls = new StackLayout
			{
				Orientation = Orientation.Vertical,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
				BackgroundColor = Theme.BackgroundColor,
				Items =
				{
					new Panel
					{
						Content = labelTitle,
						BackgroundColor = Theme.TitleBackgroundColor,
						Padding = 4
					}
				}
			};

			/*parentControls = new TableLayout
			{
				BackgroundColor = Colors.Blue,
				Rows =
				{
					new TableRow(
						TableLayout.AutoSized(labelTitle)
						)
				}
			};
			var row = new TableRow(
						TableLayout.AutoSized(labelTitle)
						);
			TableLayout tableLayout = */

			stackLayoutChildControls = new StackLayout
			{
				Orientation = Orientation.Vertical,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
				BackgroundColor = Theme.BackgroundColor
			};

			splitContainer = new Splitter
			{
				Orientation = Orientation.Horizontal,
				BackgroundColor = Theme.BackgroundColor,
				Panel1 = stackLayoutParentControls,
				Panel2 = stackLayoutChildControls,
			};

			if (tabSettings.SplitterDistance != null)
				splitContainer.Position = (int)tabSettings.SplitterDistance;
			splitContainer.MouseUp += SplitContainer_MouseUp;
			splitContainer.PositionChanged += SplitContainer_PositionChanged;
			splitContainer.MouseDoubleClick += SplitContainer_MouseDoubleClick;

			Content = splitContainer;

			//this.ResumeLayout();
		}

		private void SplitContainer_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			//throw new NotImplementedException();
		}

		/*protected void AddListChart()
		{
			if (tabModel.Chart == null)
				return;

			RemoveControl(tabChart);

			this.tabChart = new TabChart(call, this.tabModel, tabSettings);
			//tabChart.OnSelectionChanged += ListData_OnSelectionChanged;
			//tabChart.Height = 50;

			AddControl(tabChart);
		}*/

		protected void AddListActions()
		{
			if (tabModel.Actions == null)
				return;

			//RemoveControl(tabActions);

			this.tabActions = new TabActionsButtons(tabInstance, this.tabModel, tabModel.Actions as ItemCollection<TaskCreator>);
			//tabActions.OnSelectionChanged += ChildListSelectionChanged;

			//AddControl(tabActions);
			StackLayoutItem layoutItem = new StackLayoutItem(tabActions, false);
			stackLayoutParentControls.Items.Add(layoutItem);

			Panel spacer = new Panel();
			spacer.Height = 10;
			stackLayoutParentControls.Items.Add(new StackLayoutItem(spacer, false));
		}

		protected void AddListTasks()
		{
			if (tabModel.Actions == null)
				return;
			
			if (tabModel.Tasks == null)
				tabModel.Tasks = new TaskInstanceCollection();

			this.tabTasks = new TabTasks(this.tabModel);
			tabTasks.OnSelectionChanged += ChildListSelectionChanged;
			
			StackLayoutItem layoutItem = new StackLayoutItem(tabTasks, false);
			stackLayoutParentControls.Items.Add(layoutItem);

			Panel spacer = new Panel();
			spacer.Height = 10;
			stackLayoutParentControls.Items.Add(new StackLayoutItem(spacer, false));
		}

		protected void AddListData()
		{
			int index = 0;
			foreach (IList iList in tabModel.ItemList)
			{
				TabData tabData = new TabData(tabInstance, tabSettings.GetData(index), iList);
				tabData.OnSelectionChanged += ChildListSelectionChanged;
				StackLayoutItem layoutItem = new StackLayoutItem(tabData, true);
				stackLayoutParentControls.Items.Add(layoutItem);
				
				tabDatas.Add(tabData);
				index++;
			}
		}

		// how do we hide the inherited Controls?
		/*public void AddControl(Control control)
		{
			TabView tabView = control as TabView;
			if (tabView != null)
				tabView.Initialize(call);

			int rowIndex = this.tableLayoutPanelLeft.RowCount;

			// Add a new AutoSize Row Style
			this.tableLayoutPanelLeft.RowCount++;
			RowStyle rowStyle = new RowStyle();
			rowStyle.SizeType = SizeType.AutoSize;
			if (this.tableLayoutPanelLeft.RowStyles.Count <= tableLayoutPanelLeft.Controls.Count)
				this.tableLayoutPanelLeft.RowStyles.Add(rowStyle);

			this.tableLayoutPanelLeft.Controls.Add(control, 0, rowIndex);

			ReinitializeSplitterDistance();
		}
		
		public void RemoveControl(Control control)
		{
			if (control == null)
				return;

			tableLayoutPanelLeft.Controls.Remove(control);
		}*/

		public void LoadConfig()
		{
			//allowAutoScrolling = false;

			tabInstance.Reintialize();

			LoadTabSettings();

			InitializeControls();
			ReloadControls();
			//this.Dispatcher.BeginInvoke((Action)(() => { allowAutoScrolling = true; }));
		}

		private void LoadTabSettings()
		{
			tabSettings = tabInstance.LoadDefaultTabSettings();
		}

		private void UpdateSelectedChildControls()
		{
			//tableLayoutPanelRight.SuspendLayout();

			// Remove all sublists from view so we can order and resize them easily
			//panelRight.Controls.Clear();

			// Add new sublists
			Dictionary<object, Control> oldChildControls = childControls;
			childControls = new Dictionary<object, Control>();

			List<Control> orderedChildControls = new List<Control>();
			/*if (tabActions != null && tabActions.GridInitialized)
			{
				CreateChildControls(this.tabActions.SelectedItemsOrdered, oldSelectedControls, orderedChildControls);
			}*/
			if (tabTasks != null)
			{
				CreateChildControls(this.tabTasks.SelectedItems, oldChildControls, orderedChildControls);
			}

			foreach (TabData tabData in tabDatas)
			{
				CreateChildControls(tabData.SelectedItems, oldChildControls, orderedChildControls);
			}

			/*foreach (Control control in oldSelectedControls.Values)
			{
				childControls.Items.Remove(control);
			}*/
			stackLayoutChildControls.Items.Clear();

			//panelRight.RowCount = sortedChildControls.Count;
			//panelRight.Invalidate();

			// Add all child controls to the view
			//int rowIndex = 0;
			foreach (Control control in orderedChildControls)
			{
				//control.Width = tableLayoutPanelRight.Width;
				StackLayoutItem layoutItem = new StackLayoutItem(control, true);
				// Object must implement IConvertible - can't databind non-primitives
				// See Tab Data, need to change Column DataBinding
				// URI class has this problem
				// Call ToString() on any non-primitive
				stackLayoutChildControls.Items.Add(layoutItem);
				//childControls.SetRow(control, rowIndex++);
			}
			//tableLayoutPanelRight.RowCount = orderedChildControls.Count;
			//tableLayoutPanelRight.ResumeLayout();

			//Invalidate(true);
			//this.OnResize(EventArgs.Empty);
			//UpdateChildControlHeights();
		}

		private void ChildListSelectionChanged(object sender, EventArgs e)
		{
			UpdateSelectedChildControls();
		}

		private void CreateChildControls(IList newControls, Dictionary<object, Control> oldChildControls, List<Control> orderedChildControls)
		{
			foreach (object obj in newControls)
			{
				object value = obj.GetInnerValue();
				if (value == null)
					continue;

				if (oldChildControls.ContainsKey(obj))
				{
					Control userControl = oldChildControls[obj];
					orderedChildControls.Add(userControl);
					childControls.Add(obj, userControl);

					oldChildControls.Remove(obj);
				}
				else
				{
					Control userControl = CreateChildControl(obj.ToString(), value);
					if (userControl != null)
					{
						childControls[obj] = userControl;
						orderedChildControls.Add(userControl);
					}
				}
			}
		}

		private Control CreateChildControl(string name, object obj)
		{
			// Still throwing kernel trap if we let it go
			/*
			if (tableLayoutPanelRight.Width < 30)
				return null;*/

			Type type = obj.GetType();
			object value = obj.GetInnerValue();
			if (value is ITab)
			{
				ITab iTab = (ITab)value;
				TabInstance childTabInstance = tabInstance.CreateChildTab(iTab);
				//try
				{
					TabView tabView = new TabView(childTabInstance);
					tabView.Label = name;
					tabView.LoadConfig();
					return tabView;
				}
				//catch (Exception e)
				{
					//return null;
				}
			}
			else if (value is TabView)
			{
				TabView tabView = (TabView)value;
				tabView.tabInstance.ParentTabInstance = tabInstance;
				//tabView.tabInstance.bookmarkNode = bookmarkNode;
				tabView.Label = name;
				tabView.LoadConfig();
				return tabView;
			}
			else if (value is TabInstance)
			{
				//Call tabCall = call.Child(name);
				TabView tabView = new TabView((TabInstance)value);
				tabView.Label = name;
				tabView.LoadConfig();
				return tabView;
			}
			else if (value is Control)
			{
				Control control = (Control)value;
				return control;
			}
			else if (value is string || type.IsPrimitive)
			{
				TabText tabText = new TabText(name);
				tabText.Text = value.ToString();
				//if (this.tabModel.Editing && obj is ListMember)
				//	tabAvalonEdit.EnableEditing((ListMember)obj);
				return tabText;
			}
			else if (value is Uri)
			{
				TabWebBrowser tabWebBrowser = new TabWebBrowser(name, (Uri)value);
				return tabWebBrowser;
			}
			else
			{
				TabModel tabModel;
				if (value is TabModel)
				{
					tabModel = value as TabModel;
					tabModel.Name = name;
				}
				else
				{
					tabModel = TabModel.Create(name, value);
					if (tabModel == null)
						return null;
				}

				TabInstance childTabInstance = tabInstance.CreateChild(tabModel);
				TabView tabView = new TabView(childTabInstance);
				tabView.Label = name;
				tabView.LoadConfig();
				return tabView;
			}
		}

		/*public void UpdateChildControlHeights()
		{
			//this.PerformLayout();
			//splitContainer.PerformLayout();
			//panelRight.PerformLayout();

			foreach (Control childControl in selectedControls.Values)
			{
				//childControl.PerformLayout();
				Size prefSize = childControl.GetPreferredSize(Size);
				childControl.Height = Math.Min(prefSize.Height, Height / selectedControls.Count - 6);

				TabView tabView = childControl as TabView;
				if (tabView != null)
				{
					tabView.UpdateChildControlHeights();
				}
			}
		}

		public override Size GetPreferredSize(Size proposedSize)
		{
			Size size = base.GetPreferredSize(proposedSize);
			//Size dataSize = tabData.GetPreferredSize(proposedSize);
			//Size splitSize = splitContainer.GetPreferredSize(proposedSize);
			//splitContainer.Invalidate(true);
			//splitContainer.Update();
			//Size splitSize2 = splitContainer.GetPreferredSize(proposedSize);
			if (splitContainer != null)
			{
				Size panel1Size = splitContainer.Panel1.GetPreferredSize(proposedSize);
				Size panel2Size = splitContainer.Panel2.GetPreferredSize(proposedSize);
				size.Width = panel1Size.Width + 4 + panel2Size.Width;
			}
			if (tabModel.Name == "Tasks")
			{ }
			return size;
		}

		public int PreferredSplitterDistance
		{
			get
			{
				//Size baseSize = base.GetPreferredSize(Size);

				if (tabModel.Name == "Grid")
				{ }

				//Size prefSize = GetPreferredSize(Size);
				Size panel1Size = splitContainer.Panel1.GetPreferredSize(Size);

				int splitterDistance = panel1Size.Width;
				// Manually created controls don't always return the correct size (and we can't autosize them)
				foreach (Control control in tableLayoutPanelLeft.Controls)
				{
					Size preferredSize = control.GetPreferredSize(Size);
					splitterDistance = Math.Max(splitterDistance, preferredSize.Width);
				}

				splitterDistance = Math.Max(splitContainer.Panel1MinSize, splitterDistance);
				if (splitterDistance == 0)
					return Width;
				return splitterDistance;
			}
		}

		private int SplitterDistance
		{
			get
			{
				return splitContainer.SplitterDistance;
			}
			set
			{
				this.splitContainer.SplitterMoved -= new System.Windows.Forms.SplitterEventHandler(this.splitContainer_SplitterMoved);
				try
				{
					if (splitContainer.Width <= splitContainer.Panel1MinSize)
						return;

					//if (tabModel.Name == "Grid")
					//	;

					splitContainer.Width = Width;
					//splitContainer.Panel1.Width = splitterDistance;
					tableLayoutPanelLeft.Width = value;
					splitContainer.SplitterDistance = value; // might get set to less because of max window size, triggers a resize event
				}
				finally
				{
					this.splitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer_SplitterMoved);
				}

			}
		}

		private void ReinitializeSplitterDistance()
		{
			if (tabSettings != null && tabSettings.SplitterDistance != null)
				SplitterDistance = (int)tabSettings.SplitterDistance;
			else
				SplitterDistance = PreferredSplitterDistance;
		}*/

		private void UpdateNearbySplitters(int depth, TabView triggeredControl)
		{
			// todo: use max SplitterDistance if not user triggered
			/*if (call.parent != null)
			{
				foreach (Control control in call.parent.childControls.Values)
				{
					TabView tabView = control as TabView;
					if (tabView != null)
					{
						if (tabView != triggeredControl)
							tabView.splitContainer.SplitterDistance = splitContainer.SplitterDistance;
					}
				}
			}*/
		}

		/*private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
		{
			//splitContainer.Panel1.Invalidate();
			//splitContainer.Panel1.Update();
			UpdateNearbySplitters(1, this);
			tabSettings.SplitterDistance = splitContainer.SplitterDistance;
			foreach (Control control in tableLayoutPanelLeft.Controls)
			{
				control.AutoSize = false;
				control.Width = splitContainer.SplitterDistance;
				control.AutoSize = true;
			}
			SaveConfig();
		}*/

		private void TabView_Resize(object sender, EventArgs e)
		{
			//UpdateChildControlHeights();
		}

		private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LoadConfig();
		}

		private void toolStripMenuItemReset_Click(object sender, EventArgs e)
		{
			Reset();
			tabInstance.SaveTabSettings();
		}

		private void Reset()
		{
			tabSettings = new TabViewSettings();
		}

		private void toolStripMenuItemDebug_Click(object sender, EventArgs e)
		{
			// causes circular reference
			//Control debugControl = CreateChildControl("Debug", this);

			TabModel debugListCollection = new TabModel("Debug");
			debugListCollection.AddData(this);
			Control debugControl = CreateChildControl("Debug", debugListCollection);
			//AddControl(debugControl);
		}

		private void splitContainer_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			//SplitterDistance = PreferredSplitterDistance;
			this.tabInstance.SaveTabSettings();
		}

		private void SplitContainer_PositionChanged(object sender, EventArgs e)
		{
			//this.tabInstance.SaveTabSettings();
			SaveSplitterDistance();
		}

		private void SplitContainer_MouseUp(object sender, MouseEventArgs e)
		{
			// doesn't trigger for splitter, only content
		}

		private void SaveSplitterDistance()
		{
			tabSettings.SplitterDistance = splitContainer.Position;
			tabInstance.SaveTabSettings();
		}
	}
}
