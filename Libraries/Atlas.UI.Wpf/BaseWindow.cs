using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Atlas.Core;
using Atlas.Tabs;
using Microsoft.Win32;

namespace Atlas.UI.Wpf
{
	public class BaseWindow : Window, IProject
	{
		private const int MinWindowSize = 100;

		protected Project project;
		protected TabView tabView;
		protected ScrollViewer scrollViewer;
		//public WindowSettings windowSettings;

		public BaseWindow(Project project)
		{
			Debug.Assert(project != null);
			this.project = project;
			SetRegistryUseIE11Mode();
			InitializeControl();
		}

		public void InitializeControl()
		{
			LoadWindowSettings();

			Title = project.projectSettings.Name ?? "<Name>";

			Uri iconUri = new Uri("pack://application:,,,/Atlas.UI.Wpf;component/Assets/Logo.ico");
			Icon = BitmapFrame.Create(iconUri);

			SizeChanged += Window_SizeChanged;
			LocationChanged += Window_LocationChanged;

			Grid containerGrid = new Grid()
			{
				//ColumnDefinitions = new ColumnDefinitions("*"),
				//RowDefinitions = new RowDefinitions("Auto,*"), // Header, Body
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
			};
			ColumnDefinition columnDefinitionContainer = new ColumnDefinition();
			columnDefinitionContainer.Width = new GridLength(1, GridUnitType.Star);
			containerGrid.ColumnDefinitions.Add(columnDefinitionContainer);


			RowDefinition rowDefinitionToolbar = new RowDefinition();
			rowDefinitionToolbar.Height = GridLength.Auto;
			containerGrid.RowDefinitions.Add(rowDefinitionToolbar);

			RowDefinition rowDefinitionContent = new RowDefinition();
			rowDefinitionContent.Height = new GridLength(1, GridUnitType.Star);
			containerGrid.RowDefinitions.Add(rowDefinitionContent);

			ToolBar toolBar = CreateToolbar();

			containerGrid.Children.Add(toolBar);

			Grid horizontalGrid = new Grid()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
			};
			Grid.SetRow(horizontalGrid, 1);
			containerGrid.Children.Add(horizontalGrid);

			//rowDefinition.Height = GridLength.Auto;

			ColumnDefinition columnDefinitionContent = new ColumnDefinition();
			columnDefinitionContent.Width = new GridLength(1, GridUnitType.Star);
			horizontalGrid.ColumnDefinitions.Add(columnDefinitionContent);

			ColumnDefinition columnDefinitionScroll = new ColumnDefinition();
			columnDefinitionScroll.Width = new GridLength(15);
			horizontalGrid.ColumnDefinitions.Add(columnDefinitionScroll);

			scrollViewer = new ScrollViewer()
			{
				VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
			};
			Grid.SetColumn(scrollViewer, 0);
			horizontalGrid.Children.Add(scrollViewer);


			Grid rightButtonGrid = new Grid()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
			};
			Grid.SetColumn(rightButtonGrid, 1);

			RowDefinition rowDefinition = new RowDefinition();
			rowDefinition.Height = new GridLength(1, GridUnitType.Star);
			rightButtonGrid.RowDefinitions.Add(rowDefinition);

			rowDefinition = new RowDefinition();
			rowDefinition.Height = new GridLength(1, GridUnitType.Star);
			rightButtonGrid.RowDefinitions.Add(rowDefinition);

			Button buttonExpand = new Button()
			{
				Content = ">",
			};
			Grid.SetRow(buttonExpand, 0);
			rightButtonGrid.Children.Add(buttonExpand);

			Button buttonCollapse = new Button()
			{
				Content = "<",
			};
			Grid.SetRow(buttonCollapse, 1);
			rightButtonGrid.Children.Add(buttonCollapse);

			rowDefinition = new RowDefinition();
			rowDefinition.Height = new GridLength(1, GridUnitType.Star);
			horizontalGrid.RowDefinitions.Add(rowDefinition);
			horizontalGrid.Children.Add(rightButtonGrid);

			//Content = horizontalGrid;
			Content = containerGrid;
			//this.AddChild(toolBar);
			//this.AddChild(horizontalGrid);
		}

		private ToolBar CreateToolbar()
		{
			ToolBar toolBar = new ToolBar()
			{
			};
			Grid.SetRow(toolBar, 0);

			RoutedCommand commandBack = new RoutedCommand("Back", GetType());

			CommandBinding commandBindingBack = new CommandBinding(
				commandBack,
				CommandBackExecute,
				CommandBackCanExecute);

			CommandBindings.Add(commandBindingBack);

			Image imageBack = new Image()
			{
				//Source = new BitmapImage(new Uri("/Atlas.UI.Wpf;component/left-chevron.png", UriKind.Relative)),
				Source = new BitmapImage(new Uri("pack://application:,,,/Atlas.UI.Wpf;component/Assets/left-chevron.png")),
			};

			Button buttonBack = new Button()
			{
				Content = "<-",
				ToolTip = "Back",
				//Content = imageBack,
				Command = commandBindingBack.Command,
			};
			toolBar.Items.Add(buttonBack);


			RoutedCommand commandForward = new RoutedCommand("Forward", GetType());

			CommandBinding commandBindingForward = new CommandBinding(
				commandForward,
				CommandForwardExecute,
				CommandForwardCanExecute);

			CommandBindings.Add(commandBindingForward);

			Image imageForward = new Image()
			{
				//Source = new BitmapImage(new Uri("/Atlas.UI.Wpf;component/left-chevron.png", UriKind.Relative)),
				Source = new BitmapImage(new Uri("pack://application:,,,/Atlas.UI.Wpf;component/Assets/right-chevron.png")),
			};

			Button buttonForward = new Button()
			{
				Content = "->",
				ToolTip = "Forward",
				//Content = imageForward,
				Command = commandBindingForward.Command,
			};
			toolBar.Items.Add(buttonForward);
			return toolBar;
		}

		private void CommandBackCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = project.Navigator.CanSeekBackward;
		}

		private void CommandForwardCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = project.Navigator.CanSeekForward;
		}

		private void CommandBackExecute(object sender, ExecutedRoutedEventArgs e)
		{
			Bookmark bookmark = project.Navigator.SeekBackward();
			if (bookmark != null)
				tabView.tabInstance.SelectBookmark(bookmark.tabBookmark);
		}

		private void CommandForwardExecute(object sender, ExecutedRoutedEventArgs e)
		{
			Bookmark bookmark = project.Navigator.SeekForward();
			if (bookmark != null)
				tabView.tabInstance.SelectBookmark(bookmark.tabBookmark);
		}

		private bool isLoading = false;

		protected WindowSettings WindowSettings
		{
			get
			{
				WindowSettings windowSettings = new WindowSettings()
				{
					Maximized = (this.WindowState == WindowState.Maximized),
					Width = this.Width,
					Height = this.Height,
					Left = this.Left,
					Top = this.Top,
				};
				return windowSettings;
			}
			set
			{
				isLoading = true;
				this.Left = Math.Max(-10, value.Left); // Top Left is negative coordinates for some reason (-7 seen)
				this.Top = Math.Max(0, value.Top);
				this.Width = Math.Max(MinWindowSize, value.Width);
				this.Height = Math.Max(MinWindowSize, value.Height);
				this.WindowState = value.Maximized ? WindowState.Maximized : WindowState.Normal;
				isLoading = false;
			}
		}

		protected void LoadWindowSettings()
		{
			WindowSettings windowSettings = project.DataApp.Load<WindowSettings>(true);

			this.WindowSettings = windowSettings;
		}

		private void SaveWindowSettings()
		{
			if (isLoading == false)
				project.DataApp.Save(this.WindowSettings);
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			SaveWindowSettings();
		}

		private void Window_LocationChanged(object sender, EventArgs e)
		{
			SaveWindowSettings();
		}

		private void buttonExpand_Click(object sender, RoutedEventArgs e)
		{
			tabView.Width = tabView.ActualWidth + 1000;
		}

		private void buttonCollapse_Click(object sender, RoutedEventArgs e)
		{
			tabView.Width = double.NaN;
		}

		public void Restart()
		{
			Application.Current.Shutdown();
		}

		// Use IE 11 mode instead of IE 8 mode
		// Most users won't run this program as Administrator (good) so this will abort early
		public static void SetRegistryUseIE11Mode()
		{
			var pricipal = new System.Security.Principal.WindowsPrincipal(
  System.Security.Principal.WindowsIdentity.GetCurrent());
			if (pricipal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
			{
				RegistryKey registrybrowser = Registry.LocalMachine.OpenSubKey
					(@"Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
				string myProgramName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				var currentValue = registrybrowser.GetValue(myProgramName);
				if (currentValue == null || (int)currentValue != 0x00002af9)
					registrybrowser.SetValue(myProgramName, 0x00002af9, RegistryValueKind.DWord);
			}
		}

		// Use IE 11 mode instead of IE 8 mode
		// Most users won't run this program as Administrator (good) so this will abort early
		public static bool IE11ModeSet
		{
			get
			{
				var pricipal = new System.Security.Principal.WindowsPrincipal(
	  System.Security.Principal.WindowsIdentity.GetCurrent());
				RegistryKey registrybrowser = Registry.LocalMachine.OpenSubKey
					(@"Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
				string myProgramName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				var currentValue = registrybrowser.GetValue(myProgramName);
				if (currentValue != null && (int)currentValue == 0x00002af9)
					return true;
				return false;
			}
		}
	}
}
