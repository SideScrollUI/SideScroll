using SideScroll.Serialize.DataRepos;

namespace SideScroll.Serialize.KeyValue;

/// <summary>
/// Converts between the logical paths callers use and the flat keys a
/// <see cref="IKeyValueStore"/> holds them under
/// </summary>
public static class StorageKeys
{
	/// <summary>The prefix every item's data key starts with</summary>
	public const string DataPrefix = "SideScroll_Data_";

	/// <summary>The prefix every item's header key starts with</summary>
	public const string HeaderPrefix = "SideScroll_Header_";

	/// <summary>Converts a logical path to the key its data is stored under</summary>
	public static string DataKey(string path)
	{
		return DataPrefix + Uri.EscapeDataString(Normalize(path));
	}

	/// <summary>Converts a logical path to the key its header is stored under</summary>
	public static string HeaderKey(string path)
	{
		return HeaderPrefix + Uri.EscapeDataString(Normalize(path));
	}

	/// <summary>Converts a data key back to the logical path it was made from</summary>
	/// <exception cref="ArgumentException">The key isn't a data key</exception>
	public static string ToPath(string storageKey)
	{
		// The header prefix also starts with "SideScroll_", so blindly trimming the data prefix
		// off one would leave part of it in the path instead of failing
		if (!storageKey.StartsWith(DataPrefix, StringComparison.Ordinal))
		{
			throw new ArgumentException($"Not a {DataPrefix} key: {storageKey}", nameof(storageKey));
		}

		string encodedPath = storageKey[DataPrefix.Length..];
		return Uri.UnescapeDataString(encodedPath);
	}

	/// <summary>Returns whether a data key holds an item directly within a logical group path</summary>
	public static bool IsDataKeyInGroup(string storageKey, string groupPath)
	{
		string normalizedGroup = Normalize(groupPath).TrimEnd('/');
		string path = Normalize(ToPath(storageKey));
		if (!path.StartsWith(normalizedGroup + '/', StringComparison.Ordinal))
			return false;

		string relativePath = path[(normalizedGroup.Length + 1)..];
		return !relativePath.Contains('/') &&
			!relativePath.Equals(DataRepo.PrimaryIndexFileName, StringComparison.Ordinal);
	}

	// Keys are stored with forward slashes so a path written on Windows finds what another wrote
	private static string Normalize(string path) => path.Replace('\\', '/');
}
