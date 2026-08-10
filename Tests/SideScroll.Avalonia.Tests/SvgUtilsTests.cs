using Avalonia.Media;
using NUnit.Framework;
using SideScroll.Avalonia.Utilities;

namespace SideScroll.Avalonia.Tests;

public class SvgUtilsTests
{
	private const string Svg = """
		<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16"><rect width="16" height="16"/></svg>
		""";

	// U+200B ZERO WIDTH SPACE. Culture comparison treats it as ignorable, ordinal doesn't
	private const string ZeroWidthSpace = "​";

	private string _basePath = null!;

	[SetUp]
	public void Setup()
	{
		_basePath = Path.Combine(Path.GetTempPath(), "SvgUtilsTests", TestContext.CurrentContext.Test.MethodName!);

		if (Directory.Exists(_basePath))
		{
			Directory.Delete(_basePath, true);
		}
		Directory.CreateDirectory(_basePath);
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(_basePath))
		{
			Directory.Delete(_basePath, true);
		}
	}

	private string WriteSvg(string fileName)
	{
		string path = Path.Combine(_basePath, fileName);
		File.WriteAllText(path, Svg);
		return path;
	}

	// The extension rules are asserted against the check itself rather than through a load. Loading
	// needs the SkiaSharp native stack, and where that's unavailable every load returns false, which
	// made the rejection tests below pass without reaching the comparison they exist to cover

	[TestCase("icon.svg")]
	[TestCase("icon.SVG")]
	[TestCase("icon.Svg")]
	public void SvgExtensionIsAcceptedInAnyCasing(string fileName)
	{
		Assert.That(SvgUtils.HasSvgExtension(fileName), Is.True);
	}

	[Test, Description(
		"The gate was ToLower().EndsWith(\".svg\"), and both use the current culture, which treats a " +
		"zero width space as ignorable. A file the OS reads as some other extension loaded as SVG")]
	public void SvgExtensionRejectsAnIgnorableCharacter()
	{
		Assert.That(SvgUtils.HasSvgExtension("icon.svg" + ZeroWidthSpace), Is.False);
		Assert.That(SvgUtils.HasSvgExtension("icon.s" + ZeroWidthSpace + "vg"), Is.False);
	}

	[TestCase("icon.txt")]
	[TestCase("icon.svgz")]
	[TestCase("svg")]
	[TestCase("")]
	public void SvgExtensionRejectsAnythingElse(string fileName)
	{
		Assert.That(SvgUtils.HasSvgExtension(fileName), Is.False);
	}

	[Test, Description("Loading needs the SkiaSharp native stack, which a minimal Linux image doesn't have")]
	public void TryGetSvgImageLoadsAnSvgFile()
	{
		var call = new Call();
		if (!SvgUtils.TryGetSvgImage(call, WriteSvg("icon.svg"), out IImage? image))
		{
			Assert.Ignore("SVG loading is unavailable here: " + call.Log.EntriesText());
		}

		Assert.That(image, Is.Not.Null);
	}

	[Test, Description("Control: a different extension is still rejected, and no image is returned")]
	public void TryGetSvgImageRejectsAnotherExtension()
	{
		Assert.That(SvgUtils.TryGetSvgImage(new Call(), WriteSvg("icon.txt"), out var image), Is.False);
		Assert.That(image, Is.Null);
	}

	[Test, Description("SVG detection must not consume or rewind a caller-owned stream")]
	public void IsSvgPreservesStreamPosition()
	{
		using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<?xml version=\"1.0\"?><svg/>"));
		stream.Position = 5;

		Assert.That(SvgUtils.IsSvg(stream), Is.True);
		Assert.That(stream.Position, Is.EqualTo(5));
	}
}
