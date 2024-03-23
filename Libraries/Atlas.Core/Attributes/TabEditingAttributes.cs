namespace Atlas.Core;

// Allows property to be edited in UI
[AttributeUsage(AttributeTargets.Property)]
public class EditingAttribute : Attribute
{
}

/*[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ReadOnlyAttribute : Attribute
{
}*/

// Applies only to Params/TextBoxes
// Allow return key to add a new line
[AttributeUsage(AttributeTargets.Property)]
public class AcceptsReturnAttribute(bool allow = true) : Attribute
{
	public readonly bool Allow = allow;
}

// [Watermark("0123456789abcdef")]
[AttributeUsage(AttributeTargets.Property)]
public class WatermarkAttribute : Attribute
{
	public readonly string Text;
	public readonly string? MemberName; // Field or Property name, overrides Text if set

	public WatermarkAttribute(string text, string? memberName = null)
	{
		Text = text;
		MemberName = memberName;
	}
}

[AttributeUsage(AttributeTargets.Property)]
public class PasswordCharAttribute(char c) : Attribute
{
	public readonly char Character = c;
}

// The member name that contains a list of items to select this item's value from
[AttributeUsage(AttributeTargets.Property)]
public class BindListAttribute(string propertyName) : Attribute
{
	public readonly string PropertyName = propertyName;
}

// ->Toolbar: Show a separator before this item
[AttributeUsage(AttributeTargets.Property)]
public class SeparatorAttribute : Attribute
{
}

// If set, this method will appear as an Action (rename to [Action]?)
/*[AttributeUsage(AttributeTargets.Method)]
public class VisibleAttribute : Attribute
{
}*/
