using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SideScroll.Extensions;

public static class StringExtensions
{
	/// <summary>
	/// Determines whether a string contains a specified substring using case-insensitive comparison
	/// </summary>
	public static bool CaseInsensitiveContains(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
	{
		return text.Contains(value, stringComparison);
	}

	/// <summary>
	/// Reverses the characters in a string
	/// </summary>
	public static string Reverse(this string input)
	{
		char[] chars = input.ToCharArray();
		Array.Reverse(chars);
		return new string(chars);
	}

	/// <summary>
	/// Removes all occurrences of the specified prefix from the beginning of a string
	/// </summary>
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

	/// <summary>
	/// Removes the specified postfix from the end of a string if it exists
	/// </summary>
	public static string TrimEnd(this string input, string postfix)
	{
		if (input.EndsWith(postfix))
		{
			return input[..^postfix.Length];
		}

		return input;
	}

	/// <summary>
	/// Returns a substring from the specified start index to end index (inclusive)
	/// </summary>
	public static string Range(this string input, int start, int end)
	{
		end++;
		end = Math.Min(end, input.Length);
		if (end < start)
			return "";

		return input[start..end];
	}

	/// <summary>
	/// Returns a substring from the specified start index to the end of the string
	/// </summary>
	public static string Range(this string input, int start)
	{
		if (input.Length < start)
			return "";

		return input[start..];
	}

	/// <summary>
	/// Adds spaces between words in camelCase, PascalCase, and underscore-separated text (e.g., "wordsNeed_spacesAndABCToo" becomes "Words Need Spaces And ABC Too")
	/// </summary>
	public static string WordSpaced(this string? text)
	{
		return WordSpacer.Format(text);
	}

	/// <summary>
	/// Converts a string to camel case by capitalizing the first letter and lowercasing the rest
	/// </summary>
	public static string CamelCased(this string text)
	{
		string lowerCased = text.ToLower();
		string camelCased = char.ToUpper(lowerCased[0]) + lowerCased[1..];
		return camelCased;
	}

	/// <summary>
	/// Returns all index positions where the match string occurs in the source string using regex
	/// </summary>
	public static IEnumerable<int> GetAllIndexes(this string source, string matchString)
	{
		matchString = Regex.Escape(matchString);
		foreach (Match match in Regex.Matches(source, matchString))
		{
			yield return match.Index;
		}
	}

	/// <summary>
	/// Returns a list of all index positions where the value string occurs in the string
	/// </summary>
	public static List<int> AllIndexesOf(this string str, string value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(value);

		var indexes = new List<int>();
		for (int index = 0; ; index += value.Length)
		{
			index = str.IndexOf(value, index, StringComparison.Ordinal);
			if (index == -1)
				return indexes;

			indexes.Add(index);
		}
	}

	/// <summary>
	/// Yields all index positions where the value string occurs in the string
	/// </summary>
	public static IEnumerable<int> AllIndexesOfYield(this string str, string value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(value);

		for (int index = 0; ; index += value.Length)
		{
			index = str.IndexOf(value, index, StringComparison.Ordinal);
			if (index == -1)
				break;

			yield return index;
		}
	}

	/// <summary>
	/// Computes a SHA256 hash of the string and returns it as a 64-character hexadecimal string
	/// </summary>
	public static string HashSha256ToHex(this string rawData)
	{
		byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));

		// Preallocate for 64 hex characters (32 bytes * 2)
		char[] hex = new char[64];

		int i = 0;
		foreach (byte b in hashBytes)
		{
			hex[i++] = GetHexChar(b >> 4);
			hex[i++] = GetHexChar(b & 0xF);
		}

		return new string(hex);
	}

	private static char GetHexChar(int val)
	{
		return (char)(val < 10 ? '0' + val : 'a' + (val - 10));
	}

	/// <summary>
	/// Computes a SHA256 hash of the string and returns it as a 52-character Base32 string (lossless encoding)
	/// </summary>
	public static string HashSha256ToBase32(this string rawData)
	{
		byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
		return EncodeToBase32(bytes);
	}

	private static string EncodeToBase32(byte[] data)
	{
		const string base32Alphabet = "abcdefghijklmnopqrstuvwxyz234567";
		StringBuilder result = new(52);

		int buffer = data[0];
		int bitsLeft = 8;
		int index = 1;

		while (result.Length < 52)
		{
			if (bitsLeft < 5)
			{
				if (index < data.Length)
				{
					buffer <<= 8;
					buffer |= data[index++] & 0xFF;
					bitsLeft += 8;
				}
				else
				{
					buffer <<= 5 - bitsLeft;
					bitsLeft = 5;
				}
			}

			int val = (buffer >> (bitsLeft - 5)) & 0b11111;
			bitsLeft -= 5;
			result.Append(base32Alphabet[val]);
		}

		return result.ToString();
	}

	/// <summary>
	/// Determines whether a string is null or empty
	/// </summary>
	public static bool IsNullOrEmpty([NotNullWhen(false)] this string? text)
	{
		return string.IsNullOrEmpty(text);
	}
}
