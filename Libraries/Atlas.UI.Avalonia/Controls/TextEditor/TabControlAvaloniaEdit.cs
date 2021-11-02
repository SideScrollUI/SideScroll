using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using AvaloniaEdit.Highlighting;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Styling;
using Atlas.Core;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlTextEditor : AvaloniaEdit.TextEditor, IStyleable
	{
		Type IStyleable.StyleKey => typeof(AvaloniaEdit.TextEditor);

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

	public class TabControlAvaloniaEdit : Grid
	{
		public const int MaxAutoLoadSize = 1000000;

		public TabInstance TabInstance;

		public string Path;
		public ListProperty ListProperty;
		public AvaloniaEdit.TextEditor TextEditor;
		public double MaxDesiredWidth = 1000;

		public TabControlAvaloniaEdit(TabInstance tabInstance)
		{
			TabInstance = tabInstance;
			InitializeControls();
		}

		private void InitializeControls()
		{
			Background = Brushes.Transparent;
			MaxWidth = 3000;

			ColumnDefinitions = new ColumnDefinitions("*");
			RowDefinitions = new RowDefinitions("Auto, *");

			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;

			TextEditor = new TabControlTextEditor()
			{
				IsReadOnly = true,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Top,
				MaxWidth = 3000,
				MaxHeight = 2000,
				Foreground = Theme.GridForeground,
				Background = Theme.GridBackground,
				// WordWrap = true, // Doesn't work yet
				// HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, // WordWrap requires Disabled
				HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				Padding = new Thickness(6),
				FontSize = 14,
				SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JavaScript"), // handles JSON too
			};
			TextEditor.Options.AllowScrollBelowDocument = false; // Breaks top alignment
			Children.Add(TextEditor);

			//textEditor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy();
			/*ShowLineNumbers = true;
			SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");*/
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
				TextEditor.Text = new string(buffer);
			}
			else
			{
				TextEditor.Load(path);
			}
		}

		public string Text
		{
			get => TextEditor.Text;
			set
			{
				if (value is string s && s.StartsWith("{") && !s.Contains("\n"))
				{
					TextEditor.FontFamily = new FontFamily("Courier New"); // Use monospaced font for Json
				}
				TextEditor.Text = value;
			}
		}

		public void SetFormattedJson(string text)
		{
			Text = JsonUtils.Format(text);
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

		private void ListProperty_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "ValueText" && TextEditor.Text != ListProperty.ValueText.ToString())
				TextEditor.Text = ListProperty.ValueText.ToString();
		}
	}
}
