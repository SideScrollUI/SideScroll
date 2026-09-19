using SideScroll.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SideScroll.Tabs.Samples.Forms;

[PublicData]
public class SampleValidationItem
{
	// [AllowedChars] takes every character that can be typed, built up here from the attribute's constants.
	// It only filters typing, so a pasted value can still hold other characters
	private const string PersonNameChars = AllowedCharsAttribute.Letters + " .'-";
	private const string DesignationChars = AllowedCharsAttribute.Letters + AllowedCharsAttribute.Digits + " -/";

	[Header("Astronomer")]
	[DataKey, Required, StringLength(30), AllowedChars(PersonNameChars)]
	public string? Name { get; set; }

	// At least one of the [RequiredGroup("Discovery")] properties below must be named.
	// If they are all empty, the form flags every member of the group.
	[Header("Discovery (name at least one)")]
	[RequiredGroup("Discovery"), Watermark("Proxima Centauri"), AllowedChars(DesignationChars)]
	public string? Star { get; set; }

	[RequiredGroup("Discovery"), Watermark("Kepler-442b"), AllowedChars(DesignationChars)]
	public string? Planet { get; set; }

	[RequiredGroup("Discovery"), Watermark("1P/Halley"), AllowedChars(DesignationChars)]
	public string? Comet { get; set; }

	public override string? ToString() => Name;
}
