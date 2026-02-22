using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Tests.Utilities;

[Category("Json")]
public class JsonUtilsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Json");
	}

	#region TryFormat Tests (Standard Escaping)

	[Test]
	public void TryFormat_BasicFormatting()
	{
		string input = "{\"name\":\"John\",\"age\":30}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));

		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("name"));
		Assert.That(formatted.Contains("John"));
		Assert.That(formatted.Contains("age"));
		Assert.That(formatted.Contains("30"));
	}

	[Test]
	public void TryFormat_InvalidJson()
	{
		string input = "{invalid json}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted), Is.False);
		Assert.That(formatted, Is.Null);
	}

	[Test]
	public void TryFormat_EmptyObject()
	{
		string input = "{}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));
		Assert.That(formatted, Is.Not.Null);
	}

	[Test]
	public void TryFormat_EmptyArray()
	{
		string input = "[]";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));
		Assert.That(formatted, Is.Not.Null);
	}

	[Test]
	public void TryFormat_NewlinesInStructure()
	{
		string input = "{\n\"name\": \"value\",\n\"number\": 123\n}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));

		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("name"));
		Assert.That(formatted.Contains("value"));
	}

	#endregion

	#region TryFormatUnescaped Tests (Relaxed Escaping)

	[Test]
	public void TryFormatUnescaped_Apostrophe()
	{
		// With UnsafeRelaxedJsonEscaping, apostrophes should NOT be escaped
		string input = "{\"description\": \"Earth's moon\"}";
		Assert.That(JsonUtils.TryFormatUnescaped(input, out string? formatted));
		
		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("Earth's moon"), "Apostrophe should remain unescaped");
		Assert.That(formatted.Contains("\\u0027"), Is.False, "Should not contain escaped apostrophe");
	}

	[Test]
	public void TryFormatUnescaped_PlusSign()
	{
		// With UnsafeRelaxedJsonEscaping, plus signs should NOT be escaped
		string input = "{\"name\": \"+\"}";
		Assert.That(JsonUtils.TryFormatUnescaped(input, out string? formatted));
		
		string expected = "{" + Environment.NewLine + "  \"name\": \"+\"" + Environment.NewLine + "}";
		Assert.That(formatted, Is.EqualTo(expected), "Plus sign should remain unescaped");
	}

	[Test]
	public void TryFormatUnescaped_NewlinesInsideStrings()
	{
		// Test that newlines inside string values are properly escaped
		string input = "{\"message\": \"Line 1\nLine 2\"}";
		Assert.That(JsonUtils.TryFormatUnescaped(input, out string? formatted));

		// The formatted output should preserve the escaped newline
		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("\\n") || formatted.Contains("\\u000a") || formatted.Contains("\\u000A"));
	}

	[Test]
	public void TryFormatUnescaped_NewlinesOutsideStrings()
	{
		// Test that actual newlines in the JSON structure (outside strings) are handled
		string input = "{\n\"name\": \"value\",\n\"number\": 123\n}";
		Assert.That(JsonUtils.TryFormatUnescaped(input, out string? formatted));

		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("name"));
		Assert.That(formatted.Contains("value"));
	}

	[Test]
	public void TryFormatUnescaped_ComplexJsonWithNewlines()
	{
		// Test complex JSON with various newline scenarios
		string input = "{\n  \"text\": \"Hello\\nWorld\",\n  \"items\": [\n    \"item1\",\n    \"item2\"\n  ]\n}";
		Assert.That(JsonUtils.TryFormatUnescaped(input, out string? formatted));

		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("text"));
		Assert.That(formatted.Contains("items"));
	}

	[Test]
	public void TryFormatUnescaped_PlusSignInMultipleValues()
	{
		// With UnsafeRelaxedJsonEscaping, plus signs should NOT be escaped
		string input = "{\"operation\": \"+\", \"formula\": \"a+b\", \"positive\": \"+1\"}";
		Assert.That(JsonUtils.TryFormatUnescaped(input, out string? formatted));
		
		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("\"+\""), "Plus signs should remain unescaped");
		Assert.That(formatted.Contains("\"a+b\""), "Plus signs should remain unescaped");
		Assert.That(formatted.Contains("\"+1\""), "Plus signs should remain unescaped");
	}

	[Test]
	public void TryFormatUnescaped_CarriageReturnAndNewline()
	{
		// Test CRLF sequences
		string input = "{\"message\": \"Line 1\\r\\nLine 2\"}";
		Assert.That(JsonUtils.TryFormatUnescaped(input, out string? formatted));

		Assert.That(formatted, Is.Not.Null);
	}

	[Test]
	public void TryFormatUnescaped_SpecialCharactersCombination()
	{
		// With UnsafeRelaxedJsonEscaping, plus signs should NOT be escaped, but control chars should
		string input = "{\"data\": \"Value: +123\\nNext line\", \"symbol\": \"+\"}";
		Assert.That(JsonUtils.TryFormatUnescaped(input, out string? formatted));
		
		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("Value: +123"), "Plus signs should remain unescaped");
		Assert.That(formatted.Contains("\\n"), "Newline escape should be preserved");
		Assert.That(formatted.Contains("\"+\""), "Plus sign should remain unescaped");
	}

	[Test]
	public void TryFormatUnescaped_BasicFormatting()
	{
		// Test basic JSON formatting
		string input = "{\"name\":\"John\",\"age\":30}";
		Assert.That(JsonUtils.TryFormatUnescaped(input, out string? formatted));

		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("name"));
		Assert.That(formatted.Contains("John"));
	}

	[Test]
	public void TryFormatUnescaped_InvalidJson()
	{
		// Test that invalid JSON returns false
		string input = "{invalid json}";
		Assert.That(JsonUtils.TryFormatUnescaped(input, out string? formatted), Is.False);
		Assert.That(formatted, Is.Null);
	}

	[Test]
	public void TryFormatUnescaped_EmptyObject()
	{
		string input = "{}";
		Assert.That(JsonUtils.TryFormatUnescaped(input, out string? formatted));
		Assert.That(formatted, Is.Not.Null);
	}

	[Test]
	public void TryFormatUnescaped_EmptyArray()
	{
		string input = "[]";
		Assert.That(JsonUtils.TryFormatUnescaped(input, out string? formatted));
		Assert.That(formatted, Is.Not.Null);
	}

	[Test]
	public void TryFormatUnescaped_ActualNewlineInStringValue()
	{
		// Test with an actual unescaped newline in a string value (invalid JSON per spec)
		string input = "{\"name\": \"value\n1\"}";  // Actual newline character, not \n
		
		// This is technically invalid JSON (unescaped control characters in strings)
		// The EscapeUnescapedControlCharactersInStrings fallback should handle this
		bool result = JsonUtils.TryFormatUnescaped(input, out string? formatted);
		
		Assert.That(result, Is.True);
		string expected = "{" + Environment.NewLine + "  \"name\": \"value\\n1\"" + Environment.NewLine + "}";
		Assert.That(formatted, Is.EqualTo(expected));
	}

	#endregion
}
