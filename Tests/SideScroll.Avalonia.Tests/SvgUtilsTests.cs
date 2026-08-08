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

	[TestCase("icon.svg")]
	[TestCase("icon.SVG")]
	[TestCase("icon.Svg")]
	public void TryGetSvgImageAcceptsAnySvgCasing(string fileName)
	{
		Assert.That(SvgUtils.TryGetSvgImage(new Call(), WriteSvg(fileName), out _), Is.True);
	}

	[Test, Description(
		"The gate was ToLower().EndsWith(\".svg\"), and both use the current culture, which treats a " +
		"zero width space as ignorable. A file the OS reads as some other extension loaded as SVG")]
	public void TryGetSvgImageRejectsAnIgnorableCharacterInTheExtension()
	{
		Assert.That(SvgUtils.TryGetSvgImage(new Call(), WriteSvg("icon.svg" + ZeroWidthSpace), out _), Is.False);
		Assert.That(SvgUtils.TryGetSvgImage(new Call(), WriteSvg("icon.s" + ZeroWidthSpace + "vg"), out _), Is.False);
	}

	[Test, Description("Control: a different extension is still rejected, and no image is returned")]
	public void TryGetSvgImageRejectsAnotherExtension()
	{
		Assert.That(SvgUtils.TryGetSvgImage(new Call(), WriteSvg("icon.txt"), out var image), Is.False);
		Assert.That(image, Is.Null);
	}
}
