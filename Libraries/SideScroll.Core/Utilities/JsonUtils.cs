using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Core.Utilities;

public static class JsonUtils
{
	public static bool IsJson(string? text)
	{
		if (text == null) return false;

		text = text.Trim();
		return text.StartsWith('{') && text.EndsWith('}');
	}

	public static string Format(string text)
	{
		if (TryFormat(text, out string? json)) return json;

		return text;
	}

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
