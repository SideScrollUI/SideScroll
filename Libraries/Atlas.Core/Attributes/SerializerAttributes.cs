using System;

//.Attributes ?
namespace Atlas.Core
{
	// When Cloning an object, anything marked with [Static] won't be deep copied
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
	public class StaticAttribute : Attribute
	{
	}

	// Can't use [NonSerialized] since that's only for fields :(
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
	public class UnserializedAttribute : Attribute
	{
	}

	// Can't use [NonSerialized] since that's only for fields :(
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
	public class SecureAttribute : Attribute
	{
	}
}
