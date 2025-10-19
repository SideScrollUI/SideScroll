using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

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
			// System.Json has a lot of problems with newlines (not fixable?) and + (fixable)
			// Can escape newlines inside double quotes, but escaping outside strings produces parsing errors
			dynamic parsedJson = JsonConvert.DeserializeObject(text)!;
			json = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
