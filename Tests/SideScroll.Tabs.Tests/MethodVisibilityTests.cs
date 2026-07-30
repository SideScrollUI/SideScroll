using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Tabs.Lists;
using System.Reflection;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class MethodVisibilityTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	private static ListMethod GetListMethod(object obj, string methodName)
	{
		MethodInfo methodInfo = obj
			.GetType()
			.GetMethods()
			.Single(m => m.Name == methodName);

		return new ListMethod(obj, methodInfo);
	}

	public class MethodData
	{
		[Item]
		public bool? Default() => true;

		[Item, Hide(null)]
		public bool? HideNull() => null;

		[Item, Hide(null)]
		public bool? HideNullShow() => true;

		[Item, Hide(null, false)]
		public bool? HideMultiple() => false;

		[Hide(null)]
		public bool? NotAnItem() => null;
	}

	[Test]
	public void MethodRowVisibleByDefault()
	{
		ListMethod listMethod = GetListMethod(new MethodData(), nameof(MethodData.Default));
		Assert.That(listMethod.IsMethodVisible);
		Assert.That(listMethod.IsRowVisible());
	}

	[Test]
	public void MethodHideNull()
	{
		ListMethod listMethod = GetListMethod(new MethodData(), nameof(MethodData.HideNull));
		Assert.That(listMethod.IsMethodVisible);
		Assert.That(listMethod.IsRowVisible(), Is.False);
	}

	[Test]
	public void MethodHideNullShow()
	{
		ListMethod listMethod = GetListMethod(new MethodData(), nameof(MethodData.HideNullShow));
		Assert.That(listMethod.IsMethodVisible);
		Assert.That(listMethod.IsRowVisible());
	}

	[Test]
	public void MethodHideAdditionalValue()
	{
		ListMethod listMethod = GetListMethod(new MethodData(), nameof(MethodData.HideMultiple));
		Assert.That(listMethod.IsRowVisible(), Is.False);
	}

	[Test]
	public void MethodHideNullFiltersListMethodCreate()
	{
		ItemCollection<ListMethod> listMethods = ListMethod.Create(new MethodData(), true);

		Assert.That(listMethods.Select(m => m.MemberInfo.Name), Is.EqualTo(new[]
		{
			nameof(MethodData.Default),
			nameof(MethodData.HideNullShow),
		}));
	}

	[Test]
	public void MethodHideNullFiltersListMemberCreate()
	{
		ItemCollection<ListMember> listMembers = ListMember.Create(new MethodData());

		Assert.That(listMembers.Select(m => m.MemberInfo.Name), Is.EqualTo(new[]
		{
			nameof(MethodData.Default),
			nameof(MethodData.HideNullShow),
		}));
	}

	[Hide(null)]
	public class HideNullClassData
	{
		[Item]
		public bool? VisibleMethod() => true;

		[Item]
		public bool? HiddenMethod() => null;
	}

	[Test]
	public void MethodHideNullClass()
	{
		ItemCollection<ListMember> listMembers = ListMember.Create(new HideNullClassData());

		Assert.That(listMembers, Has.Exactly(1).Items);
		Assert.That(listMembers[0].MemberInfo.Name, Is.EqualTo(nameof(HideNullClassData.VisibleMethod)));
	}

	public class InvokeCountData
	{
		public int InvokeCount;

		[Item, Hide(null)]
		public string? Counted()
		{
			InvokeCount++;
			return "value";
		}
	}

	[Test]
	public void MethodHideOnlyInvokesOnce()
	{
		InvokeCountData input = new();
		ItemCollection<ListMember> listMembers = ListMember.Create(input);

		// The Counted() row plus the InvokeCount field
		Assert.That(listMembers, Has.Exactly(2).Items);

		// The visibility check caches its result, so rendering the row doesn't re-invoke the method
		Assert.That(listMembers[0].Value, Is.EqualTo("value"));
		Assert.That(input.InvokeCount, Is.EqualTo(1));
	}
}
