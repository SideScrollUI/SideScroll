using Avalonia.Controls;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.Tabs;
using SideScroll.Avalonia.Themes;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using SideScroll.Tabs.Tools.FileViewer;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Avalonia.Controls;

/// <summary>
/// Base Avalonia UserControl that initializes a SideScroll project and hosts a <see cref="TabViewer"/>.
/// </summary>
public class BaseView : UserControl
{
	/// <summary>Gets the active SideScroll project.</summary>
	public Project Project { get; protected set; }

	/// <summary>Gets the root tab viewer hosted in this view.</summary>
	public TabViewer TabViewer { get; protected set; }

	public BaseView(Project project)
	{
		SideScrollInit.Initialize();
		SideScrollTheme.InitializeFonts();

		TabFile.RegisterType<TabFileImage>(TabFileImage.DefaultExtensions);

		LoadProject(project);
	}

	public BaseView(ProjectSettings settings) :
		this(Project.Load(settings))
	{
	}

	[MemberNotNull(nameof(Project), nameof(TabViewer))]
	private void LoadProject(Project project)
	{
		project.Initialize();
		Project = project;

		ThemeManager.Initialize(project);

		Background = SideScrollTheme.TabBackground;

		Content = TabViewer = new TabViewer(Project, false);
	}

	/// <summary>Loads and displays a tab in the tab viewer.</summary>
	public virtual void LoadTab(ITab tab)
	{
		TabViewer.LoadTab(tab);
	}
}
