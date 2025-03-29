using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SideScroll.Extensions;

public static class StringExtensions
{
	public static bool CaseInsensitiveContains(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
	{
		return text.Contains(value, stringComparison);
	}

	public static string Reverse(this string input)
	{
		char[] chars = input.ToCharArray();
		Array.Reverse(chars);
		return new string(chars);
	}

	public static string? TrimStart(this string? input, string? prefix)
	{
		if (input == null || prefix == null || prefix.Length == 0)
			return input;

		while (input.StartsWith(prefix))
		{
			input = input[prefix.Length..];
		}

		return input;
	}

	public static string TrimEnd(this string input, string postfix)
	{
		if (input.EndsWith(postfix))
		{
			return input[..^postfix.Length];
		}

		return input;
	}

	public static string Range(this string input, int start, int end)
	{
		end++;
		end = Math.Min(end, input.Length);
		if (end < start)
			return "";

		return input[start..end];
	}

	public static string Range(this string input, int start)
	{
		if (input.Length < start)
			return "";

		return input[start..];
	}

	// Adds spaces between words
	// 'wordsNeed_spacesAndWNSToo' -> 'Words Need Spaces And WNS Too'
	public static string WordSpaced(this string? text)
	{
		return WordSpacer.Format(text);
	}

	public static string CamelCased(this string text)
	{
		string lowerCased = text.ToLower();
		string camelCased = char.ToUpper(lowerCased[0]) + lowerCased[1..];
		return camelCased;
	}

	public static IEnumerable<int> GetAllIndexes(this string source, string matchString)
	{
		matchString = Regex.Escape(matchString);
		foreach (Match match in Regex.Matches(source, matchString))
		{
			yield return match.Index;
		}
	}

	public static List<int> AllIndexesOf(this string str, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new ArgumentException("the string to find may not be empty", nameof(value));
		}

		var indexes = new List<int>();
		for (int index = 0; ; index += value.Length)
		{
			index = str.IndexOf(value, index);
			if (index == -1)
				return indexes;

			indexes.Add(index);
		}
	}

	public static IEnumerable<int> AllIndexesOfYield(this string str, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new ArgumentException("the string to find may not be empty", nameof(value));
		}

		for (int index = 0; ; index += value.Length)
		{
			index = str.IndexOf(value, index);
			if (index == -1)
				break;

			yield return index;
		}
	}

	// Returns a 64 character hash of the string
	// If length becomes an issue, can switch from base16 (hex) to base32 to save 12 characters
	public static string HashSha256(this string rawData)
	{
		byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData)); // 32 bytes

		var builder = new StringBuilder();
		foreach (byte b in bytes)
		{
			builder.Append(b.ToString("x2"));
		}
		return builder.ToString();
	}

	public static bool IsNullOrEmpty([NotNullWhen(false)] this string? text)
	{
		return string.IsNullOrEmpty(text);
	}
}
