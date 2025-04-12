using Avalonia.Controls;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.Tabs;
using SideScroll.Avalonia.Themes;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using SideScroll.Tabs.Tools.FileViewer;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Avalonia.Controls;

public class BaseView : UserControl
{
	public static BaseView? Instance { get; set; }

	public Project Project { get; protected set; }

	public TabViewer TabViewer { get; protected set; }

	public BaseView(Project project)
	{
		Initialize(project);
	}

	public BaseView(ProjectSettings settings)
	{
		Initialize(Project.Load(settings));
	}

	[MemberNotNull(nameof(Project), nameof(TabViewer))]
	private void Initialize(Project project)
	{
		Instance = this;

		SideScrollInit.Initialize();
		SideScrollTheme.InitializeFonts();

		TabFile.RegisterType<TabFileImage>(TabFileImage.DefaultExtensions);

		LoadProject(project);
	}

	[MemberNotNull(nameof(Project), nameof(TabViewer))]
	public void LoadProject(Project project)
	{
		project.Initialize();
		Project = project;

		ThemeManager.Initialize(project);

		Background = SideScrollTheme.TabBackground;

		Content = TabViewer = new TabViewer(Project);
	}

	public virtual void AddTab(ITab tab)
	{
		TabViewer.AddTab(tab);
	}
}
