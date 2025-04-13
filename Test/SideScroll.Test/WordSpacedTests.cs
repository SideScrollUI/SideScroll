using NUnit.Framework;
using SideScroll.Extensions;

namespace SideScroll.Test;

[Category("Core")]
public class WordSpacedTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("WordSpaced");
	}

	[Test, Description("WordSpaced Upper And Lower")]
	public void WordSpacedUpperAndLower()
	{
		string text = "CPUUsage".WordSpaced();

		Assert.That(text, Is.EqualTo("CPU Usage"));
	}

	[Test, Description("WordSpaced Underscores")]
	public void WordSpacedUnderscores()
	{
		string text = "Under_Scores".WordSpaced();

		Assert.That(text, Is.EqualTo("Under Scores"));
	}

	[Test, Description("WordSpaced Spaces")]
	public void WordSpacedSpaces()
	{
		string text = "Already Spaced".WordSpaced();

		Assert.That(text, Is.EqualTo("Already Spaced"));
	}

	[Test, Description("WordSpaced 1,234.56")]
	public void WordSpacedDecimals()
	{
		string input = "1,234.56";
		string text = input.WordSpaced();

		Assert.That(text, Is.EqualTo(input));
	}

	[Test, Description("WordSpaced Date Slashes")]
	public void WordSpacedDateSlashes()
	{
		string input = "1/2/2020";
		string text = input.WordSpaced();

		Assert.That(text, Is.EqualTo(input));
	}

	[Test, Description("WordSpaced Time Colons")]
	public void WordSpacedTimeColons()
	{
		string input = "01:02:03";
		string text = input.WordSpaced();

		Assert.That(text, Is.EqualTo(input));
	}

	[Test, Description("Fraction 1/2")]
	public void WordSpacedFractions()
	{
		string input = "1/2";
		string text = input.WordSpaced();

		Assert.That(text, Is.EqualTo("1 / 2"));
	}

	[Test, Description("WordSpaced 5XX")]
	public void WordSpaced5xx()
	{
		string text = "Http5XXErrors".WordSpaced();

		Assert.That(text, Is.EqualTo("Http 5XX Errors")); // 5xx would be better though?
	}

	[Test, Description("WordSpaced 5XXs")]
	public void WordSpaced5xxs()
	{
		string text = "Http5XXsErrors".WordSpaced();

		Assert.That(text, Is.EqualTo("Http 5XXs Errors"));
	}

	[Test, Description("WordSpaced 2APIs")]
	public void WordSpaced2Apis()
	{
		string text = "2APIs".WordSpaced();

		Assert.That(text, Is.EqualTo("2 APIs"));
	}

	[Test, Description("WordSpaced P99.9")]
	public void WordSpacedP99()
	{
		string input = "P99.9";
		string text = input.WordSpaced();

		Assert.That(text, Is.EqualTo(input));
	}

	[Test, Description("WordSpaced Timestamp")]
	public void WordSpacedTimestamp()
	{
		string input = "2021-11-10T18:02:23.225Z";
		string text = input.WordSpaced();

		Assert.That(text, Is.EqualTo("2021-11-10 T 18:02:23.225 Z"));
	}

	[Test, Description("WordSpaced Random AlphaNumeric Id")]
	public void WordSpacedRandomAlphaNumericId()
	{
		string input = "1ABC2DEFGHIJ";
		string text = input.WordSpaced();

		Assert.That(text, Is.EqualTo(input));
	}
}
