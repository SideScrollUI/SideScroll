namespace Atlas.Core;

public class ByteFormatter : ICustomFormatter
{
	static readonly string[] SizeSuffixes =
		{ "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

	public static string Format(long value, int decimalPlaces = 1)
	{
		if (value < 0)
			return "-" + Format(-value);

		int i = 0;
		decimal dValue = value;
		while (Math.Round(dValue, decimalPlaces) >= 1000)
		{
			dValue /= 1024;
			i++;
		}

		if (i == 0)
			decimalPlaces = 0; // No fractional bytes

		return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
	}

	public string Format(string? format, object? arg, IFormatProvider? formatProvider)
	{
		if (arg is long value)
			return Format(value, 1);

		return arg?.ToString() ?? "(null)";
	}
}
