using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections;

using OxyPlot.Avalonia;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabChartLegendItem : Grid
	{
		public event EventHandler<EventArgs> OnSelectionChanged;

		public OxyPlot.Series.Series series;
		//public string Label { get; set; }
		public CheckBox checkBox;
		public TextBlock textBlock;

		public TabChartLegendItem(OxyPlot.Series.Series series)
		{
			this.series = series;
			InitializeControls();
		}

		private void InitializeControls()
		{
			//this.HorizontalAlignment = HorizontalAlignment.Right;
			//this.VerticalAlignment = VerticalAlignment.Stretch;
			this.ColumnDefinitions = new ColumnDefinitions("Auto, Auto");
			this.RowDefinitions = new RowDefinitions();
			//this.Margin = new Thickness(6);

			AddCheckBox();
			AddTextBox();
		}

		private void AddCheckBox()
		{
			RowDefinitions.Add(new RowDefinition(GridLength.Auto));
			Color color = Colors.Green;
			if (series is OxyPlot.Series.LineSeries lineSeries)
				color = lineSeries.Color.ToColor();
			checkBox = new CheckBox()
			{
				Content = "",
				Foreground = Brushes.LightGray,
				Background = new SolidColorBrush(color),
				BorderThickness = new Thickness(1),
				BorderBrush = new SolidColorBrush(Colors.Black),
				Margin = new Thickness(2, 2),
				IsChecked = true,
				//VerticalAlignment = VerticalAlignment.Center,
				//HorizontalAlignment = HorizontalAlignment.Stretch,
			};
			checkBox.PointerEnter += CheckBox_PointerEnter;
			checkBox.PointerLeave += CheckBox_PointerLeave;
			checkBox.Click += CheckBox_Click;
			//checkBox.Tapped += CheckBox_Tapped; // doesn't work
			this.Children.Add(checkBox);
		}


		private void AddTextBox()
		{
			RowDefinitions.Add(new RowDefinition(GridLength.Auto));
			Color color = Colors.Green;
			if (series is OxyPlot.Series.LineSeries lineSeries)
				color = lineSeries.Color.ToColor();
			textBlock = new TextBlock()
			{
				Text = series.Title,
				Foreground = Brushes.LightGray,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
				//BorderThickness = new Thickness(1),
				//BorderBrush = new SolidColorBrush(Colors.Black),
				Margin = new Thickness(2, 2),
				//VerticalAlignment = VerticalAlignment.Center,
				//HorizontalAlignment = HorizontalAlignment.Stretch,
				[Grid.ColumnProperty] = 1,
			};
			//textBlock.PointerEnter += CheckBox_PointerEnter;
			//textBlock.PointerLeave += CheckBox_PointerLeave;
			textBlock.Tapped += TextBox_Tapped;
			//checkBox.Tapped += CheckBox_Tapped; // doesn't work
			/*Border border = new Border()
			{
				BorderThickness = new Thickness(5),
				BorderBrush = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor),
				Child = textBlock,
			};
			this.Children.Add(border);*/
			this.Children.Add(textBlock);
		}

		private void TextBox_Tapped(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
		}

		private void CheckBox_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			OnSelectionChanged?.Invoke(this, null);
		}

		private void CheckBox_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			//OnSelectionChanged?.Invoke(this, null);
		}

		private void CheckBox_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			/*CheckBox checkBox = (CheckBox)sender;
			HighlightSeries(1, 2);
			var lineSeries = (OxyPlot.Series.LineSeries)checkBox.DataContext;
			lineSeries.StrokeThickness = 2;
			lineSeries.MarkerSize = 3;
			OnSelectionChanged?.Invoke(this, null);*/
		}
	}
}
