using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;
using Atlas.UI.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.IO;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabIcons : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private Grid grid;

			public override void LoadUI(Call call, TabModel model)
			{
				grid = new Grid()
				{
					Background = UI.Avalonia.Theme.Brushes.ToolbarButtonBackground,
				};
				model.AddObject(grid);

				foreach (Stream stream in Icons.Streams.All)
					AddIcon(stream);
			}

			public void AddIcon(Stream stream)
			{
				stream.Seek(0, SeekOrigin.Begin);
				var bitmap = new Bitmap(stream);

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
