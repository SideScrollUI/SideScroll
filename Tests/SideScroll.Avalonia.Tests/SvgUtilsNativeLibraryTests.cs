using NUnit.Framework;
using SideScroll.Avalonia.Utilities;

namespace SideScroll.Avalonia.Tests;

/// <summary>
/// Covers which failures loading an SVG icon are treated as the environment rather than a bug
/// </summary>
/// <remarks>
/// <see cref="SvgUtils.TryGetSvgColorImage"/> asserts on a failure so a malformed resource is
/// caught while developing, and a button renders without its icon either way. Svg.Skia rasterizes
/// through SkiaSharp whatever Avalonia is drawing with, so a container without libfontconfig fails
/// every icon, and asserting there turned a missing library into a test failure
/// </remarks>
public class SvgUtilsNativeLibraryTests
{
	[Test, Description("The first access wraps it, which is the shape the failure actually arrives in")]
	public void ADllNotFoundInsideATypeInitializerIsRecognized()
	{
		Exception exception = new TypeInitializationException(
			"SkiaSharp.SKImageInfo",
			new DllNotFoundException("Unable to load shared library 'libSkiaSharp'"));

		Assert.That(SvgUtils.IsMissingNativeLibrary(exception), Is.True);
	}

	[Test, Description(
		"Svg.Skia's own type initializer fails on top of SkiaSharp's, so the missing library sits " +
		"two levels down rather than one")]
	public void ADllNotFoundNestedTwoLevelsDeepIsRecognized()
	{
		Exception exception = new TypeInitializationException(
			"Avalonia.Svg.Skia.SvgSource",
			new TypeInitializationException(
				"SkiaSharp.SKImageInfo",
				new DllNotFoundException("Unable to load shared library 'libSkiaSharp'")));

		Assert.That(SvgUtils.IsMissingNativeLibrary(exception), Is.True);
	}

	[Test]
	public void ADllNotFoundOnItsOwnIsRecognized()
	{
		Assert.That(SvgUtils.IsMissingNativeLibrary(new DllNotFoundException()), Is.True);
	}

	[Test, Description("A malformed resource is still asserted on, which is what the assert is for")]
	public void AnOrdinaryFailureIsNotTreatedAsAMissingLibrary()
	{
		Assert.That(SvgUtils.IsMissingNativeLibrary(new InvalidDataException("bad svg")), Is.False);
	}

	[Test, Description("A wrapper whose chain holds no missing library is still an ordinary failure")]
	public void AWrappedOrdinaryFailureIsNotTreatedAsAMissingLibrary()
	{
		Exception exception = new TypeInitializationException(
			"Avalonia.Svg.Skia.SvgSource",
			new InvalidOperationException("bad svg"));

		Assert.That(SvgUtils.IsMissingNativeLibrary(exception), Is.False);
	}

	[Test]
	public void NoExceptionIsNotAMissingLibrary()
	{
		Assert.That(SvgUtils.IsMissingNativeLibrary(null), Is.False);
	}
}
