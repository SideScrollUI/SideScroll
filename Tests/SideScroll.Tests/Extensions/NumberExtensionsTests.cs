using NUnit.Framework;
using SideScroll.Extensions;

namespace SideScroll.Tests.Extensions;

[Category("Extensions")]
public class NumberExtensionsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("NumberExtensions");
	}

	#region RoundToSignificantFigures - Double Tests

	[Test]
	public void RoundToSignificantFigures_Double_NormalPositiveNumbers()
	{
		Assert.That(1234.567.RoundToSignificantFigures(3), Is.EqualTo(1230.0).Within(0.01));
		Assert.That(1234.567.RoundToSignificantFigures(4), Is.EqualTo(1235.0).Within(0.01));
		Assert.That(1234.567.RoundToSignificantFigures(5), Is.EqualTo(1234.6).Within(0.01));
		Assert.That(0.00012345.RoundToSignificantFigures(3), Is.EqualTo(0.000123).Within(0.0000001));
		Assert.That(123.456.RoundToSignificantFigures(2), Is.EqualTo(120.0).Within(0.01));
	}

	[Test]
	public void RoundToSignificantFigures_Double_NormalNegativeNumbers()
	{
		Assert.That((-1234.567).RoundToSignificantFigures(3), Is.EqualTo(-1230.0).Within(0.01));
		Assert.That((-1234.567).RoundToSignificantFigures(4), Is.EqualTo(-1235.0).Within(0.01));
		Assert.That((-0.00012345).RoundToSignificantFigures(3), Is.EqualTo(-0.000123).Within(0.0000001));
	}

	[Test]
	public void RoundToSignificantFigures_Double_Zero()
	{
		Assert.That(0.0.RoundToSignificantFigures(3), Is.EqualTo(0.0));
		Assert.That(0.0.RoundToSignificantFigures(1), Is.EqualTo(0.0));
	}

	[Test]
	public void RoundToSignificantFigures_Double_PositiveInfinity()
	{
		double result = double.PositiveInfinity.RoundToSignificantFigures(3);
		Assert.That(double.IsPositiveInfinity(result), Is.True, "Should return positive infinity");
	}

	[Test]
	public void RoundToSignificantFigures_Double_NegativeInfinity()
	{
		double result = double.NegativeInfinity.RoundToSignificantFigures(3);
		Assert.That(double.IsNegativeInfinity(result), Is.True, "Should return negative infinity");
	}

	[Test]
	public void RoundToSignificantFigures_Double_NaN()
	{
		double result = double.NaN.RoundToSignificantFigures(3);
		Assert.That(double.IsNaN(result), Is.True, "Should return NaN");
	}

	[Test]
	public void RoundToSignificantFigures_Double_VeryLargeNumbers()
	{
		Assert.That(1.23456789e15.RoundToSignificantFigures(4), Is.EqualTo(1.235e15).Within(1e11));
		Assert.That(9.87654321e20.RoundToSignificantFigures(3), Is.EqualTo(9.88e20).Within(1e17));
	}

	[Test]
	public void RoundToSignificantFigures_Double_VerySmallNumbers()
	{
		Assert.That(1.23456e-10.RoundToSignificantFigures(3), Is.EqualTo(1.23e-10).Within(1e-13));
		Assert.That(9.87654e-15.RoundToSignificantFigures(2), Is.EqualTo(9.9e-15).Within(1e-17));
	}

	#endregion

	#region RoundToSignificantFigures - Decimal Tests

	[Test]
	public void RoundToSignificantFigures_Decimal_NormalPositiveNumbers()
	{
		Assert.That(1234.567m.RoundToSignificantFigures(3), Is.EqualTo(1230.0m));
		Assert.That(1234.567m.RoundToSignificantFigures(4), Is.EqualTo(1235.0m));
		Assert.That(1234.567m.RoundToSignificantFigures(5), Is.EqualTo(1234.6m));
		Assert.That(0.00012345m.RoundToSignificantFigures(3), Is.EqualTo(0.000123m));
		Assert.That(123.456m.RoundToSignificantFigures(2), Is.EqualTo(120.0m));
	}

	[Test]
	public void RoundToSignificantFigures_Decimal_NormalNegativeNumbers()
	{
		Assert.That((-1234.567m).RoundToSignificantFigures(3), Is.EqualTo(-1230.0m));
		Assert.That((-1234.567m).RoundToSignificantFigures(4), Is.EqualTo(-1235.0m));
		Assert.That((-0.00012345m).RoundToSignificantFigures(3), Is.EqualTo(-0.000123m));
	}

	[Test]
	public void RoundToSignificantFigures_Decimal_Zero()
	{
		Assert.That(0.0m.RoundToSignificantFigures(3), Is.EqualTo(0.0m));
		Assert.That(0.0m.RoundToSignificantFigures(1), Is.EqualTo(0.0m));
	}

	[Test]
	public void RoundToSignificantFigures_Decimal_VeryLargeNumbers()
	{
		Assert.That(123456789012345m.RoundToSignificantFigures(4), Is.EqualTo(123500000000000m));
		Assert.That(987654321098765m.RoundToSignificantFigures(3), Is.EqualTo(988000000000000m));
	}

	[Test]
	public void RoundToSignificantFigures_Decimal_VerySmallNumbers()
	{
		Assert.That(0.000000123456m.RoundToSignificantFigures(3), Is.EqualTo(0.000000123m));
		Assert.That(0.000000987654m.RoundToSignificantFigures(2), Is.EqualTo(0.00000099m));
	}

	#endregion

	#region Other Number Extension Tests

	[Test]
	public void FormattedDecimal_Test()
	{
		Assert.That(1234.5.FormattedDecimal(), Is.EqualTo("1,234.5"));
		Assert.That(1234.0.FormattedDecimal(), Is.EqualTo("1,234"));
		Assert.That(0.5.FormattedDecimal(), Is.EqualTo("0.5"));
	}

	[Test]
	public void FormattedShortDecimal_Test()
	{
		Assert.That(1234.0.FormattedShortDecimal(), Is.EqualTo("1.2 K"));
		Assert.That(1234567.0.FormattedShortDecimal(), Is.EqualTo("1.2 M"));
		Assert.That(1234567890.0.FormattedShortDecimal(), Is.EqualTo("1.2 G"));
		Assert.That(1234567890000.0.FormattedShortDecimal(), Is.EqualTo("1.2 T"));
	}

	#endregion
}
