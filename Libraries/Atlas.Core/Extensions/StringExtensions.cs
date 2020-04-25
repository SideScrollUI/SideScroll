using Atlas.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

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
			return new String(chars);
		}

		public static string Trim(this string input, string prefix)
		{
			return input.Substring(prefix.Length);
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

		public static string WordSpaced(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return "";
			StringBuilder newText = new StringBuilder(text.Length * 2);
			bool upperCase = true;
			char prevChar = ' ';
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (upperCase)
				{
					upperCase = false;
					c = char.ToUpper(c);
				}
				if (c == '_')
				{
					c = ' ';
				}
				else if (c == '|')
				{
					newText.Append(" |");
					c = ' ';
					upperCase = true;
				}
				else if (prevChar != ' ')
				{
					if (char.IsUpper(c) && !char.IsUpper(prevChar))
						newText.Append(' ');
					else if (char.IsNumber(c) && !char.IsNumber(prevChar))
						newText.Append(' ');
					else if (char.IsUpper(prevChar) && char.IsUpper(c) && i + 1 < text.Length && char.IsLower(text[i + 1]))
						newText.Append(' ');
				}
				newText.Append(c);
				prevChar = c;
			}
			return newText.ToString();
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
	}
}
