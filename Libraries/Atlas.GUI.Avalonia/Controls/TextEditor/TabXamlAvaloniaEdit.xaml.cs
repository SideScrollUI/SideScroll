using System;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Markup.Xaml;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;

using Avalonia.Markup.Xaml.Templates;

namespace Atlas.GUI.Avalonia.Tabs
{
	public class TabXamlAvaloniaEdit : UserControl //, IDisposable
	{

		public TabXamlAvaloniaEdit()
		{
			InitializeControls();
			//DataContext = new TestNode().Children;
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			return base.MeasureOverride(availableSize);
		}

		private void Initialize()
		{
			InitializeControls();
		}

		protected override void OnMeasureInvalidated()
		{
			base.OnMeasureInvalidated();
		}

		private void InitializeControls()
		{
			AvaloniaXamlLoader.Load(this);

			//this.Background = new SolidColorBrush(Theme.BackgroundColor);
			this.Background = new SolidColorBrush(Colors.Purple);
			//this.HorizontalAlignment = HorizontalAlignment.Stretch;
			//this.VerticalAlignment = VerticalAlignment.Stretch;
			this.Width = 1000;
			this.Height = 1000;
		}
	}
}
