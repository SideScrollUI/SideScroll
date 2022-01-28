using Newtonsoft.Json;
using System;

namespace Atlas.Core;

public static class JsonUtils
{
	public static string Format(string text)
	{
		try
		{
			if (text?.StartsWith("{") == true)
			{
				dynamic parsedJson = JsonConvert.DeserializeObject(text);
				string formatted = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
				return formatted;
			}
		}
		catch (Exception)
		{
		}
		return text;
	}
}
