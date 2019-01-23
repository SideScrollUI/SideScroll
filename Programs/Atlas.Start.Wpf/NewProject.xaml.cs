using Atlas.Core;
using Atlas.Tabs;
using System.Windows;
using System.Windows.Controls;

namespace Atlas.Start.Wpf
{
	public partial class NewProject : UserControl
	{
		//private App app;

		public NewProject(App app)
		{
			//this.app = app;
			InitializeComponent();
		}

		private void ButtonCreate_Click(object sender, RoutedEventArgs e)
		{
			ProjectSettings.DefaultProjectPath = textBoxProjectPath.Text;
			//this.mainWindow.Reload();
		}
	}
}
