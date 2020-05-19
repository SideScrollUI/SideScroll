using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Atlas.Tabs
{
	public class FilterExpression
	{
		public string textUppercase;
		public bool matchWord = false;

		public bool Matches(string valueUppercase)
		{
			int index = valueUppercase.IndexOf(textUppercase);
			if (index >= 0)
			//if (valueText != null && valueText.CaseInsensitiveContains(filter))
			{
				if (matchWord)
				{
					// require whitespace or start/end
					if (index > 0 && !Char.IsWhiteSpace(valueUppercase[index - 1]))
						return false;
					int nextChar = index + textUppercase.Length;
					if (nextChar < valueUppercase.Length && !Char.IsWhiteSpace(valueUppercase[nextChar]))
						return false;
				}
				return true;
			}
			return false;
		}
	}

	public class Filter
	{
		public string filterText;
		public int depth = 0;
		public List<FilterExpression> filterExpressions = new List<FilterExpression>();
		public bool isAnd = false;

		// "ABC" | 123
		// +3 "ABC" | 123
		public Filter(string filterText)
		{
			this.filterText = filterText;

			string pattern = @"^(?<Depth>\+\d+ )?(?<Filters>.+)$";
			Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

			Match match = regex.Match(filterText);
			if (!match.Success)
			{
				return;
			}

			string depthText = match.Groups["Depth"].Value;
			if (depthText.Length > 0)
				depth = int.Parse(depthText.Substring(1));
			string filters = match.Groups["Filters"].Value;

			filters = filters.ToUpper();
			string[] parts = filters.Split('|', '&'); // use a tree or something better later
			isAnd = filters.Contains('&');
			foreach (string filter in parts)
			{
				string text = filter.Trim();
				if (text.Length == 0)
					continue;
				var filterExpression = new FilterExpression();
				if (text.First() == '"' && text.Last() == '"')
				{
					filterExpression.matchWord = true;
					text = text.Substring(1);
					text = text.Substring(0, text.Length - 1);
				}
				filterExpression.textUppercase = text.ToUpper();
				filterExpressions.Add(filterExpression);
			}
		}

		public bool Matches(object obj, List<PropertyInfo> columnProperties)
		{
			string allValuesUppercase = "";
			foreach (PropertyInfo propertyInfo in columnProperties)
			{
				object value = propertyInfo.GetValue(obj);
				if (value == null)
					continue;
				string valueText = value.ToString();
				if (valueText == null)
					continue;
				allValuesUppercase += valueText.ToUpper();
			}
			if (isAnd)
			{
				foreach (FilterExpression filterExpression in filterExpressions)
				{
					if (!filterExpression.Matches(allValuesUppercase))
						return false;
				}
				return true;
			}
			else
			{
				foreach (FilterExpression filterExpression in filterExpressions)
				{
					if (filterExpression.Matches(allValuesUppercase))
						return true;
				}
				return false;
			}
		}
	}
}
