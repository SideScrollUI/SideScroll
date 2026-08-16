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
}
