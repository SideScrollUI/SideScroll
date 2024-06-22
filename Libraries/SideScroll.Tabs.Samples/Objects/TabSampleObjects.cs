using SideScroll;

namespace SideScroll.Tabs.Samples.Objects;

[ListItem]
public class TabSampleObjects
{
	public static TabSampleObjectMembers ObjectMembers => new();
	public static TabSampleObjectProperties ObjectProperties => new();
	public static Tag[] Tags => [new Tag("abc", 1.1)];
	public static TabSampleSubClassProperty SubclassProperty => new();
	public static ValueSub Subclass => new();
	public static EnumTest Enum => new();
	public static TimeSpan TimeSpan => new(1, 2, 3);
	public static TabSampleJson Json => new();
	public static Uri Uri => new("https://wikipedia.org");
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
