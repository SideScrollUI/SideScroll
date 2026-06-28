using SideScroll.Resources;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Samples;

public class TabSampleTextEditor : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("Text", TextSamples.Plain),
				new("Json", TextSamples.Json),
				new("Xml", TextSamples.Xml),

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
			return Repeat("Lots of Lines\n", lines);
		}

		private static ListItem GetStringItem(string label, int length)
		{
			string text = GetString(length);

			return new ListItem(label + " Characters", text);
		}

		private static string GetString(int length)
		{
			const string Unit = "Long String ";
			int count = (length + Unit.Length - 1) / Unit.Length;
			return Repeat(Unit, count);
		}

		private static string Repeat(string value, int count)
		{
			return string.Create(value.Length * count, value, static (span, value) =>
			{
				for (int i = 0; i < span.Length; i += value.Length)
				{
					value.CopyTo(span[i..]);
				}
			});
		}
	}
}
