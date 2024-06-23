using NUnit.Framework;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Tabs;
using System.Reflection;

namespace SideScroll.Test;

[Category("Core")]
public class TestFieldVisibility : TestBase
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	private static FieldInfo GetFieldInfo(object obj, string fieldName)
	{
		return obj
			.GetType()
			.GetFields()
			.Single(p => p.Name == fieldName);
	}

	private static ListField GetListField(object obj, string fieldName)
	{
		FieldInfo fieldInfo = GetFieldInfo(obj, fieldName);
		return new ListField(obj, fieldInfo);
	}

	public bool Default;

	[Test]
	public void TestDefault()
	{
		FieldInfo fieldInfo = GetFieldInfo(this, nameof(Default));
		Assert.IsTrue(fieldInfo.IsRowVisible());
	}

	[Hidden]
	public bool Hidden;

	[Test]
	public void TestHidden()
	{
		FieldInfo fieldInfo = GetFieldInfo(this, nameof(Hidden));
		Assert.IsFalse(fieldInfo.IsRowVisible());
	}

	[HiddenRow]
	public bool HiddenRow;

	[Test]
	public void TestHiddenRow()
	{
		FieldInfo fieldInfo = GetFieldInfo(this, nameof(HiddenRow));
		Assert.IsFalse(fieldInfo.IsRowVisible());
	}

	[Hide(null)]
	public bool? HideNull;

	[Test]
	public void TestHideNull()
	{
		ListField listField = GetListField(this, nameof(HideNull));
		Assert.IsTrue(listField.IsFieldVisible);
		Assert.IsFalse(listField.IsRowVisible());
	}

	[Hide(null)]
	public bool? HideNullShow = true;

	[Test]
	public void TestHideNullShow()
	{
		ListField listField = GetListField(this, nameof(HideNullShow));
		Assert.IsTrue(listField.IsFieldVisible);
		Assert.IsTrue(listField.IsRowVisible());
	}

	[HideRow(null)]
	public bool? HideRowNull;

	[Test]
	public void TestHideRowNull()
	{
		ListField listField = GetListField(this, nameof(HideRowNull));
		Assert.IsTrue(listField.IsFieldVisible);
		Assert.IsFalse(listField.IsRowVisible());
	}

	[HideRow(null)]
	public bool? HideRowNullShow = true;

	[Test]
	public void TestHideRowNullShow()
	{
		ListField listField = GetListField(this, nameof(HideRowNullShow));
		Assert.IsTrue(listField.IsFieldVisible);
		Assert.IsTrue(listField.IsRowVisible());
	}

	[Hide(null)]
	public class HideNullClass
	{
		public bool? VisibleField = true;
		public bool? HiddenField = null;
	}

	[Test]
	public void TestHideNullClass()
	{
		HideNullClass input = new();
		ItemCollection<ListMember> listMembers = ListMember.Create(input);

		Assert.AreEqual(1, listMembers.Count);

		Assert.AreEqual(nameof(HideNullClass.VisibleField), listMembers[0].MemberInfo.Name);
	}

	public class InlineClass
	{
		[Inline]
		public InlineData Data = new();
	}

	public class InlineData
	{
		public bool InlineProperty { get; set; } = true;
		public bool InlineField = true;
	}

	[Test]
	public void TestInlineClass()
	{
		InlineClass input = new();
		ItemCollection<ListMember> listMembers = ListMember.Create(input);

		Assert.AreEqual(2, listMembers.Count);

		Assert.AreEqual(nameof(InlineData.InlineProperty), listMembers[0].MemberInfo.Name);
		Assert.AreEqual(nameof(InlineData.InlineField), listMembers[1].MemberInfo.Name);
	}
}
