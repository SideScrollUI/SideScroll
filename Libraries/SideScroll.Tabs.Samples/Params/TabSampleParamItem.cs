using SideScroll;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Tabs.Samples.Params;

public class TabSampleParamItem : ITab, IDataView
{
	[DataValue]
	public SampleParamItem? TestItem;

	//[ButtonColumn("-")]
	public event EventHandler<EventArgs>? OnDelete;

	[ButtonColumn("-")]
	public void Delete()
	{
		OnDelete?.Invoke(this, EventArgs.Empty);
	}

	[DataKey]
	public string? Name => TestItem?.Name;

	public int? Amount => TestItem?.Amount;

	public TabSampleParamItem() { }

	public TabSampleParamItem(SampleParamItem testItem)
	{
		TestItem = testItem;
	}

	public void Load(object sender, object obj, object[] tabParams)
	{
		TestItem = (SampleParamItem)obj;
	}

	public override string? ToString() => Name;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabSampleParamItem tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.AddData(tab.TestItem);
		}
	}
}
