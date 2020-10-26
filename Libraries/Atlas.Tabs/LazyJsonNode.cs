using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Json;

namespace Atlas.Tabs
{
	public class LazyJsonNode
	{
		public static object Create(JsonValue jsonValue)
		{
			if (jsonValue == null)
				return null;
			switch (jsonValue.JsonType)
			{
				case JsonType.String: return (string)jsonValue;
				case JsonType.Number:// break;
				case JsonType.Boolean: return jsonValue.ToString();
				case JsonType.Object: return new LazyJsonObject((JsonObject)jsonValue);
				case JsonType.Array: return new LazyJsonArray((JsonArray)jsonValue);
			}
			throw new Exception("Invalid JSON Node Type");
		}

		public static object Parse(string json)
		{
			JsonValue jsonValue = JsonValue.Parse(json);
			return Create(jsonValue);
		}

		public static object LoadPath(string path)
		{
			string text = File.ReadAllText(path);
			return Parse(text);
		}
	}

	public class LazyJsonArray : LazyJsonNode
	{
		public JsonArray JsonArray;
		private List<object> _items;

		[InnerValue, StyleValue]
		public List<object> Items
		{
			get
			{
				if (_items == null)
				{
					_items = new List<object>();
					foreach (JsonValue jsonValue in JsonArray)
					{
						_items.Add(Create(jsonValue));
					}
				}
				return _items;
			}
		}

		public LazyJsonArray(JsonArray jsonArray)
		{
			JsonArray = jsonArray;
		}

		public override string ToString() => Items.Formatted();
	}

	public class LazyJsonObject : LazyJsonNode
	{
		private JsonObject JsonObject;

		private List<LazyJsonProperty> _items;

		[InnerValue, StyleValue]
		public List<LazyJsonProperty> Items
		{
			get
			{
				if (_items == null)
				{
					_items = new List<LazyJsonProperty>();
					foreach (var pair in JsonObject)
					{
						var property = new LazyJsonProperty()
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

		public LazyJsonObject(JsonObject jsonObject)
		{
			JsonObject = jsonObject;
		}

		public override string ToString() => Items.Formatted();
	}

	public class LazyJsonProperty : LazyJsonNode
	{
		public string Key { get; set; }
		[InnerValue, StyleValue]
		public object Value { get; set; }

		/*public bool HasChildren
		{
			get
			{
				switch (jsonValue.JsonType)
				{
					case JsonType.String: return (string)jsonValue;
					case JsonType.Number:// break;
					case JsonType.Boolean: return jsonValue.ToString();
					case JsonType.Object: return new LazyJsonObject((JsonObject)jsonValue);
					case JsonType.Array: return new LazyJsonArray((JsonArray)jsonValue);
				}
			}
		}*/

		public override string ToString()
		{
			return Key;
		}
	}
}
