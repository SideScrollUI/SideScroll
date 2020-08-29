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
		private JsonArray jsonArray;
		private List<object> _Items;
		[InnerValue, StyleValue]
		public List<object> Items
		{
			get
			{
				if (_Items == null)
				{
					_Items = new List<object>();
					foreach (JsonValue jsonValue in jsonArray)
					{
						_Items.Add(Create(jsonValue));
					}
				}
				return _Items;
			}
		}

		public LazyJsonArray(JsonArray jsonArray)
		{
			this.jsonArray = jsonArray;
		}

		public override string ToString() => Items.Formatted();
	}

	public class LazyJsonObject : LazyJsonNode
	{
		private JsonObject jsonObject;
		private List<LazyJsonProperty> _Items;
		[InnerValue, StyleValue]
		public List<LazyJsonProperty> Items
		{
			get
			{
				if (_Items == null)
				{
					_Items = new List<LazyJsonProperty>();
					foreach (var pair in jsonObject)
					{
						var property = new LazyJsonProperty()
						{
							Key = pair.Key,
							Value = Create(pair.Value),
						};
						_Items.Add(property);
					}
				}
				return _Items;
			}
		}

		public LazyJsonObject(JsonObject jsonObject)
		{
			this.jsonObject = jsonObject;
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
