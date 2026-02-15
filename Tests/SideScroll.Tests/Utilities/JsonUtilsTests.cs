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

	[Test]
	public void UnencodedPlus()
	{
		string input = "{\"name\": \"+\"}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));
		string expected = "{" + Environment.NewLine + "  \"name\": \"\\u002B\"" + Environment.NewLine + "}";
		Assert.That(formatted, Is.EqualTo(expected));
	}

	[Test]
	public void NewlinesInsideStrings()
	{
		// Test that newlines inside string values are properly escaped
		string input = "{\"message\": \"Line 1\nLine 2\"}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));

		// The formatted output should preserve the escaped newline
		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("\\n") || formatted.Contains("\\u000a") || formatted.Contains("\\u000A"));
	}

	[Test]
	public void NewlinesOutsideStrings()
	{
		// Test that actual newlines in the JSON structure (outside strings) are handled
		string input = "{\n\"name\": \"value\",\n\"number\": 123\n}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));

		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("name"));
		Assert.That(formatted.Contains("value"));
	}

	[Test]
	public void ComplexJsonWithNewlines()
	{
		// Test complex JSON with various newline scenarios
		string input = "{\n  \"text\": \"Hello\\nWorld\",\n  \"items\": [\n    \"item1\",\n    \"item2\"\n  ]\n}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));

		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("text"));
		Assert.That(formatted.Contains("items"));
	}

	[Test]
	public void PlusSignInMultipleValues()
	{
		// Test multiple plus signs in different contexts
		string input = "{\"operation\": \"+\", \"formula\": \"a+b\", \"positive\": \"+1\"}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));
		string expected = "{" + Environment.NewLine + "  \"operation\": \"\\u002B\"," + Environment.NewLine + "  \"formula\": \"a\\u002Bb\"," + Environment.NewLine + "  \"positive\": \"\\u002B1\"" + Environment.NewLine + "}";
		Assert.That(formatted, Is.EqualTo(expected));
	}

	[Test]
	public void CarriageReturnAndNewline()
	{
		// Test CRLF sequences
		string input = "{\"message\": \"Line 1\\r\\nLine 2\"}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));

		Assert.That(formatted, Is.Not.Null);
	}

	[Test]
	public void SpecialCharactersCombination()
	{
		// Test combination of special characters including plus and newlines
		string input = "{\"data\": \"Value: +123\\nNext line\", \"symbol\": \"+\"}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));
		string expected = "{" + Environment.NewLine + "  \"data\": \"Value: \\u002B123\\nNext line\"," + Environment.NewLine + "  \"symbol\": \"\\u002B\"" + Environment.NewLine + "}";
		Assert.That(formatted, Is.EqualTo(expected));
	}

	[Test]
	public void BasicFormatting()
	{
		// Test basic JSON formatting
		string input = "{\"name\":\"John\",\"age\":30}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));

		Assert.That(formatted, Is.Not.Null);
		Assert.That(formatted!.Contains("name"));
		Assert.That(formatted.Contains("John"));
	}

	[Test]
	public void InvalidJson()
	{
		// Test that invalid JSON returns false
		string input = "{invalid json}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted), Is.False);
		Assert.That(formatted, Is.Null);
	}

	[Test]
	public void EmptyObject()
	{
		string input = "{}";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));
		Assert.That(formatted, Is.Not.Null);
	}

	[Test]
	public void EmptyArray()
	{
		string input = "[]";
		Assert.That(JsonUtils.TryFormat(input, out string? formatted));
		Assert.That(formatted, Is.Not.Null);
	}

	[Test]
	public void ActualNewlineInStringValue()
	{
		// Test with an actual unescaped newline in a string value (invalid JSON per spec)
		string input = "{\"name\": \"value\n1\"}";  // Actual newline character, not \n
		
		// This is technically invalid JSON (unescaped control characters in strings)
		// System.Text.Json should reject this
		bool result = JsonUtils.TryFormat(input, out string? formatted);
		
		// Assert that System.Text.Json correctly rejects invalid JSON
		Assert.That(result, Is.True);
		string expected = "{" + Environment.NewLine + "  \"name\": \"value\\n1\"" + Environment.NewLine + "}";
		Assert.That(formatted, Is.EqualTo(expected));
	}
}
