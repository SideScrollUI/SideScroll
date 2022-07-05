using NUnit.Framework;

namespace Atlas.Core.Test;

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
		string formatted = JsonUtils.Format(input)!;

		Assert.False(formatted.Contains("u002B"));
	}
}
