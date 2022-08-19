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
public class AcceptsReturnAttribute : Attribute
{
	public readonly bool Allow;

	public AcceptsReturnAttribute(bool allow = true)
	{
		Allow = allow;
	}
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
public class PasswordCharAttribute : Attribute
{
	public readonly char Character;

	public PasswordCharAttribute(char c)
	{
		Character = c;
	}
}

// The member name that contains a list of items to select this item's value from
[AttributeUsage(AttributeTargets.Property)]
public class BindListAttribute : Attribute
{
	public readonly string PropertyName;

	public BindListAttribute(string propertyName)
	{
		PropertyName = propertyName;
	}
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
