using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Test;

[Category("Json")]
public class TestJson : TestBase
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
		Assert.True(JsonUtils.TryFormat(input, out string? formatted));

		Assert.False(formatted!.Contains("u002B"));
	}
}
