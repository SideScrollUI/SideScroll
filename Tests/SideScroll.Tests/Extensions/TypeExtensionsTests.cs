using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Extensions;

namespace SideScroll.Tests.Extensions;

[Category("Core")]
public class TypeExtensionsTests : BaseTest
{
	public class WriteOnlyModel
	{
		public string Value
		{
			set { }
		}
	}

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TypeExtensions");
	}

	[Test]
	public void AssemblyQualifiedShortNameInt()
	{
		string shortName = typeof(int).GetAssemblyQualifiedShortName();

		Assert.That(shortName, Is.EqualTo("System.Int32, System.Private.CoreLib"));
	}

	[Test]
	public void AssemblyQualifiedShortNameList()
	{
		string shortName = typeof(List<Tag>).GetAssemblyQualifiedShortName();

		Assert.That(shortName, Is.EqualTo("System.Collections.Generic.List`1[[SideScroll.Tag, SideScroll]], System.Private.CoreLib"));
	}

	[Test]
	public void AssemblyQualifiedShortNameListOfLists()
	{
		string shortName = typeof(List<List<Tag>>).GetAssemblyQualifiedShortName();

		Assert.That(shortName, Is.EqualTo("System.Collections.Generic.List`1[[System.Collections.Generic.List`1[[SideScroll.Tag, SideScroll]], System.Private.CoreLib]], System.Private.CoreLib"));
	}

	[Test]
	public void AssemblyQualifiedShortNameDictionary()
	{
		string shortName = typeof(Dictionary<string, Tag>).GetAssemblyQualifiedShortName();

		Assert.That(shortName, Is.EqualTo("System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib], [SideScroll.Tag, SideScroll]], System.Private.CoreLib"));
	}

	[Test]
	public void AssemblyQualifiedShortNameArrayOfGenerics()
	{
		string shortName = typeof(List<Tag>[]).GetAssemblyQualifiedShortName();

		Assert.That(shortName, Is.EqualTo("System.Collections.Generic.List`1[[SideScroll.Tag, SideScroll]][], System.Private.CoreLib"));
	}

	[Test]
	public void AssemblyQualifiedShortNameMultiDimensionalArrayOfGenerics()
	{
		string shortName = typeof(List<Tag>[,]).GetAssemblyQualifiedShortName();

		Assert.That(shortName, Is.EqualTo("System.Collections.Generic.List`1[[SideScroll.Tag, SideScroll]][,], System.Private.CoreLib"));
	}

	[Test]
	public void VisibleProperties_ExcludesWriteOnlyProperties()
	{
		var property = typeof(WriteOnlyModel).GetProperty(nameof(WriteOnlyModel.Value))!;

		Assert.That(typeof(WriteOnlyModel).GetVisibleProperties(), Does.Not.Contain(property));
		Assert.That(property.IsRowVisible(), Is.False);
		Assert.That(property.IsColumnVisible(), Is.False);
	}

	// These results are cached per (type, attribute), so the tests below cover the ways a shared
	// entry could be returned for the wrong lookup

	public class AttributeModel
	{
		[DataKey]
		public string Key { get; set; } = "Key";

		[DataValue]
		public string Value { get; set; } = "Value";

		public string Plain { get; set; } = "Plain";

		[DataKey]
		public string KeyField = "KeyField";

		[DataValue]
		public string ValueField = "ValueField";
	}

	public class OtherAttributeModel
	{
		[DataKey]
		public string OtherKey { get; set; } = "OtherKey";
	}

	public class NoAttributeModel
	{
		public string Plain { get; set; } = "Plain";
	}

	[Test, Description("Each attribute keys its own entry, rather than the first lookup for a type winning")]
	public void PropertiesWithAttribute_DoesNotShareEntriesBetweenAttributes()
	{
		var keys = typeof(AttributeModel).GetPropertiesWithAttribute<DataKeyAttribute>();
		var values = typeof(AttributeModel).GetPropertiesWithAttribute<DataValueAttribute>();

		Assert.That(keys.Select(p => p.Name), Is.EqualTo(new[] { nameof(AttributeModel.Key) }));
		Assert.That(values.Select(p => p.Name), Is.EqualTo(new[] { nameof(AttributeModel.Value) }));
	}

	[Test, Description("Each type keys its own entry within an attribute")]
	public void PropertiesWithAttribute_DoesNotShareEntriesBetweenTypes()
	{
		var keys = typeof(AttributeModel).GetPropertiesWithAttribute<DataKeyAttribute>();
		var otherKeys = typeof(OtherAttributeModel).GetPropertiesWithAttribute<DataKeyAttribute>();

		Assert.That(keys.Select(p => p.Name), Is.EqualTo(new[] { nameof(AttributeModel.Key) }));
		Assert.That(otherKeys.Select(p => p.Name), Is.EqualTo(new[] { nameof(OtherAttributeModel.OtherKey) }));
	}

	[Test, Description("Properties and fields are stored separately, so neither returns the other's members")]
	public void FieldsWithAttribute_DoesNotShareEntriesWithProperties()
	{
		var properties = typeof(AttributeModel).GetPropertiesWithAttribute<DataKeyAttribute>();
		var fields = typeof(AttributeModel).GetFieldsWithAttribute<DataKeyAttribute>();

		Assert.That(properties.Select(p => p.Name), Is.EqualTo(new[] { nameof(AttributeModel.Key) }));
		Assert.That(fields.Select(f => f.Name), Is.EqualTo(new[] { nameof(AttributeModel.KeyField) }));
	}

	[Test, Description("A type with no matches caches the empty result instead of re-searching")]
	public void PropertiesWithAttribute_CachesAnEmptyResult()
	{
		var first = typeof(NoAttributeModel).GetPropertiesWithAttribute<DataKeyAttribute>();
		var second = typeof(NoAttributeModel).GetPropertiesWithAttribute<DataKeyAttribute>();

		Assert.That(first, Is.Empty);
		Assert.That(second, Is.SameAs(first));
	}

	[Test, Description("A repeated lookup reuses the stored result rather than rebuilding it")]
	public void PropertiesWithAttribute_ReturnsTheCachedInstance()
	{
		var first = typeof(AttributeModel).GetPropertiesWithAttribute<DataValueAttribute>();
		var second = typeof(AttributeModel).GetPropertiesWithAttribute<DataValueAttribute>();

		Assert.That(second, Is.SameAs(first));
	}

	[Test]
	public void FieldsWithAttribute_ReturnsTheCachedInstance()
	{
		var first = typeof(AttributeModel).GetFieldsWithAttribute<DataValueAttribute>();
		var second = typeof(AttributeModel).GetFieldsWithAttribute<DataValueAttribute>();

		Assert.That(first.Select(f => f.Name), Is.EqualTo(new[] { nameof(AttributeModel.ValueField) }));
		Assert.That(second, Is.SameAs(first));
	}

	[Test]
	public void PropertyWithAttribute_ReturnsTheFirstMatchAndNullWithout()
	{
		Assert.That(typeof(AttributeModel).GetPropertyWithAttribute<DataKeyAttribute>()?.Name,
			Is.EqualTo(nameof(AttributeModel.Key)));
		Assert.That(typeof(NoAttributeModel).GetPropertyWithAttribute<DataKeyAttribute>(), Is.Null);
	}
}
