using System.Runtime.InteropServices.JavaScript;
using Avalonia.Interactivity;
using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Samples;
using SideScroll.Avalonia.Samples.Tabs;
using SideScroll.Logs;
using SideScroll.Tabs;

namespace SideScroll.Demo.Avalonia.Browser;

public partial class BrowserMainView : BaseView
{
	public static BrowserMainView? Instance { get; private set; }
	
	/// <summary>
	/// Global log for browser application - logs appear in browser console via LogWriterConsole
	/// </summary>
	public static Log AppLog { get; } = new();
	
	private static bool _storageModuleImported = false;
	private System.Timers.Timer? _saveTimer;
	private LogWriterConsole? _logWriter;

	public BrowserMainView() : base(BrowserProject.Load())
	{
		Instance = this;
		
		// Hook up console logging for all AppLog messages
		_logWriter = new LogWriterConsole(AppLog);
		
		LoadTab(new TabAvalonia());
		LiveChartCreator.Register();
		TabViewer.Toolbar?.AddRightControls();

		// Import storage.js and reload project with saved settings
		Loaded += OnLoadedAsync;

		// Set up auto-save timer (saves every 30 seconds)
		_saveTimer = new System.Timers.Timer(30000);
		_saveTimer.Elapsed += (s, e) => Project.Data.App.Save(Project.UserSettings);
		_saveTimer.AutoReset = true;
		_saveTimer.Start();

		AppLog.Add("BrowserMainView initialized", new Tag("Storage", "localStorage"));
	}

	private async void OnLoadedAsync(object? sender, RoutedEventArgs e)
	{
		if (_storageModuleImported)
			return;

		_storageModuleImported = true;
		
		try
		{
			// Import the storage.js module first
			await JSHost.ImportAsync("storage.js", "../storage.js");
			AppLog.Add("storage.js module imported");
			
			// Now reload the project using LoadAsync which uses project.Data.App.Load<T>()
			var newProject = await BrowserProject.LoadAsync();
			
			// Update the BaseView's project
			Project.UserSettings = newProject.UserSettings;
			Project.Initialize();
			
			AppLog.Add("User settings loaded from localStorage");
		}
		catch (Exception ex)
		{
			AppLog.Add(ex, new Tag("Context", "LoadSettings"));
		}
	}
}
