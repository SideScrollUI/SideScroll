using Atlas.Core;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Windows.Controls;
using System.IO;
using Atlas.Tabs;

namespace Atlas.GUI.Wpf
{
	public partial class TabAvalonEdit : UserControl
	{
		private const int MaxAutoLoadSize = 1000000;

		private string path;
		private ListProperty listProperty;

		public TabAvalonEdit(string label)
		{
			InitializeComponent();
			labelName.Content = label;
			textEditor.IsReadOnly = true;

			SearchPanel.Install(this.textEditor);
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
