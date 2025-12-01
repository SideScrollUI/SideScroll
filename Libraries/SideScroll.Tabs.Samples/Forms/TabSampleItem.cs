using SideScroll.Attributes;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Tabs.Samples.Forms;

public class TabSampleItem : ITab, IDataView
{
	[DataValue]
	public SampleItem? TestItem;

	//[ButtonColumn("-")]
	public event EventHandler<EventArgs>? OnDelete;

	[ButtonColumn("-")]
	public void Delete()
	{
		OnDelete?.Invoke(this, EventArgs.Empty);
	}

	[DataKey]
	public string? Name => TestItem?.Name;

	public bool? Boolean => TestItem?.Boolean;

	public int? Amount => TestItem?.Amount;

	public TabSampleItem() { }

	public TabSampleItem(SampleItem testItem)
	{
		TestItem = testItem;
	}

	public void Load(object sender, object obj, object?[] tabParams)
	{
		TestItem = (SampleItem)obj;
	}

	public override string? ToString() => Name;

	public TabInstance Create() => new Instance(this);

	private class Instance(TabSampleItem tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.AddData(tab.TestItem);
		}
	}
}
