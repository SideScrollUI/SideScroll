using System;
using System.IO;
using System.Reflection;
using Atlas.Core;
using Atlas.GUI.Avalonia;
using Atlas.Resources;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabIcons : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private Grid grid;

			public override void Load()
			{
				grid = new Grid()
				{
					Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor),
				};
				tabModel.AddObject(grid);

				foreach (Stream stream in Assets.Streams.All)
					AddIcon(stream);
			}

			public void AddIcon(Stream stream)
			{
				var assembly = Assembly.GetExecutingAssembly();
				Bitmap bitmap;
				using (stream)
				{
					bitmap = new Bitmap(stream);
				}

				var image = new Image()
				{
					Source = bitmap,
					Width = 24,
					Height = 24,
					Margin = new Thickness(8),
					[Grid.ColumnProperty] = grid.ColumnDefinitions.Count,
				};

				grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
				grid.Children.Add(image);
			}
		}
	}
}
