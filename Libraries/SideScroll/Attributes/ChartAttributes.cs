namespace SideScroll.Attributes;

/// <summary>
/// Attributes that control chart and graph display behavior for data visualization in SideScroll.
/// </summary>
/// <remarks>
/// <b>Axis Control:</b> Use <see cref="XAxisAttribute"/> and <see cref="YAxisAttribute"/> to designate 
/// which properties serve as chart axes.
/// </remarks>
internal static class _DocChartSentinel { }

/// <summary>
/// Designates the property as the X-axis (horizontal) data source for charts.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Marks the property as the independent variable for chart plotting. Typically used for 
/// time series or other numeric horizontal axis data. Supports DateTime and numeric values.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class DataPoint
/// {
///     [XAxis]
///     public DateTime Timestamp { get; set; }
///     
///     [YAxis]
///     public double Value { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class XAxisAttribute : Attribute;

/// <summary>
/// Designates the property as the Y-axis (vertical) data source for charts.
/// </summary>
/// <remarks>
/// <b>Apply to:</b> Properties.
/// <para>
/// Marks the property as the dependent variable for chart plotting. Typically used for 
/// measurements, values, or other vertical axis data. Supports numeric values.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Measurement
/// {
///     [XAxis]
///     public DateTime Timestamp { get; set; }
///     
///     [YAxis, Unit("°C")]
///     public double Temperature { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class YAxisAttribute : Attribute;

/*
// todo: Need to add unit support for LiveCharts axis first
/// <summary>
/// Specifies the unit of measurement for the field or property in charts and displays.
/// </summary>
/// <param name="name">The unit name or symbol (e.g., "°C", "mph", "MB").</param>
/// <remarks>
/// <b>Apply to:</b> Fields or properties.
/// <para>
/// Provides unit information for proper chart labeling, axis formatting, and data interpretation. 
/// Units are displayed in chart legends, tooltips, and axis labels.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class PerformanceMetric
/// {
///     [XAxis]
///     public DateTime Time { get; set; }
///     
///     [YAxis]
///     [Unit("ms")]
///     public double ResponseTime { get; set; }
///     
///     [Unit("MB")]
///     public double MemoryUsage { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class UnitAttribute(string name) : Attribute
{
	/// <summary>
	/// The unit name or symbol for this measurement.
	/// </summary>
	public string Name => name;
}*/
