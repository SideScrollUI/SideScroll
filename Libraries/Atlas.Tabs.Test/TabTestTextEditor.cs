using Atlas.Core;
using Atlas.Resources;
using System.Text;

namespace Atlas.Tabs.Test;

public class TabTestTextEditor : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("Text", Samples.Text.Plain),
				new("Json", Samples.Text.Json),
				new("Xml", Samples.Text.Xml),

				GetStringItem("1k", 1_000),
				GetStringItem("10k", 10_000),
				GetStringItem("100k", 100_000),
				//GetStringItem("500k", 500_000), // Too slow
				//GetStringItem("1m", 1_000_000),

				GetLinesItem("100", 100),
				GetLinesItem("1k", 1_000),
				GetLinesItem("10k", 10_000),
				GetLinesItem("100k", 100_000),
				GetLinesItem("500k", 500_000),
				GetLinesItem("1m", 1_000_000),
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
