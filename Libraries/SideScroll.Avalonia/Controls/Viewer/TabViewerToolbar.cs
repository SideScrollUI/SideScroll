using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Reactive;
using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs.Bookmarks;
using System.Runtime.InteropServices;

namespace SideScroll.Avalonia.Controls.Viewer;

public class TabViewerToolbar : TabControlToolbar
{
	public TabViewer TabViewer { get; }

	public bool EnableCustomTitleBar => TabViewer.Project.UserSettings.EnableCustomTitleBar == true;

	public ToolbarButton? ButtonLogo { get; protected set; }

	public ToolbarButton ButtonBack { get; protected set; }
	public ToolbarButton ButtonForward { get; protected set; }

	public ToolbarButton ButtonRefresh { get; protected set; }

	public ToolbarButton? ButtonLink { get; protected set; }
	public ToolbarButton? ButtonImport { get; protected set; }

	public ToolbarButton? ButtonMinimize { get; protected set; }
	public ToolbarButton? ButtonMaximize { get; protected set; }
	public ToolbarButton? ButtonClose { get; protected set; }

	protected static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	protected string ProjectName => TabViewer.Project.Name!;

	public TabViewerToolbar(TabViewer tabViewer)
	{
		TabViewer = tabViewer;
		Background = null;

		if (EnableCustomTitleBar)
		{
			if (IsMacOS)
			{
				MacosTitleButtons macosTitleButtons = new();
				AddControl(macosTitleButtons);
			}
			else
			{
				ButtonLogo = AddButton(ProjectName, Logo.Svg.SideScrollTranslucent, updateIconColors: false);
				ButtonLogo.DoubleTapped += ButtonLogo_DoubleTapped;
			}
			AddSeparator();
		}

		// HotKeys are handled in TabViewer
		ButtonBack = AddButton("Back (Alt + Left)", Icons.Svg.LeftArrow);
		ButtonBack.BindIsEnabled(nameof(BookmarkNavigator.CanSeekBackward), TabViewer.Project.Navigator);
		ButtonBack.Add(_ => TabViewer.SeekBackward());

		ButtonForward = AddButton("Forward (Alt + Right)", Icons.Svg.RightArrow);
		ButtonForward.BindIsEnabled(nameof(BookmarkNavigator.CanSeekForward), TabViewer.Project.Navigator);
		ButtonForward.Add(_ => TabViewer.SeekForward());
		
		AddSeparator();
		ButtonRefresh = AddButton("Refresh (Ctrl + R)", Icons.Svg.Refresh);
		ButtonRefresh.Add(Refresh);

		if (tabViewer.Project.ProjectSettings.EnableLinking)
		{
			AddSeparator();
			ButtonLink = AddButton("Link - Copy to Clipboard", Icons.Svg.Link);
			ButtonImport = AddButton("Import Link from Clipboard", Icons.Svg.Import);
		}

		if (EnableCustomTitleBar)
		{
			SubscribeToWindowState();
		}
	}

	private void ButtonLogo_DoubleTapped(object? sender, global::Avalonia.Input.TappedEventArgs e)
	{
		if (VisualRoot is Window window)
		{
			window.Close();
		}
	}

	public void AddTitle()
	{
		var textBlock = new ToolbarHeaderTextBlock(ProjectName)
		{
			IsHitTestVisible = false,
		};
		AddControl(textBlock);
	}

	public void AddRightControls()
	{
		AddFill();

		if (EnableCustomTitleBar)
		{
			if (IsMacOS)
			{
				ButtonLogo = AddButton(ProjectName, Logo.Svg.SideScrollTranslucent, updateIconColors: false);
				ButtonLogo.HorizontalAlignment = HorizontalAlignment.Right;
			}
			AddTitle();
		}

		AddVersion();
		AddWindowControls();
	}

	public ToolbarHeaderTextBlock AddVersion()
	{
		string versionLabel = 'v' + TabViewer.Project.Version.Formatted();
#if DEBUG
		versionLabel += " *";
#endif
		var textBlock = new ToolbarHeaderTextBlock(versionLabel)
		{
			Margin = new Thickness(0, 0, 20, 0)
		};
		AddControl(textBlock);
		return textBlock;
	}

	private void AddWindowControls()
	{
		if (!EnableCustomTitleBar || IsMacOS) return;

		int buttonWidth = 44;

		ButtonMinimize = AddButton("Minimize", Icons.Svg.Minimize);
		ButtonMinimize.Width = buttonWidth;
		ButtonMinimize.Add(Minimize);

		ButtonMaximize = AddButton("Maximize", Icons.Svg.Restore);
		ButtonMaximize.Width = buttonWidth;
		ButtonMaximize.Add(Maximize);

		ButtonClose = AddButton("Close", Icons.Svg.Close);
		ButtonClose.Width = buttonWidth;
		ButtonClose.Add(Close);
	}

	private void Refresh(Call call)
	{
		TabViewer.Reload(call);
	}

	private void Minimize(Call call)
	{
		if (VisualRoot is Window window)
		{
			window.WindowState = WindowState.Minimized;
		}
	}

	private Rect? _normalBounds;

	private void Maximize(Call call)
	{
		if (VisualRoot is Window window)
		{
			if (window.WindowState == WindowState.Normal)
			{
				_normalBounds = new(
					x: window.Position.X,
					y: window.Position.Y,
					width: window.Width,
					height: window.Height
				);
				window.WindowState = WindowState.Maximized;
			}
			else
			{
				window.WindowState = WindowState.Normal;
				if (_normalBounds is Rect rect)
				{
					window.Width = rect.Width;
					window.Height = rect.Height;
					window.Position = new PixelPoint((int)rect.X, (int)rect.Y);
				}
			}
		}
	}

	private void Close(Call call)
	{
		if (VisualRoot is Window window)
		{
			window.Close();
		}
	}

	private async void SubscribeToWindowState()
	{
		Window? hostWindow = VisualRoot as Window;

		while (hostWindow == null)
		{
			await Task.Delay(50);
			hostWindow = VisualRoot as Window;
		}

		hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(OnWindowStateChanged));
	}

	private void OnWindowStateChanged(WindowState state)
	{
		if (ButtonMaximize == null) return;

		if (state == WindowState.Maximized)
		{
			ButtonMaximize.SetImage(Icons.Svg.Restore);
			ToolTip.SetTip(ButtonMaximize, "Restore Down");
		}
		else
		{
			ButtonMaximize.SetImage(Icons.Svg.Maximize);
			ToolTip.SetTip(ButtonMaximize, "Maximize");
		}
	}
}
