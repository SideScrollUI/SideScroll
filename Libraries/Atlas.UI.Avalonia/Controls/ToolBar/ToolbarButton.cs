using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace Atlas.UI.Avalonia.Controls
{
	public class ToolbarButton : Button
	{
		//public static readonly StyledProperty<int> IntervalProperty =
		//	AvaloniaProperty.Register<Button, int>(nameof(Interval), 100);

		protected override void OnPointerEnter(PointerEventArgs e)
		{
			base.OnPointerEnter(e);
		}
	}
}