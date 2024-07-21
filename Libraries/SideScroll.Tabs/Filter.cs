using SideScroll.Extensions;
using SideScroll.Tabs.Bookmarks;
using SideScroll.Tabs.Settings;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SideScroll.Tabs;

public class FilterExpression
{
	public string? TextUppercase { get; set; }
	public bool MatchWord { get; set; }

	public bool Matches(string valueUppercase)
	{
		int index = valueUppercase.IndexOf(TextUppercase!);
		if (index >= 0)
		//if (valueText != null && valueText.CaseInsensitiveContains(filter))
		{
			if (MatchWord)
			{
				// require whitespace or start/end
				if (index > 0 && !char.IsWhiteSpace(valueUppercase[index - 1]))
					return false;

				int nextChar = index + TextUppercase!.Length;
				if (nextChar < valueUppercase.Length && !char.IsWhiteSpace(valueUppercase[nextChar]))
					return false;
			}
			return true;
		}
		return false;
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
	public List<FilterExpression> FilterExpressions { get; set; } = [];
	public bool IsAnd { get; set; }

	public override string ToString() => FilterText;

	// "ABC" | 123
	// +3 "ABC" | 123
	public Filter(string? filterText)
	{
		FilterText = filterText ?? "";

		string pattern = @"^(?<Depth>\+\d+ )?(?<Filters>.+)$";
		Regex regex = new(pattern, RegexOptions.IgnoreCase);

		Match match = regex.Match(FilterText);
		if (!match.Success)
			return;

		string depthText = match.Groups["Depth"].Value;
		if (depthText.Length > 0)
			Depth = int.Parse(depthText[1..]);

		string filters = match.Groups["Filters"].Value;
		filters = filters.ToUpper();
		string[] parts = filters.Split('|', '&'); // use a tree or something better later
		IsAnd = filters.Contains('&');
		foreach (string filter in parts)
		{
			string text = filter.Trim();
			if (text.Length == 0)
				continue;

			var filterExpression = new FilterExpression();
			if (text.First() == '"' && text.Last() == '"')
			{
				filterExpression.MatchWord = true;
				text = text[1..^1];
			}
			filterExpression.TextUppercase = text.ToUpper();
			FilterExpressions.Add(filterExpression);
		}
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
		List<string> uppercaseValues = new();
		GetItemSearchText(obj, columnProperties, uppercaseValues);
		if (IsAnd)
		{
			return FilterExpressions.All(f => uppercaseValues.Any(v => f.Matches(v)));
		}
		else
		{
			return FilterExpressions.Any(f => uppercaseValues.Any(v => f.Matches(v)));
		}
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
