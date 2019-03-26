using Atlas.GUI.Avalonia.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media;
using System.Reflection;

namespace Atlas.GUI.Avalonia
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

		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			//cell.Background = GetCellBrush(cell, dataItem);
			//cell.MaxHeight = 100; // don't let them have more than a few lines each

			Button button = TabControlButton.Create(buttonText);
			button.PointerEnter += Button_PointerEnter;
			button.PointerLeave += Button_PointerLeave;
			button.Click += delegate
			{
				dataItem.GetType().GetMethod(methodInfo.Name);
				methodInfo.Invoke(dataItem, new object[] {});
			};
			return button;
		}

		private void Button_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Button button = (Button)sender;
			//button.BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
			button.Background = new SolidColorBrush(Theme.ButtonBackgroundHoverColor);
		}

		private void Button_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = new SolidColorBrush(Theme.ButtonBackgroundColor);
			//button.BorderBrush = button.Background;
		}
	}
}