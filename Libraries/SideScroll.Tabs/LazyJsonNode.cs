using SideScroll.Extensions;
using System.Json;

namespace SideScroll.Tabs;

public class LazyJsonNode
{
	public static object? Create(JsonValue? jsonValue)
	{
		return jsonValue?.JsonType switch
		{
			JsonType.String => (string)jsonValue,
			JsonType.Number => jsonValue.ToString(),
			JsonType.Boolean => jsonValue.ToString(),
			JsonType.Object => new LazyJsonObject((JsonObject)jsonValue),
			JsonType.Array => new LazyJsonArray((JsonArray)jsonValue),
			null => null,
			_ => throw new Exception("Invalid JSON Node Type")
		};
	}

	public static object? Parse(string json)
	{
		JsonValue jsonValue = JsonValue.Parse(json);
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
	public JsonArray JsonArray = jsonArray;

	private List<object?>? _items;
	[InnerValue, StyleValue]
	public List<object?> Items
	{
		get
		{
			if (_items == null)
			{
				_items = new List<object?>();
				foreach (JsonValue jsonValue in JsonArray)
				{
					_items.Add(Create(jsonValue));
				}
			}
			return _items;
		}
	}

	public override string? ToString() => Items.Formatted();
}

public class LazyJsonObject(JsonObject jsonObject) : LazyJsonNode
{
	private List<LazyJsonProperty>? _items;
	[InnerValue, StyleValue]
	public List<LazyJsonProperty> Items
	{
		get
		{
			if (_items == null)
			{
				_items = new List<LazyJsonProperty>();
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
