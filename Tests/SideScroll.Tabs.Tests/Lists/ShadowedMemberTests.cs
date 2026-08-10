using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Tests;

// Reflection returns both declarations of a redeclared member when it can't resolve which one the
// compiler means, and sorting them put whichever was declared first in the assembly last. These are
// declared in both orders because only one of the two picks the wrong member

public class HiddenFieldDerivedFirst : HiddenFieldDeclaredSecond
{
	public new string Value = "derived";
}

public class HiddenFieldDeclaredSecond
{
	public string Value = "base";
}

public class HiddenFieldBaseFirst
{
	public string Value = "base";
}

public class HiddenFieldDerivedSecond : HiddenFieldBaseFirst
{
	public new string Value = "derived";
}

public class RetypedPropertyDerivedFirst : RetypedPropertyDeclaredSecond
{
	public new int Value { get; set; } = 42;
}

public class RetypedPropertyDeclaredSecond
{
	public string Value { get; set; } = "base";
}

public class RetypedPropertyBaseFirst
{
	public string Value { get; set; } = "base";
}

public class RetypedPropertyDerivedSecond : RetypedPropertyBaseFirst
{
	public new int Value { get; set; } = 42;
}

// The redeclared property is hidden by its own value, the one it hides isn't
public class HiddenValueDerivedFirst : HiddenValueDeclaredSecond
{
	[Hide("hide me")]
	public new string Value { get; set; } = "hide me";
}

public class HiddenValueDeclaredSecond
{
	public int Value { get; set; } = 7;
	public string Tail { get; set; } = "tail";
}

public class HiddenValueBaseFirst
{
	public int Value { get; set; } = 7;
	public string Tail { get; set; } = "tail";
}

public class HiddenValueDerivedSecond : HiddenValueBaseFirst
{
	[Hide("hide me")]
	public new string Value { get; set; } = "hide me";
}

// A property redeclared with the same type is resolved by reflection itself, so only one arrives
public class SamePropertyBase
{
	public string Value { get; set; } = "base";
}

public class SamePropertyDerived : SamePropertyBase
{
	public new string Value { get; set; } = "derived";
}

// A filter that drops the redeclaration leaves the hidden one behind, and it becomes the only
// member of that name, so a member marked hidden showed the base member's value instead

public class HiddenRedeclaredBase
{
	public string Value { get; set; } = "base";
}

public class HiddenRedeclaredDerived : HiddenRedeclaredBase
{
	[Hidden]
	public new int Value { get; set; } = 42;
}

public class PrivateGetterBase
{
	public string Value { get; set; } = "base";
}

public class PrivateGetterDerived : PrivateGetterBase
{
	public new int Value { private get; set; } = 42;
}

public class HiddenFieldBase
{
	public string Value = "base";
}

public class HiddenFieldDerived : HiddenFieldBase
{
	[Hidden]
	public new string Value = "derived";
}

public class VisibleRedeclaredBase
{
	[Hidden]
	public string Value { get; set; } = "base";
}

public class VisibleRedeclaredDerived : VisibleRedeclaredBase
{
	public new int Value { get; set; } = 42;
}

[Category("Tabs")]
public class ShadowedMemberTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ShadowedMembers");
	}

	[Test, Description("A field is always returned for both declarations, never resolved by reflection")]
	public void HiddenFieldsAreReturnedTwiceByReflection()
	{
		Assert.That(typeof(HiddenFieldDerivedFirst).GetFields().Where(f => f.Name == "Value").ToList(),
			Has.Count.EqualTo(2));
	}

	[Test, Description(
		"Sorting by MetadataToken follows declaration order, so a subclass declared above the type " +
		"it derives from put its own field first and the hidden base field won")]
	public void FieldListBindsTheDerivedFieldWhenDerivedIsDeclaredFirst()
	{
		var listFields = ListField.Create(new HiddenFieldDerivedFirst());

		var match = listFields.Where(lf => lf.FieldInfo.Name == "Value").ToList();
		Assert.That(match, Has.Count.EqualTo(1), "The hidden declaration shouldn't add a second row");
		Assert.That(match[0].Value, Is.EqualTo("derived"));
	}

	[Test, Description("The other declaration order already worked, so it only guards against a regression")]
	public void FieldListBindsTheDerivedFieldWhenBaseIsDeclaredFirst()
	{
		var listFields = ListField.Create(new HiddenFieldDerivedSecond());

		var match = listFields.Where(lf => lf.FieldInfo.Name == "Value").ToList();
		Assert.That(match, Has.Count.EqualTo(1));
		Assert.That(match[0].Value, Is.EqualTo("derived"));
	}

	[Test, Description("A property that changes its type can't be resolved as hiding, so both are returned")]
	public void RetypedPropertiesAreReturnedTwiceByReflection()
	{
		Assert.That(typeof(RetypedPropertyDerivedFirst).GetProperties().Where(p => p.Name == "Value").ToList(),
			Has.Count.EqualTo(2));
	}

	[Test]
	public void PropertyListBindsTheDerivedPropertyWhenDerivedIsDeclaredFirst()
	{
		var listProperties = ListProperty.Create(new RetypedPropertyDerivedFirst());

		var match = listProperties.Where(lp => lp.PropertyInfo.Name == "Value").ToList();
		Assert.That(match, Has.Count.EqualTo(1));
		Assert.That(match[0].Value, Is.EqualTo(42));
	}

	[Test]
	public void PropertyListBindsTheDerivedPropertyWhenBaseIsDeclaredFirst()
	{
		var listProperties = ListProperty.Create(new RetypedPropertyDerivedSecond());

		var match = listProperties.Where(lp => lp.PropertyInfo.Name == "Value").ToList();
		Assert.That(match, Has.Count.EqualTo(1));
		Assert.That(match[0].Value, Is.EqualTo(42));
	}

	[Test, Description("A same-signature redeclaration was already resolved, and still is")]
	public void PropertyListBindsTheDerivedPropertyForASameTypeRedeclaration()
	{
		Assert.That(typeof(SamePropertyDerived).GetProperties().Where(p => p.Name == "Value").ToList(),
			Has.Count.EqualTo(1), "Reflection resolves this one itself");

		var match = ListProperty.Create(new SamePropertyDerived())
			.Where(lp => lp.PropertyInfo.Name == "Value")
			.ToList();

		Assert.That(match, Has.Count.EqualTo(1));
		Assert.That(match[0].Value, Is.EqualTo("derived"));
	}

	[TestCase(typeof(HiddenValueDerivedFirst))]
	[TestCase(typeof(HiddenValueDerivedSecond))]
	[Description(
		"A [Hide] on the redeclared property used to fall back to showing the value of the one it " +
		"hides, which nothing resolves to, instead of hiding the row. The hidden declaration was " +
		"skipped before it could displace the base one, so the base was added as a new name")]
	public void HidingTheRedeclaredPropertyHidesTheRow(Type type)
	{
		object instance = Activator.CreateInstance(type)!;

		var listProperties = ListProperty.Create(instance);

		Assert.That(listProperties.Where(lp => lp.PropertyInfo.Name == "Value"), Is.Empty);
		Assert.That(listProperties.Select(lp => lp.PropertyInfo.Name), Has.Member("Tail"),
			"The rest of the type is unaffected");
	}

	[Test, Description("Members that aren't redeclared keep their order and all stay")]
	public void UnrelatedMembersAreUnaffected()
	{
		var listProperties = ListProperty.Create(new RetypedPropertyDerivedSecond());

		Assert.That(listProperties.Select(lp => lp.PropertyInfo.Name), Has.Member("Value"));
		Assert.That(listProperties.Select(lp => lp.PropertyInfo.Name).Distinct().Count(),
			Is.EqualTo(listProperties.Count), "No name should appear twice");
	}
	[Test, Description(
		"Hiding the redeclaration used to leave the one it hides as the only member of that name, " +
		"so the row showed the base value rather than disappearing")]
	public void HidingARedeclaredPropertyHidesBothDeclarations()
	{
		var listProperties = ListProperty.Create(new HiddenRedeclaredDerived());

		Assert.That(listProperties.Where(lp => lp.PropertyInfo.Name == "Value"), Is.Empty);
	}

	[Test, Description("A non public getter on the redeclaration excludes it the same way")]
	public void ARedeclaredPropertyWithoutAPublicGetterHidesBothDeclarations()
	{
		var listProperties = ListProperty.Create(new PrivateGetterDerived());

		Assert.That(listProperties.Where(lp => lp.PropertyInfo.Name == "Value"), Is.Empty);
	}

	[Test]
	public void HidingARedeclaredFieldHidesBothDeclarations()
	{
		var listFields = ListField.Create(new HiddenFieldDerived());

		Assert.That(listFields.Where(lf => lf.FieldInfo.Name == "Value"), Is.Empty);
	}

	[Test, Description("Hiding only the declaration being hidden leaves the redeclaration showing")]
	public void HidingTheHiddenDeclarationLeavesTheRedeclaration()
	{
		var listProperties = ListProperty.Create(new VisibleRedeclaredDerived());

		var match = listProperties.Where(lp => lp.PropertyInfo.Name == "Value").ToList();
		Assert.That(match, Has.Count.EqualTo(1));
		Assert.That(match[0].Value, Is.EqualTo(42));
	}
}
