using Atlas.Extensions;
using Atlas.Tabs;
using NUnit.Framework;
using System.Linq;
using System.Reflection;

namespace Atlas.Core.Test;

[Category("Core")]
public class TestFieldVisibility : TestBase
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	private FieldInfo GetFieldInfo(object obj, string fieldName)
	{
		return obj.GetType()
			.GetFields()
			.Where(p => p.Name == fieldName)
			.Single();
	}

	private ListField GetListField(object obj, string fieldName)
	{
		FieldInfo fieldInfo = GetFieldInfo(obj, fieldName);
		return new ListField(obj, fieldInfo);
	}

	public bool Default;

	[Test]
	public void TestDefault()
	{
		FieldInfo fieldInfo = GetFieldInfo(this, nameof(Default));
		Assert.IsTrue(fieldInfo.IsVisible());
	}

	[Hidden]
	public bool Hidden;

	[Test]
	public void TestHidden()
	{
		FieldInfo fieldInfo = GetFieldInfo(this, nameof(Hidden));
		Assert.IsFalse(fieldInfo.IsVisible());
	}

	[HiddenRow]
	public bool HiddenRow;

	[Test]
	public void TestHiddenRow()
	{
		FieldInfo fieldInfo = GetFieldInfo(this, nameof(HiddenRow));
		Assert.IsFalse(fieldInfo.IsVisible());
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
}
