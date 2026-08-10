using NUnit.Framework;
using SideScroll.Avalonia.Controls;
using System.Reflection;

namespace SideScroll.Avalonia.Tests;

/// <summary>
/// Constructing a TabTextBox needs an Avalonia Application, so these cover the watermark member
/// lookup through the helper it was separated into
/// </summary>
public class TabTextBoxTests
{
	private class BaseModel
	{
		public string HintProperty { get; set; } = "base property";
		public string HintField = "base field";
	}

	private class DerivedModel : BaseModel
	{
		public new string HintProperty { get; set; } = "derived property";
		public new string HintField = "derived field";
	}

	private static MemberInfo Resolve(string name)
	{
		MemberInfo[] memberInfos = typeof(DerivedModel).GetMember(name);
		return TabTextBox.GetMostDerived(memberInfos);
	}

	[Test, Description(
		"GetMember() returns both declarations of a hidden field, which was read as the member " +
		"being ambiguous even though the compiler resolves it to the derived one")]
	public void HiddenFieldResolvesToTheDerivedDeclaration()
	{
		Assert.That(typeof(DerivedModel).GetMember(nameof(DerivedModel.HintField)), Has.Length.EqualTo(2));

		var fieldInfo = (FieldInfo)Resolve(nameof(DerivedModel.HintField));

		Assert.That(fieldInfo.DeclaringType, Is.EqualTo(typeof(DerivedModel)));
		Assert.That(fieldInfo.GetValue(new DerivedModel()), Is.EqualTo("derived field"));
	}

	[Test, Description("A hidden property is already resolved by GetMember(), so it stays unchanged")]
	public void HiddenPropertyResolvesToTheDerivedDeclaration()
	{
		Assert.That(typeof(DerivedModel).GetMember(nameof(DerivedModel.HintProperty)), Has.Length.EqualTo(1));

		var propertyInfo = (PropertyInfo)Resolve(nameof(DerivedModel.HintProperty));

		Assert.That(propertyInfo.DeclaringType, Is.EqualTo(typeof(DerivedModel)));
		Assert.That(propertyInfo.GetValue(new DerivedModel()), Is.EqualTo("derived property"));
	}

	[Test, Description("A member declared once is returned as it is")]
	public void SingleMemberIsReturnedUnchanged()
	{
		MemberInfo[] memberInfos = typeof(BaseModel).GetMember(nameof(BaseModel.HintField));

		Assert.That(TabTextBox.GetMostDerived(memberInfos), Is.SameAs(memberInfos[0]));
	}

	[Test, Description("The declaring types choose between them, not the order GetMember() returned")]
	public void OrderOfTheDeclarationsDoesNotMatter()
	{
		MemberInfo[] memberInfos = typeof(DerivedModel).GetMember(nameof(DerivedModel.HintField));
		MemberInfo[] reversed = [.. memberInfos.Reverse()];

		Assert.That(TabTextBox.GetMostDerived(reversed).DeclaringType, Is.EqualTo(typeof(DerivedModel)));
		Assert.That(TabTextBox.GetMostDerived(memberInfos).DeclaringType, Is.EqualTo(typeof(DerivedModel)));
	}

	private class ThrowingModel
	{
		public string Throws => throw new InvalidOperationException("watermark unavailable");

		public string? Null => null;

		public string Method() => "not a value";
	}

	private static MemberInfo Member(string name) => typeof(ThrowingModel).GetMember(name)[0];

	[Test, Description(
		"A watermark member is read through reflection, so a throwing getter escaped the control's " +
		"constructor and failed the whole form instead of the one hint")]
	public void ThrowingMemberHasNoText()
	{
		Assert.That(TabTextBox.GetMemberText(Member(nameof(ThrowingModel.Throws)), new ThrowingModel()), Is.Null);
	}

	[Test]
	public void ReadableMemberReturnsItsValue()
	{
		Assert.That(TabTextBox.GetMemberText(Resolve(nameof(DerivedModel.HintProperty)), new DerivedModel()),
			Is.EqualTo("derived property"));
		Assert.That(TabTextBox.GetMemberText(Resolve(nameof(DerivedModel.HintField)), new DerivedModel()),
			Is.EqualTo("derived field"));
	}

	[Test, Description("A null value and a member that holds no value both leave the attribute's text to apply")]
	public void MembersWithoutAValueHaveNoText()
	{
		Assert.That(TabTextBox.GetMemberText(Member(nameof(ThrowingModel.Null)), new ThrowingModel()), Is.Null);
		Assert.That(TabTextBox.GetMemberText(Member(nameof(ThrowingModel.Method)), new ThrowingModel()), Is.Null);
	}
}
