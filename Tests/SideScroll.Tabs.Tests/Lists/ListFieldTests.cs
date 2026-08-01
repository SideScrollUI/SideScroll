using NUnit.Framework;
using SideScroll.Tabs.Lists;
using System;

namespace SideScroll.Tabs.Tests;

public class ListFieldTests
{
	private class TestClass
	{
		public int? NullableIntField;
		public string StringField = "";
	}

	[Test]
	public void ValueSetter_HandlesNullableTypes()
	{
		var obj = new TestClass();
		var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.NullableIntField))!;
		var listField = new ListField(obj, fieldInfo);

		// Should not throw InvalidCastException
		listField.Value = 42;
		Assert.That(obj.NullableIntField, Is.EqualTo(42));

		listField.Value = null;
		Assert.That(obj.NullableIntField, Is.Null);
	}

	[Test]
	public void ValueSetter_HandlesStringConversion()
	{
		var obj = new TestClass();
		var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.StringField))!;
		var listField = new ListField(obj, fieldInfo);

		// Should automatically call ToString() for non-string objects when the target is string
		listField.Value = 42;
		Assert.That(obj.StringField, Is.EqualTo("42"));
	}
}
