using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using SideScroll.Attributes;
using SideScroll.Tabs.Lists;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace SideScroll.Avalonia.Themes;

/// <summary>Marks a theme property with one or more Avalonia resource dictionary key names used to read and write the value.
/// Initializes the attribute with the given resource key names.</summary>
[AttributeUsage(AttributeTargets.Property)]
public class ResourceKeyAttribute(params string[] names) : Attribute
{
	/// <summary>Gets the Avalonia resource dictionary key names associated with this property.</summary>
	public string[] Names => names;
}

// Todo: Add TypeRepo for Avalonia Color serialization
/// <summary>
/// Stores all user-configurable theme colors, fonts, and sizes for a named Avalonia theme variant.
/// </summary>
[PrivateData]
public class AvaloniaThemeSettings : INotifyPropertyChanged
{
	/// <summary>Gets or sets the display name of this theme preset.</summary>
	[Required, StringLength(50)]
	public string? Name { get; set; }

	/// <summary>Gets the list of available theme variant names (Light and Dark).</summary>
	public static List<string> Variants =>
	[
		"Light",
		"Dark",
	];

	/// <summary>Gets or sets the theme variant this preset targets (e.g., "Light" or "Dark").</summary>
	[ReadOnly(true)]
	public string? Variant { get; set; }

	/// <summary>Gets or sets the version of the theme format.</summary>
	[JsonIgnore, Hidden]
	public Version? Version { get; set; }

	/// <summary>Gets or sets the timestamp when this theme was last modified.</summary>
	[JsonIgnore, Hidden]
	public DateTime? ModifiedAt { get; set; }

	/// <summary>Gets or sets the font theme section.</summary>
	[Inline]
	public FontTheme Font { get; set; } = new();

	/// <summary>Gets or sets the tab theme section.</summary>
	[Inline]
	public TabTheme Tab { get; set; } = new();

	/// <summary>Gets or sets the title bar theme section.</summary>
	[Inline]
	public TitleTheme Title { get; set; } = new();

	/// <summary>Gets or sets the toolbar theme section.</summary>
	[Inline]
	public ToolbarTheme Toolbar { get; set; } = new();

	/// <summary>Gets or sets the tooltip theme section.</summary>
	[Inline]
	public ToolTipTheme ToolTip { get; set; } = new();

	/// <summary>Gets or sets the scroll bar theme section.</summary>
	[Inline]
	public ScrollBarTheme ScrollBar { get; set; } = new();

	/// <summary>Gets or sets the data grid theme section.</summary>
	[Inline]
	public DataGridTheme DataGrid { get; set; } = new();

	/// <summary>Gets or sets the button theme section.</summary>
	[Inline]
	public ButtonTheme Button { get; set; } = new();

	/// <summary>Gets or sets the text control theme section.</summary>
	[Inline]
	public TextControlTheme TextControl { get; set; } = new();

	/// <summary>Gets or sets the text area theme section.</summary>
	[Inline]
	public TextAreaTheme TextArea { get; set; } = new();

	/// <summary>Gets or sets the text editor theme section.</summary>
	[Inline]
	public TextEditorTheme TextEditor { get; set; } = new();

	/// <summary>Gets or sets the chart theme section.</summary>
	[Inline]
	public ChartTheme Chart { get; set; } = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	public override string? ToString() => Name;

	/// <summary>Returns the list of all theme section objects (font, toolbar, chart, etc.) for enumeration.</summary>
	public List<object> GetSections() =>
	[
		Font,
		Tab,
		Title,
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

	/// <summary>Returns all <see cref="ListProperty"/> entries for every property in this settings object.</summary>
	public IEnumerable<ListProperty> GetProperties() => ListProperty.Create(this);

	/// <summary>Applies changed property values from <paramref name="newSettings"/> to this instance, notifying listeners for each modified value.</summary>
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

	/// <summary>Reads all <see cref="ResourceKeyAttribute"/>-decorated properties from the current running theme and stores them on this settings object.</summary>
	public void LoadFromCurrent()
	{
		Font.FontFamily = SideScrollTheme.ContentControlThemeFontFamily.Name;
		Font.MonospaceFontFamily = SideScrollTheme.MonospaceFontFamily.Name;

		foreach (ListProperty listProperty in GetProperties())
		{
			if (listProperty.GetCustomAttribute<ResourceKeyAttribute>() is not { } attribute) continue;

			string resourceName = attribute.Names.First();
			if (listProperty.UnderlyingType == typeof(Color))
			{
				Color color = SideScrollTheme.GetBrushColor(resourceName);
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

	/// <summary>Returns <c>true</c> if any <see cref="ResourceKeyAttribute"/>-decorated property has a <c>null</c> value.</summary>
	public bool HasNullValue()
	{
		return GetProperties()
			.Any(property => property.GetCustomAttribute<ResourceKeyAttribute>() != null && property.Value == null);
	}

	/// <summary>Temporarily switches to this settings object's theme variant to read and fill any <c>null</c> <see cref="ResourceKeyAttribute"/> properties from the current theme.</summary>
	public void FillMissingValues()
	{
		var original = Application.Current!.RequestedThemeVariant;
		Application.Current.RequestedThemeVariant = GetVariant();

		foreach (ListProperty listProperty in GetProperties())
		{
			if (listProperty.GetCustomAttribute<ResourceKeyAttribute>() is not { } attribute) continue;

			if (listProperty.Value != null) continue;

			if (listProperty.UnderlyingType == typeof(Color))
			{
				Color color = SideScrollTheme.GetBrushColor(attribute.Names.First());
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

	/// <summary>Builds an Avalonia <see cref="ResourceDictionary"/> from this settings object, mapping each <see cref="ResourceKeyAttribute"/> property to its corresponding resource key(s).</summary>
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
			if (listProperty.GetCustomAttribute<ResourceKeyAttribute>() is not { } attribute) continue;

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

	/// <summary>Returns the <see cref="ThemeVariant"/> matching <see cref="Variant"/>, always using the canonical Light/Dark singletons so that multiple variants with the same name give consistent results.</summary>
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
