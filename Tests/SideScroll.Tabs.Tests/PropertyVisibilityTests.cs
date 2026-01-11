using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using System.Reflection;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class PropertyVisibilityTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	private static PropertyInfo GetPropertyInfo(object obj, string propertyName)
	{
		return obj
			.GetType()
			.GetProperties()
			.Single(p => p.Name == propertyName);
	}

	private static ListProperty GetListProperty(object obj, string propertyName)
	{
		PropertyInfo propertyInfo = GetPropertyInfo(obj, propertyName);
		return new ListProperty(obj, propertyInfo);
	}

	public bool Default { get; set; }

	[Test]
	public void PropertyRowVisibleByDefault()
	{
		PropertyInfo propertyInfo = GetPropertyInfo(this, nameof(Default));
		Assert.That(propertyInfo.IsRowVisible());
	}

	[Hidden]
	public bool Hidden { get; set; }

	[Test]
	public void PropertyHidden()
	{
		PropertyInfo propertyInfo = GetPropertyInfo(this, nameof(Hidden));
		Assert.That(propertyInfo.IsRowVisible(), Is.False);
	}

	[HiddenRow]
	public bool HiddenRow { get; set; }

	[Test]
	public void PropertyHiddenRow()
	{
		PropertyInfo propertyInfo = GetPropertyInfo(this, nameof(HiddenRow));
		Assert.That(propertyInfo.IsRowVisible(), Is.False);
	}

	[HiddenColumn]
	public bool HiddenColumn { get; set; }

	[Test]
	public void PropertyHiddenColumn()
	{
		PropertyInfo propertyInfo = GetPropertyInfo(this, nameof(HiddenColumn));
		Assert.That(propertyInfo.IsColumnVisible(), Is.False);
	}

	[Hide(null)]
	public bool? HideNull { get; set; }

	[Test]
	public void PropertyHideNull()
	{
		ListProperty listProperty = GetListProperty(this, nameof(HideNull));
		Assert.That(listProperty.IsPropertyVisible);
		Assert.That(listProperty.IsRowVisible(), Is.False);
	}

	[Hide(null)]
	public bool? HideNullShow { get; set; } = true;

	[Test]
	public void PropertyHideNullShow()
	{
		ListProperty listProperty = GetListProperty(this, nameof(HideNullShow));
		Assert.That(listProperty.IsPropertyVisible);
		Assert.That(listProperty.IsRowVisible());
	}

	[HideRow(null)]
	public bool? HideRowNull { get; set; }

	[Test]
	public void PropertyHideRowNull()
	{
		ListProperty listProperty = GetListProperty(this, nameof(HideRowNull));
		Assert.That(listProperty.IsPropertyVisible);
		Assert.That(listProperty.IsRowVisible(), Is.False);
	}

	[HideRow(null)]
	public bool? HideRowNullShow { get; set; } = true;

	[Test]
	public void PropertyHideRowNullShow()
	{
		ListProperty listProperty = GetListProperty(this, nameof(HideRowNullShow));
		Assert.That(listProperty.IsPropertyVisible);
		Assert.That(listProperty.IsRowVisible());
	}

	[HideColumn(null)]
	public bool? HideColumnNull { get; set; }

	[Test]
	public void PropertyHideColumnNull()
	{
		ListProperty listProperty = GetListProperty(this, nameof(HideColumnNull));
		Assert.That(listProperty.IsPropertyVisible);
		Assert.That(listProperty.IsColumnVisible(), Is.False);
	}

	[HideColumn(null)]
	public bool? HideColumnNullShow { get; set; } = true;

	[Test]
	public void PropertyHideColumnNullShow()
	{
		ListProperty listProperty = GetListProperty(this, nameof(HideColumnNullShow));
		Assert.That(listProperty.IsPropertyVisible);
		Assert.That(listProperty.IsColumnVisible());
	}

	[Hide(null)]
	public class HideNullClass
	{
		public bool? VisibleProperty { get; set; } = true;
		public bool? HiddenProperty { get; set; }
	}

	[Test]
	public void PropertyHideNullClass()
	{
		HideNullClass input = new();
		ItemCollection<ListMember> listMembers = ListMember.Create(input);

		Assert.That(listMembers, Has.Exactly(1).Items);

		Assert.That(listMembers[0].MemberInfo.Name, Is.EqualTo(nameof(HideNullClass.VisibleProperty)));
	}

	public class InlineClass
	{
		[Inline]
		public InlineData Data { get; set; } = new();
	}

	public class InlineData
	{
		public bool InlineProperty { get; set; } = true;
		public bool InlineField = true;
	}

	[Test]
	public void PropertyInlineClass()
	{
		InlineData input = new();
		ItemCollection<ListMember> listMembers = ListMember.Create(input);

		Assert.That(listMembers, Has.Exactly(2).Items);

		Assert.That(listMembers[0].MemberInfo.Name, Is.EqualTo(nameof(InlineData.InlineProperty)));
		Assert.That(listMembers[1].MemberInfo.Name, Is.EqualTo(nameof(InlineData.InlineField)));
	}

	public class HideableClassData
	{
		[Hide(null)]
		public bool? HideableProperty { get; set; }
	}

	[Test]
	public void PropertyHideNullEmptyListVisible()
	{
		List<HideableClassData> list = [];
		TabDataColumns tabDataColumns = new();
		var propertyColumns = tabDataColumns.GetPropertyColumns(typeof(HideableClassData));

		Assert.That(propertyColumns, Has.Exactly(1).Items);
		Assert.That(propertyColumns[0].IsVisible(list));
	}
}
