using System;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Atlas.Core;

public static class JsonUtils
{
	public static string Format(string text)
	{
		if (text?.StartsWith("{") != true)
			return text;
		
		try
		{
			using var jsonDocument = JsonDocument.Parse(text);
			JsonSerializerOptions serializerOptions = new()
			{ 
				WriteIndented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			};
			return JsonSerializer.Serialize(jsonDocument, serializerOptions);
		}
		catch (Exception)
		{
			return text;
		}
	}
}
