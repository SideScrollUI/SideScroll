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
		ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

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
		ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

		for (int index = 0; ; index += value.Length)
		{
			index = str.IndexOf(value, index);
			if (index == -1)
				break;

			yield return index;
		}
	}

	// Returns a 64 character hash of the string
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

	// Returns a 52-character Base32 SHA256 hash (lossless)
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

	public static bool IsNullOrEmpty([NotNullWhen(false)] this string? text)
	{
		return string.IsNullOrEmpty(text);
	}
}
