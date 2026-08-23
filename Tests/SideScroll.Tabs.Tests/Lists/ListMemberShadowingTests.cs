using NUnit.Framework;
using SideScroll.Collections;
using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Tests;

public class ListMemberShadowingTests
{
#pragma warning disable CS0649 // only inspected through reflection
	public class ShadowFieldBase
	{
		public string? Alpha;
		public string? Beta;
		public string? Gamma;
	}

	/// <summary>Reflection returns both declarations for any redeclared field, retyped or not</summary>
	public class ShadowFieldRow : ShadowFieldBase
	{
		public new int Beta;
		public string? Delta;
	}
#pragma warning restore CS0649

	[Test, Description(
		"Only the properties and methods were merged by name, so a redeclared field arrived twice " +
		"and showed as two rows of the same name, the first bound to the declaration the other hides")]
	public void ARedeclaredFieldIsOneRow()
	{
		ItemCollection<ListMember> members = ListMember.Create(new ShadowFieldRow());

		Assert.That(members.Select(member => member.Name),
			Is.EqualTo(new[] { "Alpha", "Beta", "Gamma", "Delta" }));
	}

	[Test, Description("The derived declaration is the one kept, matching ListField.Create()")]
	public void ARedeclaredFieldKeepsTheDerivedDeclaration()
	{
		ItemCollection<ListMember> members = ListMember.Create(new ShadowFieldRow());

		ListMember beta = members.Single(member => member.Name == "Beta");
		Assert.That(((ListField)beta).FieldInfo.DeclaringType, Is.EqualTo(typeof(ShadowFieldRow)));
		Assert.That(((ListField)beta).FieldInfo.FieldType, Is.EqualTo(typeof(int)));
	}

	[Test, Description("Control: ListField.Create() builds the same rows for the same type")]
	public void RedeclaredFieldRowsMatchListFieldCreate()
	{
		var obj = new ShadowFieldRow();

		Assert.That(ListMember.Create(obj).Select(member => member.Name),
			Is.EqualTo(ListField.Create(obj).Select(field => field.Name)));
	}
}
