using NUnit.Framework;
using SideScroll.Extensions;

namespace SideScroll.Tests.Extensions;

/// <summary>
/// Clamping a saved window size to the space measured for it, where the maximum can fall below
/// the minimum through ordinary values
/// </summary>
public class ClampedTests
{
	[Test, Description("Math.Clamp() throws on an inverted range, which restored window settings reached")]
	public void InvertedRangeReturnsTheMinimum()
	{
		Assert.That(() => Math.Clamp(1280.0, 700.0, 0.0), Throws.ArgumentException);

		Assert.That(1280.0.Clamped(700.0, 0.0), Is.EqualTo(700.0));
	}

	[Test, Description("A display narrower than the window's minimum width")]
	public void MaximumBelowTheMinimumReturnsTheMinimum()
	{
		Assert.That(1280.0.Clamped(700.0, 640.0), Is.EqualTo(700.0));
	}

	[Test, Description("The size keeping its minimum leaves a negative maximum for the position")]
	public void NegativeMaximumReturnsTheMinimum()
	{
		// maxBounds.Width - Width + minLeft, with Width held at MinWidth above the measured space
		Assert.That(0.0.Clamped(0.0, -700.0), Is.EqualTo(0.0));
		Assert.That((-50.0).Clamped(-7.0, -707.0), Is.EqualTo(-7.0));
	}

	[Test, Description("An ordinary range still clamps both ways")]
	public void ValueWithinTheRangeIsUnchanged()
	{
		Assert.That(1280.0.Clamped(700.0, 1920.0), Is.EqualTo(1280.0));
		Assert.That(400.0.Clamped(700.0, 1920.0), Is.EqualTo(700.0));
		Assert.That(2400.0.Clamped(700.0, 1920.0), Is.EqualTo(1920.0));
	}

	[Test, Description("A range of one value is not inverted")]
	public void EqualBoundsReturnThatValue()
	{
		Assert.That(1280.0.Clamped(700.0, 700.0), Is.EqualTo(700.0));
	}

	[Test, Description("Negative positions are ordinary, a window can sit left of the primary screen")]
	public void NegativeRangeClampsWithinItself()
	{
		Assert.That((-50.0).Clamped(-7.0, 1200.0), Is.EqualTo(-7.0));
		Assert.That((-3.0).Clamped(-7.0, 1200.0), Is.EqualTo(-3.0));
	}
}
