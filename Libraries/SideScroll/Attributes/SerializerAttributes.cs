namespace SideScroll.Attributes;

/// <summary>
/// Attributes that control SideScroll's serialization, cloning, and data export behavior.
/// </summary>
/// <remarks>
/// <b>Serialization:</b> Public instance fields/properties are included by default. Use <see cref="UnserializedAttribute"/> 
/// to override defaults.
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
/// Marks types as serializable with opt-in member visibility for public export.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Fields, properties, classes, or structs.
/// <para>
/// <b>On Types:</b> The type itself is treated as <see cref="PublicDataAttribute"/> (serializable and exportable), 
/// but members are treated as <see cref="PrivateDataAttribute"/> by default (excluded from public export) unless 
/// explicitly marked with <see cref="PublicDataAttribute"/>.
/// </para>
/// <para>
/// <b>On Members:</b> Individual members can be marked as protected to exclude them from public export 
/// (currently has no effect when applied to members of <see cref="PublicDataAttribute"/> classes).
/// </para>
/// <para>
/// <b>Use Case:</b> Provides secure defaults for types that need selective public exposure. 
/// The type is serializable, but only explicitly-marked members are included in public exports.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [ProtectedData]                     // Type is serializable
/// public class Account
/// {
///     public string Email { get; set; } = "";          // Private by default - excluded from public export
///
///     [PublicData]                                     // Explicitly public - included in public export
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
