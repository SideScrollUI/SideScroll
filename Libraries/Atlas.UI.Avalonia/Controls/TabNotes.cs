using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Atlas.UI.Avalonia.View;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace Atlas.UI.Avalonia
{
	/*public class TabNotes : TabView
	{
		public TabNotes(TabInstance tabInstance) : base(tabInstance)
		{
			InitializeControls();
		}

		private void InitializeControls()
		{
			Background = new SolidColorBrush(Theme.NotesBackgroundColor);
			MaxWidth = double.MaxValue;

			//HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left;
			//HorizontalAlignment = HorizontalAlignment.Stretch;
			//VerticalAlignment = VerticalAlignment.Stretch;
			//Width = 1000;
			//Height = 500;
			//MaxWidth = 2000;

			//textEditor.TextBlock.FontSize = 30;
			//Background = Brushes.Transparent;
			/*ShowLineNumbers = true;
			SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
			TextArea.TextEntering += textEditor_TextArea_TextEntering;
			TextArea.IndentationStrategy = new Indentation.CSharp.CSharpIndentationStrategy();*//*

			TabControlAvaloniaEdit tabAvaloniaEdit = new TabControlAvaloniaEdit(tabInstance);
			tabAvaloniaEdit.textEditor.Background = new SolidColorBrush(Theme.NotesBackgroundColor);
			tabAvaloniaEdit.textEditor.Foreground = new SolidColorBrush(Theme.NotesForegroundColor);
			tabAvaloniaEdit.textEditor.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top;
			tabAvaloniaEdit.textEditor.IsReadOnly = true; // todo: allow editing?
			tabAvaloniaEdit.textEditor.Text = tabInstance.tabModel.Notes;

			tabAvaloniaEdit.MinWidth = 50;
			tabAvaloniaEdit.MaxWidth = 1500;

			this.splitControls.Background = new SolidColorBrush(Theme.NotesBackgroundColor);
			this.splitControls.AddControl(tabAvaloniaEdit, true, SeparatorType.Spacer);

			//SearchPanel.Install(this.textEditor);

			//Button button = TabButton.Create();
			var button = new Button()
			{
				Content = "^",
				Foreground = new SolidColorBrush(Theme.NotesButtonForegroundColor),
				Background = new SolidColorBrush(Theme.NotesButtonBackgroundColor),
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1),
			};
			button.Click += Button_Click;
			button.PointerEnter += Button_PointerEnter;
			button.PointerLeave += Button_PointerLeave;

			splitControls.AddControl(button, false, SeparatorType.Spacer);
		}

		private void Button_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			tabInstance.ParentTabInstance.tabViewSettings.NotesVisible = false;
			tabInstance.ParentTabInstance.SaveTabSettings();
			tabInstance.ParentTabInstance.Refresh(); // Refresh instead?
		}

		private void Button_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
			button.Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundHoverColor);
		}

		private void Button_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = new SolidColorBrush(Theme.NotesButtonBackgroundColor);
			button.BorderBrush = button.Background;
		}
	}*/
}
/*
Markdown ?
*/
