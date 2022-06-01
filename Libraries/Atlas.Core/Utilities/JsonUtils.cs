using System;
using Newtonsoft.Json;

namespace Atlas.Core;

public static class JsonUtils
{
	public static string Format(string text)
	{
		if (text?.StartsWith("{") != true)
			return text;
		
		try
		{
			// System.Json has a lot of problems with newlines (not fixable?) and + (fixable)
			// Can escape newlines inside double quotes, but escaping outside strings produces parsing errors
			dynamic parsedJson = JsonConvert.DeserializeObject(text);
			string formatted = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
			return formatted;
		}
		catch (Exception)
		{
			return text;
		}
	}
}
