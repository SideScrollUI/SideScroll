using SideScroll.Attributes;
using SideScroll.Resources;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SideScroll.Tabs.Samples.Models;

public class SolarSystem
{
	public string? Star { get; set; }
	public List<string> Links { get; set; } = [];
	public List<Planet> Planets { get; set; } = [];

	public static SolarSystem Sample => JsonSerializer.Deserialize<SolarSystem>(TextSamples.Json)!;
}

public class Planet
{
	[Required]
	public string? Name { get; set; }
	[HiddenColumn]
	public string? Description { get; set; }

	public long? DistanceKm { get; set; }
	[ColumnIndex(2)]
	public double? RadiusKm { get; set; }

	[HiddenColumn] // Todo: improve column formatting for very large numbers
	public double? MassKg { get; set; }
	[ColumnIndex(2)]
	public double? GravityM_s2 { get; set; }

	public int? Moons { get; set; }
	[ColumnIndex(2)]
	public double? OrbitalPeriodDays { get; set; }

	public bool? Inner { get; set; }

	public override string? ToString() => Name;
}
