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

namespace Atlas.GUI.Avalonia.Controls
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
			measureOverrideSize = base.MeasureOverride(constraint);
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
			//Background = new SolidColorBrush(Colors.White);
			MaxWidth = 3000;

			ColumnDefinitions = new ColumnDefinitions("Auto");
			RowDefinitions = new RowDefinitions("*");

			HorizontalAlignment = HorizontalAlignment.Left;
			//this.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.VerticalAlignment = VerticalAlignment.Stretch;

			var temp = TemplatedControl.FontFamilyProperty;
			textEditor = new AvaloniaEdit.TextEditor()
			{
				IsReadOnly = true,
				HorizontalAlignment = HorizontalAlignment.Left,
				//HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				MaxWidth = 3000,
				MaxHeight = 2000,
				Background = new SolidColorBrush(Colors.White),
				WordWrap = true,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, // WordWrap requires Disabled
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				//Text = "test",
				Margin = new Thickness(6),
				//Padding = new Thickness(6),	
				FontSize = 14,
				//[Grid.RowProperty] = 1,
			};
			textEditor.HorizontalAlignment = HorizontalAlignment.Left;
			this.Children.Add(textEditor);

			//textEditor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.
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

		// I give up, let's do it the easy way
		private void textEditor_TextChanged(object sender, EventArgs e)
		{
			if (listProperty != null)
				listProperty.ValueText = textEditor.Text;
		}
	}
}
/*
*/
