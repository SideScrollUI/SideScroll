using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Atlas.Tabs.Test
{
	[Params]
	public class ParamTestItem
	{
		public bool Boolean { get; set; } = true;
		public string Name { get; set; } = "Test";
		[ReadOnly(true)]
		public string ReadOnly { get; set; } = "ReadOnly";
		[DataKey]
		public int Amount { get; set; } = 123;
		public double Double { get; set; } = 3.14;
		public DateTime DateTime { get; set; } = DateTime.Now;
		public AttributeTargets EnumAttributeTargets { get; set; } = AttributeTargets.Event;
		public static List<ParamListItem> ListItems => new List<ParamListItem>()
		{
			new ParamListItem("One", 1),
			new ParamListItem("Two", 2),
			new ParamListItem("Three", 3),
		};
		[BindList(nameof(ListItems))]
		public ParamListItem ListItem { get; set; }

		public ParamTestItem()
		{
			ListItem = ListItems[1];
		}

		public override string ToString() => Name;

		/*[ButtonColumn("-")]
		public void Delete()
		{
			instance.Delete(Name);
		}*/
	}

	public class ParamListItem
	{
		public string Name { get; set; }
		public int Value { get; set; }

		public override string ToString() => Name;

		public ParamListItem()
		{
		}

		public ParamListItem(string name, int value)
		{
			Name = name;
			Value = value;
		}
	}

	public class TabParamItem : ITab, IDataTab
	{
		public ParamTestItem testItem;

		//[ButtonColumn("-")]
		public event EventHandler<EventArgs> OnDelete;

		[ButtonColumn("-")]
		public void Delete()
		{
			OnDelete?.Invoke(this, null);
		}

		[DataKey]
		public string Name => testItem.Name;

		public int? Amount => testItem.Amount;

		public TabParamItem()
		{
		}

		public TabParamItem(ParamTestItem testItem)
		{
			this.testItem = testItem;
		}

		public void Load(Call call, object obj)
		{
			this.testItem = (ParamTestItem)obj;
		}

		public override string ToString() => Name;

		public TabInstance Create() => new Instance(this);

		public class Instance : TabInstance
		{
			private TabParamItem tab;

			public Instance(TabParamItem tab)
			{
				this.tab = tab;
			}

			public override void Load(Call call, TabModel model)
			{
				model.AddData(tab.testItem);
			}
		}
	}
}
