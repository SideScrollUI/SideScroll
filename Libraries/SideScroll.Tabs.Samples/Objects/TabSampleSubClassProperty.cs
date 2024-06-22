using SideScroll.Core;

namespace SideScroll.Tabs.Samples.Objects;

public class TabSampleSubClassProperty : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			var items = new ItemCollection<ParentClass>();

			for (int i = 0; i < 1; i++)
			{
				items.Add(new ParentClass());
			}

			model.Items = items;
		}
	}

	public class ParentClass
	{
		public ChildClass Child { get; set; } = new();

		public override string ToString() => Child.ToString();
	}


	public class ChildClass
	{
		public string Text { get; set; } = "test";

		public override string ToString() => Text;
	}
}
