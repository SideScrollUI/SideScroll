using Atlas.Core;
using System;

namespace Atlas.Tabs.Test
{
	public class TabParamItem : ITab, IDataTab
	{
		public ParamTestItem TestItem;

		//[ButtonColumn("-")]
		public event EventHandler<EventArgs> OnDelete;

		[ButtonColumn("-")]
		public void Delete()
		{
			OnDelete?.Invoke(this, null);
		}

		[DataKey]
		public string Name => TestItem.Name;

		public int? Amount => TestItem.Amount;

		public TabParamItem()
		{
		}

		public TabParamItem(ParamTestItem testItem)
		{
			TestItem = testItem;
		}

		public void Load(Call call, object obj, object[] tabParams)
		{
			TestItem = (ParamTestItem)obj;
		}

		public override string ToString() => Name;

		public TabInstance Create() => new Instance(this);

		public class Instance : TabInstance
		{
			public TabParamItem Tab;

			public Instance(TabParamItem tab)
			{
				Tab = tab;
			}

			public override void Load(Call call, TabModel model)
			{
				model.AddData(Tab.TestItem);
			}
		}
	}
}
