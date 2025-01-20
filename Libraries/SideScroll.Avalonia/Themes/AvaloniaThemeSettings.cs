using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using SideScroll.Attributes;
using SideScroll.Tabs.Lists;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace SideScroll.Avalonia.Themes;

[AttributeUsage(AttributeTargets.Property)]
public class ResourceKeyAttribute(params string[] names) : Attribute
{
	public string[] Names => names;
}

// Todo: Add TypeRepo for Avalonia Color serialization
[Params, PrivateData]
public class AvaloniaThemeSettings : INotifyPropertyChanged
{
	[Required, StringLength(50)]
	public string? Name { get; set; }

	public static List<string> Variants =>
	[
		"Light",
		"Dark",
	];

	[ReadOnly(true)]
	public string? Variant { get; set; }

	[Inline]
	public FontTheme Font { get; set; } = new();
	[Inline]
	public TabTheme Tab { get; set; } = new();
	[Inline]
	public ToolbarTheme Toolbar { get; set; } = new();
	[Inline]
	public ToolTipTheme ToolTip { get; set; } = new();
	[Inline]
	public ScrollBarTheme ScrollBar { get; set; } = new();
	[Inline]
	public DataGridTheme DataGrid { get; set; } = new();
	[Inline]
	public ButtonTheme Button { get; set; } = new();
	[Inline]
	public TextControlTheme TextControl { get; set; } = new();
	[Inline]
	public TextAreaTheme TextArea { get; set; } = new();
	[Inline]
	public TextEditorTheme TextEditor { get; set; } = new();
	[Inline]
	public ChartTheme Chart { get; set; } = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	public override string? ToString() => Name;

	public List<object> GetSections() =>
	[
		Font,
		Tab,
		Toolbar,
		ToolTip,
		ScrollBar,
		DataGrid,
		Button,
		TextControl,
		TextArea,
		TextEditor,
		Chart,
	];

	public IEnumerable<ListProperty> GetProperties() => ListProperty.Create(this);

	public void Update(AvaloniaThemeSettings newSettings)
	{
		using var newProperties = newSettings.GetProperties().GetEnumerator();
		foreach (ListProperty listProperty in GetProperties())
		{
			object? existingValue = listProperty.Value;
			newProperties.MoveNext();
			object? newValue = newProperties.Current.Value;
			if (newValue?.Equals(existingValue) == true) continue;

			listProperty.Value = newValue;

			if (listProperty.Object is ThemeSection themeSection)
			{
				themeSection.UpdateProperty(listProperty.Name!);
			}
			else
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(listProperty.Name));
			}
		}
	}

	public void LoadFromCurrent()
	{
		Font.FontFamily = SideScrollTheme.ContentControlThemeFontFamily.Name;
		Font.MonospaceFontFamily = SideScrollTheme.MonospaceFontFamily.Name;

		foreach (ListProperty listProperty in GetProperties())
		{
			if (listProperty.GetCustomAttribute<ResourceKeyAttribute>() is not ResourceKeyAttribute attribute) continue;

			string resourceName = attribute.Names.First();
			if (listProperty.UnderlyingType == typeof(Color))
			{
				Color color = SideScrollTheme.GetBrush(resourceName).Color;
				listProperty.Value = color;
			}
			else if (listProperty.UnderlyingType == typeof(double))
			{
				double value = SideScrollTheme.GetDouble(resourceName);
				listProperty.Value = value;
			}
			else if (listProperty.UnderlyingType == typeof(FontWeight))
			{
				FontWeight value = SideScrollTheme.GetFontWeight(resourceName);
				listProperty.Value = value;
			}
		}
	}

	public bool HasNullValue()
	{
		return GetProperties()
			.Any(property => property.GetCustomAttribute<ResourceKeyAttribute>() != null && property.Value == null);
	}

	public void FillMissingValues()
	{
		var original = Application.Current!.RequestedThemeVariant;
		Application.Current.RequestedThemeVariant = GetVariant();

		foreach (ListProperty listProperty in GetProperties())
		{
			if (listProperty.GetCustomAttribute<ResourceKeyAttribute>() is not ResourceKeyAttribute attribute) continue;

			if (listProperty.Value != null) continue;

			if (listProperty.UnderlyingType == typeof(Color))
			{
				Color color = SideScrollTheme.GetBrush(attribute.Names.First()).Color;
				listProperty.Value = color;
			}
			else if (listProperty.UnderlyingType == typeof(double))
			{
				double value = SideScrollTheme.GetDouble(attribute.Names.First());
				listProperty.Value = value;
			}
		}
		Application.Current.RequestedThemeVariant = original;
	}

	public ResourceDictionary CreateDictionary()
	{
		var dictionary = new ResourceDictionary
		{
			["ContentControlThemeFontFamily"] = FontTheme.FontFamilies?.FirstOrDefault(f => f.Name == Font.FontFamily),
			["MonospaceFontFamily"] = FontTheme.FontFamilies?.FirstOrDefault(f => f.Name == Font.MonospaceFontFamily),

			["IconForegroundColor"] = Toolbar.IconForeground,
		};

		foreach (ListMember listProperty in GetProperties())
		{
			if (listProperty.GetCustomAttribute<ResourceKeyAttribute>() is not ResourceKeyAttribute attribute) continue;

			object? value = listProperty.Value;
			foreach (string name in attribute.Names)
			{
				if (value is Color color)
				{
					dictionary[name] = new SolidColorBrush(color);
				}
				else if (value is FontWeight fontWeight)
				{
					dictionary[name] = fontWeight;
				}
				else if (value is double d)
				{
					// Todo: Improve, Add generic attribute support to ListProperty.GetCustomAttribute()
					if (name.Contains("Thickness"))
					{
						dictionary[name] = new Thickness(d);
					}
					else if (name.Contains("CornerRadius"))
					{
						dictionary[name] = new CornerRadius(d);
					}
					else
					{
						dictionary[name] = d;
					}
				}
				else if (value is null)
				{
					Debug.WriteLine($"Property {listProperty} is null");
				}
			}
		}

		return dictionary;
	}

	// Multiple Variants with the same name will give different results, so always use the actual ones
	public ThemeVariant GetVariant()
	{
		return Variant switch
		{
			"Light" => ThemeVariant.Light,
			"Dark" => ThemeVariant.Dark,
			_ => ThemeVariant.Default
		};
	}
}
