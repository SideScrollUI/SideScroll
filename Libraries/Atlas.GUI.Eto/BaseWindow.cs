using System;
using Eto.Forms;
using Eto.Drawing;
using Atlas.Tabs;

namespace Atlas.GUI.Eto
{
	public class BaseWindow : Form
	{
		private const int MinWindowSize = 100;

		public Project project;
		protected TabView tabView;

		public BaseWindow(Project project)
		{
			this.project = project;

			this.Shown += MainForm_Shown; // todo: causes exception in GTK if we don't create beforehand
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			this.Shown -= MainForm_Shown;

			//Title = "Atlas.Start.Eto";
			// SynchronizationContext.Current isn't valid until form is shown, and it gets attached to the Log
			//Content = CreateView();
			InitializeControl();
		}

		public void InitializeControl()
		{
			LoadWindowSettings();

			Title = project.projectSettings.Name ?? "<Atlas.GUI.Eto>";

			ClientSize = new Size(640, 480);
			//Maximize();


			// create a few commands that can be used for the menu and toolbar
			/*var clickMe = new Command { MenuText = "Click Me!", ToolBarText = "Click Me!" };
			clickMe.Executed += (sender, e) => MessageBox.Show(this, "I was clicked!");

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += (sender, e) => MessageBox.Show(this, "About my app...");

			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					// File submenu
					new ButtonMenuItem { Text = "&File", Items = { clickMe } },
					// new ButtonMenuItem { Text = "&Edit", Items = { commands/items } },
					// new ButtonMenuItem { Text = "&View", Items = { commands/items } },
				},
				ApplicationItems =
				{
					// application (OS X) or file menu (others)
					new ButtonMenuItem { Text = "&Preferences..." },
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};

			// create toolbar			
			ToolBar = new ToolBar { Items = { clickMe } };*/

			Title = project.projectSettings.Name ?? "<Name>";
		}

		protected WindowSettings WindowSettings
		{
			get
			{
				WindowSettings windowSettings = new WindowSettings()
				{
					Maximized = (this.WindowState == WindowState.Maximized),
					Width = this.Width,
					Height = this.Height,
					Left = this.Location.X,
					Top = this.Location.Y,
				};
				return windowSettings;
			}
			set
			{
				double left = Math.Max(0, value.Left);
				double top = Math.Max(0, value.Top);

				this.Location = new Point((int)left, (int)top);
				this.Width = (int)Math.Max(MinWindowSize, value.Width);
				this.Height = (int)Math.Max(MinWindowSize, value.Height);
				this.WindowState = value.Maximized ? WindowState.Maximized : WindowState.Normal;
			}
		}

		private void LoadWindowSettings()
		{
			WindowSettings windowSettings = project.DataApp.Load<WindowSettings>();
			if (windowSettings == null)
				return;

			this.WindowSettings = windowSettings;

			this.SizeChanged += MainForm_SizeChanged;
			this.LocationChanged += MainForm_LocationChanged;
		}

		private void SaveWindowSettings()
		{
			project.DataApp.Save(this.WindowSettings);
		}

		private void MainForm_SizeChanged(object sender, EventArgs e)
		{
			SaveWindowSettings();
		}

		private void MainForm_LocationChanged(object sender, EventArgs e)
		{
			SaveWindowSettings();
		}
	}
}
