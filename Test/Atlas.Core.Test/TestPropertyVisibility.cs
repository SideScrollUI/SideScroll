using Atlas.Extensions;
using Atlas.Tabs;
using NUnit.Framework;
using System.Linq;
using System.Reflection;

namespace Atlas.Core.Test;

[Category("Core")]
public class TestPropertyVisibility : TestBase
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	private PropertyInfo GetPropertyInfo(object obj, string propertyName)
	{
		return obj.GetType()
			.GetProperties()
			.Where(p => p.Name == propertyName)
			.Single();
	}

	private ListProperty GetListProperty(object obj, string fieldName)
	{
		PropertyInfo propertyInfo = GetPropertyInfo(obj, fieldName);
		return new ListProperty(obj, propertyInfo);
	}

	public bool Default { get; set; }

	[Test]
	public void TestDefault()
	{
		PropertyInfo fieldInfo = GetPropertyInfo(this, nameof(Default));
		Assert.IsTrue(fieldInfo.IsVisible());
	}


	[Hidden]
	public bool Hidden { get; set; }

	[Test]
	public void TestHidden()
	{
		PropertyInfo fieldInfo = GetPropertyInfo(this, nameof(Hidden));
		Assert.IsFalse(fieldInfo.IsVisible());
	}

	[HiddenRow]
	public bool HiddenRow { get; set; }

	[Test]
	public void TestHiddenRow()
	{
		PropertyInfo fieldInfo = GetPropertyInfo(this, nameof(HiddenRow));
		Assert.IsFalse(fieldInfo.IsVisible());
	}

	[Hide(null)]
	public bool? HideNull { get; set; }

	[Test]
	public void TestHideNull()
	{
		ListProperty listProperty = GetListProperty(this, nameof(HideNull));
		Assert.IsTrue(listProperty.IsPropertyVisible);
		Assert.IsFalse(listProperty.IsRowVisible());
	}

	[Hide(null)]
	public bool? HideNullShow { get; set; } = true;

	[Test]
	public void TestHideNullShow()
	{
		ListProperty listProperty = GetListProperty(this, nameof(HideNullShow));
		Assert.IsTrue(listProperty.IsPropertyVisible);
		Assert.IsTrue(listProperty.IsRowVisible());
	}

	[HideRow(null)]
	public bool? HideRowNull { get; set; }

	[Test]
	public void TestHideRowNull()
	{
		ListProperty listProperty = GetListProperty(this, nameof(HideRowNull));
		Assert.IsTrue(listProperty.IsPropertyVisible);
		Assert.IsFalse(listProperty.IsRowVisible());
	}

	[HideRow(null)]
	public bool? HideRowNullShow { get; set; } = true;

	[Test]
	public void TestHideRowNullShow()
	{
		ListProperty listProperty = GetListProperty(this, nameof(HideRowNullShow));
		Assert.IsTrue(listProperty.IsPropertyVisible);
		Assert.IsTrue(listProperty.IsRowVisible());
	}

	[HideColumn(null)]
	public bool? HideColumnNull { get; set; }

	[Test]
	public void TestHideColumnNull()
	{
		ListProperty listProperty = GetListProperty(this, nameof(HideColumnNull));
		Assert.IsTrue(listProperty.IsPropertyVisible);
		Assert.IsFalse(listProperty.IsColumnVisible());
	}

	[HideColumn(null)]
	public bool? HideColumnNullShow { get; set; } = true;

	[Test]
	public void TestHideColumnNullShow()
	{
		ListProperty listProperty = GetListProperty(this, nameof(HideColumnNullShow));
		Assert.IsTrue(listProperty.IsPropertyVisible);
		Assert.IsTrue(listProperty.IsColumnVisible());
	}
}
