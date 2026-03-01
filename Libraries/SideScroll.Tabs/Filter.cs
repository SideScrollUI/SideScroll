using SideScroll.Extensions;
using SideScroll.Tabs.Bookmarks.Models;
using System.Collections;
using System.Data;
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
		TabModel tabModel = TabModel.Create("", list)!;
		TabBookmark tabBookmark = tabModel.FindMatches(Filter!, Filter!.Depth);
		return tabBookmark;
	}

	/// <summary>
	/// Determines whether an object matches the filter criteria
	/// </summary>
	public bool IsMatch(object obj)
	{
		if (Filter == null || Filter.FilterText.IsNullOrEmpty())
			return true;

		TabModel tabModel = TabModel.Create("Search", obj)!;
		TabBookmark tabBookmark = tabModel.FindMatches(Filter!, Filter.Depth);
		return tabBookmark.SelectedRows.Count > 0;
	}
}

/// <summary>
/// Parses and evaluates text search expressions with support for AND/OR operators, quoted strings, and nested depth.
/// Syntax examples: "ABC" | 123, +3 "ABC" | 123, (foo | bar) &amp; baz
/// </summary>
public class Filter
{
	/// <summary>
	/// Gets or sets the original filter text
	/// </summary>
	public string FilterText { get; set; }

	/// <summary>
	/// Gets or sets the search depth for nested objects (0 = current level only)
	/// </summary>
	public int Depth { get; set; }

	/// <summary>
	/// Gets or sets the root node of the parsed expression tree
	/// </summary>
	public FilterNode? RootNode { get; set; }

	private static readonly Regex _regex = new(@"^(?<Depth>\+\d+ )?(?<Filters>.+)$", RegexOptions.IgnoreCase);

	public override string ToString() => FilterText;

	/// <summary>
	/// Initializes a new filter by parsing the filter text expression.
	/// Supports depth prefix (+N), quoted strings, AND (&amp;), OR (|), and parentheses for grouping.
	/// </summary>
	/// <param name="filterText">The filter expression to parse (e.g., "+3 foo &amp; bar | baz")</param>
	public Filter(string? filterText)
	{
		FilterText = filterText ?? "";

		Match match = _regex.Match(FilterText);
		if (!match.Success)
			return;

		string depthText = match.Groups["Depth"].Value;
		if (depthText.Length > 0)
		{
			Depth = int.Parse(depthText[1..]);
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
				// Parse subexpression
				var subNode = ParseExpression(input, i + 1, out int closeParen);
				if (subNode != null)
				{
					nodes.Add(subNode);
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
			nodes.Add(new FilterLeafNode { TextUppercase = token.ToUpper() });
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
	public bool Matches(IList iList)
	{
		Type listType = iList.GetType();
		Type elementType = listType.GetGenericArguments()[0]; // dictionaries?
		List<PropertyInfo> visibleProperties = TabDataColumns.GetVisibleProperties(elementType);
		return Matches(iList, visibleProperties);
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

				uppercaseValues.Add(valueText.ToUpper());
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

	private static void GetItemSearchText(object obj, List<PropertyInfo> columnProperties, List<string> uppercaseValues)
	{
		foreach (PropertyInfo propertyInfo in columnProperties)
		{
			object? value = propertyInfo.GetValue(obj);

			string? valueText = value?.ToString();
			if (valueText.IsNullOrEmpty())
				continue;

			uppercaseValues.Add(valueText.ToUpper());
		}

		object? innerValue = obj.GetInnerValue();
		if (innerValue != null && innerValue != obj)
		{
			Type innerType = innerValue.GetType();
			if (innerValue is IList list)
			{
				List<PropertyInfo> visibleProperties = TabDataColumns.GetVisibleElementProperties(list);
				foreach (var item in list)
				{
					GetItemSearchText(item, visibleProperties, uppercaseValues);
				}
			}
			else
			{
				List<PropertyInfo> visibleProperties = TabDataColumns.GetVisibleProperties(innerType);
				GetItemSearchText(innerValue, visibleProperties, uppercaseValues);
			}
		}
	}
}
