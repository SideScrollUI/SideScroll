using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Atlas.UI.Avalonia.Controls
{
	public class ToolbarButton2 : Button
	{
		public ToolbarButton2()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override void OnPointerEnter(PointerEventArgs e)
		{
			base.OnPointerEnter(e);
		}
	}
}
