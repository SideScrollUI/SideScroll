using System.Runtime.Versioning;
using System.Text.Json;
using Microsoft.JSInterop;

namespace SideScroll.Demo.Avalonia.Browser;

/// <summary>
/// Provides localStorage-based persistence for browser applications
/// </summary>
[SupportedOSPlatform("browser")]
public class BrowserStorageService
{
	private readonly IJSRuntime _jsRuntime;
	private const string StoragePrefix = "SideScroll_";

	public BrowserStorageService(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
	}

	/// <summary>
	/// Saves an object to localStorage as JSON
	/// </summary>
	public async Task<bool> SaveAsync<T>(Call call, string key, T value)
	{
		try
		{
			string json = JsonSerializer.Serialize(value, new JsonSerializerOptions
			{
				WriteIndented = false
			});
			
			string fullKey = StoragePrefix + key;
			return await _jsRuntime.InvokeAsync<bool>("BrowserStorage.saveAsync", fullKey, json);
		}
		catch (Exception ex)
		{
			call.Log.Add(ex, new Tag("Key", key), new Tag("Operation", "SaveAsync"));
			return false;
		}
	}

	/// <summary>
	/// Loads an object from localStorage
	/// </summary>
	public async Task<T?> LoadAsync<T>(Call call, string key) where T : class
	{
		try
		{
			string fullKey = StoragePrefix + key;
			string? json = await _jsRuntime.InvokeAsync<string?>("BrowserStorage.loadAsync", fullKey);
			
			if (string.IsNullOrEmpty(json))
				return null;

			return JsonSerializer.Deserialize<T>(json);
		}
		catch (Exception ex)
		{
			call.Log.Add(ex, new Tag("Key", key), new Tag("Operation", "LoadAsync"));
			return null;
		}
	}

	/// <summary>
	/// Checks if a key exists in localStorage
	/// </summary>
	public async Task<bool> ExistsAsync(string key)
	{
		try
		{
			string fullKey = StoragePrefix + key;
			return await _jsRuntime.InvokeAsync<bool>("BrowserStorage.existsAsync", fullKey);
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Removes a key from localStorage
	/// </summary>
	public async Task<bool> RemoveAsync(string key)
	{
		try
		{
			string fullKey = StoragePrefix + key;
			return await _jsRuntime.InvokeAsync<bool>("BrowserStorage.removeAsync", fullKey);
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Gets storage statistics
	/// </summary>
	public async Task<StorageStats?> GetStatsAsync()
	{
		try
		{
			return await _jsRuntime.InvokeAsync<StorageStats>("BrowserStorage.getStatsAsync");
		}
		catch
		{
			return null;
		}
	}
}

public record StorageStats(int ItemCount, int EstimatedSize, string EstimatedSizeMB);
