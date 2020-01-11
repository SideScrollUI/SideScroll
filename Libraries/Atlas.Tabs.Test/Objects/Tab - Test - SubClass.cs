using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test.Objects
{
	public class TabTestSubClassProperty : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				ItemCollection<ParentClass> items = new ItemCollection<ParentClass>();

				for (int i = 0; i < 1; i++)
					items.Add(new ParentClass());

				//items.Add(new ListItem("Long Text", reallyLongText));
				tabModel.Items = items;
			}
		}

		public class ParentClass
		{
			//public string stringProperty { get; set; } = "test";
			public ChildClass child { get; set; } = new ChildClass();
			public override string ToString()
			{
				return "";
			}
		}


		public class ChildClass
		{
			public string Text { get; set; } = "test";
			public override string ToString()
			{
				return "";
			}
		}
	}
}
/*
ETO's DataGrid doesn't handle this Test properly
throws
: 'Object must implement IConvertible.'


TabView class
// Object must implement IConvertible - can't databind non-primitives
// URI class has this problem
// Call ToString() on any non-primitive
stackLayoutChildControls.Items.Add(layoutItem);

*/
