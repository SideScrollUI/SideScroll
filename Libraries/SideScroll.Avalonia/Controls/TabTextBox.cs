using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using SideScroll.Attributes;
using SideScroll.Avalonia.Controls.Converters;
using SideScroll.Avalonia.Themes;
using SideScroll.Avalonia.Utilities;
using SideScroll.Tabs.Lists;
using SideScroll.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace SideScroll.Avalonia.Controls;

/// <summary>
/// A styled text box that binds to a <see cref="ListProperty"/>, supporting validation attributes,
/// read-only mode, password masking, and auto-width resizing.
/// </summary>
public class TabTextBox : TextBox
{
	/// <inheritdoc/>
	protected override Type StyleKeyOverride => typeof(TextBox);

	/// <summary>Gets the list property this text box is bound to, or <c>null</c> if unbound.</summary>
	public ListProperty? Property { get; }

	/// <summary>Gets or sets whether pressing Enter without a modifier key inserts a new line (requires <see cref="TextBox.AcceptsReturn"/> to also be <c>true</c>).</summary>
	public bool AcceptsPlainEnter { get; set; }

	/// <summary>
	/// Gets or sets the characters that can be typed, or <c>null</c> to allow any. Defaults to the characters the bound
	/// property's type can parse, unless an <see cref="AllowedCharsAttribute"/> overrides them.
	/// </summary>
	/// <remarks>
	/// Only typed input is filtered. Pasted and assigned text is left to the binding's validation,
	/// since silently dropping part of a paste can produce a value the user never entered
	/// </remarks>
	public string? AllowedChars { get; set; }

	/// <summary>Returns the string representation of the bound property.</summary>
	public override string? ToString() => Property?.ToString();

	/// <summary>Creates an unbound text box with the default styling and border layout.</summary>
	public TabTextBox()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Top;

		MinWidth = 50;
		MaxWidth = 3000;

		InitializeBorder();
	}

	/// <summary>Creates a text box bound to <paramref name="property"/>, applying its validation, width, and display attributes.</summary>
	/// <param name="property">The list property to bind to.</param>
	public TabTextBox(ListProperty property) : this()
	{
		Property = property;

		InitializeProperty(property);
	}

	// Default Padding shows a gap between ScrollBar and Border, and has too big of a left Margin
	// Workaround: Move the Padding into the inner controls Margin
	private void InitializeBorder()
	{
		Padding = new Thickness(0);

		var style = new Style(x => x.OfType<TextPresenter>())
		{
			Setters =
			{
				new Setter(MarginProperty, new Thickness(6)),
			}
		};
		Styles.Add(style);

		style = new Style(x => x.OfType<TextBlock>())
		{
			Setters =
			{
				new Setter(MarginProperty, new Thickness(6)),
			}
		};
		Styles.Add(style);
	}

	/// <summary>Applies the property's read-only state, watermark, and layout attributes, then binds the text box to it.</summary>
	/// <param name="property">The list property to configure and bind to.</param>
	protected void InitializeProperty(ListProperty property)
	{
		IsReadOnly = !property.IsEditable;

		var passwordCharAttribute = property.GetCustomAttribute<PasswordCharAttribute>();
		if (passwordCharAttribute != null)
		{
			PasswordChar = passwordCharAttribute.Character;
		}

		SetWatermark(property);
		SetAllowedChars(property);

		if (property.PropertyInfo.GetCustomAttribute<WordWrapAttribute>() != null)
		{
			TextWrapping = TextWrapping.Wrap;
		}

		var acceptsReturnAttribute = property.GetCustomAttribute<AcceptsReturnAttribute>();
		if (acceptsReturnAttribute != null)
		{
			AcceptsReturn = true;
			AcceptsPlainEnter = acceptsReturnAttribute.AcceptsPlainEnter;
		}

		var minWidthAttribute = property.GetCustomAttribute<MinWidthAttribute>();
		if (minWidthAttribute != null)
		{
			MinWidth = minWidthAttribute.MinWidth;
		}

		var maxWidthAttribute = property.GetCustomAttribute<MaxWidthAttribute>();
		if (maxWidthAttribute != null)
		{
			MaxWidth = maxWidthAttribute.MaxWidth;
		}
		else
		{
			MaxWidth = TabForm.ControlMaxWidth;
		}

		var maxHeightAttribute = property.GetCustomAttribute<MaxHeightAttribute>();
		if (maxHeightAttribute != null)
		{
			MaxHeight = maxHeightAttribute.MaxHeight;
		}
		else
		{
			MaxHeight = TabForm.ControlMaxHeight;
		}

		if (property.GetCustomAttribute<ToolTipAttribute>() is { } toolTipAttribute)
		{
			ToolTip.SetTip(this, toolTipAttribute.Text);
		}
		else if (property.GetCustomAttribute<RangeAttribute>() is { } rangeAttribute)
		{
			ToolTip.SetTip(this, $"{rangeAttribute.Minimum} - {rangeAttribute.Maximum}");
		}

		BindProperty(property);

		AvaloniaUtils.AddContextMenu(this); // Custom menu to handle ReadOnly items better
	}

	private void SetWatermark(ListProperty property)
	{
		WatermarkAttribute? attribute = property.GetCustomAttribute<WatermarkAttribute>();
		if (attribute == null)
			return;

		if (attribute.MemberName != null)
		{
			MemberInfo[] memberInfos = property.Object.GetType().GetMember(attribute.MemberName);
			if (memberInfos.Length == 0)
			{
				// Report the missing member only when there's nothing to fall back on, matching how
				// a member that resolves to null falls through to the attribute's text below
				Watermark = attribute.Text ?? $"[Watermark member '{attribute.MemberName}' not found]";
			}
			else
			{
				Watermark = GetMemberText(GetMostDerived(memberInfos), property.Object);
			}
		}
		Watermark ??= attribute.Text;
	}

	/// <summary>
	/// Returns the member's value as the watermark text, or null when there isn't one to show
	/// </summary>
	/// <remarks>
	/// A watermark is a hint, so a member that can't be read falls back to the attribute's own text
	/// rather than failing the control it decorates. A property is read through reflection, so a
	/// throwing getter arrives wrapped and used to escape the constructor
	/// </remarks>
	internal static string? GetMemberText(MemberInfo memberInfo, object obj)
	{
		try
		{
			return memberInfo switch
			{
				PropertyInfo propertyInfo => propertyInfo.GetValue(obj)?.ToString(),
				FieldInfo fieldInfo => fieldInfo.GetValue(obj)?.ToString(),
				_ => null,
			};
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>
	/// Returns the member a subclass declares over the one it hides, matching what the compiler resolves
	/// </summary>
	/// <remarks>
	/// GetMember() resolves a hidden property to the derived one on its own, but returns both
	/// declarations of a hidden field, where the base one holds a value nothing refers to. Their
	/// order isn't guaranteed, so the declaring types are what choose between them
	/// </remarks>
	internal static MemberInfo GetMostDerived(MemberInfo[] memberInfos)
	{
		MemberInfo mostDerived = memberInfos[0];
		foreach (MemberInfo memberInfo in memberInfos)
		{
			if (mostDerived.DeclaringType!.IsAssignableFrom(memberInfo.DeclaringType))
			{
				mostDerived = memberInfo;
			}
		}
		return mostDerived;
	}

	private void SetAllowedChars(ListProperty property)
	{
		if (property.GetCustomAttribute<AllowedCharsAttribute>() is { } allowedCharsAttribute)
		{
			AllowedChars = allowedCharsAttribute.Chars;
		}
		else
		{
			AllowedChars = GetAllowedChars(property.UnderlyingType, NumberFormatInfo.CurrentInfo);
		}

		if (property.UnderlyingType == typeof(char))
		{
			MaxLength = 1;
		}
	}

	/// <summary>
	/// Returns the characters a value of <paramref name="type"/> can be typed with, or null when any can be
	/// </summary>
	/// <remarks>
	/// This allows characters rather than checking that the text parses, so the partial values typed along
	/// the way like "-" and "1." can still be entered. The separators come from <paramref name="numberFormat"/>
	/// to match the culture the binding's converter parses with. A floating point type's NaN and Infinity
	/// symbols aren't included, since their letters would allow most words
	/// </remarks>
	internal static string? GetAllowedChars(Type type, NumberFormatInfo numberFormat)
	{
		const string Digits = AllowedCharsAttribute.Digits;

		// An enum's type code is its underlying integer's, but it's entered by name
		if (type.IsEnum)
			return null;

		// Parsing accepts '-' for a culture whose negative sign is another character, like U+2212
		string negativeSign = "-" + numberFormat.NegativeSign;

		// Every integer parses a leading positive sign, the unsigned ones included
		string positiveSign = numberFormat.PositiveSign;

		// Parsing matches a plain space for a no-break space group separator, like fr-FR's U+202F,
		// which a keyboard doesn't type
		string groupSeparator = numberFormat.NumberGroupSeparator;
		if (groupSeparator is " " or " ")
		{
			groupSeparator += " ";
		}

		switch (Type.GetTypeCode(type))
		{
			case TypeCode.Byte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
				return Digits + positiveSign;

			case TypeCode.SByte:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
				return Digits + negativeSign + positiveSign;

			case TypeCode.Single:
			case TypeCode.Double:
				return Digits + negativeSign + positiveSign + "eE" +
					numberFormat.NumberDecimalSeparator + groupSeparator;

			case TypeCode.Decimal:
				return Digits + negativeSign + positiveSign +
					numberFormat.NumberDecimalSeparator + groupSeparator;
		}

		if (type == typeof(nuint))
			return Digits + positiveSign;

		if (type == typeof(nint))
			return Digits + negativeSign + positiveSign;

		return null;
	}

	/// <summary>Returns whether every character in <paramref name="text"/> is in <paramref name="allowedChars"/>, which allows any when null</summary>
	internal static bool IsAllowed(string? text, string? allowedChars)
	{
		if (text == null || allowedChars == null)
			return true;

		foreach (char c in text)
		{
			if (!allowedChars.Contains(c))
				return false;
		}
		return true;
	}

	private void BindProperty(ListProperty property)
	{
		var binding = new Binding(property.PropertyInfo.Name)
		{
			Converter = new EditValueConverter(),
			Source = property.Object,
		};
		Type type = property.UnderlyingType;
		if (property.IsEditable && (type == typeof(string) || type.IsPrimitive))
		{
			binding.Mode = BindingMode.TwoWay;
		}
		else
		{
			binding.Mode = BindingMode.OneWay;
		}
		Bind(TextBlock.TextProperty, binding);
	}

	/// <summary>Disables the pointer-over background highlight; useful for large read-only text area controls.</summary>
	public void DisableHover()
	{
		Resources.Add("TextControlBackgroundPointerOverBrush", Background);
	}

	// Move formatting to a FormattedText method/property?
	public new string? Text
	{
		get => base.Text;
		set
		{
			if (value is { } s && s.StartsWith('{') && s.Contains('\n'))
			{
				FontFamily = SideScrollTheme.MonospaceFontFamily; // Use monospaced font for Json
			}

			base.Text = value;
		}
	}

	/// <summary>Parses and pretty-prints the given JSON string into the text box.</summary>
	public void SetFormattedJson(string text)
	{
		Text = JsonUtils.Format(text);
	}

	protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
	{
		// Default validators can appear on init, and have messages that are way too long
		if (error is InvalidCastException)
		{
			base.UpdateDataValidation(property, state, new DataValidationException("Invalid format"));
		}
		else if (error == null)
		{
			base.UpdateDataValidation(property, state, error);
		}
	}

	protected override void OnTextInput(TextInputEventArgs e)
	{
		if (!IsAllowed(e.Text, AllowedChars))
		{
			e.Handled = true;
			return;
		}

		base.OnTextInput(e);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (AcceptsReturn && e.Key == Key.Enter)
		{
			if (!AcceptsPlainEnter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
			{
				// Ignore Enter but allow Shift-Enter
				return;
			}
		}

		base.OnKeyDown(e);
	}
}
