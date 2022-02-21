using System;
using System.Text.Json;

namespace Atlas.Core;

public static class JsonUtils
{
	public static string Format(string text)
	{
		try
		{
			if (text?.StartsWith("{") == true)
			{
				using var jsonDocument = JsonDocument.Parse(text);
				return JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions { WriteIndented = true });
			}
		}
		catch (Exception)
		{
		}
		return text;
	}
}
