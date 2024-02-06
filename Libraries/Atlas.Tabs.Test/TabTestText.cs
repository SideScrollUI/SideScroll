using Atlas.Core;
using System.Text;

namespace Atlas.Tabs.Test;

public class TabTestTextEditor : ITab
{
	public const string SampleText = "This is some sample text\n\n1\n2\n3\n\nhttps://www.wikipedia.org/";

	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>()
			{
				new("Sample Text", SampleText),
				new("Json", TabTestJson.Json1),

				GetStringItem("1k", 1000),
				GetStringItem("10k", 10000),
				GetStringItem("100k", 100000),
				//GetStringItem("500k", 500000), // Too slow
				//GetStringItem("1m", 1000000),

				GetLinesItem("100", 100),
				GetLinesItem("1k", 1000),
				GetLinesItem("10k", 10000),
				GetLinesItem("100k", 100000),
				GetLinesItem("500k", 500000),
				GetLinesItem("1m", 1000000),
			};
		}

		private static ListItem GetLinesItem(string label, int lines)
		{
			string text = GetLines(lines);

			return new ListItem(label + " Lines", text);
		}

		private static string GetLines(int lines)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < lines; i++)
			{
				sb.Append("Lots of Lines\n");
			}
			return sb.ToString();
		}

		private static ListItem GetStringItem(string label, int length)
		{
			string text = GetString(length);

			return new ListItem(label + " Characters", text);
		}

		private static string GetString(int length)
		{
			var sb = new StringBuilder();
			while (sb.Length < length)
			{
				sb.Append("Long String ");
			}
			return sb.ToString();
		}
	}
}
