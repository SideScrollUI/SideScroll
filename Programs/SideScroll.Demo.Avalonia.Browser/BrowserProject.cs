using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text.Json;
using SideScroll.Avalonia.Samples;
using SideScroll.Serialize.Json;
using SideScroll.Tabs;

namespace SideScroll.Demo.Avalonia.Browser;

public static partial class BrowserProject
{
	private static bool _moduleImported = false;

	/// <summary>
	/// Loads project with default settings synchronously (for initial construction)
	/// </summary>
	public static Project Load()
	{
		var projectSettings = SampleProjectSettings.Settings;
		return Project.Load<SampleUserSettings>(projectSettings);
	}

	/// <summary>
	/// Ensures the JavaScript module is imported
	/// </summary>
	private static async Task EnsureModuleImportedAsync()
	{
		if (!_moduleImported)
		{
			await JSHost.ImportAsync("main.js", "../main.js");
			_moduleImported = true;
			Console.WriteLine("✓ JavaScript module imported");
		}
	}

	/// <summary>
	/// Asynchronously loads user settings from localStorage and updates the project
	/// This should be called after the app is fully initialized
	/// </summary>
	public static async Task<bool> LoadUserSettingsFromStorageAsync(Project project)
	{
		try
		{
			await EnsureModuleImportedAsync();
			
			const string key = "SideScroll_UserSettings";
			string? json = await GetLocalStorageItem(key);
			
			if (string.IsNullOrEmpty(json))
			{
				Console.WriteLine("📭 No saved settings found in localStorage");
				return false;
			}

			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				Converters = { new TimeZoneInfoJsonConverter() }
			};

			var userSettings = JsonSerializer.Deserialize<SampleUserSettings>(json, options);
			if (userSettings != null)
			{
				project.UserSettings = userSettings;
				project.Initialize(); // Reinitialize with loaded settings
				Console.WriteLine($"✓ Loaded user settings from localStorage ({json.Length} bytes)");
				return true;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ Failed to load settings from localStorage: {ex.Message}");
		}
		
		return false;
	}

	/// <summary>
	/// Asynchronously saves user settings to localStorage
	/// </summary>
	public static async Task<bool> SaveUserSettingsToStorageAsync(Project project)
	{
		try
		{
			await EnsureModuleImportedAsync();
			
			const string key = "SideScroll_UserSettings";
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				Converters = { new TimeZoneInfoJsonConverter() }
			};
			
			string json = JsonSerializer.Serialize(project.UserSettings, options);
			
			await SetLocalStorageItem(key, json);
			Console.WriteLine($"✓ Saved user settings to localStorage ({json.Length} bytes)");
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ Failed to save settings to localStorage: {ex.Message}");
			return false;
		}
	}

	[JSImport("BrowserStorage.load", "main.js")]
	private static partial Task<string?> GetLocalStorageItem(string key);

	[JSImport("BrowserStorage.save", "main.js")]
	private static partial Task<bool> SetLocalStorageItem(string key, string value);
}

