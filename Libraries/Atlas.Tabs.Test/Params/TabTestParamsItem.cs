using Atlas.Core;
using Atlas.Serialize;

namespace Atlas.Tabs.Test.Params;

public class TabParamItem : ITab, IDataView
{
	[DataValue]
	public ParamTestItem? TestItem;

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

	public TabParamItem() { }

	public TabParamItem(ParamTestItem testItem)
	{
		TestItem = testItem;
	}

	public void Load(object sender, object obj, object[] tabParams)
	{
		TestItem = (ParamTestItem)obj;
	}

	public override string? ToString() => Name;

	public TabInstance Create() => new Instance(this);

	public class Instance(TabParamItem tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.AddData(tab.TestItem);
		}
	}
}
