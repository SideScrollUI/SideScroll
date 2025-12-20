using System.Text;

namespace SideScroll.Extensions;

/// <summary>
/// Converts concatenated text into human-readable spaced words by intelligently parsing and formatting tokens.
/// Handles camelCase, PascalCase, underscores, numbers, dates, decimals, and acronyms.
/// <example>
/// Examples:
/// <code>
/// "SomeMethodName" → "Some Method Name"
/// "wordsNeed_spacesAndABCToo" → "words Need spaces And ABC Too"
/// "GetHTTPResponse" → "Get HTTP Response"
/// "Item123Price" → "Item 123 Price"
/// "2024-12-19" → "2024-12-19" (preserved)
/// </code>
/// </example>
/// </summary>
public class WordSpacer
{
	/// <summary>
	/// Gets or sets the characters that can connect numbers in decimal or time formats
	/// </summary>
	public static HashSet<char> NumberConnectors { get; set; } = ['-', ':', '.', ','];

	/// <summary>
	/// Gets or sets the characters that act as separators between tokens
	/// </summary>
	public static HashSet<char> SeparatorSymbols { get; set; } = ['|', '/', '-'];

	/// <summary>
	/// Gets the original input text
	/// </summary>
	public string Text { get; }

	/// <summary>
	/// Gets the formatted output text with spaces added between words
	/// </summary>
	public string Formatted { get; protected set; } = "";

	/// <summary>
	/// Gets the list of parsed tokens from the input text
	/// </summary>
	public List<Token> Tokens { get; } = [];

	/// <summary>
	/// Initializes a new instance of the WordSpacer class and formats the specified text
	/// </summary>
	public WordSpacer(string? text)
	{
		Text = text ?? "";

		Tokenize();
		Format();
	}

	/// <summary>
	/// Formats the specified text by adding spaces between words
	/// </summary>
	/// <example>
	/// <code>
	/// WordSpacer.Format("SomeMethodName") → "Some Method Name"
	/// WordSpacer.Format("GetHTTPResponse") → "Get HTTP Response"
	/// </code>
	/// </example>
	public static string Format(string? text) => new WordSpacer(text).Formatted;

	private void Format()
	{
		List<Token> combined1 = MergeTimestamps(Tokens);
		List<Token> combined2 = MergeSingleUpperWithDecimal(combined1);
		List<Token> combined3 = MergeNumberAndLower(combined2);

		Formatted = string.Join(' ', combined3.Where(t => t.TokenType != TokenType.Space));
	}

	private static List<Token> MergeTimestamps(List<Token> tokens)
	{
		List<Token> combined = [];
		for (int i = 0; i < tokens.Count; i++)
		{
			Token token = tokens[i];

			// Single counts can be fractions or ratios
			if (token.TokenType == TokenType.Decimal &&
				(token.Value.Count(c => c == '/') == 2 || token.Value.Count(c => c == ':') == 2))
			{
				token.TokenType = TokenType.DateTime;
			}

			// Manually merge: Look for 3 pair sets with a separator between each
			if (token.TokenType == TokenType.Number && i + 4 < tokens.Count &&
				tokens[i + 1].TokenType == TokenType.Separator &&
				tokens[i + 2].TokenType == TokenType.Number &&
				tokens[i + 3].TokenType == TokenType.Separator &&
				tokens[i + 4].IsNumeric)
			{
				for (; i + 1 < tokens.Count; i++)
				{
					var nextToken = tokens[i + 1];

					if (!nextToken.IsNumeric &&
						nextToken.TokenType != TokenType.Separator)
						break;

					token.Value += nextToken.Value;
				}
				token.TokenType = TokenType.DateTime;
			}

			combined.Add(token);
		}

		return combined;
	}

	private static List<Token> MergeSingleUpperWithDecimal(List<Token> tokens)
	{
		List<Token> combined = [];
		for (int i = 0; i < tokens.Count; i++)
		{
			Token token = tokens[i];
			if (token.TokenType == TokenType.UpperString && token.Value.Length == 1 &&
				i + 1 < tokens.Count && tokens[i + 1].IsNumeric)
			{
				token.Value += tokens[i + 1].Value;
				i++;
			}

			combined.Add(token);
		}

		return combined;
	}

	private static List<Token> MergeNumberAndLower(List<Token> tokens)
	{
		List<Token> combined = [];
		for (int i = 0; i < tokens.Count; i++)
		{
			Token token = tokens[i];
			if (token.IsNumberLowerCompatible)
			{
				for (; i + 1 < tokens.Count; i++)
				{
					Token nextToken = tokens[i + 1];
					if (!nextToken.IsNumberLowerCompatible)
						break;

					token.Value += nextToken.Value;
					token.TokenType = TokenType.NumberLower;
				}
			}

			combined.Add(token);
		}

		return combined;
	}

	private void Tokenize()
	{
		StringBuilder stringBuilder = new();
		Token? token = null;
		for (int i = Text.Length - 1; i >= 0; i--)
		{
			char c = Text[i];
			if (c == '_')
			{
				c = ' ';
			}

			if (token?.TryAdd(c) != true)
			{
				TokenType type = Token.GetType(c);
				if (i > 1 && type == Token.GetType(Text[i - 1]))
				{
					// Group identical types together to speed things up and reduce memory allocations
					stringBuilder.Clear();
					stringBuilder.Append(c);
					for (; i > 0; i--)
					{
						char nextChar = Text[i - 1];
						if (type != Token.GetType(nextChar))
							break;

						stringBuilder.Append(nextChar);
					}
					token = new(stringBuilder.ToString().Reverse());
				}
				else
				{
					token = new(c);
				}
				Tokens.Add(token);
			}
		}

		Tokens.Reverse();
	}

	/// <summary>
	/// Defines the type of token parsed from the input text
	/// </summary>
	public enum TokenType
	{
		/// <summary>
		/// Default or unknown token type
		/// </summary>
		Default,

		/// <summary>
		/// All lowercase letters (e.g., "word", "spaces")
		/// </summary>
		LowerString,

		/// <summary>
		/// All uppercase letters, including plurals ending with 's' (e.g., "HTTP", "ABC", "XMLs")
		/// </summary>
		UpperString, // Plurals also count if string ends with 's'

		/// <summary>
		/// First letter uppercase, rest lowercase (e.g., "Some", "Method", "Name")
		/// </summary>
		Capitalized, // First letter is uppercase and the rest are lowercase

		/// <summary>
		/// Numeric digits (e.g., "123", "456")
		/// </summary>
		Number,

		/// <summary>
		/// Numbers mixed with lowercase letters (e.g., "item123", "v2a")
		/// </summary>
		NumberLower,

		/// <summary>
		/// Numbers with decimal or connector characters (e.g., "3.14", "1,000")
		/// </summary>
		Decimal,

		/// <summary>
		/// Date or time format (e.g., "2024-12-19", "10:30:45", "12/31/2024")
		/// </summary>
		DateTime,

		/// <summary>
		/// Separator character (e.g., "|", "/", "-")
		/// </summary>
		Separator,

		/// <summary>
		/// Whitespace
		/// </summary>
		Space,
	}

	/// <summary>
	/// Represents a parsed token with its type and value
	/// </summary>
	public class Token 
	{
		/// <summary>
		/// Gets or sets the type of this token
		/// </summary>
		public TokenType TokenType { get; set; }

		/// <summary>
		/// Gets or sets the string value of this token
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Initializes a new instance of the Token class with a single character
		/// </summary>
		public Token(char c)
		{
			TokenType = GetType(c);
			Value = c.ToString();
		}

		/// <summary>
		/// Initializes a new instance of the Token class with a string value
		/// </summary>
		public Token(string value)
		{
			TokenType = GetType(value.First());
			Value = value;
		}

		public override string ToString() => Value;

		/// <summary>
		/// Gets whether this token is a numeric type (Number or Decimal)
		/// </summary>
		public bool IsNumeric => TokenType == TokenType.Decimal || TokenType == TokenType.Number;

		/// <summary>
		/// Gets whether this token can be merged with numbers and lowercase letters
		/// </summary>
		public bool IsNumberLowerCompatible => TokenType == TokenType.NumberLower || TokenType == TokenType.Number || TokenType == TokenType.LowerString;

		/// <summary>
		/// Determines the token type for a character
		/// </summary>
		public static TokenType GetType(char c) => c switch
		{
			_ when char.IsDigit(c) => TokenType.Number,
			_ when char.IsUpper(c) => TokenType.UpperString,
			_ when char.IsLower(c) => TokenType.LowerString,
			' ' => TokenType.Space,
			_ when SeparatorSymbols.Contains(c) || NumberConnectors.Contains(c) => TokenType.Separator,
			_ => TokenType.Default
		};

		/// <summary>
		/// Attempts to add a character to this token
		/// </summary>
		public bool TryAdd(char c)
		{
			if (c == ' ')
			{
				return Value == " "; // Ignore duplicates
			}

			if (!CanMergeWith(c)) return false;

			Value = c + Value;

			return true;
		}

		private bool CanMergeWith(char c)
		{
			char firstChar = Value.First();
			if (char.IsDigit(c))
			{
				if (IsNumeric || TokenType == TokenType.NumberLower) return true;

				if (TokenType == TokenType.LowerString)
				{
					TokenType = TokenType.NumberLower;
					return true;
				}

				if (TokenType != TokenType.UpperString) return false;

				if (Value.Length >= 2 && Value[0] == Value[1])
				{
					// Allow 4XX and 4XXs
				}
				else if (Value.Length > 1 && Value.Last() != 's')
				{
					// Allow adding digits to all caps that isn't plural
				}
				else
				{
					return false;
				}
			}
			else if (char.IsUpper(c))
			{
				if (TokenType == TokenType.UpperString) return true;
				
				if (!char.IsLower(firstChar)) return false;

				TokenType = TokenType.Capitalized;

				// Plural
				if (firstChar == 's' && Value.Length == 1)
				{
					TokenType = TokenType.UpperString;
				}
			}
			else if (char.IsLower(c))
			{
				if (TokenType != TokenType.LowerString)
				{
					return TokenType == TokenType.NumberLower;
				}

				// Only allow one lowercase if word starts with a capital letter
				if (!char.IsLower(firstChar)) return false;
			}
			else if (NumberConnectors.Contains(c))
			{
				if (TokenType == TokenType.Decimal) return true;

				if (TokenType == TokenType.Number)
				{
					TokenType = TokenType.Decimal;
					return true;
				}
				return false;
			}
			else
			{
				return false;
			}
			return true;
		}
	}
}
