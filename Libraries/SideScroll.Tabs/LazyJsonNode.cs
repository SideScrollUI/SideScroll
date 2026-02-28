using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SideScroll.Tabs;

/// <summary>
/// Base class for lazy-loading JSON nodes that defers parsing until accessed
/// </summary>
public class LazyJsonNode
{
	/// <summary>
	/// Creates a lazy JSON wrapper from a JsonNode
	/// </summary>
	/// <returns>A LazyJsonArray, LazyJsonObject, string, or null depending on the node type</returns>
	public static object? Create(JsonNode? jsonNode)
	{
		if (jsonNode == null)
		{
			return null;
		}
		else if (jsonNode is JsonArray array)
		{
			return new LazyJsonArray(array);
		}
		else if (jsonNode is JsonObject obj)
		{
			return new LazyJsonObject(obj);
		}
		else if (jsonNode.GetValue<JsonElement>() is JsonElement jsonElement)
		{
			return jsonElement.ToString();
		}
		throw new Exception("Invalid JSON Node Type");
	}

	/// <summary>
	/// Parses JSON text and creates a lazy JSON wrapper
	/// </summary>
	public static object? Parse(string json)
	{
		JsonNode? jsonValue = JsonNode.Parse(json);
		return Create(jsonValue);
	}

	/// <summary>
	/// Loads JSON from a file path and creates a lazy JSON wrapper
	/// </summary>
	public static object? LoadPath(string path)
	{
		string text = File.ReadAllText(path);
		return Parse(text);
	}
}

/// <summary>
/// Represents a JSON array that lazily loads its items
/// </summary>
public class LazyJsonArray(JsonArray jsonArray) : LazyJsonNode
{
	/// <summary>
	/// The underlying JsonArray
	/// </summary>
	public JsonArray JsonArray => jsonArray;

	/// <summary>
	/// Lazily loaded list of array items
	/// </summary>
	[InnerValue, StyleValue]
	public List<object?> Items
	{
		get
		{
			if (_items == null)
			{
				_items = [];
				foreach (JsonNode? jsonNode in JsonArray)
				{
					_items.Add(Create(jsonNode));
				}
			}
			return _items;
		}
	}
	private List<object?>? _items;

	public override string? ToString() => Items.Formatted();
}

/// <summary>
/// Represents a JSON object that lazily loads its properties
/// </summary>
public class LazyJsonObject(JsonObject jsonObject) : LazyJsonNode
{
	/// <summary>
	/// Lazily loaded list of object properties
	/// </summary>
	[InnerValue, StyleValue]
	public List<LazyJsonProperty> Items
	{
		get
		{
			if (_items == null)
			{
				_items = [];
				foreach (var pair in jsonObject)
				{
					var property = new LazyJsonProperty
					{
						Key = pair.Key,
						Value = Create(pair.Value),
					};
					_items.Add(property);
				}
			}
			return _items;
		}
	}
	private List<LazyJsonProperty>? _items;

	public override string? ToString() => Items.Formatted();
}

/// <summary>
/// Represents a JSON property (key-value pair)
/// </summary>
public class LazyJsonProperty : LazyJsonNode
{
	/// <summary>
	/// The property key/name
	/// </summary>
	public string? Key { get; set; }

	/// <summary>
	/// The property value
	/// </summary>
	[InnerValue, StyleValue]
	public object? Value { get; set; }

	/*public bool HasChildren
	{
		get
		{
			switch (jsonValue.JsonType)
			{
				case JsonType.Object: return true;
				case JsonType.Array: return true;
			}
		}
	}*/

	public override string? ToString() => Key;
}
