using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using Avalonia.Layout;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Styling;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlAvaloniaEdit : Grid
	{
		public const int MaxAutoLoadSize = 1000000;

		public TabInstance tabInstance;

		public string path;
		public ListProperty listProperty;
		public AvaloniaEdit.TextEditor textEditor;
		public double MaxDesiredWidth = 1000;

		public bool focusTab { get; set; } = false;

		public TabControlAvaloniaEdit(TabInstance tabInstance)
		{
			this.tabInstance = tabInstance;
			InitializeControls();
		}

		public Size arrangeOverrideFinalSize;
		protected override Size ArrangeOverride(Size finalSize)
		{
			arrangeOverrideFinalSize = finalSize;
			return base.ArrangeOverride(finalSize);
		}

		public class TabControlTextEditor : AvaloniaEdit.TextEditor, IStyleable
		{
			Type IStyleable.StyleKey => typeof(AvaloniaEdit.TextEditor);

			protected override Size MeasureOverride(Size constraint)
			{
				Size  measureOverrideSize = constraint;
				try
				{
					measureOverrideSize = base.MeasureOverride(constraint);
					measureOverrideSize = new Size(measureOverrideSize.Width, Math.Min(constraint.Height, measureOverrideSize.Height + 12)); // compensate for padding bug
				}
				catch
				{
					// catch 10k line length limit exception
				}
				//Size desiredSize = DesiredSize;
				return measureOverrideSize;
			}
		}

		private void InitializeControls()
		{
			Background = Theme.Background;
			MaxWidth = 3000;

			ColumnDefinitions = new ColumnDefinitions("*");
			RowDefinitions = new RowDefinitions("*");

			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Top;

			textEditor = new TabControlTextEditor()
			{
				IsReadOnly = true,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				MaxWidth = 3000,
				MaxHeight = 2000,
				Foreground = Theme.GridForeground,
				Background = Theme.GridBackground,
				WordWrap = true,
				//HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, // WordWrap requires Disabled
				HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, // WordWrap requires Disabled, but it doesn't work
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				Padding = new Thickness(6),
				FontSize = 14,
				SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JavaScript"), // handles JSON too
			};

			var border = new Border()
			{
				Child = textEditor,
				Background = Theme.GridBackground,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};

			Children.Add(border);

			//textEditor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy();
			/*ShowLineNumbers = true;
			SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
			TextArea.TextEntering += textEditor_TextArea_TextEntering;
			TextArea.IndentationStrategy = new Indentation.CSharp.CSharpIndentationStrategy();*/

			//SearchPanel.Install(this.textEditor);
		}

		public void Load(string path)
		{
			this.path = path;
			var fileInfo = new FileInfo(path);
			if (fileInfo.Length > MaxAutoLoadSize)
			{
				// todo: add load button to load rest of content
				using (StreamReader streamReader = File.OpenText(path))
				{
					char[] buffer = new char[MaxAutoLoadSize];
					streamReader.Read(buffer, 0, buffer.Length);
					textEditor.Text = new string(buffer);
				}
			}
			else
			{
				textEditor.Load(path);
			}
		}

		public string Text
		{
			get
			{
				return textEditor.Text;
			}
			set
			{
				if (value is string s && s.StartsWith("{") && !s.Contains("\n"))
				{
					value = GetFormattedJson(s);
				}
				textEditor.Text = value;
			}
		}

		public static string GetFormattedJson(string text)
		{
			try
			{
				dynamic parsedJson = JsonConvert.DeserializeObject(text);
				string formatted = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
				return formatted;
			}
			catch (Exception)
			{
				return text;
			}
		}

		protected bool FocusTab { get => focusTab; set => focusTab = value; }

		public void EnableEditing(ListMember listMember)
		{
			listProperty = listMember as ListProperty;
			if (listProperty != null && !listProperty.Editable)
				return;

			textEditor.IsReadOnly = false;

			if (listProperty != null)
				listProperty.PropertyChanged += ListProperty_PropertyChanged;

			/*Binding binding = new Binding
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

		private void ListProperty_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "ValueText" && textEditor.Text != listProperty.ValueText.ToString())
				textEditor.Text = listProperty.ValueText.ToString();
		}
	}
}
