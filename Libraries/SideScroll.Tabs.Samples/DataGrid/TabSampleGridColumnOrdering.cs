namespace SideScroll.Tabs.Samples.DataGrid;

public class TabSampleGridColumnOrdering : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			List<TestChild> items = [];
			for (int i = 0; i < 2; i++)
			{
				TestChild item = new()
				{
				};

				items.Add(item);
			}

			model.Items = items;
		}
	}

	public abstract class TestParent
	{
		public string Field = "1";
		public string Original { get; set; } = "2";
		public virtual string Virtual { get; set; } = "3";
		public virtual string Overriden { get; set; } = "4";
		public abstract string Abstract { get; }

		public override string ToString() => Original;
	}

	public class TestChild : TestParent
	{
		public override string Overriden { get; set; } = "5";
		public override string Abstract => "6";

		public override string ToString() => Original;
	}
}
/*
Tests the column order matches for the different types
*/
