using NUnit.Framework;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Tests;

public class ListFieldTests
{
	private class TestClass
	{
#pragma warning disable CS0649 // only inspected through reflection
		public int? NullableIntField;
#pragma warning restore CS0649
		public string StringField = "";
		public readonly int ReadOnlyField = 1;
		public const int ConstField = 2;
	}

	[Test, Description(
		"IsEditable used to always be true, so forms and grids offered an editor for members " +
		"reflection then refused to assign")]
	public void IsEditable_ExcludesReadOnlyAndConstFields()
	{
		var obj = new TestClass();

		ListField Field(string name) => new(obj, typeof(TestClass).GetField(name)!);

		Assert.That(Field(nameof(TestClass.StringField)).IsEditable, Is.True);
		Assert.That(Field(nameof(TestClass.NullableIntField)).IsEditable, Is.True);
		Assert.That(Field(nameof(TestClass.ReadOnlyField)).IsEditable, Is.False, "readonly");
		Assert.That(Field(nameof(TestClass.ConstField)).IsEditable, Is.False, "const");
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
