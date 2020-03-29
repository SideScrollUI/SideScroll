using Atlas.Core;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Windows.Controls;
using System.Windows;
using Atlas.Tabs;

namespace Atlas.UI.Wpf
{
	public partial class TabNotes : UserControl
	{
		//private const int MaxAutoLoadSize = 1000000;

		//private string path;
		//private ListProperty listProperty;
		public TabInstance tabInstance;

		public TabNotes(TabInstance tabInstance)
		{
			this.tabInstance = tabInstance;

			InitializeComponent();
			labelName.Content = "Notes";
			textEditor.IsReadOnly = true;
			textEditor.Text = tabInstance.Model.Notes;

			SearchPanel.Install(this.textEditor);
		}

		/*public void Load(string path)
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
		}*/

		/*public string Text
		{
			get
			{
				return textEditor.Text;
			}
			set
			{
				textEditor.Text = value;
			}
		}*/

		/*private void ListProperty_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "ValueText" && textEditor.Text != listProperty.ValueText.ToString())
				textEditor.Text = listProperty.ValueText.ToString();
		}*/

		// I give up, let's do it the easy way
		private void textEditor_TextChanged(object sender, EventArgs e)
		{
			//if (listProperty != null)
			//	listProperty.ValueText = textEditor.Text;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			tabInstance.ParentTabInstance.tabViewSettings.NotesVisible = false;
			tabInstance.ParentTabInstance.SaveTabSettings();
			tabInstance.ParentTabInstance.Reload();
		}
	}
}
