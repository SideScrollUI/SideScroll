using Atlas.Tabs;
using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

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

		public Size measureOverrideSize;
		protected override Size MeasureOverride(Size constraint)
		{
			try
			{
				measureOverrideSize = base.MeasureOverride(constraint);
			}
			catch
			{
				// catch 10k line length limit exception
			}
			Size desiredSize = DesiredSize;
			return measureOverrideSize;
		}

		public Size arrangeOverrideFinalSize;
		protected override Size ArrangeOverride(Size finalSize)
		{
			arrangeOverrideFinalSize = finalSize;
			return base.ArrangeOverride(finalSize);
		}

		private void InitializeControls()
		{
			var theme = new global::Avalonia.Themes.Default.DefaultTheme();
			theme.FindResource("Button");
			Background = new SolidColorBrush(Theme.GridBackgroundColor);
			MaxWidth = 3000;

			ColumnDefinitions = new ColumnDefinitions("*");
			RowDefinitions = new RowDefinitions("*");

			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;

			var temp = TemplatedControl.FontFamilyProperty;
			textEditor = new AvaloniaEdit.TextEditor()
			{
				IsReadOnly = true,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				MaxWidth = 3000,
				MaxHeight = 2000,
				Foreground = new SolidColorBrush(Theme.GridForegroundColor),
				Background = new SolidColorBrush(Theme.GridBackgroundColor),
				WordWrap = true,
				//HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, // WordWrap requires Disabled
				HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, // WordWrap requires Disabled, but it doesn't work
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				//Margin = new Thickness(6),
				//Padding = new Thickness(6), // doesn't work well with scroll bars
				FontSize = 14,
				SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JavaScript"), // handles JSON too
			};

			Border border = new Border()
			{
				BorderThickness = new Thickness(6, 6, 0, 6),
				BorderBrush = new SolidColorBrush(Theme.GridBackgroundColor),
				Child = textEditor,
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
			FileInfo fileInfo = new FileInfo(path);
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
				textEditor.Text = value;
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
