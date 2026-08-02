using NUnit.Framework;
using SideScroll.Attributes;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class TabInstancePreloadTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TabInstancePreload");
	}

	// Records that its property was read, so the assertion counts items rather than getter calls
	public class CountingItem
	{
		[Hidden]
		public bool WasRead { get; private set; }

		public int Value
		{
			get
			{
				WasRead = true;
				return 1;
			}
		}
	}

	private class CountingTab(List<CountingItem> items) : ITab
	{
		public TabInstance Create() => new Instance(items);

		private class Instance(List<CountingItem> items) : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.AddItems(items);
			}
		}
	}

	private static async Task<int> CountPreloadedAsync(int maxPreloadItems, int itemCount)
	{
		int original = TabInstance.MaxPreloadItems;
		try
		{
			TabInstance.MaxPreloadItems = maxPreloadItems;

			List<CountingItem> items = [.. Enumerable.Range(0, itemCount).Select(_ => new CountingItem())];
			TabInstance tabInstance = new CountingTab(items).Create();

			// ReinitializeAsync() adds a subtask after preloading, which needs a task instance
			Call call = new()
			{
				TaskInstance = new(),
			};
			await tabInstance.ReinitializeAsync(call);

			return items.Count(item => item.WasRead);
		}
		finally
		{
			TabInstance.MaxPreloadItems = original;
		}
	}

	[TestCase(3, 10, 3, TestName = "Stops at the maximum")]
	[TestCase(10, 3, 3, TestName = "Stops at the end of a shorter list")]
	[TestCase(0, 10, 0, TestName = "A maximum of zero preloads nothing")]
	[Description(
		"The count was incremented and tested after the property getters ran, so one item past the " +
		"maximum was always evaluated and a maximum of zero still preloaded a row")]
	public async Task PreloadStopsAtMaxPreloadItems(int maxPreloadItems, int itemCount, int expected)
	{
		Assert.That(await CountPreloadedAsync(maxPreloadItems, itemCount), Is.EqualTo(expected));
	}
}
