using SideScroll.Avalonia.Controls.Toolbar;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs.Bookmarks;

namespace SideScroll.Avalonia.Controls.Viewer;

public class TabViewerToolbar : TabControlToolbar
{
	public TabViewer TabViewer { get; init; }

	public ToolbarButton ButtonBack { get; protected set; }
	public ToolbarButton ButtonForward { get; protected set; }

	public ToolbarButton ButtonRefresh { get; protected set; }

	public ToolbarButton? ButtonLink { get; protected set; }
	public ToolbarButton? ButtonImport { get; protected set; }

	public TabViewerToolbar(TabViewer tabViewer) : base(null)
	{
		TabViewer = tabViewer;

		// HotKeys are handled in TabViewer
		ButtonBack = AddButton("Back (Alt+Left)", Icons.Svg.LeftArrow);
		ButtonBack.BindIsEnabled(nameof(BookmarkNavigator.CanSeekBackward), TabViewer.Project.Navigator);
		ButtonBack.Add((call) => TabViewer.SeekBackward());

		ButtonForward = AddButton("Forward (Alt+Right)", Icons.Svg.RightArrow);
		ButtonForward.BindIsEnabled(nameof(BookmarkNavigator.CanSeekForward), TabViewer.Project.Navigator);
		ButtonForward.Add((call) => TabViewer.SeekForward());

		AddSeparator();
		ButtonRefresh = AddButton("Refresh (Ctrl+R)", Icons.Svg.Refresh);
		ButtonRefresh.Add(Refresh);

		if (tabViewer.Project.ProjectSettings.EnableLinking)
		{
			AddSeparator();
			ButtonLink = AddButton("Link - Copy to Clipboard", Icons.Svg.Link);
			ButtonImport = AddButton("Import Link from Clipboard", Icons.Svg.Import);
		}
	}

	public void AddVersion()
	{
		AddFill();

		string versionLabel = 'v' + TabViewer.Project.Version.Formatted();
#if DEBUG
		versionLabel += " *";
#endif
		var textBlock = new ToolbarHeaderTextBlock(versionLabel);
		AddControl(textBlock);
	}

	private void Refresh(Call call)
	{
		TabViewer.Reload(call);
	}
}
