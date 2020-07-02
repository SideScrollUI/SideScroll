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
		public string String { get; set; } = "Test";
		[ReadOnly(true)]
		public string ReadOnly { get; set; } = "ReadOnly";
		[DataKey]
		public int Integer { get; set; } = 123;
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

		public override string ToString() => Integer.ToString();

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
}
