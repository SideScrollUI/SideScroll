using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Atlas.UI.Avalonia.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Atlas.UI.Avalonia.View
{
	public class TabView : Grid, IDisposable
	{
		private const string TempPanelId = "TempPanelID";

		// Maybe this control should own it's own settings?
		//private TabViewSettings _tabViewSettings = new TabViewSettings();
		internal TabViewSettings TabViewSettings
		{
			get
			{
				return Instance.TabViewSettings;
				//return _tabViewSettings;
			}
			set
			{
				Instance.TabViewSettings = value;
				//_tabViewSettings = value;
			}
		}
		public TabInstance Instance;
		public TabModel Model => Instance.Model;
		public string Label { get { return Model.Name; } set { Model.Name = value; } }

		// Created Controls
		public TabControlActions TabActions;
		public TabControlTasks TabTasks;
		public List<TabControlDataGrid> TabDatas = new List<TabControlDataGrid>();
		public List<ITabSelector> CustomTabControls { get; set; } = new List<ITabSelector>(); // should everything use this?

		// Layout Controls
		private Grid _containerGrid;
		private TabControlSplitContainer _tabParentControls;
		private TabControlTitle _tabTitle;
		private GridSplitter _parentChildGridSplitter;
		private TabControlSplitContainer _tabChildControls;
		private Panel _fillerPanel; // GridSplitter doesn't work without control on right side

		private Size _arrangeOverrideFinalSize;
		private bool _childControlsFinishedLoading = false;
		private bool _isDragging = false;

		// Throttles updating selectedChildControls
		// todo: extract
		private DispatcherTimer _dispatcherTimer;  // delays auto selection to throttle updates
		private bool _updateChildControls = false;

		public override string ToString() => Model.Name;

		private TabView()
		{
			Instance = new TabInstance();
			Initialize();
		}

		public TabView(TabInstance tabInstance)
		{
			Instance = tabInstance;
			Initialize();
		}

		public void Initialize()
		{
			// Can only be initialized once
			ColumnDefinitions = new ColumnDefinitions("Auto");
			RowDefinitions = new RowDefinitions("*");

			Instance.OnModelChanged += TabInstance_OnModelChanged;
			if (Instance is ITabSelector tabSelector)
				tabSelector.OnSelectionChanged += ParentListSelectionChanged;
		}

		public async Task LoadBackgroundAsync(Call call)
		{
			Instance.Invoke(ShowLoading);

			await Instance.ReintializeAsync(call);

			Instance.Invoke(ReloadControls);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			try
			{
				_arrangeOverrideFinalSize = base.ArrangeOverride(finalSize);
			}
			catch (Exception)
			{
			}
			if (!_childControlsFinishedLoading)
			{
				AddDispatchLoader();
				//tabInstance.SetEndLoad();
			}
			return _arrangeOverrideFinalSize;
		}

		// Gets called multiple times when re-initializing
		private void InitializeControls()
		{
			Background = Theme.TabBackground; // doesn't do anything
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;
			//Focusable = true;

			AddListeners();

			// don't recreate to allow reloading (sizing doesn't work otherwise)
			if (_containerGrid == null)
			{
				// Use Grid instead of StackPanel
				// StackPanel doesn't translate layouts, and we want splitters if we want multiple children?
				// not filling the height vertically? splitter inside isn't
				_containerGrid = new Grid()
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
				_containerGrid.Children.Clear();
			}

			AddParentControls();

			// skip count of 1 to save space, needs to be visible to users, and not autocollapse some types (still needs work on this one)
			//if (TabViewSettings.SplitterDistance == null)
			{
				if (Instance.Skippable)
				{
					_containerGrid.ColumnDefinitions[0].Width = new GridLength(0);
					_tabParentControls.Width = 0;
				}
			}

			AddGridColumnSplitter();
			AddChildControls();

			// don't re-add containerGrid (sizing doesn't work otherwise?)
			if (Children.Count == 0)
				Children.Add(_containerGrid);

			ContextMenu = new TabViewContextMenu(this, Instance);

			Dispatcher.UIThread.Post(AutoSizeParentControls, DispatcherPriority.Background);
		}

		private void AutoSizeParentControls()
		{
			if (_tabParentControls == null)
				return;

			int desiredWidth = (int)_tabParentControls.DesiredSize.Width;
			if (Model.CustomSettingsPath != null && TabViewSettings.SplitterDistance != null)
			{
				desiredWidth = (int)TabViewSettings.SplitterDistance.Value;
			}

			_containerGrid.ColumnDefinitions[0].Width = new GridLength(desiredWidth);
			_tabParentControls.Width = desiredWidth;
		}

		private void AddParentControls()
		{
			_tabParentControls = new TabControlSplitContainer()
			{
				ColumnDefinitions = new ColumnDefinitions("*"),
				MinDesiredWidth = Model.MinDesiredWidth,
				MaxDesiredWidth = Model.MaxDesiredWidth,
			};
			//if (TabViewSettings.SplitterDistance != null)
			//	tabParentControls.Width = (double)TabViewSettings.SplitterDistance;
			//Grid.SetColumn(tabParentControls, 0);
			_containerGrid.Children.Add(_tabParentControls);
			UpdateSplitterDistance();

			_tabTitle = new TabControlTitle(Instance, Model.Name);
			_tabParentControls.AddControl(_tabTitle, false, SeparatorType.None);
		}

		private void AddGridColumnSplitter()
		{
			_parentChildGridSplitter = new GridSplitter
			{
				Background = Brushes.Black,
				VerticalAlignment = VerticalAlignment.Stretch,
				[Grid.ColumnProperty] = 1,
			};
			_containerGrid.Children.Add(_parentChildGridSplitter);
			_parentChildGridSplitter.DragDelta += GridSplitter_DragDelta;
			_parentChildGridSplitter.DragStarted += GridSplitter_DragStarted;
			_parentChildGridSplitter.DragCompleted += GridSplitter_DragCompleted; // bug, this is firing when double clicking splitter
			_parentChildGridSplitter.DoubleTapped += GridSplitter_DoubleTapped;

			//AddLinkButton();
		}

		private void AddLinkButton()
		{
			// todo: add more checks for validity
			if (!Instance.IsLinkable)
				return;

			var linkButton = new TabControlButton
			{
				VerticalAlignment = VerticalAlignment.Top,
				Height = 26,
				Content = "~",
				Padding = new Thickness(1),
				[Grid.ColumnProperty] = 1,
			};
			linkButton.Click += LinkButton_Click;
			_containerGrid.Children.Add(linkButton);
		}

		private async void LinkButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Bookmark bookmark = Instance.CreateBookmark();
			string uri = Instance.Project.Linker.GetLinkUri(new Call(), bookmark);
			await ClipBoardUtils.SetTextAsync(uri);
		}

		private void AddChildControls()
		{
			_tabChildControls = new TabControlSplitContainer()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto"),
				//MinWidth = 100,
			};
			Grid.SetColumn(_tabChildControls, 2);
			_containerGrid.Children.Add(_tabChildControls);
		}

		private void AddListeners()
		{
			Instance.OnRefresh += TabInstance_OnRefresh;
			Instance.OnReload += TabInstance_OnReload;
			Instance.OnLoadBookmark += TabInstance_OnLoadBookmark;
			Instance.OnClearSelection += TabInstance_OnClearSelection; // data controls should attach these instead?
			Instance.OnSelectItem += TabInstance_OnSelectItem;
			Instance.OnResize += TabInstance_OnResize;
		}

		private void RemoveListeners()
		{
			Instance.OnRefresh -= TabInstance_OnRefresh;
			Instance.OnReload -= TabInstance_OnReload;
			Instance.OnLoadBookmark -= TabInstance_OnLoadBookmark;
			Instance.OnClearSelection -= TabInstance_OnClearSelection;
			Instance.OnSelectItem -= TabInstance_OnSelectItem;
			Instance.OnResize -= TabInstance_OnResize;
		}

		private void TabInstance_OnModelChanged(object sender, EventArgs e)
		{
			ReloadControls();
		}

		private void TabInstance_OnResize(object sender, EventArgs e)
		{
			_containerGrid.ColumnDefinitions[0].Width = GridLength.Auto;
		}

		private void GridSplitter_DragDelta(object sender, global::Avalonia.Input.VectorEventArgs e)
		{
			if (TabViewSettings.SplitterDistance != null)
				_tabParentControls.Width = _containerGrid.ColumnDefinitions[0].ActualWidth;

			// force the width to update (Grid Auto Size caching problem?
			double width = _containerGrid.ColumnDefinitions[0].ActualWidth;
			TabViewSettings.SplitterDistance = width;
			_tabParentControls.Width = width;

			// remove these lines? do they do anything?
			InvalidateMeasure();
			InvalidateArrange();
			_tabParentControls.InvalidateArrange();
			_tabParentControls.InvalidateMeasure();

			//if (TabViewSettings.SplitterDistance != null)
			//	containerGrid.ColumnDefinitions[0].Width = new GridLength((double)containerGrid.ColumnDefinitions[1].);
		}

		private void GridSplitter_DragStarted(object sender, global::Avalonia.Input.VectorEventArgs e)
		{
			_isDragging = true;
		}

		private void GridSplitter_DragCompleted(object sender, global::Avalonia.Input.VectorEventArgs e)
		{
			if (_isDragging == false)
				return;
			_isDragging = false;
			InvalidateMeasure();
			InvalidateArrange();
			//TabViewSettings.SplitterDistance = (int)Math.Ceiling(e.Vector.Y); // backwards
			double width = (int)_containerGrid.ColumnDefinitions[0].ActualWidth;
			TabViewSettings.SplitterDistance = width;
			_tabParentControls.Width = width;
			_containerGrid.ColumnDefinitions[0].Width = new GridLength(width);
			//UpdateSplitterDistance();
			SaveSplitterDistance();
			UpdateSplitterFiller();
			_tabParentControls.InvalidateMeasure();
		}

		// doesn't resize bigger well
		// The Drag start, delta, and complete get called for this too. Which makes this really hard to do well
		private void GridSplitter_DoubleTapped(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			_isDragging = false;
			double desiredWidth = _tabParentControls.DesiredSize.Width;
			TabViewSettings.SplitterDistance = desiredWidth;
			_tabParentControls.Width = desiredWidth;
			//containerGrid.ColumnDefinitions[0].Width = new GridLength(desiredWidth);
			_containerGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
			//containerGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);

			_containerGrid.InvalidateArrange();
			_containerGrid.InvalidateMeasure();

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
			Instance.SaveTabSettings();
		}

		public void UpdateSplitterDistance()
		{
			if (_containerGrid == null)
				return;

			if (Instance.TabViewSettings.SplitterDistance == null || Instance.TabViewSettings.SplitterDistance <= 0.0)
			{
				_containerGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
			}
			else
			{
				_containerGrid.ColumnDefinitions[0].Width = new GridLength((int)TabViewSettings.SplitterDistance);
				if (_tabParentControls != null)
					_tabParentControls.Width = (double)TabViewSettings.SplitterDistance;
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

			IsLoaded = true;

			UpdateChildControls();
		}

		protected void AddObjects()
		{
			foreach (TabObject tabObject in Model.Objects)
			{
				object obj = tabObject.Object;
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
					AddITabControl(tabSelector, tabObject.Fill);
				}
				else if (obj is Control control)
				{
					AddControl(control, tabObject.Fill);
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
						AddControl(new TabControlParams(Instance, obj), tabObject.Fill);
					}
				}
			}
		}

		private void AddToolbar(TabToolbar toolbar)
		{
			var toolbarControl = new TabControlToolbar(Instance, toolbar);
			AddControl(toolbarControl, false);
		}

		protected void AddActions()
		{
			if (Model.Actions == null)
				return;

			TabActions = new TabControlActions(Instance, Model, Model.Actions as ItemCollection<TaskCreator>);

			_tabParentControls.AddControl(TabActions, false, SeparatorType.Spacer);
		}

		protected void AddTasks()
		{
			if (Model.Actions == null && Model.Objects == null)
				return;

			//if (tabModel.Tasks == null)
			//	tabModel.Tasks = new TaskInstanceCollection();

			TabTasks = new TabControlTasks(Instance);
			TabTasks.OnSelectionChanged += ParentListSelectionChanged;

			_tabParentControls.AddControl(TabTasks, false, SeparatorType.Spacer);
		}

		protected void AddChart(ChartSettings chartSettings)
		{
			foreach (var listGroupPair in chartSettings.ListGroups)
			{
				var tabChart = new TabControlChart(Instance, listGroupPair.Value, true);

				_tabParentControls.AddControl(tabChart, true, SeparatorType.Spacer);
				//tabChart.OnSelectionChanged += ListData_OnSelectionChanged;
			}
		}

		protected void AddData()
		{
			int index = 0;
			foreach (IList iList in Model.ItemList)
			{
				var tabData = new TabControlDataGrid(Instance, iList, true, TabViewSettings.GetData(index));
				//tabData.HorizontalAlignment = HorizontalAlignment.Stretch;
				//tabData.VerticalAlignment = VerticalAlignment.Stretch;
				tabData.OnSelectionChanged += ParentListSelectionChanged;
				bool addSplitter = (TabDatas.Count > 0);
				_tabParentControls.AddControl(tabData, true, SeparatorType.Splitter);
				TabDatas.Add(tabData);
				index++;
			}
		}

		// should we check for a Grid stretch instead of passing that parameter?
		protected void AddControl(Control control, bool fill)
		{
			//tabData.OnSelectionChanged += ParentListSelectionChanged;
			_tabParentControls.AddControl(control, fill, SeparatorType.Spacer);
		}

		protected void AddITabControl(ITabSelector control, bool fill)
		{
			control.OnSelectionChanged += ParentListSelectionChanged;
			_tabParentControls.AddControl((Control)control, fill, SeparatorType.Spacer);
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
			var textBox = new TextBox()
			{
				Text = text,
				Foreground = Theme.BackgroundText,
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
			};
			//control.OnSelectionChanged += ParentListSelectionChanged;
			_tabParentControls.AddControl(textBox, false, SeparatorType.Spacer);
		}

		public void Invalidate()
		{
			Instance.loadCalled = false;
		}

		public void Load()
		{
			if (Instance.loadCalled)
				return;
			Instance.loadCalled = true;

			Instance.StartAsync(LoadBackgroundAsync);
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
				Foreground = Theme.ToolbarButtonBackground,
				Background = Theme.TabBackground,
				HorizontalAlignment = HorizontalAlignment.Left,
			};

			Children.Add(progressBar);
		}

		public void LoadSettings()
		{
			if (Instance.TabBookmark != null && Instance.TabBookmark.ViewSettings != null)
				Instance.TabViewSettings = Instance.TabBookmark.ViewSettings;
			else if (Instance.Project.UserSettings.AutoLoad)
				LoadDefaultTabSettings();
		}

		private void LoadDefaultTabSettings()
		{
			TabViewSettings = Instance.LoadDefaultTabSettings();
		}

		private void DispatcherTimer_Tick(object sender, EventArgs e)
		{
			if (_updateChildControls)
			{
				UpdateChildControls();
			}
		}

		private void AddDispatchLoader()
		{
			if (_dispatcherTimer == null)
			{
				_dispatcherTimer = new DispatcherTimer();
				_dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10); // Every 10 milliseconds
				_dispatcherTimer.Tick += DispatcherTimer_Tick;
				_dispatcherTimer.Start();
			}
		}

		private void ClearDispatchLoader()
		{
			if (_dispatcherTimer != null)
			{
				_dispatcherTimer.Stop();
				_dispatcherTimer.Tick -= DispatcherTimer_Tick;
				_dispatcherTimer = null;
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
				if (_tabChildControls == null) // TabTasks hiding can sometimes trigger this, todo: figure out why
					return false;

				if (Bounds.Height < 50)
					return false;

				if (Instance.Depth > 50)
					return false;
				if (double.IsNaN(_tabParentControls.arrangeOverrideFinalSize.Width))
					return false;
				if (_tabChildControls.Width < 30)
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
				double offset = _tabChildControls.Bounds.X;
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
			double offset = _tabChildControls.Bounds.X;
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
			if (_fillerPanel != null)
				_fillerPanel.Width = GetFillerPanelWidth();
		}

		private void UpdateChildControls()
		{
			// These need to be set regardless of if the children show
			if (TabDatas.Count > 0)
				Instance.SelectedItems = TabDatas[0].SelectedItems;

			if (ShowChildren == false)
			{
				_updateChildControls = true;
				AddDispatchLoader();
				// Invoking was happening at bad times in the data binding
				return;
			}
			ClearDispatchLoader();
			_updateChildControls = false;

			_childControlsFinishedLoading = true;

			TabViewer.BaseViewer.SetMinScrollOffset();

			// Create new child controls
			//Dictionary<object, Control> oldChildControls = tabChildControls.gridControls;
			//tabChildControls.gridControls = new Dictionary<object, Control>();

			List<Control> orderedChildControls = CreateAllChildControls(out Dictionary<object, Control> newChildControls);

			_fillerPanel = null;

			// Add a filler panel so the grid splitter can drag to the right
			if (orderedChildControls.Count == 0)
			{
				_fillerPanel = new Panel()
				{
					Width = GetFillerPanelWidth(), // should update this after moving grid splitter
				};
				orderedChildControls.Add(_fillerPanel);
				newChildControls[TempPanelId] = _fillerPanel;
			}
			_tabChildControls.SetControls(newChildControls, orderedChildControls);
			UpdateSelectedTabInstances();

			Instance.TabBookmark = null; // clear so user can navigate and save prefs
		}

		private List<Control> CreateAllChildControls(out Dictionary<object, Control> newChildControls)
		{
			Dictionary<object, Control> oldChildControls = _tabChildControls.GridControls;
			newChildControls = new Dictionary<object, Control>();
			var orderedChildControls = new List<Control>();
			//AddNotes(newChildControls, oldChildControls, orderedChildControls);

			if (Instance is ITabSelector tabSelector && tabSelector.SelectedItems != null)
			{
				CreateChildControls(tabSelector.SelectedItems, oldChildControls, newChildControls, orderedChildControls, tabSelector);
			}

			foreach (ITabSelector tabControl in CustomTabControls)
			{
				CreateChildControls(tabControl.SelectedItems, oldChildControls, newChildControls, orderedChildControls, tabControl);
			}
			if (TabActions != null)
			{
				// show action help?
				//CreateChildControls(tabActions.SelectedItems, oldChildControls, newChildControls, orderedChildControls);
			}
			if (TabTasks != null && TabTasks.IsVisible)
			{
				CreateChildControls(TabTasks.SelectedItems, oldChildControls, newChildControls, orderedChildControls);
			}

			foreach (TabControlDataGrid tabData in TabDatas)
			{
				CreateChildControls(tabData.SelectedRows, oldChildControls, newChildControls, orderedChildControls);
			}
			return orderedChildControls;
		}

		internal void CreateChildControls(IEnumerable newList, Dictionary<object, Control> oldChildControls, Dictionary<object, Control> newChildControls, List<Control> orderedChildControls, ITabSelector tabControl = null)
		{
			//var collection = newList as DataGridSelectedItemsCollection;
			//if (collection != null && collection.)
			//	newList.

			foreach (object obj in newList)
			{
				if (newChildControls.Count >= Instance.Project.UserSettings.VerticalTabLimit)
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
			SelectedRow selectedRow = obj as SelectedRow;
			if (selectedRow != null)
				obj = selectedRow.Object;

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
				Control control = CreateChildControl(selectedRow, obj, label, tabControl);
				if (control != null)
				{
					newChildControls[obj] = control;
					orderedChildControls.Add(control);
				}
			}
		}

		internal Control CreateChildControl(SelectedRow selectedRow, object obj, string label = null, ITabSelector tabControl = null)
		{
			try
			{
				Control control = TabCreator.CreateChildControl(Instance, obj, label, tabControl);
				if (control is TabView tabView && selectedRow != null)
					tabView.Instance.SelectedRow = selectedRow;
				return control;
			}
			catch (Exception e)
			{
				// Add instructions for enabling debugger to catch these
				//call.Log.Add(e);
				return TabCreator.CreateChildControl(Instance, e, "Caught Exception", tabControl);
			}
		}

		private void UpdateSelectedTabInstances()
		{
			Instance.ChildTabInstances.Clear();
			foreach (Control control in _tabChildControls.GridControls.Values)
			{
				if (control is TabView tabView)
				{
					Instance.ChildTabInstances.Add(control, tabView.Instance);
				}
			}
		}

		private void ParentListSelectionChanged(object sender, EventArgs e)
		{
			UpdateChildControls();

			Instance.SelectionChanged(sender, e);

			Instance.UpdateNavigator();
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

			foreach (TabControlDataGrid tabData in TabDatas)
			{
				tabData.OnSelectionChanged -= ParentListSelectionChanged;
				tabData.Dispose();
			}
			TabDatas.Clear();
			if (TabActions != null)
			{
				//tabActions.Dispose();
				TabActions = null;
			}
			if (TabTasks != null)
			{
				TabTasks.OnSelectionChanged -= ParentListSelectionChanged;
				TabTasks.Dispose();
				TabTasks = null;
			}
			if (_tabParentControls != null)
			{
				_tabParentControls.Clear();
				_tabParentControls = null;
			}
			if (_tabChildControls != null)
			{
				_tabChildControls.Clear();
				_tabChildControls = null;
			}
			if (_parentChildGridSplitter != null)
			{
				_parentChildGridSplitter.DragDelta -= GridSplitter_DragDelta;
				_parentChildGridSplitter.DragStarted -= GridSplitter_DragStarted;
				_parentChildGridSplitter.DragCompleted -= GridSplitter_DragCompleted;
				_parentChildGridSplitter.DoubleTapped -= GridSplitter_DoubleTapped;
				_parentChildGridSplitter = null;
			}
			foreach (ITabSelector tabSelector in CustomTabControls)
			{
				tabSelector.OnSelectionChanged -= ParentListSelectionChanged;
			}
			CustomTabControls.Clear();

			//LogicalChildren.Clear();
			Children.Clear();
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
			foreach (var tabData in TabDatas)
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
			if (TabDatas.Count > 0)
			{
				if (e.Object is ITab itab)
				{
					foreach (var obj in TabDatas[0].Items)
					{
						if (obj == itab || obj.GetInnerValue() == itab)
							TabDatas[0].SelectedItem = obj;
					}
				}
				else
				{
					TabDatas[0].SelectedItem = e.Object;
				}
			}
		}

		private void LoadBookmark()
		{
			Instance.Project.UserSettings.AutoLoad = true;

			TabBookmark tabBookmark = Instance.TabBookmark;
			TabViewSettings = tabBookmark.ViewSettings;

			int index = 0;
			foreach (TabControlDataGrid tabData in TabDatas)
			{
				tabData.TabDataSettings = TabViewSettings.GetData(index++);
				tabData.LoadSettings();

				//if (tabInstance.tabBookmark != null)
				foreach (TabInstance childTabInstance in Instance.ChildTabInstances.Values)
				{
					if (tabBookmark.ChildBookmarks.TryGetValue(childTabInstance.Label, out TabBookmark childBookmarkNode))
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

				Instance.OnModelChanged -= TabInstance_OnModelChanged;
				if (Instance is ITabSelector tabSelector)
					tabSelector.OnSelectionChanged -= ParentListSelectionChanged;

				Instance.Dispose();

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

*/
