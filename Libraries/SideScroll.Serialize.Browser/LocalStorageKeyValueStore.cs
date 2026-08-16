using SideScroll.Serialize.KeyValue;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text.Json;

namespace SideScroll.Serialize.Browser;

/// <summary>
/// Reaches the browser's localStorage through JavaScript interop
/// </summary>
/// <remarks>
/// The only part of this library that needs a browser to run. Everything built on it goes through
/// <see cref="IKeyValueStore"/>, so the logic above is tested against an in-memory store instead
/// </remarks>
[SupportedOSPlatform("browser")]
public partial class LocalStorageKeyValueStore : IKeyValueStore
{
	/// <summary>The store every localStorage serializer and repository shares</summary>
	public static LocalStorageKeyValueStore Default { get; } = new();

	/// <inheritdoc/>
	public string? Get(string key) => GetLocalStorageItem(key);

	/// <inheritdoc/>
	public bool Set(string key, string value) => SetLocalStorageItem(key, value);

	/// <inheritdoc/>
	public bool Exists(string key) => ExistsInStorage(key);

	/// <inheritdoc/>
	public void Remove(string key) => RemoveLocalStorageItem(key);

	/// <inheritdoc/>
	public IEnumerable<string> GetKeys(string prefix)
	{
		// Since JSImport doesn't support string[], get them via JSON
		string json = GetKeysJson(prefix);
		return JsonSerializer.Deserialize<List<string>>(json) ?? [];
	}

	// JavaScript interop methods - using globalThis.BrowserStorage
	// NOTE: Requires importing the package's localStorage.js module first:
	// await JSHost.ImportAsync("SideScroll.Serialize.Browser", "../_content/SideScroll.Serialize.Browser/localStorage.js");
	[JSImport("globalThis.BrowserStorage.load")]
	private static partial string? GetLocalStorageItem(string key);

	[JSImport("globalThis.BrowserStorage.save")]
	private static partial bool SetLocalStorageItem(string key, string value);

	[JSImport("globalThis.BrowserStorage.exists")]
	private static partial bool ExistsInStorage(string key);

	[JSImport("globalThis.BrowserStorage.remove")]
	private static partial void RemoveLocalStorageItem(string key);

	[JSImport("globalThis.BrowserStorage.getKeysJson")]
	private static partial string GetKeysJson(string prefix);
}
