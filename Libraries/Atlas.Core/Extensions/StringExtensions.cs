using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Atlas.Extensions
{
	public static class StringExtensions
	{
		public static bool CaseInsensitiveContains(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
		{
			return text.IndexOf(value, stringComparison) >= 0;
		}

		public static string Reverse(this string input)
		{
			char[] chars = input.ToCharArray();
			Array.Reverse(chars);
			return new string(chars);
		}

		public static string TrimStart(this string input, string prefix)
		{
			if (input == null || prefix == null || prefix.Length == 0)
				return input;

			while (input.StartsWith(prefix))
				input = input.Substring(prefix.Length);

			return input;
		}

		public static string TrimEnd(this string input, string postfix)
		{
			if (input.EndsWith(postfix))
				return input.Substring(0, input.Length - postfix.Length);

			return input;
		}

		public static string Range(this string input, int start, int end)
		{
			end++;
			end = Math.Min(end, input.Length);
			if (end < start)
				return "";

			return input.Substring(start, end - start);
		}

		public static string Range(this string input, int start)
		{
			if (input.Length < start)
				return "";

			return input.Substring(start, input.Length - start);
		}

		private static HashSet<char> _wordSpacedSymbols = new HashSet<char>() { '|', '/', '-' };

		// Adds spaces between words
		// 'wordsNeed_spacesAndWNSToo' -> 'Words Need Spaces And WNS Too'
		public static string WordSpaced(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return "";

			var newText = new StringBuilder(text.Length * 2);
			bool upperCaseNext = true;
			char prevChar = ' ';
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				char nextChar = (i + 1) < text.Length ? text[i + 1] : ' ';
				if (upperCaseNext)
				{
					upperCaseNext = false;
					c = char.ToUpper(c);
				}

				if (c == '_')
				{
					c = ' ';
				}
				else if (_wordSpacedSymbols.Contains(c))
				{
					newText.Append(' ');
					newText.Append(c);
					c = ' ';
					upperCaseNext = true;
				}
				else if (prevChar != ' ')
				{
					if (char.IsUpper(c) && char.IsDigit(prevChar) && nextChar == c) // Don't split 5XX apart
					{
					}
					else if (char.IsUpper(c) && !char.IsUpper(prevChar)) // Add space between CamelCase
					{
						//if (nextChar
						newText.Append(' ');
					}
					else if (char.IsNumber(c) && !char.IsNumber(prevChar) && prevChar != '.' && newText.Length > 1) // Add space before 1st Number, Number10
					{
						newText.Append(' ');
					}
					else if (char.IsUpper(prevChar) && char.IsUpper(c) && char.IsLower(nextChar))
					{
						if (nextChar != 's')
							newText.Append(' '); // Add a space before first capital after caps string, assume CamelCase, CAPSName
					}
				}
				newText.Append(c);
				prevChar = c;
			}
			return newText.ToString();
		}

		public static string CamelCased(this string text)
		{
			string lowerCased = text.ToLower();
			string camelCased = char.ToUpper(lowerCased[0]) + lowerCased.Substring(1);
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
				throw new ArgumentException("the string to find may not be empty", "value");

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
				throw new ArgumentException("the string to find may not be empty", "value");

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
			if (rawData == null)
				return null;

			using SHA256 sha256Hash = SHA256.Create();
			
			byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData)); // 32 bytes

			var builder = new StringBuilder();
			foreach (byte b in bytes)
			{
				builder.Append(b.ToString("x2"));
			}
			return builder.ToString();
		}

		public static bool IsNullOrEmpty(this string text)
		{
			return string.IsNullOrEmpty(text);
		}
	}
}
