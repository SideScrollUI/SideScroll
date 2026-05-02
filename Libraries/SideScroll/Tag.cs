using SideScroll.Attributes;
using SideScroll.Extensions;

namespace SideScroll;

/// <summary>
/// Specifies the display behavior of a tag
/// </summary>
public enum TagType
{
	/// <summary>
	/// Standard tag display behavior
	/// </summary>
	Default,
	/// <summary>
	/// Only show for original set of Tags
	/// </summary>
	Unique,
	/// <summary>
	/// Show in all Tag combinations
	/// </summary>
	Required,
}

/// <summary>
/// Represents a name-value pair with optional type information
/// </summary>
public class Tag
{
	/// <summary>
	/// Gets or sets the maximum length for tag values when formatting
	/// </summary>
	public static int MaxValueLength { get; set; } = 10_000;

	/// <summary>
	/// Gets or sets the tag name
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the tag value
	/// </summary>
	[InnerValue, StyleValue]
	public object? Value { get; set; }

	/// <summary>
	/// Gets or sets the tag type that controls display behavior
	/// </summary>
	[HiddenColumn]
	public TagType Type { get; set; }

	/// <summary>Returns a formatted string representation of the tag as <c>[ Name = Value ]</c>.</summary>
	public override string ToString()
	{
		string? text = Value.Formatted(MaxValueLength);
		if (text?.Contains(' ') == true)
		{
			text = '"' + text + '"';
		}

		return "[ " + Name + " = " + text + " ]";
	}

	/// <summary>Initializes an empty tag with no name or value.</summary>
	public Tag() { }

	/// <summary>Initializes a tag using the object's string representation as both name and value.</summary>
	public Tag(object value)
	{
		Name = value?.ToString() ?? "(null)";
		Value = value;
	}

	/// <summary>Initializes a tag as a copy of another tag.</summary>
	public Tag(Tag tag)
	{
		Name = tag.Name;
		Value = tag.Value;
		Type = tag.Type;
	}

	/// <summary>Initializes a tag with the given name, value, and optional tag type.</summary>
	public Tag(string name, object? value, TagType type = TagType.Default)
	{
		Name = name;
		Value = value;
		Type = type;
	}
}

/// <summary>
/// Exception that includes associated tags for additional context
/// </summary>
public class TaggedException(string text, params Tag[] tags) : Exception
{
	/// <summary>
	/// Gets the exception text
	/// </summary>
	public string Text => text;

	/// <summary>
	/// Gets the tags associated with this exception
	/// </summary>
	public Tag[] Tags => tags;

	/// <summary>
	/// Gets the formatted message including tags
	/// </summary>
	[WordWrap, MinWidth(300)]
	public override string Message => tags.Length > 0 ? $"{Text} {string.Join<Tag>(' ', tags)}" : Text;
}
