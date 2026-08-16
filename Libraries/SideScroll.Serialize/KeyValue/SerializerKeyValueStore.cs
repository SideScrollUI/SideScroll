using SideScroll.Serialize.Atlas;
using SideScroll.Serialize.Json;
using SideScroll.Tasks;
using System.Text.Json;

namespace SideScroll.Serialize.KeyValue;

/// <summary>
/// Serializes objects as JSON into an <see cref="IKeyValueStore"/> rather than onto a file system
/// </summary>
/// <remarks>
/// Split out of the browser's localStorage serializer, which is now this over a store that reaches
/// localStorage. Everything here is storage agnostic, so it can be tested against any store
/// </remarks>
public class SerializerKeyValueStore : SerializerFile
{
	/// <summary>The header stored alongside an item's data</summary>
	protected sealed class StorageHeader
	{
		/// <summary>The format version the item was written with</summary>
		public int Version { get; set; } = 1;

		/// <summary>The name the item was saved under</summary>
		public string? Name { get; set; }
	}

	/// <summary>The store this reads and writes</summary>
	protected IKeyValueStore Store { get; }

	/// <summary>The key this instance's data is stored under</summary>
	public string StorageKey { get; }

	/// <summary>Initializes a serializer over a store, for a logical path</summary>
	public SerializerKeyValueStore(IKeyValueStore store, string basePath, string name = "") :
		base(basePath, name)
	{
		Store = store;
		StorageKey = StorageKeys.DataKey(basePath);
		DataPath = basePath; // Keep original path for compatibility
	}

	/// <summary>Returns whether this instance's data is stored, treating an unreachable store as absent</summary>
	public override bool Exists
	{
		get
		{
			try
			{
				return Store.Exists(StorageKey);
			}
			catch
			{
				return false;
			}
		}
	}

	/// <summary>No-op: a key value store has nothing to create ahead of writing</summary>
	protected override void EnsureStorageExists() { }

	/// <summary>Serializes <paramref name="obj"/> to JSON and stores it under <see cref="StorageKey"/>.</summary>
	/// <exception cref="SerializerException">The store rejected the write, it can be limited by a quota</exception>
	/// <remarks>
	/// Serialization failures aren't caught either, so a save that stored nothing reaches the
	/// caller instead of returning as though it had succeeded. Callers can discard their only copy
	/// of what they saved, so failing quietly loses it
	/// </remarks>
	protected override void SaveInternal(Call call, object obj, string? name = null, bool publicOnly = false)
	{
		var options = publicOnly
			? JsonConverters.PublicSerializerOptions
			: JsonConverters.PrivateSerializerOptions;

		string json = JsonSerializer.Serialize(obj, obj.GetType(), options);

		if (!Store.Set(StorageKey, json))
		{
			throw new SerializerException("Failed to save to storage",
				new Tag("Name", name),
				new Tag("Type", obj.GetType()),
				new Tag("Key", StorageKey),
				new Tag("Size", json.Length));
		}

		// The data is stored at this point, so a rejected header leaves it without one rather than
		// losing it. Still a failed save, the header is what names it when it's loaded back
		string headerJson = JsonSerializer.Serialize(new StorageHeader { Name = name });
		if (!Store.Set(StorageKeys.HeaderKey(BasePath), headerJson))
		{
			throw new SerializerException("Saved to storage without its header",
				new Tag("Name", name),
				new Tag("Type", obj.GetType()),
				new Tag("Key", StorageKey));
		}

		call.Log.AddDebug("Saved to storage",
			new Tag("Name", name),
			new Tag("Key", StorageKey),
			new Tag("Size", json.Length));
	}

	/// <summary>Reads JSON from <see cref="StorageKey"/> and deserializes it to <paramref name="expectedType"/>.</summary>
	/// <remarks>
	/// SerializerFile.Load() finishes the task for every path through here, so this only marks the
	/// failure. It can't leave that to Load() either, these exceptions are caught rather than let
	/// out to it
	/// </remarks>
	protected override object? LoadInternal(Call call, bool lazy, TaskInstance? taskInstance, bool publicOnly = false, Type? expectedType = null)
	{
		var options = publicOnly
			? JsonConverters.PublicSerializerOptions
			: JsonConverters.PrivateSerializerOptions;

		try
		{
			string? json = Store.Get(StorageKey);

			if (string.IsNullOrEmpty(json))
			{
				call.Log.AddDebug("No data found in storage",
					new Tag("Key", StorageKey));
				return null;
			}

			call.Log.AddDebug("Loaded from storage",
				new Tag("Key", StorageKey),
				new Tag("Size", json.Length));

			// Use expectedType if provided, otherwise fallback to Dictionary
			return expectedType != null
				? JsonSerializer.Deserialize(json, expectedType, options)
				: JsonSerializer.Deserialize<Dictionary<string, object?>>(json, options);
		}
		catch (Exception e)
		{
			call.Log.Add(e, new Tag("Key", StorageKey));

			if (taskInstance != null)
			{
				taskInstance.Errored = true;
				taskInstance.Message ??= e.Message;
			}

			return null;
		}
	}

	/// <inheritdoc/>
	public override SerializerHeader LoadHeader(Call call)
	{
		string? json = Store.Get(StorageKeys.HeaderKey(BasePath));
		if (string.IsNullOrEmpty(json))
		{
			return new SerializerHeader { Name = Name };
		}

		StorageHeader? header = JsonSerializer.Deserialize<StorageHeader>(json);
		return new SerializerHeader
		{
			Version = header?.Version is { } version ? checked((ushort)version) : null,
			Name = header?.Name,
		};
	}

	/// <summary>Returns whether data is stored for a logical path</summary>
	public bool PathExists(string path) => Store.Exists(StorageKeys.DataKey(path));

	/// <summary>Removes the data and header stored for a logical path</summary>
	public void RemovePath(string path)
	{
		Store.Remove(StorageKeys.DataKey(path));
		Store.Remove(StorageKeys.HeaderKey(path));
	}

	/// <summary>Returns every stored data key, treating an unreachable store as holding none</summary>
	public IReadOnlyList<string> GetAllKeys()
	{
		try
		{
			return [.. Store.GetKeys(StorageKeys.DataPrefix)];
		}
		catch
		{
			return [];
		}
	}
}
