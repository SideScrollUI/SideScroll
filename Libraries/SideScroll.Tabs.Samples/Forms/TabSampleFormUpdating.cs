using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Forms;

public class TabSampleFormUpdating : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRandomize { get; set; } = new ToolButton("Randomize", Icons.Svg.Refresh);
	}

	public class Instance : TabInstance
	{
		protected SynchronizationContext Context = SynchronizationContext.Current ?? new();

		private SampleItemDataBinding? _sampleItem;
		private readonly Random _random = new();

		public override void Load(Call call, TabModel model)
		{
			_sampleItem = new SampleItemDataBinding(Context);
			Randomize(call);
			model.AddObject(_sampleItem!, editable: true);

			var toolbar = new Toolbar();
			toolbar.ButtonRandomize.Action = Randomize;
			model.AddObject(toolbar);
		}

		private void Randomize(Call call)
		{
			while (true)
			{
				int value = _random.Next() % 3;

				if (value.ToString() == _sampleItem?.Value) continue;

				_sampleItem!.Value = value.ToString();
				break;
			}
		}
	}
}
