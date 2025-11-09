using SideScroll.Attributes;
using SideScroll.Extensions;

namespace SideScroll;

public enum TagType
{
	Default,
	Unique, // Only show for original set of Tags
	Required, // Show in all Tag combinations
}

public class Tag
{
	public static int MaxValueLength { get; set; } = 10_000;

	public string? Name { get; set; }

	[InnerValue, StyleValue]
	public object? Value { get; set; }

	[HiddenColumn]
	public TagType Type { get; set; }

	public override string ToString()
	{
		string? text = Value.Formatted(MaxValueLength);
		if (text?.Contains(' ') == true)
		{
			text = '"' + text + '"';
		}

		return "[ " + Name + " = " + text + " ]";
	}

	public Tag() { }

	public Tag(object value)
	{
		Name = value?.ToString() ?? "(null)";
		Value = value;
	}

	public Tag(Tag tag)
	{
		Name = tag.Name;
		Value = tag.Value;
		Type = tag.Type;
	}

	public Tag(string name, object? value, TagType type = TagType.Default)
	{
		Name = name;
		Value = value;
		Type = type;
	}
}

public class TaggedException(string text, params Tag[] tags) : Exception()
{
	public string Text => text;
	public Tag[] Tags => tags;

	[WordWrap, MinWidth(300)]
	public override string Message => tags?.Length > 0 ? $"{Text} {string.Join<Tag>(' ', tags)}" : Text;
}
