using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Params;

public class TabSampleParamUpdating : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRandomize { get; set; } = new ToolButton("Randomize", Icons.Svg.Refresh);
	}

	public class Instance : TabInstance
	{
		protected SynchronizationContext Context = SynchronizationContext.Current ?? new();

		private SampleParamItemDataBinding? _paramTestItem;
		private readonly Random _random = new();

		public override void Load(Call call, TabModel model)
		{
			_paramTestItem = new SampleParamItemDataBinding(Context);
			Randomize(call);
			model.AddObject(_paramTestItem!);

			var toolbar = new Toolbar();
			toolbar.ButtonRandomize.Action = Randomize;
			model.AddObject(toolbar);
		}

		private void Randomize(Call call)
		{
			while (true)
			{
				int value = _random.Next() % 3;

				if (value.ToString() == _paramTestItem?.Value) continue;

				_paramTestItem!.Value = value.ToString();
				break;
			}
		}
	}
}
