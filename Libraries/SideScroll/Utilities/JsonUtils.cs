using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace SideScroll.Utilities;

/// <summary>
/// Provides utilities for working with JSON text
/// </summary>
public static class JsonUtils
{
	/// <summary>
	/// Determines whether a string is valid JSON format
	/// </summary>
	/// <returns>True if the text appears to be JSON (starts with { or [ and ends with } or ]); otherwise, false</returns>
	public static bool IsJson(string? text)
	{
		if (text == null) return false;

		text = text.Trim();
		return 
			(text.StartsWith('{') && text.EndsWith('}')) || 
			(text.StartsWith('[') && text.EndsWith(']'));
	}

	/// <summary>
	/// Formats JSON text with proper indentation
	/// </summary>
	/// <returns>The formatted JSON text, or the original text if formatting fails</returns>
	public static string Format(string text)
	{
		if (TryFormat(text, out string? json)) return json;

		return text;
	}

	private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
	{
		WriteIndented = true,
	};

	/// <summary>
	/// Attempts to format JSON text with proper indentation
	/// </summary>
	/// <returns>True if the text was successfully formatted; otherwise, false</returns>
	public static bool TryFormat(string text, [NotNullWhen(true)] out string? json)
	{
		json = default;
		if (!IsJson(text)) return false;

		try
		{
			// First try parsing as-is (handles valid JSON with structural whitespace)
			using JsonDocument document = JsonDocument.Parse(text);
			json = JsonSerializer.Serialize(document.RootElement, _jsonSerializerOptions);
			return true;
		}
		catch (JsonException)
		{
			// If parsing fails, try escaping unescaped control characters inside strings
			// This handles legacy systems that produce malformed JSON
			try
			{
				string escaped = EscapeUnescapedControlCharactersInStrings(text);
				using JsonDocument document = JsonDocument.Parse(escaped);
				json = JsonSerializer.Serialize(document.RootElement, _jsonSerializerOptions);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
		catch (Exception)
		{
			return false;
		}
	}
	
	/// <summary>
	/// Escapes unescaped control characters inside JSON string values
	/// This is a workaround for legacy systems that produce malformed JSON with unescaped control characters
	/// Note: This is a simple heuristic and may not handle all edge cases
	/// </summary>
	public static string EscapeUnescapedControlCharactersInStrings(string json)
	{
		var result = new System.Text.StringBuilder(json.Length + 100);
		bool inString = false;
		bool escaped = false;
		
		for (int i = 0; i < json.Length; i++)
		{
			char c = json[i];
			
			if (escaped)
			{
				result.Append(c);
				escaped = false;
				continue;
			}
			
			if (c == '\\' && inString)
			{
				result.Append(c);
				escaped = true;
				continue;
			}
			
			if (c == '"')
			{
				result.Append(c);
				inString = !inString;
				continue;
			}
			
			// Escape control characters inside strings only
			if (inString && char.IsControl(c))
			{
				switch (c)
				{
					case '\n':
						result.Append("\\n");
						break;
					case '\r':
						result.Append("\\r");
						break;
					case '\t':
						result.Append("\\t");
						break;
					default:
						// For other control characters, use Unicode escape
						result.Append($"\\u{(int)c:X4}");
						break;
				}
			}
			else
			{
				result.Append(c);
			}
		}
		
		return result.ToString();
	}
}
