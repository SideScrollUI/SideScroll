using SideScroll.Core;

namespace SideScroll.Tabs.Samples.DataGrid;

[ListItem]
public class TabSampleArray
{
	public string[] StringArray =>
	[
		"Item 1",
		"Item 2",
		"Item 3",
	];

	public Pet[] Classes =>
	[
		new("Casper"),
		new("Panda Bear"),
	];

	// todo: this currently shows as a single column
	public int[,] MultiDimensional => new [,]
	{
		{ 1, 2 },
		{ 3, 4 },
	};

	public bool[,] SingleItem => new bool[1,1];

	public class Pet(string name)
	{
		public string Name { get; set; } = name;

		public override string ToString() => Name;
	}
}
