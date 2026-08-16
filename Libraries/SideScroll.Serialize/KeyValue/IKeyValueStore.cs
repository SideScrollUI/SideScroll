namespace SideScroll.Serialize.KeyValue;

/// <summary>
/// A flat key to string store, which <see cref="SerializerKeyValueStore"/> serializes into
/// </summary>
/// <remarks>
/// The browser's localStorage is the reason this exists, but nothing here depends on it. Keeping
/// the interface separate from its implementation is what lets the serializer above it be tested
/// without a browser to run in
/// </remarks>
public interface IKeyValueStore
{
	/// <summary>Returns the value stored for a key, or null if there isn't one</summary>
	string? Get(string key);

	/// <summary>Stores a value, returning false if it couldn't be stored</summary>
	/// <remarks>A store can be limited by a quota, which is a failure rather than an exception</remarks>
	bool Set(string key, string value);

	/// <summary>Returns whether a key holds a value</summary>
	/// <remarks>
	/// Throws if the store can't be reached, so a caller removing entries based on this doesn't
	/// treat a failure as everything being missing
	/// </remarks>
	bool Exists(string key);

	/// <summary>Removes whatever is stored for a key</summary>
	void Remove(string key);

	/// <summary>Returns every key starting with a prefix</summary>
	IEnumerable<string> GetKeys(string prefix);
}
