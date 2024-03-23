namespace Atlas.Core;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class XAxisAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class YAxisAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class UnitAttribute(string name) : Attribute
{
	public readonly string Name = name;
}
