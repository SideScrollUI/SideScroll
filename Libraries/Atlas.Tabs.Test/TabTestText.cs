using Atlas.Core;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Tabs.Test
{
	public class TabTestTextEditor : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new List<ListItem>()
				{
					new ListItem("Sample Text", "This is some sample text\n\n1\n2\n3"),
					new ListItem("Json", TabTestJson.Json1),
					GetListItem("1k", 1000),
					GetListItem("10k", 10000),
					GetListItem("100k", 100000),
					GetListItem("500k", 500000),
					GetListItem("1m", 1000000),
				};
			}

			private ListItem GetListItem(string label, int length)
			{
				string text = GetString(length);

				return new ListItem(label + " Characters", text);
			}

			private string GetString(int length)
			{
				var sb = new StringBuilder();
				while (sb.Length < length)
				{
					sb.Append("StringBuilder ");
				}
				return sb.ToString();
			}
		}
	}
}
