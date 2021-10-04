﻿using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Reflection;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlTextBox : TextBox, IStyleable, ILayoutable
	{
		Type IStyleable.StyleKey => typeof(TextBox);

		public TabControlTextBox()
		{
			InitializeComponent();
		}

		public TabControlTextBox(ListProperty property)
		{
			InitializeComponent();

			InitializeProperty(property);
		}

		private void InitializeComponent()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Top;

			Background = Theme.Background;

			MinWidth = 50;
			MaxWidth = 3000;

			InitializeBorder();
		}

		// Default Padding shows a gap between ScrollBar and Border
		// Workaround: Move the Padding into the inner controls Margin
		private void InitializeBorder()
		{
			Padding = new Thickness(0);

			// add back the Padding we removed to just the presenter
			var style = new Style(x => x.OfType<TextPresenter>())
			{
				Setters =
				{
					new Setter(TextPresenter.MarginProperty, new Thickness(6, 3)),
				}
			};
			Styles.Add(style);

			style = new Style(x => x.OfType<TextBlock>())
			{
				Setters =
				{
					new Setter(TextBlock.MarginProperty, new Thickness(6, 3)),
				}
			};
			Styles.Add(style);
		}

		private void InitializeProperty(ListProperty property)
		{
			IsReadOnly = !property.Editable;
			if (IsReadOnly)
				Background = Theme.TextBackgroundDisabled;

			PasswordCharAttribute passwordCharAttribute = property.PropertyInfo.GetCustomAttribute<PasswordCharAttribute>();
			if (passwordCharAttribute != null)
				PasswordChar = passwordCharAttribute.Character;

			ExampleAttribute attribute = property.PropertyInfo.GetCustomAttribute<ExampleAttribute>();
			if (attribute != null)
				Watermark = attribute.Text;

			if (property.PropertyInfo.GetCustomAttribute<WordWrapAttribute>() != null)
			{
				TextWrapping = TextWrapping.Wrap;
				AcceptsReturn = true;
				MaxHeight = 500;
			}

			BindProperty(property);

			AvaloniaUtils.AddContextMenu(this); // Custom menu to handle ReadOnly items better
		}

		private void BindProperty(ListProperty property)
		{
			var binding = new Binding(property.PropertyInfo.Name)
			{
				Converter = new EditValueConverter(),
				//StringFormat = "Hello {0}",
				Source = property.Object,
			};
			Type type = property.UnderlyingType;
			if (type == typeof(string) || type.IsPrimitive)
				binding.Mode = BindingMode.TwoWay;
			else
				binding.Mode = BindingMode.OneWay;
			this.Bind(TextBlock.TextProperty, binding);
		}

		public void DisableHover()
		{
			Resources.Add("ThemeBackgroundHoverBrush", Background); // Highlighting is too distracting for large controls
		}

		// Move formatting to a FormattedText method/property?
		public new string Text
		{
			get => base.Text;
			set
			{
				if (value is string s && s.StartsWith("{") && s.Contains("\n"))
				{
					FontFamily = new FontFamily("Courier New");
				}

				base.Text = value;
			}
		}

		public void SetFormattedJson(string text)
		{
			Text = JsonUtils.Format(text);
		}
	}
}
