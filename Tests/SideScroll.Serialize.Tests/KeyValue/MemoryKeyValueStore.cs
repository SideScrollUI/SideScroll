using SideScroll.Serialize.KeyValue;

namespace SideScroll.Serialize.Tests.KeyValue;

/// <summary>
/// An in-memory <see cref="IKeyValueStore"/> standing in for the browser's localStorage, with the
/// failures a real store has: a quota that rejects a write, and being unreachable
/// </summary>
public class MemoryKeyValueStore : IKeyValueStore
{
	private readonly Dictionary<string, string> _items = [];

	/// <summary>Rejects every write when set, the way a store out of quota does</summary>
	public bool RejectWrites { get; set; }

	/// <summary>Rejects only writes to keys starting with this, for failing one write of several</summary>
	public string? RejectWritesToPrefix { get; set; }

	/// <summary>Throws from every operation when set, the way an unreachable store does</summary>
	public bool Unreachable { get; set; }

	/// <summary>The keys currently holding a value</summary>
	public IReadOnlyCollection<string> Keys => _items.Keys;

	/// <summary>Returns the value stored directly, bypassing the failure switches</summary>
	public string? Peek(string key) => _items.GetValueOrDefault(key);

	/// <summary>Stores a value directly, bypassing the failure switches</summary>
	public void Poke(string key, string value) => _items[key] = value;

	/// <inheritdoc/>
	public string? Get(string key)
	{
		ThrowIfUnreachable();
		return _items.GetValueOrDefault(key);
	}

	/// <inheritdoc/>
	public bool Set(string key, string value)
	{
		ThrowIfUnreachable();

		if (RejectWrites) return false;
		if (RejectWritesToPrefix != null && key.StartsWith(RejectWritesToPrefix, StringComparison.Ordinal))
			return false;

		_items[key] = value;
		return true;
	}

	/// <inheritdoc/>
	public bool Exists(string key)
	{
		ThrowIfUnreachable();
		return _items.ContainsKey(key);
	}

	/// <inheritdoc/>
	public void Remove(string key)
	{
		ThrowIfUnreachable();
		_items.Remove(key);
	}

	/// <inheritdoc/>
	public IEnumerable<string> GetKeys(string prefix)
	{
		ThrowIfUnreachable();
		return _items.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)).ToList();
	}

	private void ThrowIfUnreachable()
	{
		if (Unreachable)
			throw new InvalidOperationException("Store is unreachable");
	}
}
