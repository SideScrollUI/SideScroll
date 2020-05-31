using Atlas.Core;
using Atlas.Extensions;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlParams : Grid
	{
		public const int ControlMaxWidth = 500;
		private TabInstance tabInstance;
		private object obj;
		private bool autoGenerateRows;

		public TabControlParams(TabInstance tabInstance, object obj, bool autoGenerateRows = true, string columnDefinitions = "Auto,*")
		{
			this.tabInstance = tabInstance;
			this.obj = obj;
			this.autoGenerateRows = autoGenerateRows;

			InitializeControls(columnDefinitions);
		}

		private void InitializeControls(string columnDefinitions)
		{
			HorizontalAlignment = HorizontalAlignment.Stretch;
			ColumnDefinitions = new ColumnDefinitions(columnDefinitions);
			Margin = new Thickness(15, 6);
			MinWidth = 100;
			MaxWidth = 2000;

			if (autoGenerateRows)
			{
				ItemCollection<ListProperty> properties = ListProperty.Create(obj);
				foreach (ListProperty property in properties)
				{
					AddPropertyRow(property);
				}
			}
		}

		public List<Control> AddObjectRow(object obj)
		{
			int rowIndex = RowDefinitions.Count;
			int columnIndex = 0;

			/*RowDefinition spacerRow = new RowDefinition();
			spacerRow.Height = new GridLength(5);
			RowDefinitions.Add(spacerRow);*/

			var rowDefinition = new RowDefinition()
			{
				Height = new GridLength(1, GridUnitType.Auto),
			};
			RowDefinitions.Add(rowDefinition);

			var controls = new List<Control>();
			foreach (PropertyInfo propertyInfo in obj.GetType().GetVisibleProperties())
			{
				var property = new ListProperty(obj, propertyInfo);
				Control control = CreatePropertyControl(property);
				if (control == null)
					continue;

				AddControl(control, columnIndex, rowIndex);
				controls.Add(control);
				columnIndex++;
			}
			return controls;
		}

		private void AddControl(Control control, int columnIndex, int rowIndex)
		{
			Grid.SetColumn(control, columnIndex);
			Grid.SetRow(control, rowIndex);
			Children.Add(control);
		}

		public Control AddPropertyRow(string propertyName)
		{
			PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
			return AddPropertyRow(new ListProperty(obj, propertyInfo));
		}

		public Control AddPropertyRow(PropertyInfo propertyInfo)
		{
			return AddPropertyRow(new ListProperty(obj, propertyInfo));
		}

		public Control AddPropertyRow(ListProperty property)
		{
			Control control = CreatePropertyControl(property);
			if (control == null)
				return null;

			int rowIndex = RowDefinitions.Count;
			{
				var spacerRow = new RowDefinition();
				spacerRow.Height = new GridLength(5);
				RowDefinitions.Add(spacerRow);
				rowIndex++;
			}
			var rowDefinition = new RowDefinition()
			{
				Height = new GridLength(1, GridUnitType.Auto),
			};
			RowDefinitions.Add(rowDefinition);

			var textLabel = new TextBlock()
			{
				Text = property.Name,
				Margin = new Thickness(0, 3, 10, 3),
				Foreground = Theme.BackgroundText,
				Background = Theme.Background,
				VerticalAlignment = VerticalAlignment.Center,
				//HorizontalAlignment = HorizontalAlignment.Stretch,
				MaxWidth = 500,
				[Grid.RowProperty] = rowIndex,
				[Grid.ColumnProperty] = 0,
			};
			Children.Add(textLabel);

			AddControl(control, 1, rowIndex);

			return control;
		}

		private Control CreatePropertyControl(ListProperty property)
		{
			Type type = property.UnderlyingType;

			BindListAttribute listAttribute = type.GetCustomAttribute<BindListAttribute>();
			listAttribute = listAttribute ?? property.propertyInfo.GetCustomAttribute<BindListAttribute>();

			Control control = null;
			if (type == typeof(bool))
			{
				control = new TabControlCheckBox(property);
			}
			else if (type.IsEnum || listAttribute != null)
			{
				control = new TabControlComboBox(property, listAttribute);
			}
			else if (typeof(DateTime).IsAssignableFrom(type))
			{
				control = new TabDateTimePicker(property);
			}
			else if (!typeof(IList).IsAssignableFrom(type))
			{
				control = new TabControlTextBox(property);
			}

			return control;
		}
	}
}

