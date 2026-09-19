using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Avalonia.Controls;
using SideScroll.Tabs.Lists;
using System.Globalization;
using System.Reflection;

namespace SideScroll.Avalonia.Tests;

/// <summary>
/// Constructing a TabTextBox needs an Avalonia Application, so outside of the headless tests these cover
/// the watermark member lookup and allowed chars through the helpers they were separated into
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

	// ─── Allowed chars ───────────────────────────────────────────────────

	private static readonly NumberFormatInfo Invariant = NumberFormatInfo.InvariantInfo;

	private static readonly string MinusSign = ((char)0x2212).ToString(); // Looks like a hyphen as a literal

	[TestCase(typeof(byte))]
	[TestCase(typeof(ushort))]
	[TestCase(typeof(uint))]
	[TestCase(typeof(ulong))]
	[TestCase(typeof(nuint))]
	public void UnsignedIntegersAllowOnlyDigits(Type type)
	{
		string? allowedChars = TabTextBox.GetAllowedChars(type, Invariant);

		Assert.That(TabTextBox.IsAllowed("0123456789", allowedChars), Is.True);
		Assert.That(TabTextBox.IsAllowed("-", allowedChars), Is.False);
		Assert.That(TabTextBox.IsAllowed(".", allowedChars), Is.False);
		Assert.That(TabTextBox.IsAllowed("a", allowedChars), Is.False);
	}

	[TestCase(typeof(sbyte))]
	[TestCase(typeof(short))]
	[TestCase(typeof(int))]
	[TestCase(typeof(long))]
	[TestCase(typeof(nint))]
	public void SignedIntegersAlsoAllowTheNegativeSign(Type type)
	{
		string? allowedChars = TabTextBox.GetAllowedChars(type, Invariant);

		Assert.That(TabTextBox.IsAllowed("-123", allowedChars), Is.True);
		Assert.That(TabTextBox.IsAllowed(".", allowedChars), Is.False);
		Assert.That(TabTextBox.IsAllowed("e", allowedChars), Is.False);
	}

	[TestCase(typeof(float))]
	[TestCase(typeof(double))]
	public void FloatingPointAllowsSeparatorsAndExponents(Type type)
	{
		string? allowedChars = TabTextBox.GetAllowedChars(type, Invariant);

		Assert.That(TabTextBox.IsAllowed("-1,234.5e+10", allowedChars), Is.True);
		Assert.That(TabTextBox.IsAllowed("1E-5", allowedChars), Is.True);
		Assert.That(TabTextBox.IsAllowed("x", allowedChars), Is.False);
	}

	[Test, Description("A decimal doesn't parse an exponent")]
	public void DecimalAllowsSeparatorsButNoExponent()
	{
		string? allowedChars = TabTextBox.GetAllowedChars(typeof(decimal), Invariant);

		Assert.That(TabTextBox.IsAllowed("-1,234.5", allowedChars), Is.True);
		Assert.That(TabTextBox.IsAllowed("e", allowedChars), Is.False);
	}

	[Test, Description(
		"The binding's converter parses with the current culture, so the separators that can be typed " +
		"have to come from it too")]
	public void SeparatorsComeFromTheNumberFormat()
	{
		var numberFormat = new NumberFormatInfo
		{
			NumberDecimalSeparator = ",",
			NumberGroupSeparator = "'",
		};

		string? allowedChars = TabTextBox.GetAllowedChars(typeof(double), numberFormat);

		Assert.That(TabTextBox.IsAllowed("1'234,5", allowedChars), Is.True);
		Assert.That(TabTextBox.IsAllowed(".", allowedChars), Is.False);
	}

	[Test, Description("Parsing accepts '-' for a culture whose negative sign is another character")]
	public void HyphenIsAllowedAlongsideTheCulturesNegativeSign()
	{
		var numberFormat = new NumberFormatInfo
		{
			NegativeSign = MinusSign,
		};

		string? allowedChars = TabTextBox.GetAllowedChars(typeof(int), numberFormat);

		Assert.That(TabTextBox.IsAllowed("-", allowedChars), Is.True);
		Assert.That(TabTextBox.IsAllowed(MinusSign, allowedChars), Is.True);
	}

	[TestCase(typeof(string))]
	[TestCase(typeof(bool))]
	[TestCase(typeof(char))]
	[TestCase(typeof(DateTime))]
	[TestCase(typeof(DayOfWeek), Description = "An enum's type code is its underlying integer's")]
	public void OtherTypesAreUnrestricted(Type type)
	{
		Assert.That(TabTextBox.GetAllowedChars(type, Invariant), Is.Null);
	}

	[Test]
	public void NullAllowedCharsAllowsAnything()
	{
		Assert.That(TabTextBox.IsAllowed("anything", null), Is.True);
		Assert.That(TabTextBox.IsAllowed(null, "0"), Is.True);
	}

	private class InputModel
	{
		public int Count { get; set; }

		[AllowedChars(AllowedCharsAttribute.Hex)]
		public string Color { get; set; } = "";

		[AllowedChars(AllowedCharsAttribute.Hex)]
		public int HexCount { get; set; }

		public char Letter { get; set; }

		public string Name { get; set; } = "";
	}

	private static TabTextBox CreateTextBox(string propertyName)
	{
		var textBox = new TabTextBox(new ListProperty(new InputModel(), propertyName));
		var window = new Window { Content = textBox };
		window.Show();
		textBox.Focus();
		textBox.Clear();
		return textBox;
	}

	private static void TypeText(TabTextBox textBox, string text)
	{
		foreach (char c in text)
		{
			textBox.RaiseEvent(new TextInputEventArgs
			{
				RoutedEvent = InputElement.TextInputEvent,
				Text = c.ToString(),
			});
		}
	}

	[AvaloniaTest]
	public void TypingIntoAnIntDropsOtherCharacters()
	{
		TabTextBox textBox = CreateTextBox(nameof(InputModel.Count));

		TypeText(textBox, "-1a2.3");

		Assert.That(textBox.Text, Is.EqualTo("-123"));
	}

	[AvaloniaTest]
	public void AttributeRestrictsAString()
	{
		TabTextBox textBox = CreateTextBox(nameof(InputModel.Color));

		TypeText(textBox, "#ffg00");

		Assert.That(textBox.Text, Is.EqualTo("ff00"));
	}

	[AvaloniaTest]
	public void AttributeOverridesTheTypesDefault()
	{
		TabTextBox textBox = CreateTextBox(nameof(InputModel.HexCount));

		Assert.That(textBox.AllowedChars, Is.EqualTo(AllowedCharsAttribute.Hex));
	}

	[AvaloniaTest]
	public void StringIsUnrestricted()
	{
		TabTextBox textBox = CreateTextBox(nameof(InputModel.Name));

		TypeText(textBox, "a-1 .");

		Assert.That(textBox.Text, Is.EqualTo("a-1 ."));
	}

	[AvaloniaTest]
	public void CharIsLimitedToOneCharacter()
	{
		TabTextBox textBox = CreateTextBox(nameof(InputModel.Letter));

		Assert.That(textBox.MaxLength, Is.EqualTo(1));
	}
}
