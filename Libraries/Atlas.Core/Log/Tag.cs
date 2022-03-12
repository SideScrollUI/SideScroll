using Atlas.Extensions;

namespace Atlas.Core;

public enum TagType
{
	Default,
	Unique, // Only show for original set of Tags
	Required, // Show in all Tag combinations
};

public class Tag
{
	public string Name { get; set; }

	[InnerValue, StyleValue]
	public object Value { get; set; }

	public TagType Type;

	public override string ToString()
	{
		string text = Value.Formatted();
		if (text?.Contains(" ") == true)
			text = '"' + text + '"';

		return "[ " + Name + " = " + text + " ]";
	}

	public Tag()
	{
	}

	public Tag(object value)
	{
		Name = value.ToString();
		Value = value;
	}

	public Tag(Tag tag)
	{
		Name = tag.Name;
		Value = tag.Value;
		Type = tag.Type;
	}

	public Tag(string name, object value, TagType type = TagType.Default)
	{
		Name = name;
		Value = value;
		Type = type;
	}
}
