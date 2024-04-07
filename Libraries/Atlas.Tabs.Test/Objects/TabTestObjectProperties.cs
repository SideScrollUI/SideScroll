using Atlas.Core;
using System.ComponentModel;

namespace Atlas.Tabs.Test.Objects;

public class TabTestObjectProperties : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private readonly PropertyTest _propertyTest = new();

		public override void Load(Call call, TabModel model)
		{
			model.ReloadOnThemeChange = true;
			model.Items = ListProperty.Create(_propertyTest);
			model.Editing = true;

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Toggle", Toggle),
			};
		}

		private void Toggle(Call call)
		{
			_propertyTest.Boolean = !_propertyTest.Boolean;
		}
	}
}

public class PropertyTest : INotifyPropertyChanged
{
	[Editing]
	public bool Boolean
	{
		get => _boolean;
		set
		{
			_boolean = value;
			NotifyPropertyChangedContext(nameof(Boolean));
		}
	}
	private bool _boolean;

	public event PropertyChangedEventHandler? PropertyChanged;

	private void NotifyPropertyChangedContext(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public List<int> List { get; set; } = [1, 2, 3];
}
