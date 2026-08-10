using System.Runtime.CompilerServices;

namespace SideScroll.Serialize.Atlas;

/// <summary>
/// Identifies objects the serializer has already seen, by reference except for the immutable types
/// that are safe to share
/// </summary>
/// <remarks>
/// <para>
/// A type that overrides <see cref="object.Equals(object)"/> to compare only some of its members,
/// which comparing by an id and every record does, made two distinct objects indistinguishable
/// here. The second was then stored as a reference to the first, so whatever else differed between
/// them was replaced by the first object's values rather than merely aliased.
/// </para>
/// <para>
/// The types below are exempt because they can't be changed after they're created, so two equal
/// instances are interchangeable and sharing one loses nothing. That sharing is what keeps a value
/// repeated across rows, most often a string, from being stored once per occurrence.
/// </para>
/// </remarks>
public class SerializerObjectComparer : IEqualityComparer<object>
{
	/// <summary>Gets the shared comparer instance.</summary>
	public static SerializerObjectComparer Instance { get; } = new();

	/// <summary>
	/// Returns whether two equal instances of this object's type can be represented by one of them
	/// </summary>
	/// <remarks>
	/// <para>
	/// The reference types listed are immutable, matching the ones
	/// <see cref="Serializer.Clone(object)"/> returns as-is instead of copying.
	/// </para>
	/// <para>
	/// A value type has to be included whatever it contains, because reading one boxes it again
	/// each time, so the box a member is saved from is never the box it was queued as. Sharing one
	/// loses nothing either way, since a box already holds a copy rather than the original.
	/// </para>
	/// </remarks>
	public static bool IsShareable(object obj)
	{
		return obj is string or Type or Version or Uri or TimeZoneInfo ||
			obj.GetType().IsValueType;
	}

	/// <inheritdoc/>
	public new bool Equals(object? x, object? y)
	{
		if (ReferenceEquals(x, y))
			return true;

		if (x == null || y == null)
			return false;

		// Anything else is only the same object when it's the same reference, however its own
		// Equals() compares it
		if (!IsShareable(x))
			return false;

		return x.Equals(y);
	}

	/// <inheritdoc/>
	public int GetHashCode(object obj)
	{
		return IsShareable(obj) ? obj.GetHashCode() : RuntimeHelpers.GetHashCode(obj);
	}
}
