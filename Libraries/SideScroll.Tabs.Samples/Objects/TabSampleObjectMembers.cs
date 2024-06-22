using SideScroll.Core;

namespace SideScroll.Tabs.Samples.Objects;

public class TabSampleObjectMembers : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private readonly SampleMembers _sampleMembers = new();

		public override void Load(Call call, TabModel model)
		{
			model.ReloadOnThemeChange = true;
			model.Items = ListMember.Create(_sampleMembers);
		}
	}
}

public class SampleMembers
{
	public static readonly string? StaticStringField = "Static Value";

	public bool BoolProperty => true;
	public bool BoolField = false;
	[Item]
	public bool BoolMethod() => false;

	public string? StringProperty => "Properties will show in both Rows and Columns by default, and will be cached the first time they're needed";
	public string? StringField = "Fields only show as Rows and not in Columns";
	[Item]
	public string StringMethod() => "Method results will be cached the first time they're needed";

	public MyClass ObjectProperty => new();
	public MyClass ObjectField = new();

	public MyClass? NullObjectProperty { get; set; }
	public MyClass? NullObjectField;
}
