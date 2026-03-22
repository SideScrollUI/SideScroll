using Avalonia.Interactivity;
using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Samples;
using SideScroll.Avalonia.Samples.Tabs;
using SideScroll.Tabs;

namespace SideScroll.Demo.Avalonia.Browser;

public class BrowserMainView : BaseView
{
	public static BrowserMainView? Instance { get; private set; }
	
	private bool _settingsLoaded = false;
	private System.Timers.Timer? _saveTimer;

	public BrowserMainView() : base(BrowserProject.Load())
	{
		Instance = this;
		
		LoadTab(new TabAvalonia());
		LiveChartCreator.Register();
		TabViewer.Toolbar?.AddRightControls();

		// Use Avalonia's Loaded event to load settings asynchronously after UI is ready
		Loaded += OnLoadedAsync;
		
		// Set up auto-save timer (saves every 30 seconds)
		_saveTimer = new System.Timers.Timer(30000);
		_saveTimer.Elapsed += async (s, e) => await SaveSettingsAsync();
		_saveTimer.AutoReset = true;

		Console.WriteLine("✓ BrowserMainView initialized with localStorage support");
	}

	private async void OnLoadedAsync(object? sender, RoutedEventArgs e)
	{
		if (_settingsLoaded)
			return;

		_settingsLoaded = true;
		
		// Load settings from localStorage after the UI is fully initialized
		Console.WriteLine("🔄 Loading settings from localStorage...");
		bool loaded = await BrowserProject.LoadUserSettingsFromStorageAsync(Project);
		
		if (!loaded)
		{
			// Save default settings for next time
			Console.WriteLine("💾 Saving default settings...");
			await BrowserProject.SaveUserSettingsToStorageAsync(Project);
		}
		
		// Start auto-save timer after initial load
		_saveTimer?.Start();
	}

	private async Task SaveSettingsAsync()
	{
		if (!_settingsLoaded)
			return; // Don't save during initial load

		await BrowserProject.SaveUserSettingsToStorageAsync(Project);
	}
}
