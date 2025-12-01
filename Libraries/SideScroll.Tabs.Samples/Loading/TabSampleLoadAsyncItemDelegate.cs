using SideScroll.Tabs.Lists;

namespace SideScroll.Tabs.Samples.Loading;

public class TabSampleLoadAsyncItemDelegate : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<IListItem>
			{
				new ListDelegate(SlowItemAsync),
			};
		}

		// todo: fix, this is being called twice and blocking the UI the 1st time
		// Preloading doesn't trigger for methods, and most results won't be cached
		// Need a new CollectionView that can preload and cache?
		private static async Task<object?> SlowItemAsync(Call call)
		{
			await Task.Delay(2000);
			return "finished";
		}
	}
}
