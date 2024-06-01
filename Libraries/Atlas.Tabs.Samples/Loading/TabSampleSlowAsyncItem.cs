using Atlas.Core;

namespace Atlas.Tabs.Samples.Loading;

public class TabSampleSlowAsyncItem : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new ItemCollection<IListItem>
			{
				new ListDelegate(SlowItemAsync),
			};
		}

		// todo: fix, this is being called twice and blocking the UI the 1st time
		// Preloading doesn't trigger for methods, and most results won't be cached
		// Need a new CollectionView that can preload and cache?
		private static async Task<object?> SlowItemAsync(Call call)
		{
			await Task.Delay(1000);
			return "finished";
		}
	}
}
