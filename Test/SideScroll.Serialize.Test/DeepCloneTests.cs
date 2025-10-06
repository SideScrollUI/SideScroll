using NUnit.Framework;

namespace SideScroll.Serialize.Test;

[Category("DeepClone")]
public class DeepCloneTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("DeepClone");
	}

	[SetUp]
	public void Setup()
	{
	}

	class StringClass
	{
		public string Value = "value";
	}

	[Test]
	public void DeepCloneStringField()
	{
		var input = new StringClass();

		var output = input.DeepClone();

		Assert.That(output.Value, Is.SameAs(input.Value));
	}
}
