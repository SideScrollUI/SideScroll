using Atlas.Core;

namespace Atlas.Tabs.Test.Objects;

[ListItem]
public class TabTestObjects
{
	public static TabTestObjectMembers ObjectMembers => new();
	public static TabTestObjectProperties ObjectProperties => new();
	public static Tag[] Tags => [new Tag("abc", 1.1)];
	public static TabTestSubClassProperty SubclassProperty => new();
	public static ValueSub Subclass => new();
	public static EnumTest Enum => new();
	public static TimeSpan TimeSpan => new(1, 2, 3);
}

public class MyClass
{
	public string Description { get; set; } = "Really long value that keeps going on and on and on, or at least for a really long time and we see some wordwrap";

	public override string ToString() => Description;
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
