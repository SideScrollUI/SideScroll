using Atlas.Core;
using System.Threading.Tasks;

namespace Atlas.Tabs.Test.Loading
{
	public class TabTestSlowAsyncItem: ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new ItemCollection<IListItem>()
				{
					new ListDelegate(SlowItemAsync),
				};
			}

			// todo: fix, this is being called twice and blocking the UI the 1st time
			public async Task<object> SlowItemAsync(Call call)
			{
				await Task.Delay(1000);
				return "finished";
			}
		}
	}
}
