using System;

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
public class UnitAttribute : Attribute
{
	public readonly string Name;

	public UnitAttribute(string name)
	{
		Name = name;
	}
}
