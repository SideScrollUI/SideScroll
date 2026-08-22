using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Tabs;
using System.Reflection;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class TabDataColumnsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize(nameof(TabDataColumnsTests));
	}

	private class CachedRow
	{
		public string Name { get; set; } = "name";
		public int Value { get; set; }
	}

	[Test, Description(
		"The cache handed out the list it stored, so one caller sorting, clearing, or appending " +
		"through it changed the columns every later grid got for that type")]
	public void VisiblePropertiesCannotBeMutatedByACaller()
	{
		IReadOnlyList<PropertyInfo> properties = TabDataColumns.GetVisibleProperties(typeof(CachedRow));
		int count = properties.Count;

		Assert.That(count, Is.GreaterThan(0), "precondition: it found properties");
		Assert.That(properties, Is.InstanceOf<System.Collections.ObjectModel.ReadOnlyCollection<PropertyInfo>>());

		// A caller casting back to the concrete type is refused at runtime, not just by the signature
		Assert.Throws<NotSupportedException>(() => ((IList<PropertyInfo>)properties).Clear());

		Assert.That(TabDataColumns.GetVisibleProperties(typeof(CachedRow)), Has.Count.EqualTo(count));
	}

	private class ButtonRow
	{
		[ButtonColumn]
		public void Invokable() { }

		[ButtonColumn]
		public void NeedsAParameter(int value) { }

		[ButtonColumn]
		public void IsGeneric<T>() { }
	}

	[Test, Description(
		"The grid invokes a button column with no arguments, so a method needing one, or needing " +
		"a type argument bound first, threw when its button was pressed")]
	public void ButtonColumnsExcludeMethodsTheGridCannotInvoke()
	{
		List<TabMethodColumn> columns = TabDataColumns.GetMethodColumns(typeof(ButtonRow));

		Assert.That(columns.Select(column => column.MethodInfo.Name),
			Is.EqualTo(new[] { nameof(ButtonRow.Invokable) }));
	}

	[Test, Description("Control: an invokable button column is still discovered")]
	public void ButtonColumnsIncludeAnInvokableMethod()
	{
		List<TabMethodColumn> columns = TabDataColumns.GetMethodColumns(typeof(ButtonRow));

		Assert.That(columns, Is.Not.Empty);
		MethodInfo methodInfo = columns[0].MethodInfo;

		Assert.DoesNotThrow(() => methodInfo.Invoke(new ButtonRow(), []));
	}

	public class ShadowBaseRow
	{
		public string Value { get; set; } = "base";
		public int Other { get; set; } = 1;
	}

	/// <summary>Redeclares with a different type, which reflection returns both declarations for</summary>
	public class RetypedShadowRow : ShadowBaseRow
	{
		public new int Value { get; set; } = 5;
	}

	/// <summary>Redeclares with the same signature, which reflection collapses itself</summary>
	public class SameTypeShadowRow : ShadowBaseRow
	{
		public new string Value { get; set; } = "derived";
	}

	[Test, Description(
		"Reflection returns both declarations for a property redeclared with a different type, and " +
		"the name keyed lookup threw for the duplicate. Only reached once columns have been " +
		"reordered, so a grid worked until it was dragged and then threw from then on")]
	public void ReorderingColumnsWithARetypedShadowedProperty()
	{
		var columns = new TabDataColumns(["Value"]);

		List<TabPropertyColumn> result = columns.GetPropertyColumns(typeof(RetypedShadowRow));

		Assert.That(result.Select(column => column.PropertyInfo.Name), Is.EqualTo(new[] { "Value", "Other" }));
	}

	[Test, Description("The derived declaration is the one kept, matching what ListProperty.Create() does")]
	public void ARetypedShadowedPropertyKeepsTheDerivedDeclaration()
	{
		var columns = new TabDataColumns(["Value"]);

		List<TabPropertyColumn> result = columns.GetPropertyColumns(typeof(RetypedShadowRow));

		PropertyInfo value = result.Single(column => column.PropertyInfo.Name == "Value").PropertyInfo;
		Assert.That(value.DeclaringType, Is.EqualTo(typeof(RetypedShadowRow)));
		Assert.That(value.PropertyType, Is.EqualTo(typeof(int)));
	}

	[Test, Description("A same signature redeclaration was already fine, reflection collapses it")]
	public void ReorderingColumnsWithASameTypeShadowedProperty()
	{
		var columns = new TabDataColumns(["Value"]);

		List<TabPropertyColumn> result = columns.GetPropertyColumns(typeof(SameTypeShadowRow));

		Assert.That(result.Select(column => column.PropertyInfo.Name), Is.EqualTo(new[] { "Value", "Other" }));
	}

	[Test, Description("Control: ordinary types are unaffected, and the requested order still applies")]
	public void ReorderingColumnsWithoutShadowingIsUnchanged()
	{
		var columns = new TabDataColumns(["Other", "Value"]);

		List<TabPropertyColumn> result = columns.GetPropertyColumns(typeof(ShadowBaseRow));

		Assert.That(result.Select(column => column.PropertyInfo.Name), Is.EqualTo(new[] { "Other", "Value" }));
	}

	[Test, Description("Control: with no reordering the properties come back as they are")]
	public void NoColumnOrderReturnsEveryProperty()
	{
		var columns = new TabDataColumns();

		List<TabPropertyColumn> result = columns.GetPropertyColumns(typeof(ShadowBaseRow));

		Assert.That(result.Select(column => column.PropertyInfo.Name), Is.EqualTo(new[] { "Value", "Other" }));
	}
}
