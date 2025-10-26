namespace SideScroll.Attributes;

/// <summary>
/// Attributes that control editing behavior and input formatting for properties in SideScroll UI controls.
/// </summary>
/// <remarks>
/// <b>Editing Control:</b> Use <see cref="EditColumnAttribute"/> to enable property editing in the UI.
/// <para>
/// <b>Input Behavior:</b> Use <see cref="AcceptsReturnAttribute"/> and <see cref="PasswordCharAttribute"/> 
/// to control text input behavior and security.
/// </para>
/// <para>
/// <b>Data Binding:</b> Use <see cref="BindListAttribute"/> to bind properties to selection lists.
/// </para>
/// <para>
/// <b>UI Enhancement:</b> Use <see cref="WatermarkAttribute"/>, <see cref="HeaderAttribute"/>, and 
/// <see cref="SeparatorAttribute"/> to improve user experience and organization.
/// </para>
/// </remarks>
internal static class _DocTabEditingSentinel { }

/// <summary>
/// Enables property editing in DataGrid columns.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Makes the property editable in DataGrid columns. Currently only supports bool properties.
/// Without this attribute, properties are read-only in the DataGrid.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class UserProfile
/// {
///     [EditColumn]
///     public bool IsActive { get; set; } = true;
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class EditColumnAttribute : Attribute;

/// <summary>
/// Controls whether text inputs accepts Return for multi-line entry.
/// </summary>
/// <param name="acceptsPlainEnter">Whether to require Shift + Return for new lines (default: true).</param>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Applies to text boxes and form inputs. When enabled, pressing Enter or Shift + Enter creates 
/// a new line instead of submitting the form. If acceptsPlainEnter is false, Regular Enter will still submit the form
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Comment
/// {
///     [AcceptsReturn(true)]
///     public string Text { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class AcceptsReturnAttribute(bool acceptsPlainEnter = false) : Attribute
{
	/// <summary>
	/// Whether the input accepts Shift+Return for new lines.
	/// </summary>
	public bool AcceptsPlainEnter => acceptsPlainEnter;
}

/// <summary>
/// Displays placeholder text in empty input fields, optionally sourced from another member.
/// </summary>
/// <param name="text">The watermark text to display.</param>
/// <param name="memberName">Optional member name to use as watermark source (overrides text).</param>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Shows helpful placeholder text when the input is empty. If memberName is specified, 
/// the watermark text comes from that member's value instead of the static text.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class LoginForm
/// {
///     [Watermark("Enter your username")]
///     public string Username { get; set; } = "";
///     
///     [Watermark("", "DefaultEmail")]
///     public string Email { get; set; } = "";
///     
///     public string DefaultEmail => "user@example.com";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class WatermarkAttribute(string text, string? memberName = null) : Attribute
{
	/// <summary>
	/// The watermark text to display.
	/// </summary>
	public string Text => text;
	
	/// <summary>
	/// Optional member name to use as watermark source (overrides Text if set).
	/// </summary>
	public string? MemberName => memberName;
}

/// <summary>
/// Specifies the character to use for masking password input.
/// </summary>
/// <param name="c">The character to display instead of actual input characters.</param>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Masks sensitive input by displaying the specified character instead of the actual text.
/// Commonly used with '*' or '•' for password fields.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Credentials
/// {
///     [PasswordChar('*')]
///     public string Password { get; set; } = "";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class PasswordCharAttribute(char c) : Attribute
{
	/// <summary>
	/// The character used to mask input.
	/// </summary>
	public char Character => c;
}

/// <summary>
/// Binds the property to a list of selectable items from another member.
/// </summary>
/// <param name="propertyName">The name of the member containing the list of selectable items.</param>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Creates a dropdown or selection control populated with items from the specified member.
/// The target member should contain a collection of items to choose from.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class OrderForm
/// {
///     [BindList("AvailableProducts")]
///     public Product SelectedProduct { get; set; }
///     
///     public List&lt;Product&gt; AvailableProducts { get; set; } = new();
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class BindListAttribute(string propertyName) : Attribute
{
	/// <summary>
	/// The name of the member containing the list of selectable items.
	/// </summary>
	public string PropertyName => propertyName;
}

/// <summary>
/// Displays a header title and separator before this property in forms.
/// </summary>
/// <param name="text">The header text to display.</param>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Creates a visual section break with a title, useful for organizing related properties 
/// into logical groups within forms.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Settings
/// {
///     [Header("Display Options")]
///     public string Theme { get; set; } = "";
///     
///     public bool ShowToolbar { get; set; } = true;
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class HeaderAttribute(string text) : Attribute
{
	/// <summary>
	/// The header text to display.
	/// </summary>
	public string Text => text;
}

/// <summary>
/// Displays a separator line before this property in toolbars and forms.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Creates a visual separator without text, useful for creating subtle divisions 
/// between property groups in the UI.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Configuration
/// {
///     public string Name { get; set; } = "";
///     
///     [Separator]
///     public bool IsEnabled { get; set; } = true;
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class SeparatorAttribute : Attribute;
