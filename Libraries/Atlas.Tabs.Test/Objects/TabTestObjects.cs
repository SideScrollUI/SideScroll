using Atlas.Core;
using System;

namespace Atlas.Tabs.Test.Objects
{
	public class TabTestObjects : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new ItemCollection<ListItem>()
				{
					new("Object Members", new TestObjectMembers()),
					new("Tags", new Tag[] { new Tag("abc", 1.1) }),
					new("Subclass Property", new TabTestSubClassProperty()),
					new("Subclass", new ValueSub()),
					new("Enum", new EnumTest()),
					new("TimeSpan", new TimeSpan(1, 2, 3)),
				};
			}

			public class MyClass
			{
				public string Name { get; set; } = "Eve";
			}

			public enum EnumTest
			{
				One = 1,
				Two = 2,
				Four = 4,
				Eight = 8,
			}
		}
	}

	public class ValueBase
	{
		public int Value = 1;
	}

	public class ValueSub : ValueBase
	{
		public new int Value = 2;
	}

	public class TestObjectMembers
	{
		public static readonly string StaticStringField;

		public bool BoolField;
		public bool BoolProperty { get; }

		[Item]
		public bool BoolMethod() => true;

		public string StringField;
		public string StringProperty { get; }

		[Item]
		public string StringMethod() => "string";
	}
}
