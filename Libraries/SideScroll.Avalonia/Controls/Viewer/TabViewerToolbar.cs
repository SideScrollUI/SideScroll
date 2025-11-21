using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs.Bookmarks;

namespace SideScroll.Avalonia.Controls.Viewer;

public class TabViewerToolbar : TabControlToolbar
{
	public TabViewer TabViewer { get; }

	public ToolbarButton ButtonBack { get; protected set; }
	public ToolbarButton ButtonForward { get; protected set; }

	public ToolbarButton ButtonRefresh { get; protected set; }

	public ToolbarButton? ButtonLink { get; protected set; }
	public ToolbarButton? ButtonImport { get; protected set; }

	public ToolbarButton? ButtonMinimize { get; protected set; }
	public ToolbarButton? ButtonMaximize { get; protected set; }
	public ToolbarButton? ButtonClose { get; protected set; }

	public TabViewerToolbar(TabViewer tabViewer)
	{
		TabViewer = tabViewer;
		Background = null;

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
	}
	
	public void AddTitle()
	{
		var textBlock = new ToolbarHeaderTextBlock(TabViewer.Project.Name!)
		{
			HorizontalAlignment = HorizontalAlignment.Right,
			IsHitTestVisible = false,
		};
		AddControl(textBlock, true);
	}

	public void AddVersion()
	{
		AddTitle();

		//AddFill();

		string versionLabel = 'v' + TabViewer.Project.Version.Formatted();
#if DEBUG
		versionLabel += " *";
#endif
		var textBlock = new ToolbarHeaderTextBlock(versionLabel);
		textBlock.Margin = new Thickness(0, 0, 20, 0);
		AddControl(textBlock);

		ButtonMinimize = AddButton("Minimize", Icons.Svg.DownArrow);
		ButtonMinimize.Add(Minimize);
		ButtonMaximize = AddButton("Maximize", Icons.Svg.UpArrow);
		ButtonMaximize.Add(Maximize);
		ButtonClose = AddButton("Close", Icons.Svg.Delete);
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
}
