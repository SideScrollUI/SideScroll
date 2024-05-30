using Atlas.Core.Utilities;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;

namespace Atlas.UI.Avalonia.Controls;

public class TabControlTextEditor : AvaloniaEdit.TextEditor
{
	protected override Type StyleKeyOverride => typeof(AvaloniaEdit.TextEditor);

	protected override Size MeasureOverride(Size constraint)
	{
		Size measureOverrideSize = constraint;
		try
		{
			measureOverrideSize = base.MeasureOverride(constraint);
		}
		catch
		{
			// catch 10k line length limit exception
		}
		//Size desiredSize = DesiredSize;
		return measureOverrideSize;
	}
}

public enum TextType
{
	Default,
	Json,
	Xml,
}

public class TabControlAvaloniaEdit : Grid
{
	public const int MaxAutoLoadSize = 1_000_000;

	public TabInstance TabInstance;

	public string? Path;
	public ListProperty? ListProperty;
	public AvaloniaEdit.TextEditor TextEditor;

	public TextType TextType { get; set; }

	public TabControlAvaloniaEdit(TabInstance tabInstance)
	{
		TabInstance = tabInstance;

		Background = AtlasTheme.TextEditorBackgroundBrush;

		MinWidth = 50; // WordWrap causes freezing below certain values
		MaxWidth = 3000;

		ColumnDefinitions = new ColumnDefinitions("*");
		RowDefinitions = new RowDefinitions("*");

		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;

		TextEditor = new TabControlTextEditor
		{
			IsReadOnly = true,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Top,
			MaxWidth = 3000,
			MaxHeight = 2000,
			Background = Brushes.Transparent,
			Foreground = AtlasTheme.TextEditorForegroundBrush,
			WordWrap = true,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, // WordWrap requires Disabled
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
			Padding = new Thickness(6),
			FontSize = 14,
		};
		TextEditor.Options.AllowScrollBelowDocument = false; // Breaks top alignment
		Children.Add(TextEditor);

		ActualThemeVariantChanged += TabControlAvaloniaEdit_ActualThemeVariantChanged;

		//textEditor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy();
		/*ShowLineNumbers = true;
		SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");*/
	}

	private void TabControlAvaloniaEdit_ActualThemeVariantChanged(object? sender, EventArgs e)
	{
		UpdateTheme();
	}

	public void Load(string path)
	{
		Path = path;

		var fileInfo = new FileInfo(path);
		if (fileInfo.Length > MaxAutoLoadSize)
		{
			// todo: add load button to load rest of content
			using StreamReader streamReader = File.OpenText(path);

			var buffer = new char[MaxAutoLoadSize];
			streamReader.Read(buffer, 0, buffer.Length);
			Text = new string(buffer);
		}
		else
		{
			Text = File.ReadAllText(path);
			//TextEditor.Load(path); // Doesn't work
		}

		UpdateLineNumbers();
	}

	public string Text
	{
		get => TextEditor.Text;
		set
		{
			TextEditor.Text = value;
			UpdateTheme();
			// UpdateLineNumbers(); // Enable for all?
		}
	}

	private void UpdateTheme()
	{
		Background = AtlasTheme.TextEditorBackgroundBrush;
		TextEditor.TextArea.Foreground = AtlasTheme.TextEditorForegroundBrush;
		TextEditor.TextArea.TextView.LinkTextForegroundBrush = AtlasTheme.LinkTextForegroundBrush;

		if (TextType == TextType.Json)
		{
			EnableMonospace();
			EnableJsonSyntaxHighlighting();
		}
		else if (TextType == TextType.Xml)
		{
			EnableMonospace();
			EnableXmlSyntaxHighlighting();
		}
	}

	public void EnableMonospace()
	{
		if (AtlasTheme.MonospaceFontFamily is FontFamily fontFamily)
		{
			TextEditor.FontFamily = fontFamily;
		}
	}

	private void EnableJsonSyntaxHighlighting()
	{
		TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Json");

		SetHighlightColor("Bool", AtlasTheme.JsonHighlightBoolBrush.Color);
		SetHighlightColor("Number", AtlasTheme.JsonHighlightNumberBrush.Color);
		SetHighlightColor("String", AtlasTheme.JsonHighlightStringBrush.Color);
		SetHighlightColor("Null", AtlasTheme.JsonHighlightNullBrush.Color);
		SetHighlightColor("FieldName", AtlasTheme.JsonHighlightFieldNameBrush.Color);
		SetHighlightColor("Punctuation", AtlasTheme.JsonHighlightPunctuationBrush.Color);
	}

	private void EnableXmlSyntaxHighlighting()
	{
		TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");

		SetHighlightColor("Comment", AtlasTheme.XmlHighlightCommentBrush.Color);
		SetHighlightColor("CData", AtlasTheme.XmlHighlightCDataBrush.Color);
		SetHighlightColor("DocType", AtlasTheme.XmlHighlightDocTypeBrush.Color);
		SetHighlightColor("XmlDeclaration", AtlasTheme.XmlHighlightDeclarationBrush.Color);
		SetHighlightColor("XmlTag", AtlasTheme.XmlHighlightTagBrush.Color);
		SetHighlightColor("AttributeName", AtlasTheme.XmlHighlightAttributeNameBrush.Color);
		SetHighlightColor("AttributeValue", AtlasTheme.XmlHighlightAttributeValueBrush.Color);
		SetHighlightColor("Entity", AtlasTheme.XmlHighlightEntityBrush.Color);
		SetHighlightColor("BrokenEntity", AtlasTheme.XmlHighlightBrokenEntityBrush.Color);
	}

	private void SetHighlightColor(string name, Color color)
	{
		var highlightColor = TextEditor.SyntaxHighlighting.GetNamedColor(name);
		highlightColor.Foreground = new SimpleHighlightingBrush(color);
	}

	public void SetFormatted(string text)
	{
		try
		{
			if (JsonUtils.TryFormat(text, out string? json))
			{
				Text = json;
				TextType = TextType.Json;
			}
			else if (XmlUtils.TryFormat(text, out string? formatted) || text.StartsWith("<?xml"))
			{
				Text = formatted ?? text;
				TextType = TextType.Xml;
			}
			else
			{
				Text = text;
			}
		}
		catch (Exception)
		{
			Text = text;
		}
	}

	private void UpdateLineNumbers()
	{
		if (TextEditor.LineCount <= 1)
			return;
		
		TextEditor.ShowLineNumbers = true;

		foreach (Control control in TextEditor.TextArea.LeftMargins)
		{
			if (control is LineNumberMargin margin)
			{
				margin.Opacity = 0.5;
			}
		}
	}

	public void EnableEditing(ListMember listMember)
	{
		ListProperty = listMember as ListProperty;
		if (ListProperty != null && !ListProperty.Editable)
			return;

		TextEditor.IsReadOnly = false;

		if (ListProperty != null)
			ListProperty.PropertyChanged += ListProperty_PropertyChanged;

		/*var binding = new Binding
		{
			Path = new PropertyPath("BindableValueText"),
			Mode = BindingMode.TwoWay,
			UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
			NotifyOnTargetUpdated = true,
			NotifyOnSourceUpdated = true,
			//BindsDirectlyToSource = true
		};

		binding.Source = listMember;
		textEditor.SetBinding(BindableTextEditor.TextProperty, binding);
		labelUrl.SetBinding(TextBox.TextProperty, binding);*/
	}

	private void ListProperty_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ListProperty.ValueText) &&
			ListProperty?.ValueText?.ToString() is string value &&
			value != TextEditor.Text)
		{
			TextEditor.Text = value;
		}
	}
}
