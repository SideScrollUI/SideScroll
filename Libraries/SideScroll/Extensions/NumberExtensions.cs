namespace SideScroll.Extensions;

/// <summary>
/// Extension methods for formatting and manipulating numeric values
/// </summary>
public static class NumberExtensions
{
	/// <summary>
	/// Formats a double as a decimal number with thousand separators and one optional decimal place
	/// </summary>
	public static string FormattedDecimal(this double d)
	{
		return d.ToString("#,0.#");
	}

	/// <summary>
	/// Formats a double using short suffixes (K, M, G, T) with configurable minimum precision
	/// </summary>
	public static string FormattedShortDecimal(this double d, int minimumPrecision = 0)
	{
		double absValue = Math.Abs(d);
		string suffix;
		double scaled;

		if (absValue >= 1e12)
		{
			scaled = d / 1e12;
			suffix = " T";
		}
		else if (absValue >= 1e9)
		{
			scaled = d / 1e9;
			suffix = " G";
		}
		else if (absValue >= 1e6)
		{
			scaled = d / 1e6;
			suffix = " M";
		}
		else if (absValue >= 1e3)
		{
			scaled = d / 1e3;
			suffix = " K";
		}
		else if (minimumPrecision == 0)
		{
			return d.Formatted()!;
		}
		else
		{
			scaled = d;
			suffix = string.Empty;
		}

		string format;
		if (minimumPrecision > 0)
		{
			format = "{0:N" + minimumPrecision + "}{1}";
		}
		else
		{
			// Show no decimals unless needed
			format = scaled % 1 == 0 ? "{0:0}{1}" : "{0:0.#}{1}";
		}

		return string.Format(format, scaled, suffix);
	}

	/// <summary>
	/// Rounds a double to the specified number of significant figures
	/// </summary>
	public static double RoundToSignificantFigures(this double num, int significantFigures)
	{
		return (double)RoundToSignificantFigures((decimal)num, significantFigures);
	}

	/// <summary>
	/// Rounds a decimal to the specified number of significant figures
	/// </summary>
	public static decimal RoundToSignificantFigures(this decimal num, int significantFigures)
	{
		if (num == 0) return 0;

		int d = (int)Math.Floor(Math.Log10((double)Math.Abs(num)));
		int power = significantFigures - d - 1;

		decimal magnitude = (decimal)Math.Pow(10, power);

		decimal shifted = Math.Round(num * magnitude, 0, MidpointRounding.AwayFromZero);
		decimal ret = shifted / magnitude;

		return ret;
	}
}
