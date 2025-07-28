namespace SideScroll.Attributes;

// Allows property to be edited in UI
[AttributeUsage(AttributeTargets.Property)]
public class EditingAttribute : Attribute;

/*[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ReadOnlyAttribute : Attribute
{
}*/

// Applies only to Params/TextBoxes
// Allow return key to add a new line
[AttributeUsage(AttributeTargets.Property)]
public class AcceptsReturnAttribute(bool allow = true) : Attribute
{
	public bool Allow => allow;
}

// [Watermark("0123456789abcdef")]
[AttributeUsage(AttributeTargets.Property)]
public class WatermarkAttribute(string text, string? memberName = null) : Attribute
{
	public string Text => text;
	public string? MemberName => memberName; // Field or Property name, overrides Text if set
}

[AttributeUsage(AttributeTargets.Property)]
public class PasswordCharAttribute(char c) : Attribute
{
	public char Character => c;
}

// The member name that contains a list of items to select this item's value from
[AttributeUsage(AttributeTargets.Property)]
public class BindListAttribute(string propertyName) : Attribute
{
	public string PropertyName => propertyName;
}

// ->Toolbar: Show a title and separator before this item
[AttributeUsage(AttributeTargets.Property)]
public class HeaderAttribute(string text) : Attribute
{
	public string Text => text;
}

// ->Toolbar: Show a separator before this item
[AttributeUsage(AttributeTargets.Property)]
public class SeparatorAttribute : Attribute;
