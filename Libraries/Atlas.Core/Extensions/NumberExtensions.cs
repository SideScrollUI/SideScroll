namespace Atlas.Extensions;

public static class NumberExtensions
{
	public static string FormattedDecimal(this double d)
	{
		return d.ToString("#,0.#");
	}

	public static string FormattedShortDecimal(this double d)
	{
		double ad = Math.Abs(d);
		string prefix = "{0:#,0.#} ";
		if (ad >= 1E12)
		{
			return string.Format(prefix + "T", d / 1E12);
		}
		else if (ad >= 1E9)
		{
			return string.Format(prefix + "G", d / 1E9);
		}
		else if (ad >= 1E6)
		{
			return string.Format(prefix + "M", d / 1E6);
		}
		else if (ad >= 1E3)
		{
			return string.Format(prefix + "K", d / 1E3);
		}
		else
		{
			return d.Formatted()!;
		}
	}

	public static double RoundToSignificantFigures(this double num, int significantFigures)
	{
		return (double)RoundToSignificantFigures((decimal)num, significantFigures);
	}

	public static decimal RoundToSignificantFigures(this decimal num, int significantFigures)
	{
		if (num == 0) return 0;

		int d = (int)Math.Ceiling(Math.Log10((double)Math.Abs(num)));
		int power = significantFigures - d;

		decimal magnitude = (decimal)Math.Pow(10, power);

		decimal shifted = Math.Round(num * magnitude, 0, MidpointRounding.AwayFromZero);
		decimal ret = shifted / magnitude;

		return ret;
	}
}
