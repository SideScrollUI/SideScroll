using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Atlas.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace Atlas.UI.Avalonia.View;

public interface IControlCreator
{
	void AddControl(TabInstance tabInstance, TabControlSplitContainer container, object obj);
}

public class TabView : Grid, IDisposable
{
	private const string FillerPanelId = "FillerPanelId";
	private const int MinDesiredSplitterDistance = 50;

	// Model.Objects
	public static Dictionary<Type, IControlCreator> ControlCreators { get; set; } = new();

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

	public string Label
	{
		get => Model.Name;
		set => Model.Name = value;
	}

	// Created Controls
	public TabControlActions? TabActions;
	public TabControlTasks? TabTasks;
	public List<ITabDataControl> TabDatas = new();
	public List<ITabSelector> CustomTabControls { get; set; } = new(); // should everything use this?

	// Layout Controls
	private Grid? _containerGrid;
	private TabControlSplitContainer? _tabParentControls;
	private TabControlTitle? _tabTitle;
	private GridSplitter? _parentChildGridSplitter;
	private TabControlSplitContainer? _tabChildControls;
	private Panel? _fillerPanel; // GridSplitter doesn't work without control on right side

	private Size _arrangeOverrideFinalSize;
	private bool _childControlsFinishedLoading;
	private bool _isDragging;

	// Throttles updating selectedChildControls
	// todo: extract
	private DispatcherTimer? _dispatcherTimer;  // delays auto selection to throttle updates
	private bool _updateChildControls;

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

		await Instance.ReinitializeAsync(call);
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
		Background = Theme.TabBackground;
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
				_tabParentControls!.Width = 0;
			}
		}

		AddGridColumnSplitter();
		AddChildControls();

		// don't re-add containerGrid (sizing doesn't work otherwise?)
		if (Children.Count == 0)
			Children.Add(_containerGrid);

		// Reassigning leaks memory
		if (ContextMenu == null)
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

		_containerGrid!.ColumnDefinitions[0].Width = new GridLength(desiredWidth);
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
		_containerGrid!.Children.Add(_tabParentControls);
		UpdateSplitterDistance();

		_tabTitle = new TabControlTitle(Instance, Model.Name);
		_tabParentControls.AddControl(_tabTitle, false, SeparatorType.None);

		_tabParentControls.KeyDown += ParentControls_KeyDown;
	}

	private void ParentControls_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter)
		{
			Instance.DefaultAction?.Invoke();
			e.Handled = true;
			return;
		}
	}

	private void AddGridColumnSplitter()
	{
		_parentChildGridSplitter = new GridSplitter
		{
			Background = Brushes.Black,
			VerticalAlignment = VerticalAlignment.Stretch,
			[Grid.ColumnProperty] = 1,
			Width = Theme.SplitterSize,
		};
		_containerGrid!.Children.Add(_parentChildGridSplitter);

		_parentChildGridSplitter.DragDelta += GridSplitter_DragDelta;
		_parentChildGridSplitter.DragStarted += GridSplitter_DragStarted;
		_parentChildGridSplitter.DragCompleted += GridSplitter_DragCompleted; // bug, this is firing when double clicking splitter
		_parentChildGridSplitter.DoubleTapped += GridSplitter_DoubleTapped;
	}

	private void AddChildControls()
	{
		_tabChildControls = new TabControlSplitContainer
		{
			ColumnDefinitions = new ColumnDefinitions("Auto"),
		};
		Grid.SetColumn(_tabChildControls, 2);
		_containerGrid!.Children.Add(_tabChildControls);
	}

	private void AddListeners()
	{
		Instance.OnRefresh += TabInstance_OnRefresh;
		Instance.OnReload += TabInstance_OnReload;
		Instance.OnLoadBookmark += TabInstance_OnLoadBookmark;
		Instance.OnClearSelection += TabInstance_OnClearSelection; // data controls should attach these instead?
		Instance.OnSelectItems += TabInstance_OnSelectItems;
		Instance.OnResize += TabInstance_OnResize;
	}

	private void RemoveListeners()
	{
		Instance.OnRefresh -= TabInstance_OnRefresh;
		Instance.OnReload -= TabInstance_OnReload;
		Instance.OnLoadBookmark -= TabInstance_OnLoadBookmark;
		Instance.OnClearSelection -= TabInstance_OnClearSelection;
		Instance.OnSelectItems -= TabInstance_OnSelectItems;
		Instance.OnResize -= TabInstance_OnResize;
	}

	private void TabInstance_OnModelChanged(object? sender, EventArgs e)
	{
		ReloadControls();
	}

	private void TabInstance_OnResize(object? sender, EventArgs e)
	{
		_containerGrid!.ColumnDefinitions[0].Width = GridLength.Auto;
	}

	private void GridSplitter_DragDelta(object? sender, VectorEventArgs e)
	{
		if (TabViewSettings.SplitterDistance != null)
			_tabParentControls!.Width = _containerGrid!.ColumnDefinitions[0].ActualWidth;

		// force the width to update (Grid Auto Size caching problem?
		double width = _containerGrid!.ColumnDefinitions[0].ActualWidth;
		TabViewSettings.SplitterDistance = width;
		_tabParentControls!.Width = width;

		//if (TabViewSettings.SplitterDistance != null)
		//	containerGrid.ColumnDefinitions[0].Width = new GridLength((double)containerGrid.ColumnDefinitions[1].);
	}

	private void GridSplitter_DragStarted(object? sender, VectorEventArgs e)
	{
		_isDragging = true;
	}

	private void GridSplitter_DragCompleted(object? sender, VectorEventArgs e)
	{
		if (_isDragging == false)
			return;
		_isDragging = false;

		InvalidateMeasure();
		InvalidateArrange();

		//TabViewSettings.SplitterDistance = (int)Math.Ceiling(e.Vector.Y); // backwards
		double width = (int)_containerGrid!.ColumnDefinitions[0].ActualWidth;
		TabViewSettings.SplitterDistance = width;
		_tabParentControls!.Width = width;
		_containerGrid.ColumnDefinitions[0].Width = new GridLength(width);

		//UpdateSplitterDistance();
		Instance.SaveTabSettings();
		UpdateSplitterFiller();
		_tabParentControls.InvalidateMeasure();
	}

	// doesn't resize bigger well
	// The Drag start, delta, and complete get called for this too. Which makes this really hard to do well
	private void GridSplitter_DoubleTapped(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
	{
		_isDragging = false;
		double desiredWidth = _tabParentControls!.DesiredSize.Width;
		TabViewSettings.SplitterDistance = desiredWidth;
		_tabParentControls.Width = desiredWidth;
		//containerGrid.ColumnDefinitions[0].Width = new GridLength(desiredWidth);
		_containerGrid!.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
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
		Instance.SaveTabSettings();
		UpdateSplitterFiller();
	}

	public void UpdateSplitterDistance()
	{
		if (_containerGrid == null)
			return;

		if (TabViewSettings.SplitterDistance is double splitterDistance && splitterDistance > MinDesiredSplitterDistance)
		{
			_containerGrid.ColumnDefinitions[0].Width = new GridLength((int)splitterDistance);
			if (_tabParentControls != null)
				_tabParentControls.Width = (double)splitterDistance;
		}
		else
		{
			_containerGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
		}
	}

	public void ReloadControls()
	{
		ClearControls(false);

		InitializeControls();

		AddObjects();
		AddActions();
		if (TabTasks == null)
			AddTasks();
		AddData();

		UpdateChildControls();
	}

	protected void AddObjects()
	{
		foreach (TabObject tabObject in Model.Objects)
		{
			object obj = tabObject.Object!;
			if (ControlCreators.TryGetValue(obj.GetType(), out IControlCreator? controlCreator))
			{
				controlCreator.AddControl(Instance, _tabParentControls!, obj);
			}
			else if (obj is TabToolbar toolbar)
			{
				AddToolbar(toolbar);
				AddTasks();
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
				ParamsAttribute? paramsAttribute = obj.GetType().GetCustomAttribute<ParamsAttribute>();
				if (paramsAttribute != null)
				{
					AddControl(new TabControlParams(obj), tabObject.Fill);
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

		TabActions = new TabControlActions(Instance, Model);

		_tabParentControls!.AddControl(TabActions, false, SeparatorType.Spacer);
	}

	protected void AddTasks()
	{
		if (Model.Actions == null && Model.Objects == null)
			return;

		TabTasks = new TabControlTasks(Instance);
		TabTasks.OnSelectionChanged += ParentListSelectionChanged;

		_tabParentControls!.AddControl(TabTasks, false, SeparatorType.Spacer);
	}

	protected void AddData()
	{
		int index = 0;
		foreach (IList iList in Model.ItemList)
		{
			var tabData = new TabControlDataGrid(Instance, iList, true, TabViewSettings.GetData(index));
			tabData.OnSelectionChanged += ParentListSelectionChanged;
			_tabParentControls!.AddControl(tabData, true, SeparatorType.Splitter);
			TabDatas.Add(tabData);
			index++;
		}
	}

	// should we check for a Grid stretch instead of passing that parameter?
	protected void AddControl(Control control, bool fill)
	{
		_tabParentControls!.AddControl(control, fill, SeparatorType.Splitter);
	}

	protected void AddITabControl(ITabSelector control, bool fill)
	{
		control.OnSelectionChanged += ParentListSelectionChanged;
		_tabParentControls!.AddControl((Control)control, fill, SeparatorType.Spacer);
		CustomTabControls.Add(control);
	}

	protected void AddControlString(string text)
	{
		TextBox textBox = new()
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
			MinWidth = 50,
			MaxWidth = 1000,
			TextWrapping = TextWrapping.Wrap,
			AcceptsReturn = true,
		};
		textBox.Resources.Add("TextBackgroundDisabledBrush", Brushes.Transparent);
		//textBox.Resources.Add("TextControlBackgroundFocused", Brushes.Transparent);
		
		AvaloniaUtils.AddContextMenu(textBox);
		_tabParentControls!.AddControl(textBox, false, SeparatorType.Spacer);
	}

	public void Invalidate()
	{
		Instance.LoadCalled = false;
	}

	public void Load()
	{
		if (Instance.LoadCalled)
			return;
		Instance.LoadCalled = true;

		Instance.StartAsync(LoadBackgroundAsync);
	}

	public void ShowLoading()
	{
		ClearControls(true);

		// This will get cleared when the view reloads
		var progressBar = new ProgressBar
		{
			IsIndeterminate = true,
			MinWidth = 100,
			MinHeight = 100,
			MaxWidth = 200,
			Foreground = Theme.ToolbarButtonBackground,
			Background = Theme.TabBackground,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Stretch,
		};

		Children.Add(progressBar);
	}

	public void LoadSettings()
	{
		if (Instance.TabBookmark != null && Instance.TabBookmark.ViewSettings != null)
		{
			Instance.TabViewSettings = Instance.TabBookmark.ViewSettings;
		}
		else if (Instance.Project.UserSettings.AutoLoad)
		{
			LoadDefaultTabSettings();
		}
	}

	private void LoadDefaultTabSettings()
	{
		TabViewSettings = Instance.LoadDefaultTabSettings();
	}

	private void DispatcherTimer_Tick(object? sender, EventArgs e)
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
			_dispatcherTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(10),
			};
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

			if (!_tabParentControls!.IsArrangeValid)
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
			IControl? control = Parent;
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

	private double GetFillerPanelWidth()
	{
		IControl? control = Parent;
		double offset = _tabChildControls!.Bounds.X;
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

	private void UpdateChildControls(bool recreate = false)
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

		TabViewer.BaseViewer!.SetMinScrollOffset();

		// Create new child controls
		//Dictionary<object, Control> oldChildControls = tabChildControls.gridControls;
		//tabChildControls.gridControls = new Dictionary<object, Control>();

		List<Control> orderedChildControls = CreateAllChildControls(recreate, out Dictionary<object, Control> newChildControls);

		_fillerPanel = null;

		// Add a filler panel so the grid splitter can drag to the right
		if (orderedChildControls.Count == 0)
		{
			_fillerPanel = new Panel()
			{
				Width = GetFillerPanelWidth(), // should update this after moving grid splitter
			};
			orderedChildControls.Add(_fillerPanel);
			newChildControls[FillerPanelId] = _fillerPanel;
		}
		else if (Instance.TabBookmark != null)
		{
			Instance.SaveTabSettings();
			Instance.TabBookmarkLoaded = Instance.TabBookmark;
			Instance.TabBookmark = null; // clear so user can navigate and save prefs
		}
		_tabChildControls!.SetControls(newChildControls, orderedChildControls);
		UpdateSelectedTabInstances();

	}

	private List<Control> CreateAllChildControls(bool recreate, out Dictionary<object, Control> newChildControls)
	{
		Dictionary<object, Control> oldChildControls = recreate ? new() : _tabChildControls!.GridControls;
		newChildControls = new Dictionary<object, Control>();
		var orderedChildControls = new List<Control>();
		//AddNotes(newChildControls, oldChildControls, orderedChildControls);

		if (Instance is ITabSelector tabSelector && tabSelector.SelectedItems != null)
		{
			CreateChildControls(tabSelector.SelectedItems, oldChildControls, newChildControls, orderedChildControls, tabSelector);
		}

		foreach (ITabSelector tabControl in CustomTabControls)
		{
			if (tabControl.SelectedItems != null)
			{
				CreateChildControls(tabControl.SelectedItems, oldChildControls, newChildControls, orderedChildControls, tabControl);
			}
		}

		if (TabActions != null)
		{
			// show action help?
			//CreateChildControls(tabActions.SelectedItems, oldChildControls, newChildControls, orderedChildControls);
		}

		if (TabTasks?.IsVisible == true)
		{
			CreateChildControls(TabTasks.SelectedItems, oldChildControls, newChildControls, orderedChildControls);
		}

		foreach (ITabDataControl tabData in TabDatas)
		{
			CreateChildControls(tabData.SelectedRows, oldChildControls, newChildControls, orderedChildControls);
		}
		return orderedChildControls;
	}

	internal void CreateChildControls(IEnumerable newList, Dictionary<object, Control> oldChildControls, Dictionary<object, Control> newChildControls, List<Control> orderedChildControls, ITabSelector? tabControl = null)
	{
		foreach (object obj in newList)
		{
			if (newChildControls.Count >= Instance.Project.UserSettings.VerticalTabLimit)
				break;

			GetOrCreateChildControl(oldChildControls, newChildControls, orderedChildControls, obj, null, tabControl);
		}
	}

	private void GetOrCreateChildControl(Dictionary<object, Control> oldChildControls, Dictionary<object, Control> newChildControls, List<Control> orderedChildControls, object obj, string? label = null, ITabSelector? tabControl = null)
	{
		// duplicate work
		//object value = obj.GetInnerValue(); // performance issues? cache this?
		//if (value == null)
		//	return;
		SelectedRow? selectedRow = obj as SelectedRow;
		if (selectedRow != null)
			obj = selectedRow.Object!;

		if (oldChildControls.ContainsKey(obj))
		{
			// Reuse existing control
			Control control = oldChildControls[obj];
			if (newChildControls.ContainsKey(obj))
			{
				Debug.WriteLine("TabView has already added child control " + obj.ToString());
			}
			else
			{
				newChildControls.Add(obj, control);
				orderedChildControls.Add(control);
			}
		}
		else
		{
			// Create a new control
			Control? control = CreateChildControl(selectedRow, obj, label, tabControl);
			if (control != null)
			{
				newChildControls[obj] = control;
				orderedChildControls.Add(control);
			}
		}
	}

	internal Control? CreateChildControl(SelectedRow? selectedRow, object obj, string? label = null, ITabSelector? tabControl = null)
	{
		try
		{
			Control? control = TabCreator.CreateChildControl(Instance, obj, label, tabControl);
			if (control is TabView tabView && selectedRow != null)
				tabView.Instance.SelectedRow = selectedRow;

			if (control != null)
			{
				TabViewer.BaseViewer!.TabLoaded(obj, control);
			}

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
		foreach (Control control in _tabChildControls!.GridControls.Values)
		{
			if (control is TabView tabView)
			{
				Instance.ChildTabInstances.Add(control, tabView.Instance);
			}
		}
	}

	private void ParentListSelectionChanged(object? sender, TabSelectionChangedEventArgs e)
	{
		e ??= new TabSelectionChangedEventArgs();

		UpdateChildControls(e.Recreate);

		Instance.SelectionChanged(sender, e);

		Instance.UpdateNavigator();
	}

	private void ClearControls(bool dispose)
	{
		RemoveListeners();

		ClearDispatchLoader();

		//gridParentControls.MouseDown -= gridParentControls_MouseDown;
		//horizontalSplitter.DragCompleted -= horizontalSplitter_DragCompleted;
		//horizontalSplitter.MouseDoubleClick -= horizontalSplitter_MouseDoubleClick;

		//RequestBringIntoView -= UserControl_RequestBringIntoView;

		foreach (ITabDataControl tabData in TabDatas)
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

		if (_tabTitle != null)
		{
			_tabTitle.Dispose();
			_tabTitle = null;
		}

		if (_tabParentControls != null)
		{
			_tabParentControls.KeyDown -= ParentControls_KeyDown;
			_tabParentControls.Clear(dispose);
			_tabParentControls = null;
		}

		if (_tabChildControls != null)
		{
			_tabChildControls.Clear(dispose);
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

	private void TabInstance_OnRefresh(object? sender, EventArgs e)
	{
		Load();
	}

	private void TabInstance_OnReload(object? sender, EventArgs e)
	{
		//tabInstance.Reinitialize(true);
		Load();
	}

	private void TabInstance_OnClearSelection(object? sender, EventArgs e)
	{
		foreach (ITabDataControl tabData in TabDatas)
		{
			tabData.SelectedItem = null; // dataGrid.UnselectAll() doesn't work
		}
	}

	private void TabInstance_OnLoadBookmark(object? sender, EventArgs e)
	{
		LoadBookmark();
	}

	private void TabInstance_OnSelectItems(object? sender, TabInstance.EventSelectItems e)
	{
		if (TabDatas.Count > 0)
		{
			if (e.List.Count == 0)
			{
				TabDatas[0].SelectedItem = null;
			}
			else if (e.List[0] is ITab)
			{
				var newItems = new HashSet<object>();
				foreach (var obj in e.List)
					newItems.Add(obj);

				var matching = new List<object>();
				foreach (var obj in TabDatas[0].Items!)
				{
					if (newItems.Contains(obj) || newItems.Contains(obj.GetInnerValue()!))
						matching.Add(obj);
				}
				TabDatas[0].SelectedItems = matching;
			}
			else
			{
				TabDatas[0].SelectedItems = e.List;
			}
		}
		else if (CustomTabControls.Count > 0)
		{
			foreach (ITabSelector tabSelector in CustomTabControls)
			{
				if (tabSelector is ITabItemSelector itemSelector)
				{
					itemSelector.SelectedItems = e.List;
				}
			}
		}
	}

	private void LoadBookmark()
	{
		Instance.Project.UserSettings.AutoLoad = true;

		TabBookmark tabBookmark = Instance.TabBookmark!;
		TabViewSettings = tabBookmark.ViewSettings;

		int index = 0;
		foreach (ITabDataControl tabData in TabDatas)
		{
			tabData.TabDataSettings = TabViewSettings.GetData(index++);
			tabData.LoadSettings();

			//if (tabInstance.tabBookmark != null)
			foreach (TabInstance childTabInstance in Instance.ChildTabInstances.Values)
			{
				if (tabBookmark.ChildBookmarks.TryGetValue(childTabInstance.Label, out TabBookmark? childBookmarkNode))
				{
					childTabInstance.SelectBookmark(childBookmarkNode);
				}
			}
		}
	}

	#region IDisposable Support
	private bool _disposedValue = false; // To detect redundant calls

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// TODO: dispose managed state (managed objects).
			}

			// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
			// TODO: set large fields to null.

			ClearControls(true);

			Instance.OnModelChanged -= TabInstance_OnModelChanged;
			if (Instance is ITabSelector tabSelector)
				tabSelector.OnSelectionChanged -= ParentListSelectionChanged;

			Instance.Dispose();

			if (ContextMenu is IDisposable contextMenu)
			{
				contextMenu.Dispose();
				ContextMenu = null;
			}

			_disposedValue = true;
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

*/
