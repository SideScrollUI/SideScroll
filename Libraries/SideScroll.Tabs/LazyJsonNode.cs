using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SideScroll.Tabs;

public class LazyJsonNode
{
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
		else if (jsonNode?.GetValue<JsonElement>() is JsonElement jsonElement)
		{
			return jsonElement.ToString();
		}
		throw new Exception("Invalid JSON Node Type");
	}

	public static object? Parse(string json)
	{
		JsonNode? jsonValue = JsonNode.Parse(json);
		return Create(jsonValue);
	}

	public static object? LoadPath(string path)
	{
		string text = File.ReadAllText(path);
		return Parse(text);
	}
}

public class LazyJsonArray(JsonArray jsonArray) : LazyJsonNode
{
	public JsonArray JsonArray => jsonArray;

	[InnerValue, StyleValue]
	public List<object?> Items
	{
		get
		{
			if (_items == null)
			{
				_items = new List<object?>();
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

public class LazyJsonObject(JsonObject jsonObject) : LazyJsonNode
{
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

public class LazyJsonProperty : LazyJsonNode
{
	public string? Key { get; set; }

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
