using Atlas.Core;
using System.Threading;

namespace Atlas.Tabs.Test.Loading;

public class TabTestSlowLoad : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			Thread.Sleep(5000);

			model.AddObject("Finished");
		}
	}
}
