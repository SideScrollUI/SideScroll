namespace SideScroll.Tabs.Samples.Objects;

public class TabSampleSubClassProperty : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ParentClass>
			{
				new()
			};
		}
	}

	private class ParentClass
	{
		public ChildClass Child { get; set; } = new();

		public override string ToString() => Child.ToString();
	}


	private class ChildClass
	{
		public string Text { get; set; } = "test";

		public override string ToString() => Text;
	}
}
