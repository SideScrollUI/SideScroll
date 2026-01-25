using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Avalonia.Utilities;
using SideScroll.Tabs.Lists;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.DataGrids;

public class TextBlockElement : TextBlock
{
	protected override Type StyleKeyOverride => typeof(TextBlock);

	public DataGridPropertyTextColumn Column { get; }
	public PropertyInfo PropertyInfo { get; }

	public new Size DesiredSize { get; set; }

	public TextBlockElement(DataGridPropertyTextColumn column, PropertyInfo propertyInfo)
	{
		Column = column;
		PropertyInfo = propertyInfo;

		Initialize();
	}

	private void Initialize()
	{
		Margin = new Thickness(5, 5, 5, 2); // Shift down since tailing is rare

		if (!Column.FormatConverter.IsFormatted)
		{
			TextAlignment = TableUtils.GetTextAlignment(PropertyInfo.PropertyType);
		}

		if (Column.WordWrap)
		{
			TextWrapping = TextWrapping.Wrap;
		}
		else
		{
			VerticalAlignment = VerticalAlignment.Center;
		}
	}

	protected override Size MeasureCore(Size availableSize)
	{
		Size measured = base.MeasureCore(availableSize);

		// override the default DesiredSize so the desired max width is used for sizing
		// control will still fill all available space
		double maxDesiredWidth = Column.MaxDesiredWidth;
		if (Column.DisplayIndex == 1 &&
			DataContext is IMaxDesiredWidth iMaxWidth &&
			iMaxWidth.MaxDesiredWidth != null &&
			DataContext is IListPair)
		{
			maxDesiredWidth = iMaxWidth.MaxDesiredWidth.Value;
		}

		double maxDesiredHeight = Column.MaxDesiredHeight;
		if (DataContext is IMaxDesiredHeight iMaxHeight &&
			iMaxHeight.MaxDesiredHeight != null &&
			DataContext is IListItem)
		{
			maxDesiredHeight = iMaxHeight.MaxDesiredHeight.Value;
		}

		DesiredSize = measured.
			WithWidth(Math.Min(maxDesiredWidth, measured.Width)).
			WithHeight(Math.Min(maxDesiredHeight, measured.Height));

		return DesiredSize;
	}
}
