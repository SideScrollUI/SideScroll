using SideScroll.Extensions;
using SideScroll.Tabs.Bookmarks;
using SideScroll.Tabs.Settings;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SideScroll.Tabs;

public enum FilterOperator
{
	And,
	Or
}

public abstract class FilterNode
{
	public abstract bool Matches(List<string> uppercaseValues);
}

public class FilterLeafNode : FilterNode
{
	public string? TextUppercase { get; set; }

	public override bool Matches(List<string> uppercaseValues)
	{
		return uppercaseValues.Any(v => v.Contains(TextUppercase!, StringComparison.Ordinal));
	}
}

public class FilterOperatorNode : FilterNode
{
	public FilterOperator Operator { get; set; }
	public List<FilterNode> Children { get; set; } = [];

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

public class SearchFilter
{
	public Filter? Filter { get; set; }

	public TabBookmark FindMatches(IList list)
	{
		TabModel tabModel = TabModel.Create("", list)!;
		TabBookmark bookmarkNode = tabModel.FindMatches(Filter!, Filter!.Depth);
		return bookmarkNode;
	}

	public bool IsMatch(object obj)
	{
		if (Filter == null || Filter.FilterText.IsNullOrEmpty())
			return true;

		TabModel tabModel = TabModel.Create("Search", obj)!;
		TabBookmark bookmarkNode = tabModel.FindMatches(Filter!, Filter.Depth);
		return bookmarkNode.SelectedObjects.Count > 0;
	}
}

public class Filter
{
	public string FilterText { get; set; }
	public int Depth { get; set; }
	public FilterNode? RootNode { get; set; }

	private static readonly Regex _regex = new(@"^(?<Depth>\+\d+ )?(?<Filters>.+)$", RegexOptions.IgnoreCase);

	public override string ToString() => FilterText;

	// "ABC" | 123
	// +3 "ABC" | 123
	// (foo | bar) & baz
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
		if (nodes.Count > 0)
			return nodes[0];
		
		return null;
	}

	public bool Matches(IList iList)
	{
		Type listType = iList.GetType();
		Type elementType = listType.GetGenericArguments()[0]; // dictionaries?
		List<PropertyInfo> visibleProperties = TabDataSettings.GetVisibleProperties(elementType);
		return Matches(iList, visibleProperties);
	}

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
				List<PropertyInfo> visibleProperties = TabDataSettings.GetVisibleElementProperties(list); // cache me
				foreach (var item in list)
				{
					GetItemSearchText(item, visibleProperties, uppercaseValues);
				}
			}
			else
			{
				List<PropertyInfo> visibleProperties = TabDataSettings.GetVisibleProperties(innerType); // cache me
				if (visibleProperties != null)
				{
					GetItemSearchText(innerValue, visibleProperties, uppercaseValues);
				}
			}
		}
	}
}
