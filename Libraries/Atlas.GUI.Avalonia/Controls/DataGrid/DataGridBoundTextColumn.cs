using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace Atlas.GUI.Avalonia
{
	public class DataGridBoundTextColumn : DataGridTextColumn
	{
		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			//cell.Background = GetCellBrush(cell, dataItem);
			cell.MaxHeight = 100; // don't let them have more than a few lines each

			TextBlock textBlock = (TextBlock)base.GenerateElement(cell, dataItem);
			textBlock.DoubleTapped += delegate
			{
				((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard)))
				.SetTextAsync(textBlock.Text);
			};
			return textBlock;
		}
	}
}