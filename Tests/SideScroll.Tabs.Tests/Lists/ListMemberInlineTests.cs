using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class ListMemberInlineTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Tabs");
	}

	public class Inner
	{
		public string Name { get; set; } = "inner";
	}

	public class Wrapper
	{
		[Inline]
		public Inner Value { get; set; } = new();
	}

	// An [Inline] member that returns another instance of the same type never stops expanding
	public class SelfInlining
	{
		public string Name { get; set; } = "self";

		[Inline]
		public SelfInlining Inlined => new();
	}

	[Test]
	public void InlineExpandsInnerMembers()
	{
		ItemCollection<ListMember> members = ListMember.Create(new Wrapper(), false);

		Assert.That(members.Select(m => m.Name), Does.Contain("Name"));
	}

	[Test, Description("A self referencing [Inline] stops instead of overflowing the stack")]
	public void InlineStopsAtMaxDepth()
	{
		ItemCollection<ListMember> members = ListMember.Create(new SelfInlining(), false);

		Assert.That(members, Is.Not.Empty);
	}

	[Test, Description("ListProperty expands and limits [Inline] the same way")]
	public void ListPropertyInlineStopsAtMaxDepth()
	{
		ItemCollection<ListProperty> properties = ListProperty.Create(new SelfInlining(), false);

		Assert.That(properties, Is.Not.Empty);
	}

	[Test]
	public void ListPropertyInlineExpandsInnerMembers()
	{
		ItemCollection<ListProperty> properties = ListProperty.Create(new Wrapper(), false);

		Assert.That(properties.Select(p => p.Name), Does.Contain("Name"));
	}
}
