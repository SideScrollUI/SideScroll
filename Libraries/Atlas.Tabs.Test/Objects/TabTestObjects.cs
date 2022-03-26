using Atlas.Core;
using System;

namespace Atlas.Tabs.Test.Objects;

[ListItem]
public class TabTestObjects
{
	public static TestObjectMembers ObjectMembers => new();
	public static Tag[] Tags => new Tag[] { new Tag("abc", 1.1) };
	public static TabTestSubClassProperty SubclassProperty => new();
	public static ValueSub Subclass => new();
	public static EnumTest Enum => new();
	public static TimeSpan TimeSpan => new(1, 2, 3);
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
