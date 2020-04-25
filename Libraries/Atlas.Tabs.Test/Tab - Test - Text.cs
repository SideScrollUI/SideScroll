using Atlas.Core;
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
				string shortText = GetString(1000);
				string longText = GetString(10000);
				string reallyLongText = GetString(100000);

				model.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Sample Text", "This is some sample text\n\n1\n2\n3"),
					new ListItem("Short Text - [" + shortText.Length + "]", shortText),
					new ListItem("Medium Text - [" + longText.Length + "]", longText),
					new ListItem("Long Text - [" + reallyLongText.Length + "]", reallyLongText),
				};
			}

			private string GetString(int length)
			{
				StringBuilder stringBuilder = new StringBuilder();
				while (stringBuilder.Length < length)
					stringBuilder.Append("StringBuilder ");
				return stringBuilder.ToString();
			}
		}
	}
}
