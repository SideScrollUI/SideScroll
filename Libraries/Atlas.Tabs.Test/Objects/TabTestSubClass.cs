using Atlas.Core;

namespace Atlas.Tabs.Test.Objects;

public class TabTestSubClassProperty : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			var items = new ItemCollection<ParentClass>();

			for (int i = 0; i < 1; i++)
				items.Add(new ParentClass());

			//items.Add(new ListItem("Long Text", reallyLongText));
			model.Items = items;
		}
	}

	public class ParentClass
	{
		//public string stringProperty { get; set; } = "test";
		public ChildClass Child { get; set; } = new();

		public override string ToString() => Child.ToString();
	}


	public class ChildClass
	{
		public string Text { get; set; } = "test";

		public override string ToString() => Text;
	}
}
