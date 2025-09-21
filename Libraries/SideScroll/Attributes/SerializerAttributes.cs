namespace SideScroll.Attributes;

/// <summary>
/// Attributes that control SideScroll's serialization, cloning, and data export behavior.
/// </summary>
/// <remarks>
/// <b>Serialization:</b> Public instance fields/properties are included by default. Use <see cref="SerializedAttribute"/> 
/// or <see cref="UnserializedAttribute"/> to override defaults.
/// <para>
/// <b>Cloning:</b> Deep cloner handles circular references. Use <see cref="CloneReferenceAttribute"/> to skip deep-copying.
/// </para>
/// <para>
/// <b>Export Control:</b> Use <see cref="PublicDataAttribute"/>, <see cref="ProtectedDataAttribute"/>, or 
/// <see cref="PrivateDataAttribute"/> to control what gets exported publicly. Types without permission attributes 
/// generate warnings during public export.
/// </para>
/// <para>
/// <b>Compatibility:</b> Use <see cref="DeprecatedNameAttribute"/> for backward compatibility with renamed members.
/// </para>
/// </remarks>
internal static class _DocNamespaceSentinel { }

/// <summary>
/// Prevents deep copying during cloning - copies the reference instead of the value.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Classes or structs.
/// <para>
/// When applied to types, all members default to reference cloning (individual members can override).
/// Improves performance for immutable or unchanging data.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [CloneReference]
/// public class Node
/// {
///     public Node? Parent { get; set; }
///     public string Name { get; set; } = "";
/// }
/// </code>
/// </example>
// TODO: Consider adding support for individual fields and properties in the future
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class CloneReferenceAttribute : Attribute
{
}

/// <summary>
/// Forces serialization of members that would otherwise be excluded.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Classes or structs.
/// <para>
/// Use when you need to explicitly include members that the serializer would normally skip.
/// When applied to types, all members default to being serialized (individual members can override).
/// Missing constructors in base classes is a common reason to use this
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [Serialized]
/// public class Example
/// {
///     public int ForceInclude { get; set; }
/// }
/// </code>
/// </example>
// TODO: Consider adding support for individual fields and properties in the future
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class SerializedAttribute : Attribute
{
}

/// <summary>
/// Prevents serialization entirely (both local and export). Not overridable
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields, properties, classes, or structs.
/// <para>
/// Use when <see cref="NonSerializedAttribute"/> isn't available (properties, classes, structs).
/// Not overridable
/// </para>
/// <para>
/// <b>vs. <see cref="PrivateDataAttribute"/>:</b> This prevents all serialization; PrivateData allows 
/// local serialization but blocks public export.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Example
/// {
///     [Unserialized] public string? TempOnly { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class UnserializedAttribute : Attribute
{
}

/// <summary>
/// Marks data as safe for public export and import in links
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields, properties, classes, or structs.
/// <para>
/// Included in public exports (<c>publicOnly = true</c>). When applied to types, all members 
/// default to public (individual members can override).
/// </para>
/// <para>
/// <b>Note:</b> Types without permission attributes generate warnings during public export and will be excluded. 
/// Most common framework types (string, DateTime, collections) are always allowed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [PublicData]
/// public class PublicProfile
/// {
///     public string DisplayName { get; set; } = "";
///     [PrivateData] public string SecretNote { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class PublicDataAttribute : Attribute
{
}

/// <summary>
/// Marks data as protected - excluded from public export but available for internal use.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields, properties, classes, or structs.
/// <para>
/// When applied to types, members default to protected (excluded from public export) unless 
/// explicitly marked with <see cref="PublicDataAttribute"/>.
/// </para>
/// <para>
/// <b>Public Export:</b> Protected data is excluded from <c>publicOnly = true</c> exports, 
/// allowing you to set secure defaults while permitting selective public exposure.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [ProtectedData]
/// public class Account
/// {
///     // Defaults to private unless explicitly marked public
///     public string Email { get; set; } = "";
///
///     [PublicData]                     // Explicitly allowed for public export
///     public string PublicId { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class ProtectedDataAttribute : Attribute
{
}

/// <summary>
/// Marks data as private - excluded from public export but still serialized locally.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields, properties, classes, or structs.
/// <para>
/// Private data is serialized locally but excluded from public exports (<c>publicOnly = true</c>).
/// When applied to types, all members default to private (individual members can override).
/// </para>
/// <para>
/// <b>Note:</b> Private data doesn't trigger export warnings and may require encryption for sensitive content.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Credentials
/// {
///     [PrivateData] public string PasswordHash { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class PrivateDataAttribute : Attribute
{
}

/// <summary>
/// Specifies legacy names to accept during deserialization for backward compatibility.
/// </summary>
/// <param name="name">Primary deprecated name to accept.</param>
/// <param name="names">Additional deprecated names (aliases) to accept.</param>
/// <remarks>
/// <b>Apply to:</b> Fields or properties.
/// <para>
/// Maintains compatibility with data serialized using older member names.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Item
/// {
///     [DeprecatedName("OldName", "OldName2", ...)]
///     public string NewName { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DeprecatedNameAttribute(string name, params string[] names) : Attribute
{
	/// <summary>
	/// All deprecated names accepted during deserialization.
	/// </summary>
	public string[] Names { get; } =
	[
		name,
		.. names
	];
}
