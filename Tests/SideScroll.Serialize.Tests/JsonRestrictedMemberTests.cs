using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Serialize.Json;
using System.Text.Json;

namespace SideScroll.Serialize.Tests;

/// <summary>
/// The restrictions on what reaches a public export have to hold in both directions. Excluding a
/// member while writing and then accepting it while reading let crafted json populate the members a
/// public export deliberately omits
/// </summary>
[Category("Serialize")]
public class JsonRestrictedMemberTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("JsonRestrictedMembers");
	}

	[PublicData]
	public class PublicModel
	{
		public string? Allowed { get; set; }

		[PrivateData]
		public string? Private { get; set; }

		[Unserialized]
		public string? Unserialized { get; set; }
	}

	[ProtectedData]
	public class ProtectedModel
	{
		[PublicData]
		public string? Allowed { get; set; }

		public string? Restricted { get; set; }
	}

	private const string CraftedPublic = """
		{"Allowed":"ok","Private":"INJECTED","Unserialized":"INJECTED"}
		""";

	[Test, Description("A [PrivateData] member overrides the [PublicData] type around it in both directions")]
	public void PrivateMemberIsNotReadFromPublicJson()
	{
		var model = JsonSerializer.Deserialize<PublicModel>(CraftedPublic, JsonConverters.PublicSerializerOptions)!;

		Assert.That(model.Private, Is.Null);
		Assert.That(model.Allowed, Is.EqualTo("ok"), "The members a public export does include still load");
	}

	[Test, Description("An [Unserialized] member is never written, so naming one can't populate it either")]
	public void UnserializedMemberIsNotReadFromPublicJson()
	{
		var model = JsonSerializer.Deserialize<PublicModel>(CraftedPublic, JsonConverters.PublicSerializerOptions)!;

		Assert.That(model.Unserialized, Is.Null);
	}

	[Test, Description("A [ProtectedData] type only exports its [PublicData] members, so only those load")]
	public void RestrictedMemberOfProtectedTypeIsNotReadFromPublicJson()
	{
		string crafted = """{"Allowed":"ok","Restricted":"INJECTED"}""";

		var model = JsonSerializer.Deserialize<ProtectedModel>(crafted, JsonConverters.PublicSerializerOptions)!;

		Assert.That(model.Restricted, Is.Null);
		Assert.That(model.Allowed, Is.EqualTo("ok"));
	}

	[Test, Description("An [Unserialized] member stays excluded in both directions for private options too")]
	public void UnserializedMemberIsNotReadFromPrivateJson()
	{
		var model = JsonSerializer.Deserialize<PublicModel>(CraftedPublic, JsonConverters.PrivateSerializerOptions)!;

		Assert.That(model.Unserialized, Is.Null);
	}

	[Test, Description("Private options exist to keep [PrivateData], so they still round-trip it")]
	public void PrivateOptionsStillRoundTripPrivateMembers()
	{
		var original = new PublicModel { Allowed = "ok", Private = "kept" };

		string json = JsonSerializer.Serialize(original, JsonConverters.PrivateSerializerOptions);
		var model = JsonSerializer.Deserialize<PublicModel>(json, JsonConverters.PrivateSerializerOptions)!;

		Assert.That(json, Does.Contain("kept"));
		Assert.That(model.Private, Is.EqualTo("kept"));
	}

	[Test, Description("Writing is unchanged, the restricted members were already left out")]
	public void PublicJsonStillOmitsRestrictedMembers()
	{
		var model = new PublicModel { Allowed = "ok", Private = "secret", Unserialized = "internal" };

		string json = JsonSerializer.Serialize(model, JsonConverters.PublicSerializerOptions);

		Assert.That(json, Does.Contain("ok"));
		Assert.That(json, Does.Not.Contain("secret"));
		Assert.That(json, Does.Not.Contain("internal"));
	}

	public record RecordModel([property: PrivateData] string? Private, string? Allowed);

	[Test, Description(
		"Known gap: a member bound to a constructor parameter is assigned through the constructor, " +
		"which no JsonPropertyInfo governs, so clearing the setter doesn't reach it")]
	public void PrivateConstructorParameterIsStillRead()
	{
		string crafted = """{"Allowed":"ok","Private":"INJECTED"}""";

		var model = JsonSerializer.Deserialize<RecordModel>(crafted, JsonConverters.PublicSerializerOptions)!;

		Assert.That(model.Private, Is.EqualTo("INJECTED"));
		Assert.That(JsonSerializer.Serialize(model, JsonConverters.PublicSerializerOptions),
			Does.Not.Contain("INJECTED"), "Writing still excludes it");
	}
}
