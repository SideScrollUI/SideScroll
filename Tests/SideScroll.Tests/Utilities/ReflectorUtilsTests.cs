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
		public List<string>? Items { get; init; }
		public Dictionary<string, string>? Values { get; init; }
	}

	private sealed class Child
	{
		public string? Name { get; init; }
	}

	[TestCase("Items[2]")]
	[TestCase("Items[bad]")]
	[TestCase("Items[")]
	[TestCase("Items[]")]
	[TestCase("Items[0]trailing")]
	[TestCase("Values[missing]")]
	public void FollowPropertyPath_UnresolvedIndex_ReturnsNull(string path)
	{
		var value = new Parent
		{
			Items = ["first"],
			Values = new Dictionary<string, string> { ["found"] = "value" },
		};

		Assert.DoesNotThrow(() => ReflectorUtils.FollowPropertyPath(value, path));
		Assert.That(ReflectorUtils.FollowPropertyPath(value, path), Is.Null);
	}

	[Test]
	public void FollowPropertyPath_NullIndexedProperty_ReturnsNull()
	{
		var value = new Parent();

		Assert.That(ReflectorUtils.FollowPropertyPath(value, "Items[0]"), Is.Null);
	}
}
