using Atlas.Core;
using Atlas.Resources;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Atlas.Tabs.Test.Models;

public class SolarSystem
{
	public string? Star { get; set; }
	public List<string> Links { get; set; } = new();
	public List<Planet> Planets { get; set; } = new();

	public static SolarSystem Sample => JsonSerializer.Deserialize<SolarSystem>(Samples.Text.Json)!;
}

public class Planet
{
	[Required]
	public string? Name { get; set; }
	[HiddenColumn]
	public string? Description { get; set; }
	public long? DistanceKm { get; set; }
	public double? RadiusKm { get; set; }
	[HiddenColumn] // Todo: improve column formatting for very large numbers
	public double? MassKg { get; set; }
	public double? GravityM_s2 { get; set; }
	public int? Moons { get; set; }
	public double? OrbitalPeriodDays { get; set; }
	public bool? Inner { get; set; }

	public override string? ToString() => Name;
}
