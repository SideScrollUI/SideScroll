using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Atlas.UI.Avalonia.Tabs;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Atlas.UI.Avalonia.View
{
	public class TabView : Grid, IDisposable
	{
		// Maybe this control should own it's own settings?
		//private TabViewSettings _tabViewSettings = new TabViewSettings();
		internal TabViewSettings TabViewSettings
		{
			get
			{
				return tabInstance.tabViewSettings;
				//return _tabViewSettings;
			}
			set
			{
				tabInstance.tabViewSettings = value;
				//_tabViewSettings = value;
			}
		}
		public TabInstance tabInstance;
		public TabModel Model => tabInstance.Model;
		public string Label { get { return Model.Name; } set { Model.Name = value; } }

		// Created Controls
		public TabControlActions tabActions;
		public TabControlTasks tabTasks;
		public List<TabControlDataGrid> tabDatas = new List<TabControlDataGrid>();
		public List<ITabSelector> CustomTabControls { get; set; } = new List<ITabSelector>(); // should everything use this?
		public TabControlBookmarks tabBookmarks;

		// Layout Controls
		private Grid containerGrid;
		private TabControlSplitContainer tabParentControls;
		private GridSplitter parentChildGridSplitter;
		private TabControlSplitContainer tabChildControls;

		//private bool allowAutoScrolling = false; // stop new controls from triggering the ScrollView automatically

		public override string ToString() => Model.Name;

		private TabView()
		{
			tabInstance = new TabInstance();
			Initialize();
		}

		public TabView(TabInstance tabInstance)
		{
			this.tabInstance = tabInstance;
			Initialize();
		}

		public void Initialize()
		{
			// Can only be initialized once
			ColumnDefinitions = new ColumnDefinitions("Auto");
			RowDefinitions = new RowDefinitions("*");

			tabInstance.OnModelChanged += TabInstance_OnModelChanged;
			if (tabInstance is ITabSelector tabSelector)
				tabSelector.OnSelectionChanged += ParentListSelectionChanged;
		}

		public async Task LoadBackgroundAsync(Call call)
		{
			tabInstance.Invoke(ShowLoading);

			await tabInstance.ReintializeAsync(call);

			tabInstance.Invoke(ReloadControls);
		}

		private Size arrangeOverrideFinalSize;
		protected override Size ArrangeOverride(Size finalSize)
		{
			try
			{
				arrangeOverrideFinalSize = base.ArrangeOverride(finalSize);
			}
			catch (Exception)
			{
			}
			if (!childControlsFinishedLoading)
			{
				AddDispatchLoader();
				//tabInstance.SetEndLoad();
			}
			return arrangeOverrideFinalSize;
		}

		private bool childControlsFinishedLoading = false;

		// Gets called multiple times when re-initializing
		private void InitializeControls()
		{
			Background = new SolidColorBrush(Theme.BackgroundColor); // doesn't do anything
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;
			//Focusable = true;

			AddListeners();

			// don't recreate to allow reloading (sizing doesn't work otherwise)
			if (containerGrid == null)
			{
				// Use Grid instead of StackPanel
				// StackPanel doesn't translate layouts, and we want splitters if we want multiple children?
				// not filling the height vertically? splitter inside isn't
				containerGrid = new Grid()
				{
					ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto"), // Controls, Splitter, Child Tabs
					RowDefinitions = new RowDefinitions("*"), // Single Row
					HorizontalAlignment = HorizontalAlignment.Stretch,
					VerticalAlignment = VerticalAlignment.Stretch,
					//Background = new SolidColorBrush(Theme.BackgroundColor),
				};
			}
			else
			{
				containerGrid.Children.Clear();
			}

			AddParentControls();

			// skip count of 1 to save space, needs to be visible to users, and not autocollapse some types (still needs work on this one)
			//if (TabViewSettings.SplitterDistance == null)
			{
				if (tabInstance.Skippable)
				{
					containerGrid.ColumnDefinitions[0].Width = new GridLength(0);
					tabParentControls.Width = 0;
				}
			}

			AddGridColumnSplitter();
			AddChildControls();

			// don't re-add containerGrid (sizing doesn't work otherwise?)
			if (Children.Count == 0)
				Children.Add(containerGrid);

			ContextMenu = new TabViewContextMenu(this, tabInstance);

			Dispatcher.UIThread.Post(AutoSizeParentControls, DispatcherPriority.Background);
		}

		private void AutoSizeParentControls()
		{
			if (tabParentControls == null)
				return;

			int desiredWidth = (int)tabParentControls.DesiredSize.Width;
			if (Model.CustomSettingsPath != null && TabViewSettings.SplitterDistance != null)
			{
				desiredWidth = (int)TabViewSettings.SplitterDistance.Value;
			}

			containerGrid.ColumnDefinitions[0].Width = new GridLength(desiredWidth);
			tabParentControls.Width = desiredWidth;
		}

		private void AddParentControls()
		{
			tabParentControls = new TabControlSplitContainer()
			{
				ColumnDefinitions = new ColumnDefinitions("*"),
				MinDesiredWidth = Model.MinDesiredWidth,
				MaxDesiredWidth = Model.MaxDesiredWidth,
			};
			//if (TabViewSettings.SplitterDistance != null)
			//	tabParentControls.Width = (double)TabViewSettings.SplitterDistance;
			//Grid.SetColumn(tabParentControls, 0);
			containerGrid.Children.Add(tabParentControls);
			UpdateSplitterDistance();

			TabControlTitle tabTitle = new TabControlTitle(tabInstance, Model.Name);
			tabParentControls.AddControl(tabTitle, false, SeparatorType.None);
		}

		private void AddGridColumnSplitter()
		{
			parentChildGridSplitter = new GridSplitter
			{
				Background = Brushes.Black,
				VerticalAlignment = VerticalAlignment.Stretch,
				[Grid.ColumnProperty] = 1,
			};
			containerGrid.Children.Add(parentChildGridSplitter);
			parentChildGridSplitter.DragDelta += GridSplitter_DragDelta;
			parentChildGridSplitter.DragStarted += GridSplitter_DragStarted;
			parentChildGridSplitter.DragCompleted += GridSplitter_DragCompleted; // bug, this is firing when double clicking splitter
			parentChildGridSplitter.DoubleTapped += GridSplitter_DoubleTapped;
		}

		private void AddChildControls()
		{
			tabChildControls = new TabControlSplitContainer()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto"),
				//MinWidth = 100,
			};
			Grid.SetColumn(tabChildControls, 2);
			containerGrid.Children.Add(tabChildControls);
		}

		private void AddListeners()
		{
			tabInstance.OnRefresh += TabInstance_OnRefresh;
			tabInstance.OnReload += TabInstance_OnReload;
			tabInstance.OnLoadBookmark += TabInstance_OnLoadBookmark;
			tabInstance.OnClearSelection += TabInstance_OnClearSelection; // data controls should attach these instead?
			tabInstance.OnSelectItem += TabInstance_OnSelectItem;
			tabInstance.OnResize += TabInstance_OnResize;
		}

		private void RemoveListeners()
		{
			tabInstance.OnRefresh -= TabInstance_OnRefresh;
			tabInstance.OnReload -= TabInstance_OnReload;
			tabInstance.OnLoadBookmark -= TabInstance_OnLoadBookmark;
			tabInstance.OnClearSelection -= TabInstance_OnClearSelection;
			tabInstance.OnSelectItem -= TabInstance_OnSelectItem;
		}

		private void TabInstance_OnModelChanged(object sender, EventArgs e)
		{
			ReloadControls();
		}

		private void TabInstance_OnResize(object sender, EventArgs e)
		{
			containerGrid.ColumnDefinitions[0].Width = GridLength.Auto;
		}

		private void GridSplitter_DragDelta(object sender, global::Avalonia.Input.VectorEventArgs e)
		{
			if (TabViewSettings.SplitterDistance != null)
				tabParentControls.Width = containerGrid.ColumnDefinitions[0].ActualWidth;

			// force the width to update (Grid Auto Size caching problem?
			double width = containerGrid.ColumnDefinitions[0].ActualWidth;
			TabViewSettings.SplitterDistance = width;
			tabParentControls.Width = width;

			// remove these lines? do they do anything?
			InvalidateMeasure();
			InvalidateArrange();
			tabParentControls.InvalidateArrange();
			tabParentControls.InvalidateMeasure();

			//if (TabViewSettings.SplitterDistance != null)
			//	containerGrid.ColumnDefinitions[0].Width = new GridLength((double)containerGrid.ColumnDefinitions[1].);
		}

		private bool isDragging = false;
		private void GridSplitter_DragStarted(object sender, global::Avalonia.Input.VectorEventArgs e)
		{
			isDragging = true;
		}

		private void GridSplitter_DragCompleted(object sender, global::Avalonia.Input.VectorEventArgs e)
		{
			if (isDragging == false)
				return;
			isDragging = false;
			InvalidateMeasure();
			InvalidateArrange();
			//TabViewSettings.SplitterDistance = (int)Math.Ceiling(e.Vector.Y); // backwards
			double width = (int)containerGrid.ColumnDefinitions[0].ActualWidth;
			TabViewSettings.SplitterDistance = width;
			tabParentControls.Width = width;
			containerGrid.ColumnDefinitions[0].Width = new GridLength(width);
			//UpdateSplitterDistance();
			SaveSplitterDistance();
			UpdateSplitterFiller();
			tabParentControls.InvalidateMeasure();
		}

		// doesn't resize bigger well
		// The Drag start, delta, and complete get called for this too. Which makes this really hard to do well
		private void GridSplitter_DoubleTapped(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			isDragging = false;
			double desiredWidth = tabParentControls.DesiredSize.Width;
			TabViewSettings.SplitterDistance = desiredWidth;
			tabParentControls.Width = desiredWidth;
			//containerGrid.ColumnDefinitions[0].Width = new GridLength(desiredWidth);
			containerGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
			//containerGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);

			containerGrid.InvalidateArrange();
			containerGrid.InvalidateMeasure();

			//tabParentControls.grid.Width = new GridLength(1, GridUnitType.Auto);
			//tabParentControls.grid.Width = tabParentControls.grid.DesiredSize.Width;
			//tabParentControls.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
			//tabParentControls.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
			/*tabParentControls.InvalidateArrange();
			tabParentControls.InvalidateMeasure();
			tabParentControls.Arrange(this.Bounds);
			tabParentControls.Measure(this.Bounds.Size);
			//containerGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
			containerGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
			containerGrid.InvalidateArrange();
			containerGrid.InvalidateMeasure();
			containerGrid.Measure(this.Bounds.Size);*/

			//containerGrid.ColumnDefinitions[0].Width = new GridLength(tabParentControls.DesiredSize.Width); // DesiredSize too large, can we try not setting grid width?
			SaveSplitterDistance();
			UpdateSplitterFiller();
		}

		private void SaveSplitterDistance()
		{
			/*if (gridColumnLists.Width.IsAbsolute)
				tabConfiguration.SplitterDistance = (int)Math.Ceiling(gridColumnLists.Width.Value);
			else
				tabConfiguration.SplitterDistance = null;*/
			tabInstance.SaveTabSettings();
		}

		public void UpdateSplitterDistance()
		{
			if (containerGrid == null)
				return;

			if (tabInstance.tabViewSettings.SplitterDistance == null || tabInstance.tabViewSettings.SplitterDistance <= 0.0)
			{
				containerGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
			}
			else
			{
				containerGrid.ColumnDefinitions[0].Width = new GridLength((int)TabViewSettings.SplitterDistance);
				if (tabParentControls != null)
					tabParentControls.Width = (double)TabViewSettings.SplitterDistance;
			}
		}

		public bool IsLoaded { get; set; } = false;

		public void ReloadControls()
		{
			ClearControls();

			InitializeControls();

			AddObjects();
			AddActions();
			AddTasks();
			AddData();

			AddBookmarks(); // todo
			IsLoaded = true;

			UpdateChildControls();
		}

		protected void AddObjects()
		{
			foreach (TabObject tabObject in Model.Objects)
			{
				object obj = tabObject.obj;
				if (obj is ChartSettings chartSettings)
				{
					AddChart(chartSettings);
				}
				else if (obj is TabToolbar toolbar)
				{
					AddToolbar(toolbar);
				}
				else if (obj is ITabSelector tabSelector)
				{
					AddITabControl(tabSelector, tabObject.fill);
				}
				else if (obj is Control control)
				{
					AddControl(control, tabObject.fill);
				}
				else if (obj is string text)
				{
					AddControlString(text);
				}
				else
				{
					ParamsAttribute attribute = obj.GetType().GetCustomAttribute<ParamsAttribute>();
					if (attribute != null)
					{
						AddControl(new TabControlParams(tabInstance, obj), tabObject.fill);
					}
				}
			}
		}

		private void AddToolbar(TabToolbar toolbar)
		{
			var properties = toolbar.GetType().GetVisibleProperties();
			var toolbarControl = new TabControlToolbar();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.GetCustomAttribute<SeparatorAttribute>() != null)
					toolbarControl.AddSeparator();
				var propertyValue = propertyInfo.GetValue(toolbar);
				if (propertyValue is ToolButton toolButton)
				{
					var buttonControl = toolbarControl.AddButton(toolButton.Label, toolButton.Icon);
					buttonControl.Add(toolButton.Action);
				}
				else if (propertyValue is string text)
				{
					toolbarControl.AddLabel(text);
				}
			}
			AddControl(toolbarControl, false);
		}

		protected void AddActions()
		{
			if (Model.Actions == null)
				return;

			tabActions = new TabControlActions(tabInstance, Model, Model.Actions as ItemCollection<TaskCreator>);

			tabParentControls.AddControl(tabActions, false, SeparatorType.Spacer);
		}

		protected void AddTasks()
		{
			if (Model.Actions == null)
				return;

			//if (tabModel.Tasks == null)
			//	tabModel.Tasks = new TaskInstanceCollection();

			tabTasks = new TabControlTasks(tabInstance);
			tabTasks.OnSelectionChanged += ParentListSelectionChanged;

			tabParentControls.AddControl(tabTasks, false, SeparatorType.Spacer);
		}

		protected void AddChart(ChartSettings chartSettings)
		{
			foreach (var listGroupPair in chartSettings.ListGroups)
			{
				TabControlChart tabChart = new TabControlChart(tabInstance, listGroupPair.Value);

				tabParentControls.AddControl(tabChart, false, SeparatorType.Spacer);
				//tabChart.OnSelectionChanged += ListData_OnSelectionChanged;
			}
		}

		protected void AddData()
		{
			tabDatas.Clear();
			int index = 0;
			foreach (IList iList in Model.ItemList)
			{
				TabControlDataGrid tabData = new TabControlDataGrid(tabInstance, iList, true, TabViewSettings.GetData(index));
				//tabData.HorizontalAlignment = HorizontalAlignment.Stretch;
				//tabData.VerticalAlignment = VerticalAlignment.Stretch;
				tabData.OnSelectionChanged += ParentListSelectionChanged;
				bool addSplitter = (tabDatas.Count > 0);
				tabParentControls.AddControl(tabData, true, SeparatorType.Splitter);
				tabDatas.Add(tabData);
				index++;
			}
		}

		// should we check for a Grid stretch instead of passing that parameter?
		protected void AddControl(Control control, bool fill)
		{
			//tabData.OnSelectionChanged += ParentListSelectionChanged;
			tabParentControls.AddControl(control, fill, SeparatorType.Spacer);
		}

		protected void AddITabControl(ITabSelector control, bool fill)
		{
			control.OnSelectionChanged += ParentListSelectionChanged;
			tabParentControls.AddControl((Control)control, fill, SeparatorType.Spacer);
			CustomTabControls.Add(control);
		}

		protected void AddControlString(string text)
		{
			/*TextBlock textBlock = new TextBlock()
			{
				Text = text,
				TextWrapping = TextWrapping.Wrap,
				MaxWidth = 1000,
				Foreground = new SolidColorBrush(Colors.White),
				FontSize = 16,
				Margin = new Thickness(4),
			};*/
			TextBox textBox = new TextBox()
			{
				Text = text,
				Foreground = new SolidColorBrush(Colors.White),
				Background = Brushes.Transparent,
				BorderThickness = new Thickness(0),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				IsReadOnly = true,
				FontSize = 16,
				Padding = new Thickness(6, 3),
				//Margin = new Thickness(4),
				//Focusable = true, // already set?
				MinWidth = 50,
				MaxWidth = 1000,
				TextWrapping = TextWrapping.Wrap,
				//TextWrapping = TextWrapping.Wrap, // would be a useful feature if it worked
				//[Grid.RowProperty] = rowIndex,
				//[Grid.ColumnProperty] = columnIndex,
			};
			//control.OnSelectionChanged += ParentListSelectionChanged;
			tabParentControls.AddControl(textBox, false, SeparatorType.Spacer);
		}

		protected void AddBookmarks()
		{
			/*
			//if (tabModel.Bookmarks == null)
				return;

			tabBookmarks = new TabControlBookmarks(tabInstance);

			tabParentControls.AddControl(tabBookmarks, false, SeparatorType.Splitter);
			*/
		}

		public void Invalidate()
		{
			tabInstance.loadCalled = false;
		}

		public void Load()
		{
			if (tabInstance.loadCalled)
				return;
			tabInstance.loadCalled = true;

			tabInstance.StartAsync(LoadBackgroundAsync);

			//Dispatcher.BeginInvoke((Action)(() => { allowAutoScrolling = true; }));
		}

		public void ShowLoading()
		{
			ClearControls();

			// This will get cleared when the view reloads
			var progressBar = new ProgressBar()
			{
				IsIndeterminate = true,
				MinWidth = 100,
				MinHeight = 100,
				MaxWidth = 200,
				Foreground = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor),
				Background = new SolidColorBrush(Theme.BackgroundColor),
				HorizontalAlignment = HorizontalAlignment.Left,
			};

			Children.Add(progressBar);
		}

		public void LoadSettings()
		{
			//allowAutoScrolling = false;

			if (tabInstance.tabBookmark != null && tabInstance.tabBookmark.tabViewSettings != null)
				tabInstance.tabViewSettings = tabInstance.tabBookmark.tabViewSettings;
			else if (tabInstance.project.userSettings.AutoLoad)
				LoadDefaultTabSettings();
		}

		private void LoadDefaultTabSettings()
		{
			TabViewSettings = tabInstance.LoadDefaultTabSettings();
		}

		// Throttles updating selectedChildControls
		// todo: extract
		private DispatcherTimer dispatcherTimer;  // delays auto selection to throttle updates
		private bool updateChildControls = false;
		private void DispatcherTimer_Tick(object sender, EventArgs e)
		{
			if (updateChildControls)
			{
				UpdateChildControls();
			}
		}

		private void AddDispatchLoader()
		{
			if (dispatcherTimer == null)
			{
				dispatcherTimer = new DispatcherTimer();
				dispatcherTimer.Tick += DispatcherTimer_Tick;
				dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10); // Every 10 milliseconds
				dispatcherTimer.Start();
			}
		}

		private void ClearDispatchLoader()
		{
			if (dispatcherTimer == null)
			{
				dispatcherTimer = new DispatcherTimer();
				dispatcherTimer.Stop();
				dispatcherTimer.Tick -= DispatcherTimer_Tick;
				dispatcherTimer = null;
			}
		}

		private bool ShowChildren
		{
			get
			{
				if (IsArrangeValid == false)
					return false;

				// Only add children if they'll be visible
				if (IsVisible == false)
					return false;
				if (tabChildControls == null) // TabTasks hiding can sometimes trigger this, todo: figure out why
					return false;

				if (Bounds.Height < 50)
					return false;

				if (tabInstance.Depth > 50)
					return false;
				if (double.IsNaN(tabParentControls.arrangeOverrideFinalSize.Width))
					return false;
				if (tabChildControls.Width < 30)
					return false;

				//if (tabParentControls.arrangeOverrideFinalSize.Width < 30)
				//	return false;
				//if (tabParentControls.Width < 30) // this breaks if we collapse splitter
				//	return false;
				//if (double.IsNaN(tabParentControls.Width))
				//	return false;
				//if (arrangeOverrideFinalSize.Width < 30)
				//	return false;
				//if (Bounds.Width < 30 || double.IsNaN(Bounds.Width))
				//	return false;
				//if (tabChildControls.arrangeOverrideFinalSize.Width < 30) // doesn't work if this is in a AutoSize container
				//	return false;
				//if (double.IsNaN(tabChildControls.arrangeOverrideFinalSize.Width))
				//	return false;

				//if (rendered == false)
				//	return false;

				// don't show if the new control won't have enough room
				IControl control = Parent;
				double offset = tabChildControls.Bounds.X;
				while (control != null)
				{
					if (control is ScrollViewer scrollViewer)
					{
						if (offset - scrollViewer.Offset.X > scrollViewer.Bounds.Width)
							return false;
						break;
					}
					else
					{
						offset += control.Bounds.X;
						control = control.Parent;
					}
				}
				//GetControlOffset(Parent);
				//var window = (BaseWindow)VisualRoot;
				//window.scrollViewer.View

				return true;
			}
		}

		// The GridSplitter doesn't work well if there's not a control on each side of the splitter, so add a filler panel
		/*private double GetControlOffset(IControl control)
		{
			if (cont
		}*/
		/*public class FillerPanel : Panel
		{
			public FillerPanel()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch;
				VerticalAlignment = VerticalAlignment.Stretch;
				Background = new SolidColorBrush(Colors.Azure);
			}

			protected override Size MeasureCore(Size availableSize)
			{
				Size size = base.MeasureCore(availableSize);
				return size;
			}

			protected override Size MeasureOverride(Size availableSize)
			{
				Size size = base.MeasureOverride(availableSize);
				return size;
			}

			protected override void ArrangeCore(Rect finalRect)
			{
				base.ArrangeCore(finalRect);
			}

			protected override Size ArrangeOverride(Size finalSize)
			{
				Size size = base.ArrangeOverride(finalSize);
				return size;
			}
		}*/

		private double GetFillerPanelWidth()
		{
			IControl control = Parent;
			double offset = tabChildControls.Bounds.X;
			while (control != null)
			{
				if (control is ScrollViewer scrollViewer)
				{
					double width = scrollViewer.Bounds.Width - (offset - scrollViewer.Offset.X);
					return Math.Max(0, width - 10);
					//return width;
				}
				else
				{
					offset += control.Bounds.X;
					control = control.Parent;
				}
			}
			return 0;
		}

		public void UpdateSplitterFiller()
		{
			if (fillerPanel != null)
				fillerPanel.Width = GetFillerPanelWidth();
		}

		private const string tempPanelId = "TempPanelID";
		private Panel fillerPanel;
		private void UpdateChildControls()
		{
			// These need to be set regardless of if the children show
			if (tabDatas.Count > 0)
				tabInstance.SelectedItems = tabDatas[0].SelectedItems;

			if (ShowChildren == false)
			{
				updateChildControls = true;
				AddDispatchLoader();
				// Invoking was happening at bad times in the data binding
				return;
			}
			ClearDispatchLoader();
			updateChildControls = false;

			childControlsFinishedLoading = true;

			BaseWindow.baseWindow.SetMinScrollOffset();

			// Create new child controls
			//Dictionary<object, Control> oldChildControls = tabChildControls.gridControls;
			//tabChildControls.gridControls = new Dictionary<object, Control>();

			List<Control> orderedChildControls = CreateAllChildControls(out Dictionary<object, Control> newChildControls);

			fillerPanel = null;

			// Add a filler panel so the grid splitter can drag to the right
			if (orderedChildControls.Count == 0)
			{
				fillerPanel = new Panel()
				{
					Width = GetFillerPanelWidth(), // should update this after moving grid splitter
				};
				orderedChildControls.Add(fillerPanel);
				newChildControls[tempPanelId] = fillerPanel;
			}
			tabChildControls.SetControls(newChildControls, orderedChildControls);
			UpdateSelectedTabInstances();
		}

		private List<Control> CreateAllChildControls(out Dictionary<object, Control> newChildControls)
		{
			Dictionary<object, Control> oldChildControls = tabChildControls.gridControls;
			newChildControls = new Dictionary<object, Control>();
			List<Control> orderedChildControls = new List<Control>();
			//AddNotes(newChildControls, oldChildControls, orderedChildControls);

			if (tabInstance is ITabSelector tabSelector && tabSelector.SelectedItems != null)
			{
				CreateChildControls(tabSelector.SelectedItems, oldChildControls, newChildControls, orderedChildControls, tabSelector);
			}

			foreach (ITabSelector tabControl in CustomTabControls)
			{
				CreateChildControls(tabControl.SelectedItems, oldChildControls, newChildControls, orderedChildControls, tabControl);
			}
			if (tabActions != null)
			{
				// show action help?
				//CreateChildControls(tabActions.SelectedItems, oldChildControls, newChildControls, orderedChildControls);
			}
			if (tabTasks != null && (tabTasks.IsVisible || Model.Tasks?.Count > 0))
			{
				CreateChildControls(tabTasks.SelectedItems, oldChildControls, newChildControls, orderedChildControls);
			}

			foreach (TabControlDataGrid tabData in tabDatas)
			{
				CreateChildControls(tabData.SelectedItems, oldChildControls, newChildControls, orderedChildControls);
			}
			return orderedChildControls;
		}

		internal void CreateChildControls(IList newList, Dictionary<object, Control> oldChildControls, Dictionary<object, Control> newChildControls, List<Control> orderedChildControls, ITabSelector tabControl = null)
		{
			//var collection = newList as DataGridSelectedItemsCollection;
			//if (collection != null && collection.)
			//	newList.

			foreach (object obj in newList)
			{
				if (newChildControls.Count >= tabInstance.project.userSettings.SubTabLimit)
					break;
				GetOrCreateChildControl(oldChildControls, newChildControls, orderedChildControls, obj, null, tabControl);
			}
		}

		private void GetOrCreateChildControl(Dictionary<object, Control> oldChildControls, Dictionary<object, Control> newChildControls, List<Control> orderedChildControls, object obj, string label = null, ITabSelector tabControl = null)
		{
			//var collection = newList as DataGridSelectedItemsCollection;
			//if (collection != null && collection.)
			//	newList.

			// duplicate work
			//object value = obj.GetInnerValue(); // performance issues? cache this?
			//if (value == null)
			//	return;

			if (oldChildControls.ContainsKey(obj))
			{
				// Reuse existing control
				Control control = oldChildControls[obj];
				newChildControls.Add(obj, control);
				orderedChildControls.Add(control);

				//oldChildControls.Remove(obj);
			}
			else
			{
				// Create a new control
				Control control = CreateChildControl(obj, label, tabControl);
				if (control != null)
				{
					newChildControls[obj] = control;
					orderedChildControls.Add(control);
				}
			}
		}

		internal Control CreateChildControl(object obj, string label = null, ITabSelector tabControl = null)
		{
			try
			{
				return TabCreator.CreateChildControl(tabInstance, obj, label, tabControl);
			}
			catch (Exception e)
			{
				// Add instructions for enabling debugger to catch these
				//call.log.AddError(e.Message);
				return TabCreator.CreateChildControl(tabInstance, e, "Caught Exception", tabControl);
			}
		}

		private void UpdateSelectedTabInstances()
		{
			tabInstance.childTabInstances.Clear();
			foreach (Control control in tabChildControls.gridControls.Values)
			{
				if (control is TabView tabView)
				{
					tabInstance.childTabInstances.Add(control, tabView.tabInstance);
				}
			}
		}

		private void ParentListSelectionChanged(object sender, EventArgs e)
		{
			UpdateChildControls();
		}

		private void ClearControls()
		{
			IsLoaded = false;
			RemoveListeners();

			ClearDispatchLoader();

			//gridParentControls.MouseDown -= gridParentControls_MouseDown;
			//horizontalSplitter.DragCompleted -= horizontalSplitter_DragCompleted;
			//horizontalSplitter.MouseDoubleClick -= horizontalSplitter_MouseDoubleClick;

			//RequestBringIntoView -= UserControl_RequestBringIntoView;

			//tabDatas.Clear();
			foreach (TabControlDataGrid tabData in tabDatas)
			{
				tabData.OnSelectionChanged -= ParentListSelectionChanged;
				tabData.Dispose();
			}
			tabDatas.Clear();
			if (tabActions != null)
			{
				//tabActions.Dispose();
				tabActions = null;
			}
			if (tabTasks != null)
			{
				tabTasks.OnSelectionChanged -= ParentListSelectionChanged;
				tabTasks.Dispose();
				tabTasks = null;
			}
			if (tabParentControls != null)
			{
				tabParentControls.Clear();
				tabParentControls = null;
			}
			if (tabChildControls != null)
			{
				tabChildControls.Clear();
				tabChildControls = null;
			}

			//LogicalChildren.Clear();
			Children.Clear();
			CustomTabControls.Clear();
		}

		private void TabInstance_OnRefresh(object sender, EventArgs e)
		{
			Load();
		}

		private void TabInstance_OnReload(object sender, EventArgs e)
		{
			//tabInstance.Reintialize(true);
			Load();
		}

		private void TabInstance_OnClearSelection(object sender, EventArgs e)
		{
			foreach (var tabData in tabDatas)
			{
				tabData.SelectedItem = null; // dataGrid.UnselectAll() doesn't work
			}
		}

		private void TabInstance_OnLoadBookmark(object sender, EventArgs e)
		{
			LoadBookmark();
		}

		private void TabInstance_OnSelectItem(object sender, TabInstance.EventSelectItem e)
		{
			if (tabDatas.Count > 0)
				tabDatas[0].SelectedItem = e.obj;
		}

		private void LoadBookmark()
		{
			tabInstance.project.userSettings.AutoLoad = true;

			TabBookmark tabBookmark = tabInstance.tabBookmark;
			TabViewSettings = tabBookmark.tabViewSettings;

			int index = 0;
			foreach (TabControlDataGrid tabData in tabDatas)
			{
				tabData.tabDataSettings = TabViewSettings.GetData(index++);
				tabData.LoadSettings();

				//if (tabInstance.tabBookmark != null)
				foreach (TabInstance childTabInstance in tabInstance.childTabInstances.Values)
				{
					if (tabBookmark.tabChildBookmarks.TryGetValue(childTabInstance.Label, out TabBookmark childBookmarkNode))
					{
						childTabInstance.SelectBookmark(childBookmarkNode);
					}
				}
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				ClearControls();

				tabInstance.Dispose();

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~TabView() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}

/*

private void UpdateSelectedTabInstances()
{
	tabInstance.children.Clear();
	foreach (Control control in childControls.Values)
	{
		TabView tabView = control as TabView;
		if (tabView != null)
		{
			tabInstance.children.Add(tabView.tabInstance);
		}
	}
}

private void UpdateNearbySplitters(int depth, TabView triggeredControl)
{
	// todo: use max SplitterDistance if not user triggered
	if (call.parent != null)
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
	}
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
	tabConfiguration.SplitterDistance = splitContainer.SplitterDistance;
	foreach (Control control in tableLayoutPanelLeft.Controls)
	{
		control.AutoSize = false;
		control.Width = splitContainer.SplitterDistance;
		control.AutoSize = true;
	}*//*
}

private void verticalGridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
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

*/
