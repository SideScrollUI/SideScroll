using Atlas.Core;
using Atlas.Extensions;
using Atlas.Serialize;
using Atlas.Tabs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Atlas.UI.Wpf
{
	public partial class TabView : UserControl, IDisposable
	{
		public TabInstance tabInstance;
		public TabModel tabModel;
		public string Label { get { return tabModel.Name; } set { tabModel.Name = value; } }

		private TabViewSettings tabSettings
		{
			get
			{
				return tabInstance.tabViewSettings;
			}
			set
			{
				tabInstance.tabViewSettings = value;
				if (tabInstance.tabViewSettings.SplitterDistance == null)
					gridColumnLists.Width = new GridLength(1, GridUnitType.Auto);
				else
					gridColumnLists.Width = new GridLength((int)value.SplitterDistance);
			}
		}

		public TabActions tabActions;
		public TabTasks tabTasks;
		public List<TabData> tabDatas = new List<TabData>();
		//public TabChart tabChart;
		//public List<TabChart> tabCharts;
		public TabBookmarks tabBookmarks;
		private bool allowAutoScrolling = false; // stop new controls from triggering the ScrollView automatically

		private Dictionary<object, Control> childControls = new Dictionary<object, Control>();

		public TabView()
		{
			this.tabInstance = new TabInstance();
			Initialize();
		}

		public TabView(TabInstance tabInstance)
		{
			this.tabInstance = tabInstance;
			Initialize();
		}

		public override string ToString()
		{
			return tabModel.Name;
		}

		private void Initialize()
		{
			tabModel = tabInstance.Model;
			InitializeComponent();
			AddListeners();
		}

		private void AddListeners()
		{
			tabInstance.OnReload += TabInstance_OnReload;
			tabInstance.OnLoadBookmark += TabInstance_OnLoadBookmark;
			tabInstance.OnClearSelection += TabInstance_OnClearSelection;
			tabInstance.OnSelectItem += TabInstance_OnSelectItem;

			//this.GotFocus += TabView_GotFocus;
			//this.LostFocus += TabView_LostFocus;

			gridParentControls.GotFocus += GridParentControls_GotFocus;
			gridParentControls.LostFocus += GridParentControls_LostFocus;
		}

		private void GridParentControls_GotFocus(object sender, RoutedEventArgs e)
		{
			gridParentControls.Background = (SolidColorBrush)Resources[Keys.BackgroundFocusedBrush];
		}

		private void GridParentControls_LostFocus(object sender, RoutedEventArgs e)
		{
			gridParentControls.Background = (SolidColorBrush)Resources[Keys.BackgroundBrush];
		}

		private void TabView_GotFocus(object sender, RoutedEventArgs e)
		{
			//Background = (SolidColorBrush)Resources[Keys.BackgroundFocusedBrush];
		}

		private void TabView_LostFocus(object sender, RoutedEventArgs e)
		{
			//Background = (SolidColorBrush)Resources[Keys.BackgroundBrush];
		}

		public void Load()
		{
			allowAutoScrolling = false;

			tabInstance.Reintialize(true);

			LoadTabSettings();

			ReloadControls();
			this.Dispatcher.BeginInvoke((Action)(() => { allowAutoScrolling = true; }));
		}

		private void Reset()
		{
			tabSettings = new TabViewSettings()
			{
				Name = tabModel.Name,
			};
		}

		private void LoadTabSettings()
		{
			tabSettings = tabInstance.LoadDefaultTabSettings();
		}

		private void AddRowSpacer(Grid grid, int index)
		{
			if (grid.Children.Count <= 1)
				return;

			RowDefinition gridRow = new RowDefinition();
			gridRow.Height = new GridLength(5);
			grid.RowDefinitions.Add(gridRow);

			// Add a dummy panel so the children count equals the rowdefinition count, otherwise we need to track which rowdefinitions belong to which control
			Rectangle panel = new Rectangle();
			Grid.SetRow(panel, index);
			grid.Children.Add(panel);
		}

		private void AddRowSplitter(Grid grid, int index)
		{
			RowDefinition gridRow = new RowDefinition();
			gridRow.Height = new GridLength(6);
			grid.RowDefinitions.Insert(index, gridRow);

			GridSplitter gridSplitter = new GridSplitter()
			{
				Background = (SolidColorBrush)Resources[Keys.SplitterBrush],
				ShowsPreview = true,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Center,
				Height = 6,
			};
			//gridSplitter.DragCompleted += rowSplitter_DragCompleted;
			Grid.SetRow(gridSplitter, index);
			grid.Children.Insert(index, gridSplitter);
		}

		private void AddParentControl(UIElement element, bool fill, bool addSplitter)
		{
			if (addSplitter)
				AddRowSplitter(gridParentControls, gridParentControls.RowDefinitions.Count);
			else
				AddRowSpacer(gridParentControls, gridParentControls.RowDefinitions.Count);
			RowDefinition rowDefinition = new RowDefinition();
			if (fill)
				rowDefinition.Height = new GridLength(1, GridUnitType.Star);
			else
				rowDefinition.Height = GridLength.Auto;
			gridParentControls.RowDefinitions.Add(rowDefinition);

			Grid.SetRow(element, gridParentControls.RowDefinitions.Count - 1);
			gridParentControls.Children.Add(element);
		}

		private void AddChildControl(UIElement element, bool fill, int index)
		{
			if (gridChildControls.Children.Count > 0 && index > 0)
			{
				AddRowSplitter(gridChildControls, index - 1);
			}
			RowDefinition rowDefinition = new RowDefinition();
			if (fill)
				rowDefinition.Height = new GridLength(1, GridUnitType.Star);
			else
				rowDefinition.Height = GridLength.Auto;
			gridChildControls.RowDefinitions.Insert(index, rowDefinition);

			Grid.SetRow(element, index);
			gridChildControls.Children.Insert(index, element);

			if (index == 0 && gridChildControls.Children.Count > 1)
			{
				AddRowSplitter(gridChildControls, index + 1);
			}
		}

		protected void AddActions()
		{
			if (tabModel.Actions == null)
				return;

			this.tabActions = new TabActions(tabInstance, this.tabModel, tabModel.Actions as ItemCollection<TaskCreator>);
			tabActions.Initialize();
			
			AddParentControl(tabActions, false, false);
		}

		protected void AddTasks()
		{
			if (tabModel.Actions == null)
				return;
			
			if (tabModel.Tasks == null)
				tabModel.Tasks = new TaskInstanceCollection();

			this.tabTasks = new TabTasks(this.tabModel);
			tabTasks.OnSelectionChanged += ParentListSelectionChanged;
			tabTasks.Initialize();

			AddParentControl(tabTasks, false, false);
		}

		protected void AddListData() // IList items)
		{
			tabDatas.Clear();
			int index = 0;
			foreach (IList iList in tabModel.ItemList)
			{
				TabData tabData = new TabData(tabInstance, iList, tabSettings.GetData(index));
				tabData.OnSelectionChanged += ParentListSelectionChanged;
				tabData.Initialize();
				bool addSplitter = (tabDatas.Count > 0);
				AddParentControl(tabData, true, addSplitter);
				tabDatas.Add(tabData);
				index++;
			}
		}

		protected void AddChart(ChartSettings chartSettings)
		{
			//if (tabModel.ChartSettings == null)
			//	return;

			//this.tabChart = new TabChart(this.tabModel);
			//tabChart.OnSelectionChanged += ListData_OnSelectionChanged;

			//AddParentControl(tabChart, true, false);
			//gridChildControls.Children.Add(element);

			TabChart tabChart = new TabChart(tabModel, chartSettings);
			//tabCharts.Add(tabChart);

			//tabParentControls.AddControl(tabChart, false, false);
			AddParentControl(tabChart, true, false);
		}

		protected void AddBookmarks()
		{
			if (tabModel.Bookmarks == null)
				return;

			this.tabBookmarks = new TabBookmarks();
			tabBookmarks.Initialize(tabInstance);

			AddParentControl(tabBookmarks, false, true);
		}

		private void ClearControls()
		{
			foreach (TabData tabData in tabDatas)
			{
				tabData.OnSelectionChanged -= ParentListSelectionChanged;
				tabData.Dispose();
			}
			tabDatas.Clear();
			if (tabActions != null)
			{
				tabActions.Dispose();
				tabActions = null;
			}
			if (tabTasks != null)
			{
				tabTasks.OnSelectionChanged -= ParentListSelectionChanged;
				tabTasks.Dispose();
				tabTasks = null;
			}
			gridParentControls.Children.Clear();
			gridParentControls.RowDefinitions.Clear();

			foreach (Control control in childControls.Values)
			{
				IDisposable iDisposable = control as IDisposable;
				if (iDisposable != null)
					iDisposable.Dispose();
			}
			childControls.Clear();
			gridChildControls.RowDefinitions.Clear();
			gridChildControls.Children.Clear();
		}

		public void ReloadControls()
		{
			ClearControls();

			Label labelTitle = new Label();
			labelTitle.Content = tabModel.Name;
			labelTitle.FontSize = 14;
			labelTitle.Background = (SolidColorBrush)Resources[Keys.TitleBackgroundBrush];
			labelTitle.Foreground = (SolidColorBrush)Resources[Keys.TitleForegroundBrush];
			labelTitle.BorderThickness = new Thickness(1);
			labelTitle.BorderBrush = Brushes.Black;
			AddParentControl(labelTitle, false, false);

			foreach (object obj in tabModel.Objects)
			{
				if (obj is ChartSettings)
				{
					AddChart(obj as ChartSettings);
				}
			}

			AddActions();
			AddTasks();
			AddListData();
			AddBookmarks();
		}

		private void ParentListSelectionChanged(object sender, EventArgs e)
		{
			UpdateSelectedChildControls();
		}

		private void UpdateSelectedChildControls()
		{
			Dictionary<object, Control> oldChildControls = childControls;
			Dictionary<object, Control> newChildControls;
			List<Control> orderedChildControls = CreateAllChildControls(out newChildControls);
			//tabChildControls.SetControls(newChildControls, orderedChildControls);
			//UpdateSelectedTabInstances();

			this.childControls = newChildControls;

			RemoveControls(oldChildControls);
			AddControls(oldChildControls, orderedChildControls);


			UpdateSelectedTabInstances();
		}

		private void RemoveControls(Dictionary<object, Control> oldChildControls)
		{
			// Remove any children not in use anymore
			foreach (var oldChild in oldChildControls)
			{
				if (childControls.ContainsKey(oldChild.Key))
					continue;

				int index = gridChildControls.Children.IndexOf(oldChild.Value);

				gridChildControls.RowDefinitions.RemoveAt(index);
				gridChildControls.Children.RemoveAt(index);

				if (index > 0)// && index < gridChildControls.RowDefinitions.Count)
				{
					// remove splitter
					index--;
					gridChildControls.RowDefinitions.RemoveAt(index);
					gridChildControls.Children.RemoveAt(index);
				}
				else if (index == 0 && gridChildControls.Children.Count > 1)
				{
					gridChildControls.RowDefinitions.RemoveAt(index);
					gridChildControls.Children.RemoveAt(index);
				}

				IDisposable iDisposable = oldChild.Value as IDisposable;
				if (iDisposable != null)
					iDisposable.Dispose();
			}
		}

		private void AddControls(Dictionary<object, Control> oldChildControls, List<Control> orderedChildControls)
		{
			// Add all child controls to the view
			int newIndex = 0;
			foreach (Control control in orderedChildControls)
			{
				bool fill = !(control is TabNotes); // don't show for notes, needs to be configurable
				if (!oldChildControls.ContainsValue(control))
				{
					AddChildControl(control, fill, newIndex);
				}
				else
				{
					Debug.Assert(gridChildControls.Children.Count >= newIndex);
					Grid.SetRow(control, newIndex);
					if (fill)
						//rowDefinition.Height = new GridLength(1000);
						gridChildControls.RowDefinitions[newIndex].Height = new GridLength(1, GridUnitType.Star);
					else
						gridChildControls.RowDefinitions[newIndex].Height = GridLength.Auto;
					if (newIndex > 0)
					{
						UIElement splitter = gridChildControls.Children[newIndex - 1];
						Grid.SetRow(splitter, newIndex - 1);
					}
				}
				newIndex += 2;
			}
		}

		private List<Control> CreateAllChildControls(out Dictionary<object, Control> newChildControls)
		{
			//Dictionary<object, Control> oldChildControls = tabChildControls.gridControls;
			Dictionary<object, Control> oldChildControls = childControls;
			newChildControls = new Dictionary<object, Control>();
			List<Control> orderedChildControls = new List<Control>();
			/*if (tabModel.Notes != null && tabModel.Notes.Length > 0 && tabInstance.tabViewSettings.NotesVisible)
			{
				// Could add control to class instead of this
				Control controlNotes;
				if (oldChildControls.TryGetValue(tabModel.Notes, out controlNotes))
				//if (!oldChildControls.ContainsKey(tabModel.Notes))
				{
					newChildControls[tabModel.Notes] = controlNotes;
					orderedChildControls.Add(controlNotes);
				}
				else
				{
					TabModel notesTabModel = new TabModel("Notes");
					notesTabModel.AddData(tabModel.Notes);
					notesTabModel.Notes = tabModel.Notes;
					TabInstance childTabInstance = tabInstance.CreateChildTab(notesTabModel);
					//TabAvaloniaEdit tabAvaloniaEdit = new TabAvaloniaEdit(childTabInstance);
					TabNotes tabNotes = new TabNotes(childTabInstance);
					newChildControls[tabModel.Notes] = tabNotes;
					orderedChildControls.Add(tabNotes);
				}
				//GetOrCreateChildControl(oldChildControls, newChildControls, orderedChildControls, tabModel.Notes, "Notes");
				//CreateChildControls(new List<string>() { tabModel.Notes }, oldChildControls, newChildControls, orderedChildControls);
			}*/
			if (tabActions != null)
			{
				// show action help?
				//CreateChildControls(this.tabActions.SelectedItems, oldChildControls, newChildControls, orderedChildControls);
			}
			if (tabTasks != null)
			{
				CreateChildControls(this.tabTasks.SelectedItems, oldChildControls, newChildControls, orderedChildControls);
			}

			foreach (TabData tabData in tabDatas)
			{
				//bool asCharts = (tabModel.ChartSettings != null);
				CreateChildControls(tabData.SelectedItems, oldChildControls, newChildControls, orderedChildControls);
			}
			return orderedChildControls;
		}

		private void CreateChildControls(IList newControls, Dictionary<object, Control> oldChildControls, Dictionary<object, Control> newChildControls, List<Control> orderedChildControls)
		{
			foreach (object obj in newControls)
			{
				object value = obj.GetInnerValue();
				if (value == null)
					continue;

				if (oldChildControls.ContainsKey(obj))
				{
					// Reuse existing control
					Control userControl = oldChildControls[obj];
					orderedChildControls.Add(userControl);
					newChildControls.Add(obj, userControl);

					//oldChildControls.Remove(obj);
				}
				else
				{
					// Create a new control
					string name = obj.Formatted();
					if (name == null || name.Length == 0)
						name = "(" + obj.GetType().Name + ")";
					Control userControl = CreateChildControl(name, obj);
					if (userControl != null)
					{
						newChildControls[obj] = userControl;
						orderedChildControls.Add(userControl);
					}
				}
			}
		}

		private Control CreateChildControl(string name, object obj)
		{
			if (gridChildControls.ActualWidth < 30)
				return null;

			object value = obj.GetInnerValue();
			if (value == null)
				return null;

			// Can we refactor this?
			// can we move this into the instance?
			// FindMatches uses this with bookmarks
			TabBookmark bookmarkNode = null;
			if (tabInstance.tabBookmark != null && tabInstance.tabBookmark.tabChildBookmarks != null)
			{
				if (tabInstance.tabBookmark.tabChildBookmarks.TryGetValue(name, out bookmarkNode))
				{
					if (bookmarkNode.tabModel != null)
						value = bookmarkNode.tabModel;
				}
				/*foreach (Bookmark.Node node in tabInstance.bookmarkNode.nodes)
				{
					bookmarkNode = node;
					break;
				}*/
			}
			Type type = value.GetType();

			if (value is ITab)
			{
				ITab iTab = (ITab)value;
				TabInstance childTabInstance = tabInstance.CreateChildTab(iTab);
				//try
				{
					TabView tabView = new TabView(childTabInstance);
					tabView.Label = name;
					tabView.Load();
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
				tabView.tabInstance.tabBookmark = bookmarkNode;
				tabView.Label = name;
				tabView.Load();
				return tabView;
			}
			else if (value is Control)
			{
				Control control = (Control)value;
				return control;
			}
			else if (value is FilePath)
			{
				FilePath filePath = (FilePath)value;
				TabAvalonEdit tabAvalonEdit = new TabAvalonEdit(name);
				tabAvalonEdit.Load(filePath.path);
				return tabAvalonEdit;
			}
			else if (value is string || type.IsPrimitive)
			{
				TabAvalonEdit tabAvalonEdit = new TabAvalonEdit(name);
				tabAvalonEdit.Text = value.ToString();
				if (this.tabModel.Editing && obj is ListMember)
					tabAvalonEdit.EnableEditing((ListMember)obj);
				return tabAvalonEdit;
			}
			else if (value is Uri)
			{
				TabWebBrowser tabWebBrowser = new TabWebBrowser(name, (Uri)value);
				return tabWebBrowser;
			}
			else
			{
				TabModel childTabModel;
				if (value is TabModel)
				{
					childTabModel = (TabModel)value;
					childTabModel.Name = name;
				}
				else
				{
					childTabModel = TabModel.Create(name, value);
					if (childTabModel == null)
						return null;
				}
				childTabModel.Editing = this.tabModel.Editing;

				TabInstance childTabInstance = tabInstance.CreateChild(childTabModel);
				TabView tabView = new TabView(childTabInstance);
				tabView.Label = name;
				tabView.Load();
				return tabView;
			}
		}

		private void UpdateSelectedTabInstances()
		{
			tabInstance.childTabInstances.Clear();
			foreach (Control control in childControls.Values)
			{
				TabView tabView = control as TabView;
				if (tabView != null)
				{
					tabInstance.childTabInstances[control] = tabView.tabInstance;
				}
			}
		}

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

		private void toolStripMenuItemDebug_Click(object sender, EventArgs e)
		{
			TabModel debugListCollection = new TabModel("Debug");
			TabView tabView = this.Clone<TabView>(tabInstance.taskInstance.Call);
			debugListCollection.AddData(tabView);
			Control debugControl = CreateChildControl("Debug", debugListCollection);
		}

		private void Reload_Clicked(object sender, RoutedEventArgs e)
		{
			Load();
		}

		private void Reset_Clicked(object sender, RoutedEventArgs e)
		{
			Reset();
			tabInstance.SaveTabSettings();
			//Load();
		}

		private void TabInstance_OnReload(object sender, EventArgs e)
		{
			Load();
		}

		private void TabInstance_OnLoadBookmark(object sender, EventArgs e)
		{
			LoadBookmark();
		}

		private void LoadBookmark()
		{
			TabBookmark bookmarkNode = tabInstance.tabBookmark;
			tabSettings = bookmarkNode.tabViewSettings;

			//LoadTabSettings();
			int index = 0;
			foreach (TabData tabData in tabDatas)
			{
				tabData.tabDataSettings = tabSettings.GetData(index++);
				tabData.LoadSettings();

				//if (tabInstance.bookmarkNode != null)
				foreach (TabInstance tabInstance in tabInstance.childTabInstances.Values)
				{
					TabBookmark childBookmarkNode = null;
					if (bookmarkNode.tabChildBookmarks.TryGetValue(tabInstance.Label, out childBookmarkNode))
					{
						tabInstance.SelectBookmark(childBookmarkNode);
					}
				}
			}
		}

		private void TabInstance_OnClearSelection(object sender, EventArgs e)
		{
			foreach (TabData tabData in tabDatas)
			{
				tabData.SelectedItem = null; // dataGrid.UnselectAll() doesn't work
			}
		}

		private void TabInstance_OnSelectItem(object sender, TabInstance.EventSelectItem e)
		{
			tabDatas[0].SelectedItem = e.obj;
		}

		private void SaveSplitterDistance()
		{
			if (gridColumnLists.Width.IsAbsolute)
				tabSettings.SplitterDistance = (int)Math.Ceiling(gridColumnLists.Width.Value);
			else
				tabSettings.SplitterDistance = null;
			tabInstance.SaveTabSettings();
		}

		private void horizontalSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			//gridColumnLists.MaxWidth = // window width
			gridColumnLists.Width = new GridLength(1, GridUnitType.Auto);
			SaveSplitterDistance();
		}

		private void horizontalSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			SaveSplitterDistance();

			/*UpdateNearbySplitters(1, this);
			tabSettings.SplitterDistance = splitContainer.SplitterDistance;
			foreach (Control control in tableLayoutPanelLeft.Controls)
			{
				control.AutoSize = false;
				control.Width = splitContainer.SplitterDistance;
				control.AutoSize = true;
			}*/
		}

		private void rowSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			//SaveSplitterDistance();
		}

		private void gridParentControls_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (tabDatas.Count > 0)
			{
				tabDatas[0].dataGrid.Focus();
				e.Handled = true;
			}
		}

		private void UserControl_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			if (!allowAutoScrolling)
				e.Handled = true;
		}

		public void Dispose()
		{
			tabInstance.OnReload -= TabInstance_OnReload;
			tabInstance.OnLoadBookmark -= TabInstance_OnLoadBookmark;
			tabInstance.OnClearSelection -= TabInstance_OnClearSelection;
			tabInstance.OnSelectItem -= TabInstance_OnSelectItem;

			gridParentControls.MouseDown -= gridParentControls_MouseDown;
			horizontalSplitter.DragCompleted -= horizontalSplitter_DragCompleted;
			horizontalSplitter.MouseDoubleClick -= horizontalSplitter_MouseDoubleClick;

			RequestBringIntoView -= UserControl_RequestBringIntoView;

			ClearControls();

			tabInstance.Dispose();
		}

		private void DebugItem_Click(object sender, RoutedEventArgs e)
		{
			Type type = GetType();
			//Log log = new Log();
			Serializer serializer = new Serializer();
			object copy = serializer.Clone(this);
			tabModel.AddData(copy);
		}
	}
}
