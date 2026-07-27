using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Tests.Utilities;

[Category("Core")]
public class ReflectorUtilsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ReflectorUtils");
	}

	[Test]
	public void FollowPropertyPath_MissingRootProperty_ReturnsNull()
	{
		var value = new Parent { Child = new Child { Name = "Found" } };

		object? result = ReflectorUtils.FollowPropertyPath(value, "Missing");

		Assert.That(result, Is.Null);
	}

	[Test]
	public void FollowPropertyPath_MissingNestedProperty_ReturnsNull()
	{
		var value = new Parent { Child = new Child { Name = "Found" } };

		object? result = ReflectorUtils.FollowPropertyPath(value, "Child.Missing");

		Assert.That(result, Is.Null);
	}

	[Test]
	public void FollowPropertyPath_ExistingNestedProperty_ReturnsValue()
	{
		var value = new Parent { Child = new Child { Name = "Found" } };

		object? result = ReflectorUtils.FollowPropertyPath(value, "Child.Name");

		Assert.That(result, Is.EqualTo("Found"));
	}

	private sealed class Parent
	{
		public Child? Child { get; init; }
	}

	private sealed class Child
	{
		public string? Name { get; init; }
	}
}
