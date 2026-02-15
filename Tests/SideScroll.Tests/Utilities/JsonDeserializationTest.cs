using System.Text.Json;
using NUnit.Framework;

namespace SideScroll.Tests.Utilities;

[TestFixture]
public class JsonDeserializationTest
{
	[Test]
	public void TestDeserializationWithoutEncoder()
	{
		// JSON with ACTUAL unescaped newline (technically invalid JSON) - this is what the comment was about
		string json = @"{""name"": ""value
1"", ""data"": ""+test""}";  // Actual newline character in the string
		
		// Deserialize WITHOUT encoder settings (like HttpMemoryCache currently does)
		var optionsWithoutEncoder = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
		};
		
		var resultWithout = JsonSerializer.Deserialize<TestObject>(json, optionsWithoutEncoder);
		
		Assert.That(resultWithout, Is.Not.Null);
		Assert.That(resultWithout!.Name, Is.EqualTo("value\n1")); // Should have actual newline
		Assert.That(resultWithout.Data, Is.EqualTo("+test"));
		
		Console.WriteLine($"Without encoder - Name: '{resultWithout.Name}'");
		Console.WriteLine($"Without encoder - Data: '{resultWithout.Data}'");
	}
	
	[Test]
	public void TestDeserializationWithEncoder()
	{
		// Same JSON with ACTUAL unescaped newline
		string json = @"{""name"": ""value
1"", ""data"": ""+test""}";  // Actual newline character in the string
		
		// Deserialize WITH encoder settings
		var optionsWithEncoder = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		};
		
		var resultWith = JsonSerializer.Deserialize<TestObject>(json, optionsWithEncoder);
		
		Assert.That(resultWith, Is.Not.Null);
		Assert.That(resultWith!.Name, Is.EqualTo("value\n1")); // Should have actual newline
		Assert.That(resultWith.Data, Is.EqualTo("+test"));
		
		Console.WriteLine($"With encoder - Name: '{resultWith.Name}'");
		Console.WriteLine($"With encoder - Data: '{resultWith.Data}'");
	}
	
	[Test]
	public void TestSerializationComparison()
	{
		var obj = new TestObject { Name = "value\n1", Data = "+test" };
		
		// Serialize WITHOUT encoder
		var optionsWithoutEncoder = new JsonSerializerOptions
		{
			WriteIndented = true,
		};
		string jsonWithout = JsonSerializer.Serialize(obj, optionsWithoutEncoder);
		Console.WriteLine("Without encoder:");
		Console.WriteLine(jsonWithout);
		
		// Serialize WITH encoder
		var optionsWithEncoder = new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		};
		string jsonWith = JsonSerializer.Serialize(obj, optionsWithEncoder);
		Console.WriteLine("\nWith encoder:");
		Console.WriteLine(jsonWith);
		
		// This should show the difference - encoder affects OUTPUT
		Assert.That(jsonWithout, Is.Not.EqualTo(jsonWith));
	}
	
	[Test]
	public void TestDeserializationWithPreprocessing()
	{
		// JSON with ACTUAL unescaped newline (invalid JSON)
		string invalidJson = @"{""name"": ""value
1"", ""data"": ""+test""}";
		
		// Pre-process to escape control characters
		string validJson = EscapeControlCharacters(invalidJson);
		Console.WriteLine($"Original: {invalidJson.Replace("\r", "\\r").Replace("\n", "\\n")}");
		Console.WriteLine($"Escaped: {validJson}");
		
		var options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
		};
		
		var result = JsonSerializer.Deserialize<TestObject>(validJson, options);
		
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Name, Is.EqualTo("value\n1")); // Should have actual newline in the object
		Assert.That(result.Data, Is.EqualTo("+test"));
	}
	
	/// <summary>
	/// Escapes control characters in JSON strings to make invalid JSON valid
	/// This is a workaround for legacy systems that produce malformed JSON
	/// </summary>
	private static string EscapeControlCharacters(string json)
	{
		return json
			.Replace("\r\n", "\\n")  // CRLF -> \n
			.Replace("\r", "\\n")     // CR -> \n  
			.Replace("\n", "\\n")     // LF -> \n
			.Replace("\t", "\\t");    // Tab -> \t
	}
	
	private class TestObject
	{
		public string Name { get; set; } = "";
		public string Data { get; set; } = "";
	}
}
