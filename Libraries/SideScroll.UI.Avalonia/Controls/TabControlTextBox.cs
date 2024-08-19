using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using SideScroll.Attributes;
using SideScroll.Tabs.Lists;
using SideScroll.Utilities;
using SideScroll.UI.Avalonia.Controls.Converters;
using SideScroll.UI.Avalonia.Themes;
using SideScroll.UI.Avalonia.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SideScroll.UI.Avalonia.Controls;

public class TabControlTextBox : TextBox
{
	protected override Type StyleKeyOverride => typeof(TextBox);

	public ListProperty? Property { get; init; }

	public override string? ToString() => Property?.ToString();

	public TabControlTextBox()
	{
		InitializeComponent();
	}

	public TabControlTextBox(ListProperty property)
	{
		Property = property;

		InitializeComponent();

		InitializeProperty(property);
	}

	private void InitializeComponent()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Top;

		MinWidth = 50;
		MaxWidth = 3000;

		InitializeBorder();
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
				new Setter(TextPresenter.MarginProperty, new Thickness(6)),
			}
		};
		Styles.Add(style);

		style = new Style(x => x.OfType<TextBlock>())
		{
			Setters =
			{
				new Setter(TextBlock.MarginProperty, new Thickness(6)),
			}
		};
		Styles.Add(style);
	}

	private void InitializeProperty(ListProperty property)
	{
		IsReadOnly = !property.Editable;

		PasswordCharAttribute? passwordCharAttribute = property.GetCustomAttribute<PasswordCharAttribute>();
		if (passwordCharAttribute != null)
		{
			PasswordChar = passwordCharAttribute.Character;
		}

		SetWatermark(property);

		if (property.PropertyInfo.GetCustomAttribute<WordWrapAttribute>() != null)
		{
			TextWrapping = TextWrapping.Wrap;
		}

		AcceptsReturnAttribute? acceptsReturnAttribute = property.GetCustomAttribute<AcceptsReturnAttribute>();
		if (acceptsReturnAttribute != null)
		{
			AcceptsReturn = acceptsReturnAttribute.Allow;
		}

		MaxWidthAttribute? maxWidthAttribute = property.GetCustomAttribute<MaxWidthAttribute>();
		if (maxWidthAttribute != null)
		{
			MaxWidth = maxWidthAttribute.MaxWidth;
		}
		else
		{
			MaxWidth = TabControlParams.ControlMaxWidth;
		}

		MaxHeightAttribute? maxHeightAttribute = property.GetCustomAttribute<MaxHeightAttribute>();
		if (maxHeightAttribute != null)
		{
			MaxHeight = maxHeightAttribute.MaxHeight;
		}
		else
		{
			MaxHeight = TabControlParams.ControlMaxHeight;
		}

		if (property.GetCustomAttribute<RangeAttribute>() is RangeAttribute rangeAttribute)
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
			if (memberInfos.Length != 1)
			{
				throw new Exception($"Found {memberInfos.Length} members with name {attribute.MemberName}");
			}

			MemberInfo memberInfo = memberInfos.First();
			if (memberInfo is PropertyInfo propertyInfo)
			{
				Watermark = propertyInfo.GetValue(property.Object)?.ToString();
			}
			else if (memberInfo is FieldInfo fieldInfo)
			{
				Watermark = fieldInfo.GetValue(property.Object)?.ToString();
			}
		}
		Watermark ??= attribute.Text;
	}

	private void BindProperty(ListProperty property)
	{
		var binding = new Binding(property.PropertyInfo.Name)
		{
			Converter = new EditValueConverter(),
			//StringFormat = "Hello {0}",
			Source = property.Object,
		};
		Type type = property.UnderlyingType;
		if (type == typeof(string) || type.IsPrimitive)
		{
			binding.Mode = BindingMode.TwoWay;
		}
		else
		{
			binding.Mode = BindingMode.OneWay;
		}
		this.Bind(TextBlock.TextProperty, binding);
	}

	// Highlighting is too distracting for large controls
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
			if (value is string s && s.StartsWith('{') && s.Contains('\n'))
			{
				FontFamily = SideScrollTheme.MonospaceFontFamily; // Use monospaced font for Json
			}

			base.Text = value;
		}
	}

	public void SetFormattedJson(string text)
	{
		Text = JsonUtils.Format(text);
	}
}
