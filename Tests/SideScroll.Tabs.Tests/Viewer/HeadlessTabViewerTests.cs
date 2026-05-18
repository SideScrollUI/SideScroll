using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Viewer;

namespace SideScroll.Tabs.Tests.Viewer;

[Category("Tabs")]
public class HeadlessTabViewerTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("HeadlessTabViewer");
	}

	// ─── Test stubs ───────────────────────────────────────────────────────────

	/// <summary>
	/// Simple <see cref="ITab"/> that puts its child tabs into the model's item list.
	/// </summary>
	private class NamedTab(string name, params ITab[] children) : ITab
	{
		public override string ToString() => name;

		public TabInstance Create() => new Instance(name, children);

		private class Instance(string name, ITab[] children) : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Name = name;
				model.Items = children.Cast<object>().ToList();
			}
		}
	}

	/// <summary>
	/// <see cref="ITabCreatorAsync"/> that lazily creates a <see cref="NamedTab"/>.
	/// </summary>
	private class AsyncCreatorItem(string name) : ITabCreatorAsync
	{
		public override string ToString() => name;

		public Task<ITab?> CreateAsync(Call call) =>
			Task.FromResult<ITab?>(new NamedTab(name));
	}

	/// <summary>
	/// <see cref="ILoadAsync"/> that returns a simple string result.
	/// </summary>
	private class StringLoader(string value) : ILoadAsync
	{
		public override string ToString() => value;

		public Task<object?> LoadAsync(Call call) =>
			Task.FromResult<object?>(value);
	}

	/// <summary>
	/// <see cref="TabInstanceAsync"/> subclass that populates a model item asynchronously.
	/// </summary>
	private class AsyncTab(string name) : ITab
	{
		public override string ToString() => name;

		public TabInstance Create() => new Instance(name);

		private class Instance(string name) : TabInstanceAsync
		{
			public override async Task LoadAsync(Call call, TabModel model)
			{
				await Task.Yield(); // simulate async work
				model.Name = name;
				model.AddObject(name + "-data");
			}
		}
	}

	// ─── IsHeadless flag ──────────────────────────────────────────────────

	/// <summary>
	/// Tab that does a slow operation unless <see cref="TabInstance.IsHeadless"/> is set.
	/// </summary>
	private class SlowTab : ITab
	{
		public override string ToString() => "Slow";

		public TabInstance Create() => new Instance();

		private class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				if (!IsHeadless)
					Thread.Sleep(5_000); // would make the test very slow

				model.Name = "Slow";
				model.AddObject("done");
			}
		}
	}

	[Test, Description(
		"HeadlessTabView.LoadAsync sets IsHeadless=true before calling Load, " +
		"allowing tabs to skip slow or UI-only operations.")]
	public async Task LoadTabAsync_SetsIsHeadless_AllowsTabToSkipSlowOperations()
	{
		var viewer = new HeadlessTabViewer(new Project());
		var sw = System.Diagnostics.Stopwatch.StartNew();

		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new SlowTab());
		sw.Stop();

		Assert.That(rootView.Instance.IsHeadless, Is.True,
			"HeadlessTabView.LoadAsync must set IsHeadless = true on the instance.");
		Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2_000),
			"Tab should have skipped the Thread.Sleep because IsHeadless was true.");
		Assert.That(rootView.Model.Objects, Has.Count.EqualTo(1));
	}

	[Test, Description(
		"IsHeadless propagates to child tab instances so nested tabs can also skip slow ops.")]
	public async Task LoadTabAsync_IsHeadless_PropagatestoChildViews()
	{
		var viewer = new HeadlessTabViewer(new Project());
		var root = new NamedTab("Root", new SlowTab());

		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, root);
		await rootView.SelectAllItemsAsync(Call);

		Assert.That(rootView.ChildViews, Has.Count.EqualTo(1));
		Assert.That(rootView.ChildViews[0].Instance.IsHeadless, Is.True,
			"IsHeadless should be set on child tab instances too.");
	}

	// ─── HeadlessTabViewer.LoadTabAsync ────────────────────────────────────

	[Test]
	public async Task LoadTabAsync_SimpleITab_CreatesRootView()
	{
		var viewer = new HeadlessTabViewer(new Project());
		var tab = new NamedTab("Root");

		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, tab);

		Assert.That(rootView, Is.Not.Null);
		Assert.That(rootView.Label, Is.EqualTo("Root"));
		Assert.That(rootView.ChildViews, Is.Empty);
		Assert.That(viewer.RootView, Is.SameAs(rootView));
	}

	[Test]
	public async Task LoadTabAsync_AsyncTabInstanceAsync_LoadsModelViaLoadAsync()
	{
		var viewer = new HeadlessTabViewer(new Project());
		var tab = new AsyncTab("AsyncRoot");

		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, tab);

		Assert.That(rootView, Is.Not.Null);
		Assert.That(rootView.Model, Is.Not.Null);
		Assert.That(rootView.Model.Name, Is.EqualTo("AsyncRoot"));
	}

	// ─── SelectAllItemsAsync / SelectItemsAsync with ITab children ─────────

	[Test]
	public async Task SelectAllItemsAsync_ITabChildren_CreatesChildViews()
	{
		var viewer = new HeadlessTabViewer(new Project());
		var root = new NamedTab("Root",
			new NamedTab("Child1"),
			new NamedTab("Child2"));

		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, root);
		await rootView.SelectAllItemsAsync(Call);

		Assert.That(rootView.ChildViews, Has.Count.EqualTo(2));
		Assert.That(rootView.ChildViews[0].Label, Is.EqualTo("Child1"));
		Assert.That(rootView.ChildViews[1].Label, Is.EqualTo("Child2"));
	}

	[Test]
	public async Task SelectAllItemsAsync_NoChildren_ChildViewsRemainsEmpty()
	{
		var viewer = new HeadlessTabViewer(new Project());
		var root = new NamedTab("Root");

		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, root);
		await rootView.SelectAllItemsAsync(Call);

		Assert.That(rootView.ChildViews, Is.Empty);
	}

	// ─── TryCreateChildViewAsync with ITabCreatorAsync ─────────────────────

	[Test]
	public async Task TryCreateChildViewAsync_ITabCreatorAsync_ResolvesAsyncAndCreatesView()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new NamedTab("Root"));

		var asyncItem = new AsyncCreatorItem("AsyncChild");
		HeadlessTabView? child = await rootView.TryCreateChildViewAsync(Call, asyncItem);

		Assert.That(child, Is.Not.Null);
		Assert.That(child!.Label, Is.EqualTo("AsyncChild"));
	}

	[Test]
	public async Task SelectItemsAsync_ITabCreatorAsyncItems_CreatesChildViews()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new NamedTab("Root"));

		await rootView.SelectItemsAsync(Call, new List<ITabCreatorAsync>
		{
			new AsyncCreatorItem("Async1"),
			new AsyncCreatorItem("Async2"),
		});

		Assert.That(rootView.ChildViews, Has.Count.EqualTo(2));
		Assert.That(rootView.ChildViews[0].Label, Is.EqualTo("Async1"));
		Assert.That(rootView.ChildViews[1].Label, Is.EqualTo("Async2"));
	}

	// ─── TryCreateChildViewAsync with ILoadAsync ───────────────────────────

	[Test]
	public async Task TryCreateChildViewAsync_ILoadAsync_CreatesChildView()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new NamedTab("Root"));

		var loader = new StringLoader("loaded-value");
		HeadlessTabView? child = await rootView.TryCreateChildViewAsync(Call, loader);

		Assert.That(child, Is.Not.Null);
		Assert.That(child!.Label, Is.EqualTo("loaded-value"));
	}

	[Test]
	public async Task SelectItemsAsync_ILoadAsyncItems_CreatesChildViews()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new NamedTab("Root"));

		await rootView.SelectItemsAsync(Call, new List<ILoadAsync>
		{
			new StringLoader("Item1"),
			new StringLoader("Item2"),
		});

		Assert.That(rootView.ChildViews, Has.Count.EqualTo(2));
		Assert.That(rootView.ChildViews[0].Label, Is.EqualTo("Item1"));
		Assert.That(rootView.ChildViews[1].Label, Is.EqualTo("Item2"));
	}

	// ─── TryCreateChildViewAsync – null / bool short-circuits ─────────────

	[Test]
	public async Task TryCreateChildViewAsync_BoolValue_ReturnsNull()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new NamedTab("Root"));

		HeadlessTabView? child = await rootView.TryCreateChildViewAsync(Call, false);

		Assert.That(child, Is.Null);
	}

	// ─── ListItem inner-value diagnostics ────────────────────────────────

	/// <summary>
	/// A plain "section" class that is NOT <see cref="ITab"/>, <see cref="ITabCreatorAsync"/>,
	/// or <see cref="ILoadAsync"/>. In the full Avalonia UI these classes are navigated to via
	/// reflected properties (e.g. decorated with <c>[ListItem]</c>), but the headless viewer
	/// cannot resolve them because they carry no navigation interface.
	/// Examples from production: <c>TabSamples</c>, <c>TabCustomCharts</c>.
	/// </summary>
	private class PlainSectionClass
	{
		public NamedTab SubItem1 { get; } = new("SubItem1");
		public NamedTab SubItem2 { get; } = new("SubItem2");
	}

	/// <summary>
	/// Reproduces the <c>TabAvaloniaSamples</c> structure:
	/// 5 <see cref="ListItem"/>s where 2 wrap non-<see cref="ITab"/> plain classes.
	/// </summary>
	private class TabLike_AvaloniaSamples : ITab
	{
		public override string ToString() => "AvaloniaSamples";

		public TabInstance Create() => new Instance();

		private class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new List<ListItem>
				{
					// Samples: plain class with [ListItem]-style properties — NOT ITab
					new("Samples",  new PlainSectionClass()),
					// Controls: proper ITab ✓
					new("Controls", new NamedTab("Controls")),
					// Charts: another plain class — NOT ITab (mirrors TabCustomCharts)
					new("Charts",   new PlainSectionClass()),
					// Links: proper ITab ✓
					new("Links",    new NamedTab("Links")),
					// Settings: proper ITab ✓
					new("Settings", new NamedTab("Settings")),
				};
			}
		}
	}

	[Test, Description(
		"Mirrors the TabAvaloniaSamples structure: 5 ListItems where 2 wrap plain classes " +
		"(TabSamples / TabCustomCharts). All 5 should appear in the schema — plain-object items " +
		"become leaf nodes (no children) so that the schema is complete.")]
	public async Task SelectAllItemsAsync_ListItemWrappingPlainObject_AllItemsAppearAsChildren()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new TabLike_AvaloniaSamples());

		Assert.That(rootView.Model.ItemList.Sum(l => l.Count), Is.EqualTo(5),
			"Model should contain all 5 ListItem entries");

		await rootView.SelectAllItemsAsync(Call);

		Assert.That(rootView.ChildViews, Has.Count.EqualTo(5),
			"Every ListItem should produce a child view; plain-object items become leaf nodes.");

		Assert.That(rootView.ChildViews[0].Label, Is.EqualTo("Samples"),   "Samples (plain class) → leaf node");
		Assert.That(rootView.ChildViews[1].Label, Is.EqualTo("Controls"),  "Controls (ITab) → navigable child");
		Assert.That(rootView.ChildViews[2].Label, Is.EqualTo("Charts"),    "Charts (plain class) → leaf node");
		Assert.That(rootView.ChildViews[3].Label, Is.EqualTo("Links"),     "Links (ITab) → navigable child");
		Assert.That(rootView.ChildViews[4].Label, Is.EqualTo("Settings"),  "Settings (ITab) → navigable child");
	}

	[Test, Description(
		"A ListItem wrapping a plain complex object produces an expandable node. " +
		"TabModel.AddData is called which populates the model with object members. " +
		"ChildViews is empty until SelectAllItemsAsync is explicitly called.")]
	public async Task TryCreateChildViewAsync_ListItemWithPlainObject_ReturnsLeafNodeWithNoChildren()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new NamedTab("Root"));

		var item = new ListItem("Samples", new PlainSectionClass());
		HeadlessTabView? child = await rootView.TryCreateChildViewAsync(Call, item);

		Assert.That(child, Is.Not.Null,
			"A plain-object ListItem should produce a child view, not null.");
		Assert.That(child!.Label, Is.EqualTo("Samples"));
		Assert.That(child.ChildViews, Is.Empty,
			"ChildViews empty until SelectAllItemsAsync is called.");
	}

	// ─── [ListItem]-decorated classes ────────────────────────────────────

	/// <summary>
	/// Aggregator class decorated with <c>[ListItem]</c>: its public properties are reflected
	/// by <see cref="TabModel.AddData"/> (via <see cref="IListItem.Create"/>) into a navigable
	/// item collection — exactly like <c>TabSamples</c> in the Avalonia samples.
	/// </summary>
	[ListItem]
	private class ListItemSection
	{
		public NamedTab Child1 { get; } = new("Child1");
		public NamedTab Child2 { get; } = new("Child2");
	}

	[Test, Description(
		"A ListItem wrapping a [ListItem]-decorated class should expand the class's reflected " +
		"properties into Model.ItemList entries when LoadAsync is called.")]
	public async Task TryCreateChildViewAsync_ListItemDecoratedClass_PopulatesModelItems()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new NamedTab("Root"));

		var item = new ListItem("Samples", new ListItemSection());
		HeadlessTabView? child = await rootView.TryCreateChildViewAsync(Call, item);

		Assert.That(child, Is.Not.Null);
		Assert.That(child!.Label, Is.EqualTo("Samples"));
		Assert.That(child.Model.ItemList.Sum(l => l.Count), Is.EqualTo(2),
			"Two reflected properties (Child1, Child2) should appear as items in the model.");
	}

	[Test, Description(
		"Calling SelectAllItemsAsync on a [ListItem]-expanded node should create grandchildren " +
		"for each ITab property – mirrors how the Avalonia UI navigates into TabSamples.")]
	public async Task SelectAllItemsAsync_ListItemDecoratedClass_CreatesGrandchildren()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new NamedTab("Root"));

		var item = new ListItem("Samples", new ListItemSection());
		HeadlessTabView? samplesView = await rootView.TryCreateChildViewAsync(Call, item);

		Assert.That(samplesView, Is.Not.Null);

		await samplesView!.SelectAllItemsAsync(Call);

		Assert.That(samplesView.ChildViews, Has.Count.EqualTo(2),
			"Both ITab properties of ListItemSection should become child views.");
		Assert.That(samplesView.ChildViews.Select(c => c.Label),
			Is.EquivalentTo(new[] { "Child1", "Child2" }),
			"Labels should match the NamedTab names.");
	}

	[Test, Description("Confirms ListItems wrapping ITab values DO create navigable child views.")]
	public async Task TryCreateChildViewAsync_ListItemWithITabValue_CreatesChildView()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new NamedTab("Root"));

		var item = new ListItem("Controls", new NamedTab("Controls"));
		HeadlessTabView? child = await rootView.TryCreateChildViewAsync(Call, item);

		Assert.That(child, Is.Not.Null);
		Assert.That(child!.Label, Is.EqualTo("Controls"));
	}

	// ─── Nested depth (SelectAllItemsAsync – one level) ───────────────────

	[Test]
	public async Task SelectAllItemsAsync_NestedTabs_TraversesOneLevelAtATime()
	{
		var viewer = new HeadlessTabViewer(new Project());
		var grandchild = new NamedTab("Grandchild");
		var child = new NamedTab("Child", grandchild);
		var root = new NamedTab("Root", child);

		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, root);
		await rootView.SelectAllItemsAsync(Call);

		Assert.That(rootView.ChildViews, Has.Count.EqualTo(1), "one child at root level");
		Assert.That(rootView.ChildViews[0].Label, Is.EqualTo("Child"));
		Assert.That(rootView.ChildViews[0].ChildViews, Is.Empty, "grandchildren not yet selected");

		await rootView.ChildViews[0].SelectAllItemsAsync(Call);
		Assert.That(rootView.ChildViews[0].ChildViews, Has.Count.EqualTo(1));
		Assert.That(rootView.ChildViews[0].ChildViews[0].Label, Is.EqualTo("Grandchild"));
	}

	// ─── SelectAllItemsRecursiveAsync ────────────────────────────────────

	/// <summary>Builds a linear chain of depth <paramref name="depth"/>: Root → L1 → L2 → … → Ln.</summary>
	private static NamedTab BuildChain(int depth)
	{
		NamedTab leaf = new("L" + depth);
		for (int i = depth - 1; i >= 1; i--)
			leaf = new NamedTab("L" + i, leaf);
		return new NamedTab("Root", leaf);
	}

	[Test, Description("maxDepth=0 means the root's children are never selected.")]
	public async Task SelectAllItemsRecursiveAsync_MaxDepthZero_NoChildrenSelected()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView root = await viewer.LoadTabAsync(Call, BuildChain(3));

		await root.SelectAllItemsRecursiveAsync(Call, maxDepth: 0);

		Assert.That(root.ChildViews, Is.Empty, "maxDepth=0 should not expand any level.");
	}

	[Test, Description("maxDepth=1 selects only direct children, same as SelectAllItemsAsync.")]
	public async Task SelectAllItemsRecursiveAsync_MaxDepthOne_OnlyDirectChildren()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView root = await viewer.LoadTabAsync(Call, BuildChain(3));

		await root.SelectAllItemsRecursiveAsync(Call, maxDepth: 1);

		Assert.That(root.ChildViews, Has.Count.EqualTo(1));
		Assert.That(root.ChildViews[0].Label, Is.EqualTo("L1"));
		Assert.That(root.ChildViews[0].ChildViews, Is.Empty,
			"maxDepth=1 should not recurse into grandchildren.");
	}

	[Test, Description("maxDepth=2 selects children and grandchildren.")]
	public async Task SelectAllItemsRecursiveAsync_MaxDepthTwo_SelectsGrandchildren()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView root = await viewer.LoadTabAsync(Call, BuildChain(3));

		await root.SelectAllItemsRecursiveAsync(Call, maxDepth: 2);

		Assert.That(root.ChildViews[0].Label, Is.EqualTo("L1"));
		Assert.That(root.ChildViews[0].ChildViews[0].Label, Is.EqualTo("L2"));
		Assert.That(root.ChildViews[0].ChildViews[0].ChildViews, Is.Empty,
			"maxDepth=2 should not reach L3.");
	}

	[Test, Description("Default maxDepth=5 traverses 5 levels into a deep chain.")]
	public async Task SelectAllItemsRecursiveAsync_DefaultMaxDepth_TraversesFiveLevels()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView root = await viewer.LoadTabAsync(Call, BuildChain(6));

		await root.SelectAllItemsRecursiveAsync(Call); // default maxDepth = 5

		HeadlessTabView node = root;
		for (int level = 1; level <= 5; level++)
		{
			Assert.That(node.ChildViews, Has.Count.EqualTo(1), $"level {level} should have 1 child");
			node = node.ChildViews[0];
			Assert.That(node.Label, Is.EqualTo("L" + level));
		}

		Assert.That(node.ChildViews, Is.Empty,
			"L5 should have no children — L6 is beyond the default maxDepth of 5.");
	}

	[Test, Description("A tree shallower than maxDepth is fully traversed without errors.")]
	public async Task SelectAllItemsRecursiveAsync_ShallowerThanMaxDepth_FullyTraversed()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView root = await viewer.LoadTabAsync(Call, BuildChain(3));

		await root.SelectAllItemsRecursiveAsync(Call, maxDepth: 5);

		HeadlessTabView l1 = root.ChildViews[0];
		HeadlessTabView l2 = l1.ChildViews[0];
		HeadlessTabView l3 = l2.ChildViews[0];

		Assert.That(l1.Label, Is.EqualTo("L1"));
		Assert.That(l2.Label, Is.EqualTo("L2"));
		Assert.That(l3.Label, Is.EqualTo("L3"));
		Assert.That(l3.ChildViews, Is.Empty, "Leaf node has no children.");
	}
}
