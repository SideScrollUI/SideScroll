using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Tests.Utilities;

[Category("Core")]
public class XmlUtilsTests
{
	[Test, Description("Formatting a complete XML document preserves its declaration")]
	public void TryFormatPreservesXmlDeclaration()
	{
		const string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><root><child /></root>";

		Assert.That(XmlUtils.TryFormat(xml, out string? formatted), Is.True);
		Assert.That(formatted, Does.StartWith("<?xml"));
		Assert.That(formatted, Does.Contain("<root>"));
	}
}
