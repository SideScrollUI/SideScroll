using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Bookmarks.Tabs;
using SideScroll.Tabs.Headless;
using SideScroll.Tabs.Lists;
using SideScroll.Tasks;

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

		Assert.That(rootView.Model.ItemLists.Sum(l => l.Count), Is.EqualTo(5),
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
		Assert.That(child.Model.ItemLists.Sum(l => l.Count), Is.EqualTo(2),
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

	[Test, Description(
		"When a list exceeds MaxAllowedItems, the node is flagged ItemsTruncated and only the " +
		"capped number of items are listed; a list within the limit is not flagged.")]
	public async Task SelectAllItemsAsync_MaxAllowedItems_FlagsItemsTruncated()
	{
		var cappedViewer = new HeadlessTabViewer(new Project())
		{
			Options = new HeadlessTabOptions { MaxAllowedItems = 2 },
		};
		var root = new NamedTab("Root",
			new NamedTab("A"), new NamedTab("B"), new NamedTab("C"));

		HeadlessTabView rootView = await cappedViewer.LoadTabAsync(Call, root);
		await rootView.SelectAllItemsAsync(Call);

		Assert.That(rootView.ChildViews, Has.Count.EqualTo(2), "Only MaxAllowedItems items listed (and explored).");
		Assert.That(rootView.ItemsTruncated, Is.True, "A third item remained past the cap.");

		// A list exactly at the limit is not flagged.
		var exactViewer = new HeadlessTabViewer(new Project())
		{
			Options = new HeadlessTabOptions { MaxAllowedItems = 2 },
		};
		var exactRoot = new NamedTab("Root", new NamedTab("A"), new NamedTab("B"));

		HeadlessTabView exactView = await exactViewer.LoadTabAsync(Call, exactRoot);
		await exactView.SelectAllItemsAsync(Call);

		Assert.That(exactView.ChildViews, Has.Count.EqualTo(2));
		Assert.That(exactView.ItemsTruncated, Is.False,
			"A list ending exactly at the limit should not be flagged truncated.");
	}

	[Test, Description(
		"A non-allowlisted list lists its rows (up to MaxOtherItems) but only explores MaxOtherChildren " +
		"of them into child views.")]
	public async Task SelectAllItemsAsync_NonAllowlistedList_ListsItemsButSamplesChildren()
	{
		var viewer = new HeadlessTabViewer(new Project())
		{
			// MaxOtherItems defaults to 50, MaxOtherChildren defaults to 1.
			Options = new HeadlessTabOptions { AllowedElementTypes = [typeof(IListItem)] },
		};
		var root = new NamedTab("Root",
			new NamedTab("A"), new NamedTab("B"), new NamedTab("C"));

		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, root);
		await rootView.SelectAllItemsAsync(Call);

		Assert.That(rootView.ChildViews, Has.Count.EqualTo(1),
			"Only MaxOtherChildren rows should be explored into child views.");
		Assert.That(rootView.ListItems.Single().Value, Has.Count.EqualTo(3),
			"All rows should still be listed as items (labels are cheap and safe).");
		Assert.That(rootView.ItemsTruncated, Is.False,
			"3 rows is within MaxOtherItems (50), so items are not truncated.");
	}

	[Test, Description(
		"MaxOtherChildren = 0 explores no rows of a non-allowlisted list (no tab loads) while still " +
		"listing the item labels.")]
	public async Task SelectAllItemsAsync_NonAllowlistedList_ZeroOtherChildren_ExploresNone()
	{
		var viewer = new HeadlessTabViewer(new Project())
		{
			Options = new HeadlessTabOptions
			{
				AllowedElementTypes = [typeof(IListItem)],
				MaxOtherChildren = 0,
			},
		};
		var root = new NamedTab("Root", new NamedTab("A"), new NamedTab("B"));

		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, root);
		await rootView.SelectAllItemsAsync(Call);

		Assert.That(rootView.ChildViews, Is.Empty, "No rows explored when MaxOtherChildren is 0.");
		Assert.That(rootView.ListItems.Single().Value, Has.Count.EqualTo(2),
			"Rows are still listed as labels even when none are explored.");
	}

	/// <summary>Tab whose model holds a list of leaf (string) rows alongside no navigable children.</summary>
	private class StringListTab : ITab
	{
		public override string ToString() => "Strings";

		public TabInstance Create() => new Instance();

		private class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Name = "Strings";
				model.Items = new List<string> { "one", "two", "three" };
			}
		}
	}

	[Explorable]
	private class ExplorableRow(string name)
	{
		public string Name => name;
		public override string ToString() => name;
	}

	[Explorable(false)]
	private class NotExplorableRow(string name)
	{
		public string Name => name;
		public override string ToString() => name;
	}

	private class ExplorableRowTab(System.Collections.IList rows) : ITab
	{
		public TabInstance Create() => new Instance(rows);

		private class Instance(System.Collections.IList rows) : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Name = "Rows";
				model.Items = rows;
			}
		}
	}

	[Test, Description(
		"[Explorable] on the element type opts a non-allowlisted list in to full exploration, " +
		"overriding the allowlist.")]
	public async Task SelectAllItemsAsync_ExplorableAttribute_OverridesAllowlistToAllow()
	{
		var viewer = new HeadlessTabViewer(new Project())
		{
			// ExplorableRow is not an IListItem, so only the [Explorable] attribute makes it allowed.
			Options = new HeadlessTabOptions { AllowedElementTypes = [typeof(IListItem)], MaxOtherChildren = 1 },
		};
		var rows = new List<ExplorableRow> { new("A"), new("B"), new("C") };

		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new ExplorableRowTab(rows));
		await rootView.SelectAllItemsAsync(Call);

		Assert.That(rootView.ChildViews, Has.Count.EqualTo(3),
			"[Explorable] should make all rows explored despite MaxOtherChildren and the allowlist.");
	}

	[Test, Description(
		"[Explorable(false)] on the element type opts an allowlisted list out, so it is only sampled.")]
	public async Task SelectAllItemsAsync_ExplorableFalseAttribute_OverridesAllowlistToSample()
	{
		var viewer = new HeadlessTabViewer(new Project())
		{
			// No allowlist => normally all allowed; the attribute forces "other" (sampled) caps.
			Options = new HeadlessTabOptions { MaxOtherChildren = 1 },
		};
		var rows = new List<NotExplorableRow> { new("A"), new("B"), new("C") };

		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new ExplorableRowTab(rows));
		await rootView.SelectAllItemsAsync(Call);

		Assert.That(rootView.ChildViews, Has.Count.EqualTo(1),
			"[Explorable(false)] should force the sampled child cap even without an allowlist.");
		Assert.That(rootView.ListItems.Single().Value, Has.Count.EqualTo(3),
			"All rows are still listed as items.");
	}

	[Test, Description(
		"Leaf rows (strings/primitives) are listed as labels but never consume the child-exploration " +
		"budget, so a tiny MaxOtherChildren doesn't get spent on a leaf.")]
	public async Task SelectAllItemsAsync_LeafRows_DoNotConsumeChildBudget()
	{
		var viewer = new HeadlessTabViewer(new Project())
		{
			Options = new HeadlessTabOptions
			{
				AllowedElementTypes = [typeof(IListItem)], // string list is "other"
				MaxOtherChildren = 1,
			},
		};

		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new StringListTab());
		await rootView.SelectAllItemsAsync(Call);

		Assert.That(rootView.ListItems.Single().Value, Has.Count.EqualTo(3),
			"All three leaf strings should be listed as items.");
		Assert.That(rootView.ChildViews, Is.Empty,
			"Leaf rows should not be explored into child views, even with budget available.");
	}

	[Test, Description(
		"A node stopped at the depth limit with more to expand is flagged DepthTruncated, " +
		"while a genuine leaf is not.")]
	public async Task SelectAllItemsRecursiveAsync_DepthLimit_FlagsTruncatedVsLeaf()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView root = await viewer.LoadTabAsync(Call, BuildChain(3)); // Root → L1 → L2 → L3

		await root.SelectAllItemsRecursiveAsync(Call, maxDepth: 2);

		HeadlessTabView l2 = root.ChildViews[0].ChildViews[0];
		Assert.That(l2.Label, Is.EqualTo("L2"));
		Assert.That(l2.DepthTruncated, Is.True,
			"L2 still has L3 to expand but was cut off by maxDepth — should be flagged truncated.");

		// Re-traverse fully so L3 (a real leaf) is reached.
		var viewer2 = new HeadlessTabViewer(new Project());
		HeadlessTabView root2 = await viewer2.LoadTabAsync(Call, BuildChain(3));
		await root2.SelectAllItemsRecursiveAsync(Call, maxDepth: 5);

		HeadlessTabView l3 = root2.ChildViews[0].ChildViews[0].ChildViews[0];
		Assert.That(l3.Label, Is.EqualTo("L3"));
		Assert.That(l3.DepthTruncated, Is.False, "L3 is a genuine leaf — not truncated.");
	}

	// ─── SelectBookmarkItemsRecursiveAsync ───────────────────────────────

	[Test, Description(
		"A bookmark-guided traversal follows only the selected path and stops at the bookmark's " +
		"leaf, even when deeper levels exist within maxDepth.")]
	public async Task LoadAndTraverseAsync_WithBookmark_FollowsPathAndStopsAtLeaf()
	{
		var viewer = new HeadlessTabViewer(new Project());
		var bookmark = Bookmark.Create("L1", "L2", "L3"); // leaf at L3

		HeadlessTabView root = await viewer.LoadAndTraverseAsync(Call, BuildChain(6), bookmark);

		HeadlessTabView l1 = root.ChildViews[0];
		HeadlessTabView l2 = l1.ChildViews[0];
		HeadlessTabView l3 = l2.ChildViews[0];

		Assert.That(l1.Label, Is.EqualTo("L1"));
		Assert.That(l2.Label, Is.EqualTo("L2"));
		Assert.That(l3.Label, Is.EqualTo("L3"));
		Assert.That(l3.ChildViews, Is.Empty,
			"Traversal should stop at the bookmark leaf (L3) and not expand L4 even though maxDepth allows it.");
	}

	[Test, Description("A bookmark-guided traversal ignores siblings that the bookmark did not select.")]
	public async Task LoadAndTraverseAsync_WithBookmark_IgnoresUnselectedSiblings()
	{
		var viewer = new HeadlessTabViewer(new Project());
		var a = new NamedTab("A", new NamedTab("A1"));
		var b = new NamedTab("B", new NamedTab("B1"));
		var root = new NamedTab("Root", a, b);
		var bookmark = Bookmark.Create("A"); // only select A

		HeadlessTabView rootView = await viewer.LoadAndTraverseAsync(Call, root, bookmark);

		Assert.That(rootView.ChildViews, Has.Count.EqualTo(1), "Only the selected sibling should be expanded.");
		Assert.That(rootView.ChildViews[0].Label, Is.EqualTo("A"));
		Assert.That(rootView.ChildViews[0].ChildViews, Is.Empty,
			"A is the bookmark leaf, so A1 should not be expanded.");
	}

	[Test, Description("A bookmark with no selections expands nothing (the root is treated as a leaf).")]
	public async Task LoadAndTraverseAsync_WithEmptyBookmark_ExpandsNothing()
	{
		var viewer = new HeadlessTabViewer(new Project());
		var bookmark = new Bookmark(); // no selected rows

		HeadlessTabView root = await viewer.LoadAndTraverseAsync(Call, BuildChain(3), bookmark);

		Assert.That(root.ChildViews, Is.Empty,
			"An empty bookmark selects nothing, so no children should be expanded.");
	}

	// ─── SchemaNode object handling ──────────────────────────────────────

	/// <summary>Tab that adds a list of actions via <see cref="TabModel.AddActions"/>.</summary>
	private class ActionsTab : ITab
	{
		public override string ToString() => "Actions";

		public TabInstance Create() => new Instance();

		private class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Name = "Actions";
				model.AddActions([
					new TaskAction("First", () => { }) { Description = "Does the first thing" },
					new TaskAction("Second", () => { }),
				]);
			}
		}
	}

	[Test, Description(
		"A TabModel.AddActions list of TaskCreators is exported as labeled/described actions, " +
		"not a raw array ToString.")]
	public async Task SchemaNode_TaskCreatorList_ExportsActions()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView rootView = await viewer.LoadTabAsync(Call, new ActionsTab());

		SchemaNode node = SchemaNode.From(rootView);

		Assert.That(node.Objects, Has.Count.EqualTo(1));
		Assert.That(node.Objects![0], Is.InstanceOf<SchemaActions>(),
			"TaskCreator list should map to a SchemaActions, not a text object.");
		List<SchemaAction>? actions = ((SchemaActions)node.Objects[0]).Actions;
		Assert.That(actions, Is.Not.Null);
		Assert.That(actions!.Select(a => a.Label), Is.EqualTo(new[] { "First", "Second" }));
		Assert.That(actions![0].Description, Is.EqualTo("Does the first thing"));
	}

	/// <summary>Typed row used to verify list columns and items in the schema.</summary>
	private class DataRow(string name, int count)
	{
		public string Name => name;
		public int Count => count;
		public override string ToString() => name;
	}

	/// <summary>Tab whose model holds a typed data list (a data grid).</summary>
	private class DataListTab : ITab
	{
		public override string ToString() => "Data";

		public TabInstance Create() => new Instance();

		private class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Name = "Data";
				model.Items = new List<DataRow>
				{
					new("Alpha", 1),
					new("Beta", 2),
				};
			}
		}
	}

	[Test, Description(
		"An item list is exported as a List object under Objects, carrying its visible columns and " +
		"items (each row with an optional expanded child); there is no separate Children collection.")]
	public async Task SchemaNode_ItemList_ExportsListWithColumnsAndItems()
	{
		var viewer = new HeadlessTabViewer(new Project());
		HeadlessTabView rootView = await viewer.LoadAndTraverseAsync(Call, new DataListTab());

		SchemaNode node = SchemaNode.From(rootView);

		Assert.That(node.Objects, Has.Count.EqualTo(1), "The data list should appear as a single List object.");
		Assert.That(node.Objects![0], Is.InstanceOf<SchemaList>());
		var list = (SchemaList)node.Objects[0];

		// The polymorphic object serializes with a clean "Type" discriminator, no $type/$value wrapping.
		string json = System.Text.Json.JsonSerializer.Serialize(node, SideScroll.Serialize.Json.JsonConverters.PublicSerializerOptions);
		Assert.That(json, Does.Contain("\"Type\": \"List\""));
		Assert.That(json, Does.Not.Contain("$type"));

		Assert.That(list!.Columns!.Select(c => c.Label), Is.EqualTo(new[] { "Name", "Count" }),
			"Columns should mirror the element type's visible properties.");
		Assert.That(list.Columns!.Select(c => c.Type), Is.EqualTo(new[] { "String", "Int32" }));

		Assert.That(list.Items!.Select(i => i.Label), Is.EqualTo(new[] { "Alpha", "Beta" }));
		Assert.That(list.Items!.All(i => i.Child != null), Is.True,
			"Each row expands into a child (its member list), so Child should be set.");
	}

	[Test, Description(
		"When two rows share a label, the bookmark's RowIndex disambiguates which one is followed.")]
	public async Task LoadAndTraverseAsync_DuplicateLabels_DisambiguatedByRowIndex()
	{
		var viewer = new HeadlessTabViewer(new Project());
		var first = new NamedTab("Dup", new NamedTab("FromFirst"));   // index 0
		var second = new NamedTab("Dup", new NamedTab("FromSecond")); // index 1
		var root = new NamedTab("Root", first, second);

		// Select the second "Dup" by index, then its child "FromSecond".
		var bookmark = new Bookmark();
		var dupRow = new SelectedRowView(new SelectedRow { Label = "Dup", RowIndex = 1 });
		bookmark.TabBookmark.AddSelected(dupRow);
		dupRow.TabBookmark.SelectRows("FromSecond");

		HeadlessTabView rootView = await viewer.LoadAndTraverseAsync(Call, root, bookmark);

		Assert.That(rootView.ChildViews, Has.Count.EqualTo(1),
			"Only the row at RowIndex 1 should match, not both 'Dup' rows.");
		Assert.That(rootView.ChildViews[0].ChildViews.Select(c => c.Label),
			Is.EqualTo(new[] { "FromSecond" }),
			"The matched row must be the second 'Dup' (index 1), confirmed by its child.");
	}
}
