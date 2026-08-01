using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Extensions;

namespace SideScroll.Tests.Extensions;

[Category("Core")]
public class InnerValueTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	public class Wrapper
	{
		[InnerValue]
		public object? Inner { get; set; }
	}

	public class SelfReferencing
	{
		[InnerValue]
		public object Inner => this;
	}

	public class InnerField
	{
		[InnerValue]
		public object? Inner;
	}

	public class NoInnerValue
	{
		public string Name { get; set; } = "test";
	}

	[Test]
	public void GetInnerValue_Unwraps()
	{
		Wrapper wrapper = new() { Inner = "value" };

		Assert.That(wrapper.GetInnerValue(), Is.EqualTo("value"));
	}

	[Test]
	public void GetInnerValue_UnwrapsNested()
	{
		Wrapper wrapper = new() { Inner = new Wrapper { Inner = "value" } };

		Assert.That(wrapper.GetInnerValue(), Is.EqualTo("value"));
	}

	[Test]
	public void GetInnerValue_UnwrapsField()
	{
		InnerField wrapper = new() { Inner = "value" };

		Assert.That(wrapper.GetInnerValue(), Is.EqualTo("value"));
	}

	[Test]
	public void GetInnerValue_NoAttributeReturnsObject()
	{
		NoInnerValue obj = new();

		Assert.That(obj.GetInnerValue(), Is.SameAs(obj));
	}

	[Test]
	public void GetInnerValue_NullInnerReturnsNull()
	{
		Wrapper wrapper = new();

		Assert.That(wrapper.GetInnerValue(), Is.Null);
	}

	[Test, Description("A self referencing [InnerValue] stops instead of overflowing the stack")]
	public void GetInnerValue_SelfReferencing()
	{
		SelfReferencing obj = new();

		Assert.That(obj.GetInnerValue(), Is.SameAs(obj));
	}

	[Test, Description("Cyclic [InnerValue] members stop instead of overflowing the stack")]
	public void GetInnerValue_Cycle()
	{
		Wrapper first = new();
		Wrapper second = new() { Inner = first };
		first.Inner = second;

		Assert.That(first.GetInnerValue(), Is.InstanceOf<Wrapper>());
	}
}
