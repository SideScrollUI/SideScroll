namespace SideScroll.Utilities;

/// <summary>
/// Provides formatting for byte values into human-readable size representations (KB, MB, GB, etc.)
/// </summary>
public class ByteFormatter : ICustomFormatter
{
	/// <summary>
	/// Gets or sets the array of size suffixes used for formatting byte values
	/// </summary>
	public static string[] SizeSuffixes { get; set; } =
		["bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

	/// <summary>
	/// Formats a byte value into a human-readable string with the appropriate size suffix
	/// </summary>
	/// <param name="value">The byte value to format</param>
	/// <param name="decimalPlaces">The number of decimal places to include in the result (default is 1)</param>
	/// <returns>A formatted string representing the byte value with appropriate size suffix</returns>
	public static string Format(long value, int decimalPlaces = 1)
	{
		// Take the magnitude as a decimal, negating long.MinValue as a long overflows back onto itself
		string sign = value < 0 ? "-" : "";
		decimal dValue = Math.Abs((decimal)value);

		int i = 0;
		while (Math.Round(dValue, decimalPlaces) >= 1024)
		{
			dValue /= 1024;
			i++;
		}

		if (i == 0)
		{
			decimalPlaces = 0; // No fractional bytes
		}

		return sign + string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
	}

	/// <summary>
	/// Formats an object using the ICustomFormatter interface
	/// </summary>
	/// <param name="format">The format string (not used)</param>
	/// <param name="arg">The object to format (expected to be a long value)</param>
	/// <param name="formatProvider">The format provider (not used)</param>
	/// <returns>A formatted string representation of the argument</returns>
	public string Format(string? format, object? arg, IFormatProvider? formatProvider)
	{
		if (arg is long value)
		{
			return Format(value);
		}

		return arg?.ToString() ?? "(null)";
	}
}
