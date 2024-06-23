namespace SideScroll.Attributes;

// When Cloning an object, anything marked with [Static] won't be deep copied
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class StaticAttribute : Attribute;

// Override serializer defaults (constructor only check for now)
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
public class SerializedAttribute : Attribute;

// Can't use [NonSerialized] since that's only for fields :(
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
public class UnserializedAttribute : Attribute;

// Serialized when exported for public usage
// Fields & Properties will default to [PublicData] unless overriden with another permission
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
public class PublicDataAttribute : Attribute;

// All class members will default to [PrivateData]
// Members can override if reference or type specifies [PublicData]/[ProtectedData]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
public class ProtectedDataAttribute : Attribute;

// Not serialized when exported for public usage, data is only saved locally, could require encryption
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
public class PrivateDataAttribute : Attribute;
