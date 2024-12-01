using Avalonia.Controls;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using SideScroll.Tabs.Tools.FileViewer;
using SideScroll.UI.Avalonia.Tabs;
using SideScroll.UI.Avalonia.Themes;
using SideScroll.UI.Avalonia.Viewer;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.UI.Avalonia;

public class BaseView : UserControl
{
	private const int MinWindowWidth = 700;
	private const int MinWindowHeight = 500;

	private const int DefaultWindowWidth = 1280;
	private const int DefaultWindowHeight = 800;

	public static BaseView? Instance { get; set; }

	public Project Project;

	public TabViewer TabViewer;

	private bool _loadComplete;

	public BaseView(Project project)
	{
		Initialize(project);
	}

	public BaseView(ProjectSettings settings)
	{
		Initialize(new Project(settings));
	}

	[MemberNotNull(nameof(Project), nameof(TabViewer))]
	private void Initialize(Project project)
	{
		Instance = this;

		SideScrollInit.Initialize();

		TabFile.RegisterType<TabFileImage>(TabFileImage.DefaultExtensions);

		LoadProject(project);
	}

	[MemberNotNull(nameof(Project), nameof(TabViewer))]
	public void LoadProject(Project project)
	{
		Project = project;

		InitializeComponent();

		_loadComplete = true;
	}

	// Load here instead of in xaml for better control
	[MemberNotNull(nameof(TabViewer))]
	private void InitializeComponent()
	{
		Background = SideScrollTheme.TabBackground;

		MinWidth = MinWindowWidth;
		MinHeight = MinWindowHeight;

		Content = TabViewer = new TabViewer(Project);
	}

	public virtual void AddTab(ITab tab)
	{
		TabViewer.AddTab(tab);
	}
}
