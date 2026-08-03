using SideScroll.Extensions;
using SideScroll.Tabs.Bookmarks.Models;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SideScroll.Tabs;

/// <summary>
/// Logical operator types for combining filter expressions
/// </summary>
public enum FilterOperator
{
	/// <summary>
	/// All conditions must match
	/// </summary>
	And,

	/// <summary>
	/// Any condition can match
	/// </summary>
	Or
}

/// <summary>
/// Base class for filter expression tree nodes
/// </summary>
public abstract class FilterNode
{
	/// <summary>
	/// Determines whether this node matches the provided values
	/// </summary>
	/// <param name="uppercaseValues">List of uppercase text values to match against</param>
	public abstract bool Matches(List<string> uppercaseValues);
}

/// <summary>
/// Leaf node representing a single search term in the filter expression tree
/// </summary>
public class FilterLeafNode : FilterNode
{
	/// <summary>
	/// Gets or sets the uppercase search text to match
	/// </summary>
	public string? TextUppercase { get; set; }

	/// <summary>
	/// Checks if any value contains the search text
	/// </summary>
	public override bool Matches(List<string> uppercaseValues)
	{
		return uppercaseValues.Any(v => v.Contains(TextUppercase!, StringComparison.Ordinal));
	}
}

/// <summary>
/// Negation node that inverts the match result of its child node
/// </summary>
public class FilterNotNode : FilterNode
{
	/// <summary>
	/// Gets or sets the child filter node to negate
	/// </summary>
	public FilterNode? Child { get; set; }

	/// <summary>
	/// Matches when the child node does not match
	/// </summary>
	public override bool Matches(List<string> uppercaseValues)
	{
		return Child == null || !Child.Matches(uppercaseValues);
	}
}

/// <summary>
/// Operator node combining multiple filter nodes with AND or OR logic
/// </summary>
public class FilterOperatorNode : FilterNode
{
	/// <summary>
	/// Gets or sets the logical operator (AND or OR)
	/// </summary>
	public FilterOperator Operator { get; set; }

	/// <summary>
	/// Gets or sets the child filter nodes
	/// </summary>
	public List<FilterNode> Children { get; set; } = [];

	/// <summary>
	/// Evaluates all children using the specified operator logic
	/// </summary>
	public override bool Matches(List<string> uppercaseValues)
	{
		if (Children.Count == 0)
			return true;

		if (Operator == FilterOperator.And)
		{
			return Children.All(child => child.Matches(uppercaseValues));
		}
		else // OR
		{
			return Children.Any(child => child.Matches(uppercaseValues));
		}
	}
}

/// <summary>
/// Helper class for applying filters to tab data and finding matches
/// </summary>
public class SearchFilter
{
	/// <summary>
	/// Gets or sets the filter to apply
	/// </summary>
	public Filter? Filter { get; set; }

	/// <summary>
	/// Finds all matching items in a list using the filter
	/// </summary>
	public TabBookmark FindMatches(IList list)
	{
		// Create() returns null when there's nothing to show for the list
		if (Filter == null || TabModel.Create("", list) is not { } tabModel)
			return new TabBookmark();

		return tabModel.FindMatches(Filter, Filter.Depth);
	}

	/// <summary>
	/// Determines whether an object matches the filter criteria
	/// </summary>
	public bool IsMatch(object obj)
	{
		if (Filter == null || Filter.FilterText.IsNullOrEmpty())
			return true;

		// Scalars (DateTime, decimal, Guid) don't produce a model, match on their own text instead
		if (TabModel.Create("Search", obj) is not { } tabModel)
			return Filter.Matches(obj, []);

		TabBookmark tabBookmark = tabModel.FindMatches(Filter, Filter.Depth);
		return tabBookmark.SelectedRows.Count > 0;
	}
}

/// <summary>
/// Parses and evaluates text search expressions with support for AND/OR/NOT operators, quoted strings, and nested depth.
/// Syntax examples: "ABC" | 123, +3 "ABC" | 123, (foo | bar) &amp; baz, -foo, !(foo | bar)
/// Quote a term to search for a literal leading - or ! (e.g. "-5")
/// </summary>
public class Filter
{
	/// <summary>
	/// Gets or sets the original filter text
	/// </summary>
	public string FilterText { get; set; }

	/// <summary>
	/// Maximum search depth a `+N` prefix can request (default: 32)
	/// </summary>
	/// <remarks>
	/// This is what bounds the recursion in <see cref="TabModel.FindMatches"/>, which has no cycle
	/// detection of its own. A searchable object graph that references itself would otherwise
	/// recurse until the stack overflowed, which can't be caught
	/// </remarks>
	public static int MaxDepth { get; set; } = 32;

	/// <summary>
	/// Gets or sets the search depth for nested objects (0 = current level only)
	/// </summary>
	public int Depth { get; set; }

	/// <summary>
	/// Gets or sets the root node of the parsed expression tree
	/// </summary>
	public FilterNode? RootNode { get; set; }

	private static readonly Regex _regex = new(@"^(?<Depth>\+\d+ )?(?<Filters>.+)$", RegexOptions.IgnoreCase);

	/// <summary>Returns the filter's <see cref="FilterText"/>.</summary>
	public override string ToString() => FilterText;

	/// <summary>
	/// Initializes a new filter by parsing the filter text expression.
	/// Supports depth prefix (+N), quoted strings, AND (&amp;), OR (|), and parentheses for grouping.
	/// </summary>
	/// <param name="filterText">The filter expression to parse (e.g., "+3 foo &amp; bar | baz")</param>
	/// <param name="depth">The default search depth, used when the filter text has no depth prefix (+N)</param>
	public Filter(string? filterText, int depth = 0)
	{
		FilterText = filterText ?? "";
		Depth = depth;

		Match match = _regex.Match(FilterText);
		if (!match.Success)
			return;

		// TryParse, the digits are unbounded and this runs for every keystroke in the search box
		string depthText = match.Groups["Depth"].Value;
		if (depthText.Length > 0 && int.TryParse(depthText[1..], out int parsedDepth))
		{
			Depth = Math.Min(parsedDepth, MaxDepth);
		}

		string filters = match.Groups["Filters"].Value;

		// Parse into tree structure
		RootNode = ParseExpression(filters, 0, out _);
	}

	private static FilterNode? ParseExpression(string input, int startIndex, out int endIndex)
	{
		List<FilterNode> nodes = [];
		List<FilterOperator> operators = [];

		int i = startIndex;
		bool insideQuotes = false;
		int tokenStart = i;

		while (i < input.Length)
		{
			char c = input[i];

			if (c == '"')
			{
				insideQuotes = !insideQuotes;
				i++;
			}
			else if (!insideQuotes && c == '(')
			{
				// A pending - or ! token negates the subexpression: -(foo | bar)
				string prefix = input[tokenStart..i].Trim();
				bool negate = prefix is "-" or "!";

				if (!negate && prefix.Length > 0)
				{
					AddToken(input, tokenStart, i, nodes);
					operators.Add(FilterOperator.And);
				}

				// Parse subexpression
				var subNode = ParseExpression(input, i + 1, out int closeParen);
				if (subNode != null)
				{
					nodes.Add(negate ? new FilterNotNode { Child = subNode } : subNode);
				}
				i = closeParen + 1;
				tokenStart = i;
			}
			else if (!insideQuotes && c == ')')
			{
				// End of subexpression
				AddToken(input, tokenStart, i, nodes);
				endIndex = i;
				return BuildTree(nodes, operators);
			}
			else if (!insideQuotes && (c == '&' || c == '|'))
			{
				// Add token before operator
				AddToken(input, tokenStart, i, nodes);

				// Add operator
				operators.Add(c == '&' ? FilterOperator.And : FilterOperator.Or);

				i++;
				tokenStart = i;
			}
			else if (!insideQuotes && c == ' ')
			{
				// Space is implicit AND - but only if not followed by an explicit operator
				if (i > tokenStart && !string.IsNullOrWhiteSpace(input[tokenStart..i]))
				{
					// Look ahead to see if next non-space character is an operator
					int lookAhead = i + 1;
					while (lookAhead < input.Length && input[lookAhead] == ' ')
					{
						lookAhead++;
					}

					bool nextIsOperator = lookAhead < input.Length && (input[lookAhead] == '&' || input[lookAhead] == '|');

					if (!nextIsOperator)
					{
						AddToken(input, tokenStart, i, nodes);
						operators.Add(FilterOperator.And);
					}
					else
					{
						// Just add the token, operator will be added when we encounter it
						AddToken(input, tokenStart, i, nodes);
					}
				}
				i++;
				tokenStart = i;
			}
			else
			{
				i++;
			}
		}

		// Add final token
		AddToken(input, tokenStart, i, nodes);

		endIndex = i;
		return BuildTree(nodes, operators);
	}

	private static void AddToken(string input, int start, int end, List<FilterNode> nodes)
	{
		if (start >= end)
			return;

		string token = input[start..end].Trim();
		if (string.IsNullOrWhiteSpace(token))
			return;

		// A leading - or ! negates the token; quote the term to match these characters literally
		bool negate = false;
		if (token[0] == '-' || token[0] == '!')
		{
			negate = true;
			token = token[1..].Trim();
		}

		// Remove quotes if present
		if (token.Length >= 2 && token.First() == '"' && token.Last() == '"')
		{
			// Both opening and closing quotes present
			token = token[1..^1];
		}
		else if (token.Length >= 1 && token.First() == '"')
		{
			// Only opening quote present - auto-close it by removing the opening quote
			token = token[1..];
		}

		if (!string.IsNullOrWhiteSpace(token))
		{
			// Invariant, the values are uppercased the same way and compared ordinally. Culture
			// casing would make 'i' uppercase to 'İ' in tr-TR and stop matching 'I'
			FilterNode node = new FilterLeafNode { TextUppercase = token.ToUpperInvariant() };
			if (negate)
			{
				node = new FilterNotNode { Child = node };
			}
			nodes.Add(node);
		}
	}

	private static FilterNode? BuildTree(List<FilterNode> nodes, List<FilterOperator> operators)
	{
		if (nodes.Count == 0)
			return null;

		if (nodes.Count == 1)
			return nodes[0];

		// Ensure we have the right number of operators (should be nodes.Count - 1)
		// If mismatch, just return the nodes we have
		if (operators.Count != nodes.Count - 1)
		{
			// Try to recover by returning what we have
			if (nodes.Count == 1)
				return nodes[0];

			// Create an AND node with all nodes as a fallback
			return new FilterOperatorNode
			{
				Operator = FilterOperator.And,
				Children = [.. nodes]
			};
		}

		// Build tree respecting operator precedence (AND has higher precedence than OR)
		// First, handle all AND operations
		for (int i = 0; i < operators.Count; i++)
		{
			if (operators[i] == FilterOperator.And)
			{
				// Safety check
				if (i + 1 >= nodes.Count)
					break;

				// Merge nodes[i] and nodes[i+1] into an AND node
				FilterOperatorNode andNode = new()
				{
					Operator = FilterOperator.And,
					Children = [nodes[i], nodes[i + 1]]
				};

				nodes.RemoveAt(i + 1);
				nodes[i] = andNode;
				operators.RemoveAt(i);
				i--;
			}
		}

		// Then handle all OR operations
		if (operators.Any(op => op == FilterOperator.Or))
		{
			FilterOperatorNode orNode = new()
			{
				Operator = FilterOperator.Or,
				Children = [.. nodes]
			};
			return orNode;
		}

		// If only one node remains after AND operations
		return nodes.FirstOrDefault();
	}

	/// <summary>
	/// Determines whether any items in the list match the filter
	/// </summary>
	public bool Matches(IList list)
	{
		// Resolves arrays and non-generic list subclasses, unlike GetGenericArguments()
		Type? elementType = list.GetType().GetElementTypeForAll();
		List<PropertyInfo> visibleProperties = elementType != null
			? TabDataColumns.GetVisibleProperties(elementType)
			: []; // Untyped list, the items still match on their own text

		foreach (object? item in list)
		{
			if (item == null) continue;

			if (Matches(item, visibleProperties))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Determines whether an object matches the filter using the specified properties
	/// </summary>
	/// <param name="obj">The object to check</param>
	/// <param name="columnProperties">The properties to extract text values from</param>
	public bool Matches(object obj, List<PropertyInfo> columnProperties)
	{
		List<string> uppercaseValues = [];
		if (obj is DataRowView dataRowView)
		{
			foreach (var item in dataRowView.Row.ItemArray)
			{
				string? valueText = item?.ToString();
				if (valueText.IsNullOrEmpty())
					continue;

				uppercaseValues.Add(valueText.ToUpperInvariant());
			}
		}
		else
		{
			GetItemSearchText(obj, columnProperties, uppercaseValues);
		}

		// Use tree structure
		if (RootNode != null)
		{
			return RootNode.Matches(uppercaseValues);
		}

		return true;
	}

	// Inner values can reference each other, limit the nesting instead of overflowing the stack
	private const int MaxSearchTextDepth = 4;

	/// <summary>
	/// Maximum number of text values collected from a single item when searching (default: 1,000).
	/// Inner lists are nested and enumerated in full, and this runs for every row on every
	/// keystroke, so the total is capped instead of the items per list
	/// </summary>
	public static int MaxSearchTextValues
	{
		get => _maxSearchTextValues;
		// A cap below one collects nothing, not even the row's own label, so every search would
		// report no matches at all
		set => _maxSearchTextValues = Math.Max(1, value);
	}
	private static int _maxSearchTextValues = 1_000;

	private static void GetItemSearchText(object obj, List<PropertyInfo> columnProperties, List<string> uppercaseValues, int depth = MaxSearchTextDepth)
	{
		if (uppercaseValues.Count >= MaxSearchTextValues)
			return;

		if (obj.ToString()?.ToUpperInvariant() is { } objText)
		{
			uppercaseValues.Add(objText);
		}

		foreach (PropertyInfo propertyInfo in columnProperties)
		{
			try
			{
				object? value = propertyInfo.GetValue(obj);

				string? valueText = value?.ToString();
				if (valueText.IsNullOrEmpty())
					continue;

				uppercaseValues.Add(valueText.ToUpperInvariant());
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
			}
		}

		object? innerValue = obj.GetInnerValue();
		if (innerValue != null && innerValue != obj)
		{
			if (innerValue is IList list)
			{
				if (depth <= 0) return;

				List<PropertyInfo> visibleProperties = TabDataColumns.GetVisibleElementProperties(list);
				foreach (var item in list)
				{
					if (item == null) continue;

					// Stop instead of walking the rest of a large list for nothing
					if (uppercaseValues.Count >= MaxSearchTextValues)
						break;

					GetItemSearchText(item, visibleProperties, uppercaseValues, depth - 1);
				}
			}
			else
			{
				// Only add the inner value's own ToString() — recursing into its sub-properties
				// would expose grandchild text at the wrong search depth level.
				if (innerValue.ToString()?.ToUpperInvariant() is { } innerText)
				{
					uppercaseValues.Add(innerText);
				}
			}
		}
	}
}
