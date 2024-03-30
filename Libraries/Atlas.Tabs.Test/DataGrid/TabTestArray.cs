using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid;

[ListItem]
public class TabTestArray
{
	public string[] stringArray =>
	[
		"Item 1",
		"Item 2",
		"Item 3",
	];

	public Pet[] classes =>
	[
		new("Casper"),
		new("Panda Bear"),
	];

	// todo: this currently shows as a single column
	public int[,] multiDimensional => new [,]
	{
		{ 1, 2 },
		{ 3, 4 },
	};

	public class Pet(string name)
	{
		public string Name { get; set; } = name;

		public override string ToString() => Name;
	}
}
