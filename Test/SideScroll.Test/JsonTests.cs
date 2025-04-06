using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Test;

[Category("Json")]
public class JsonTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Json");
	}

	[Test]
	public void UnencodedPlus()
	{
		string input = "{\"name\": \"+\"}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));

		Assert.That(formatted!.Contains("u002B"), Is.False);
	}
}
