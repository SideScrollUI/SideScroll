using SideScroll.Core;

namespace SideScroll.Tabs.Samples.Loading;

public class TabSampleSkip : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			var sampleItems = new List<SampleItem>
			{
				new(1, "Item 1"),
			};

			model.Items = new List<ListItem>
			{
				new("Sample Items", sampleItems),
			};
		}
	}
}

public class SampleItem(int id, string name)
{
	public int Id { get; set; } = id;
	public string Name { get; set; } = name;

	public override string ToString() => Name;
}
