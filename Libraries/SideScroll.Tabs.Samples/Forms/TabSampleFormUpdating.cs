using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Forms;

public class TabSampleFormUpdating : ITab
{
	public TabInstance Create() => new Instance();

	private class Toolbar : TabToolbar
	{
		public ToolButton ButtonRandomize { get; } = new ToolButton("Randomize", Icons.Svg.Refresh);
	}

	private class Instance : TabInstance
	{
		private readonly SynchronizationContext _context = SynchronizationContext.Current ?? new();

		private SampleItemDataBinding? _sampleItem;
		private readonly Random _random = new();

		public override void Load(Call call, TabModel model)
		{
			_sampleItem = new SampleItemDataBinding(_context);
			Randomize(call);
			model.AddForm(_sampleItem!);

			Toolbar toolbar = new();
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
