using Atlas.Core;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabChartLineSeries : OxyPlot.Series.LineSeries
	{
		public TabControlChart chart;
		public ListSeries listSeries;

		// DataPoint is sealed
		public Dictionary<DataPoint, object> datapointLookup = new Dictionary<DataPoint, object>();

		private bool useDateTimeAxis;

		public TabChartLineSeries(TabControlChart chart, ListSeries listSeries, bool useDateTimeAxis)
		{
			this.chart = chart;
			this.listSeries = listSeries;
			this.useDateTimeAxis = useDateTimeAxis;
			InitializeComponent(listSeries);
		}

		private void InitializeComponent(ListSeries listSeries)
		{
			Title = listSeries.Name;
			if (Title.Length == 0)
				Title = "<NA>";
			LineStyle = LineStyle.Solid;
			StrokeThickness = 2;
			TextColor = OxyColors.Black;
			CanTrackerInterpolatePoints = false;
			MinimumSegmentLength = 2;
			MarkerSize = 3;
			MarkerType = listSeries.iList.Count < 20 ? MarkerType.Circle : MarkerType.None;
			LoadTrackFormat();

			// use circle markers if there's a single point all alone, otherwise it won't show
			if (HasSinglePoint())
				MarkerType = MarkerType.Circle;
		}

		private bool HasSinglePoint()
		{
			bool prevNan1 = false;
			bool prevNan2 = false;
			foreach (DataPoint dataPoint in Points)
			{
				bool nan = double.IsNaN(dataPoint.Y);
				if (prevNan2 && !prevNan1 && nan)
					return true;

				prevNan2 = prevNan1;
				prevNan1 = nan;
			}
			return false;
		}

		public override TrackerHitResult GetNearestPoint(ScreenPoint point, bool interpolate)
		{
			TrackerHitResult result = base.GetNearestPoint(point, interpolate);
			if (result == null)
				return null;

			if (datapointLookup.TryGetValue(result.DataPoint, out object obj))
			{
				if (DescriptionProperty is PropertyInfo propertyInfo)
				{
					object value = propertyInfo.GetValue(obj);
					if (value is string text && text.Length > 0)
						result.Text += "\n\n" + text;
				}
			}
			if (listSeries.Description != null)
				result.Text += "\n\n" + listSeries.Description;
			return result;
		}

		private PropertyInfo DescriptionProperty
		{
			get
			{
				Type type = listSeries.iList[0].GetType();
				var props = type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(DescriptionAttribute)));
				if (props.ToList().Count == 0)
					return null;
				return props.First() as PropertyInfo;
			}
		}

		private void LoadTrackFormat()
		{
			string xTrackerFormat = listSeries.xPropertyName ?? "Index: {2:#,0.###}";
			if (useDateTimeAxis || listSeries.xPropertyInfo?.PropertyType == typeof(DateTime))
				xTrackerFormat = "Time: {2:yyyy-M-d H:mm:ss.FFF}";
			TrackerFormatString = "{0}\n" + xTrackerFormat + "\nValue: {4:#,0.###}";
			/*if (listSeries.iList.Count > 0)
			{
				if (DescriptionProperty != null)
					TrackerFormatString += "\n\nDescription {" + DescriptionProperty.Name + "}";
			}*/
		}
	}
}
