using Atlas.UI.Avalonia.Controls;
using Avalonia;
using Avalonia.Controls;
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

			var button = new TabControlButton(buttonText);
			button.Click += Button_Click;
			return button;
		}

		private void Button_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Button button = (Button)sender;
			methodInfo.Invoke(button.DataContext, new object[] { });
		}
	}
}