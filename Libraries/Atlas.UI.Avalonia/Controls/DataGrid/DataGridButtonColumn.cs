using Atlas.UI.Avalonia.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Reflection;

namespace Atlas.UI.Avalonia
{
	public class DataGridButtonColumn : DataGridTextColumn // todo: fix type
	{
		public MethodInfo methodInfo;
		public string buttonText;

		public DataGridButtonColumn(MethodInfo methodInfo, string buttonText)
		{
			this.methodInfo = methodInfo;
			this.buttonText = buttonText;
		}

		// This doesn't get called when reusing cells
		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			//cell.Background = GetCellBrush(cell, dataItem);
			//cell.MaxHeight = 100; // don't let them have more than a few lines each

			Button button = new TabControlButton(buttonText);
			button.PointerEnter += Button_PointerEnter;
			button.PointerLeave += Button_PointerLeave;
			button.Click += Button_Click;
			return button;
		}

		private void Button_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Button button = (Button)sender;
			methodInfo.Invoke(button.DataContext, new object[] { });
		}

		private void Button_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = Theme.ButtonBackgroundHover;
			//button.BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
		}

		private void Button_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = Theme.ButtonBackground;
			//button.BorderBrush = button.Background;
		}
	}
}