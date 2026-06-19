using SideScroll.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SideScroll.Tabs.Samples.Forms;

[PublicData]
public class SampleValidationItem
{
	[Header("Astronomer")]
	[DataKey, Required, StringLength(30)]
	public string? Name { get; set; }

	// At least one of the [RequiredGroup("Discovery")] properties below must be named.
	// If they are all empty, the form flags every member of the group.
	[Header("Discovery (name at least one)")]
	[RequiredGroup("Discovery"), Watermark("Proxima Centauri")]
	public string? Star { get; set; }

	[RequiredGroup("Discovery"), Watermark("Kepler-442b")]
	public string? Planet { get; set; }

	[RequiredGroup("Discovery"), Watermark("Halley")]
	public string? Comet { get; set; }

	public override string? ToString() => Name;
}
