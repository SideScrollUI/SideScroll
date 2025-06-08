using System.Text;

namespace SideScroll.Extensions;

public class WordSpacer
{
	public static HashSet<char> NumberConnectors { get; set; } = ['-', ':', '.', ','];
	public static HashSet<char> SeparatorSymbols { get; set; } = ['|', '/', '-'];

	public string Text { get; init; }
	public string Formatted { get; private set; } = "";

	public List<Token> Tokens { get; set; } = [];

	public WordSpacer(string? text)
	{
		Text = text ?? "";

		Tokenize();
		Format();
	}

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
			if (token.TokenType == TokenType.Number || token.TokenType == TokenType.LowerString)
			{
				for (; i + 1 < tokens.Count; i++)
				{
					Token nextToken = tokens[i + 1];
					if (nextToken.TokenType != TokenType.Number && nextToken.TokenType != TokenType.LowerString)
						break;

					token.Value += nextToken.Value;
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

	public enum TokenType
	{
		Default,
		LowerString,
		UpperString, // Plurals also count if string ends with 's'
		Capitalized, // First letter is uppercase and the rest are lowercase
		Number,
		NumberLower,
		Decimal,
		DateTime,
		Separator,
		Space,
	}

	public class Token 
	{
		public TokenType TokenType { get; set; }
		public string Value { get; set; }

		public Token(char c)
		{
			TokenType = GetType(c);
			Value = c.ToString();
		}

		public Token(string value)
		{
			TokenType = GetType(value.First());
			Value = value.ToString();
		}

		public override string ToString() => Value;

		public bool IsNumeric => TokenType == TokenType.Decimal || TokenType == TokenType.Number;

		public static TokenType GetType(char c) => c switch
		{
			_ when char.IsDigit(c) => TokenType.Number,
			_ when char.IsUpper(c) => TokenType.UpperString,
			_ when char.IsLower(c) => TokenType.LowerString,
			' ' => TokenType.Space,
			_ when SeparatorSymbols.Contains(c) || NumberConnectors.Contains(c) => TokenType.Separator,
			_ => TokenType.Default
		};

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
					if (TokenType == TokenType.NumberLower) return true;

					if (TokenType == TokenType.Number)
					{
						TokenType = TokenType.NumberLower;
						return true;
					}

					return false;
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
