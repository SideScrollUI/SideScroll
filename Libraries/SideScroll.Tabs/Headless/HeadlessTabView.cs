using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Tabs.Bookmarks.Models;
using System.Collections;
using System.Reflection;

namespace SideScroll.Tabs.Headless;

/// <summary>
/// Represents a single tab node loaded by a <see cref="HeadlessTabViewer"/>.
/// Loads tab instance data without creating any UI controls.
/// </summary>
/// <remarks>Initializes a new <see cref="HeadlessTabView"/> for the given tab instance and label.</remarks>
public class HeadlessTabView(TabInstance instance, string label)
{
	/// <summary>Gets the tab instance backing this view.</summary>
	public TabInstance Instance => instance;

	/// <summary>Gets or sets the display label for this tab node.</summary>
	public string Label { get; set; } = label;

	/// <summary>Gets the data model populated after <see cref="LoadAsync"/> is called.</summary>
	public TabModel Model { get; private set; } = new();

	/// <summary>Gets the explored child tab views (the subset of items that were loaded and recursed into).</summary>
	public List<HeadlessTabView> ChildViews { get; } = [];

	private readonly Dictionary<IList, List<HeadlessTabItem>> _listItems = [];

	/// <summary>
	/// Per item list, the rows that were listed — each with a label and an optional explored child.
	/// Keyed by the originating <see cref="TabModel.ItemLists"/> entry.
	/// </summary>
	public IReadOnlyDictionary<IList, List<HeadlessTabItem>> ListItems => _listItems;

	/// <summary>The model item list this view was created from, set when it is added as a child.</summary>
	public IList? SourceList { get; internal set; }

	/// <summary>
	/// True when recursive traversal stopped at this node because the depth limit was reached while
	/// there was still content to expand. Distinguishes a depth-truncated node from a real leaf.
	/// </summary>
	public bool DepthTruncated { get; private set; }

	private readonly HashSet<IList> _truncatedLists = [];

	/// <summary>Item lists that were not fully expanded into children (per-list cap reached with items remaining).</summary>
	public IReadOnlySet<IList> TruncatedLists => _truncatedLists;

	/// <summary>
	/// True when any item list was not fully listed because the applicable per-list item cap
	/// (<see cref="HeadlessTabOptions.MaxAllowedItems"/> or <see cref="HeadlessTabOptions.MaxOtherItems"/>)
	/// was reached with rows remaining.
	/// </summary>
	public bool ItemsTruncated => _truncatedLists.Count > 0;

	/// <summary>
	/// Traversal options propagated from the <see cref="HeadlessTabViewer"/> that created this view.
	/// Includes <see cref="HeadlessTabOptions.TabFilter"/>, the per-list item/children caps,
	/// and <see cref="HeadlessTabOptions.MaxDepth"/>.
	/// </summary>
	public HeadlessTabOptions Options { get; init; } = new();

	/// <summary>Returns the label of this tab view.</summary>
	public override string ToString() => Label;

	/// <summary>
	/// Loads the tab data into the model asynchronously.
	/// Mirrors <c>TabInstance.LoadModelAsync</c>: calls <see cref="ITabAsync.LoadAsync"/> first for async
	/// instances (e.g. <see cref="TabInstanceLoadAsync"/>, <see cref="TabCreatorAsync"/>,
	/// <see cref="TabInstanceAsync"/> subclasses), then calls <see cref="TabInstance.Load"/>.
	/// Sets <see cref="TabInstance.IsHeadless"/> to <c>true</c> before loading so that tabs can skip
	/// slow or UI-only operations (e.g. network calls, <c>Thread.Sleep</c>) during headless traversal.
	/// </summary>
	public async Task LoadAsync(Call call)
	{
		Instance.IsHeadless = true;

		Model = new TabModel(Label);

		// ITabAsync instances load via LoadAsync (same order as TabInstance.LoadModelAsync)
		if (Instance is ITabAsync tabAsync)
		{
			try
			{
				await tabAsync.LoadAsync(call, Model);
			}
			catch (Exception e)
			{
				call.Log.Add(e);
			}
		}

		try
		{
			Instance.Load(call, Model);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}

		// Sync Label with any name the Load/LoadAsync set on the model
		Label = Model.Name;
	}

	/// <summary>
	/// Selects all items from every item list in the model and creates child views for those
	/// that resolve to an <see cref="ITab"/>.
	/// </summary>
	public async Task SelectAllItemsAsync(Call call)
	{
		// Snapshot so that async loading of child views cannot invalidate the outer enumerator
		// (some models populate their lists lazily or use observable collections).
		foreach (IList itemList in Model.ItemLists.ToList())
		{
			// Allowlisted element types are listed and explored more generously; others list labels
			// freely (columns are always shown) but only sample a few children.
			bool allowed = IsElementTypeAllowed(itemList);
			int maxItems = allowed ? Options.MaxAllowedItems : Options.MaxOtherItems;
			int maxChildren = allowed ? Options.MaxAllowedChildren : Options.MaxOtherChildren;

			await SelectItemsAsync(call, itemList, maxItems, maxChildren);
		}
	}

	/// <summary>
	/// Returns <c>true</c> if <paramref name="itemList"/>'s element type may be fully explored.
	/// An <see cref="ExplorableAttribute"/> on the element type overrides the
	/// <see cref="HeadlessTabOptions.AllowedElementTypes"/> allowlist; otherwise allowlist
	/// membership decides (and when no allowlist is configured, every list is permitted).
	/// </summary>
	private bool IsElementTypeAllowed(IList itemList)
	{
		Type listType = itemList.GetType();
		Type elementType = listType.IsGenericType
			? listType.GetGenericArguments()[0]
			: typeof(object);

		// [Explorable] / [Explorable(false)] on the element type overrides the allowlist.
		if (elementType.GetCustomAttribute<ExplorableAttribute>() is { } explorable)
			return explorable.Value;

		if (Options.AllowedElementTypes == null)
			return true;

		return Options.AllowedElementTypes.Any(allowed => allowed.IsAssignableFrom(elementType));
	}

	/// <summary>
	/// Returns <c>true</c> when the call's task has been cancelled — e.g. the
	/// <see cref="HeadlessTabOptions.MaxTime"/> budget elapsed or the user cancelled it.
	/// </summary>
	private static bool IsCancelled(Call call) => call.TaskInstance?.CancelToken.IsCancellationRequested == true;

	/// <summary>
	/// Recursively selects all items up to <paramref name="maxDepth"/> levels deep.
	/// Each level calls <see cref="SelectAllItemsAsync"/> then recurses into every child.
	/// </summary>
	/// <param name="call">The call context for logging.</param>
	/// <param name="maxDepth">Maximum number of levels to traverse (default 5).</param>
	public async Task SelectAllItemsRecursiveAsync(Call call, int maxDepth = 5)
	{
		using var callTimer = call.Timer("Loading Tab", new Tag("Label", Label));

		if (maxDepth <= 0)
		{
			// Stopped before expanding: flag as truncated if there were rows we could have expanded.
			DepthTruncated = Model.ItemLists.Any(list => list.Count > 0);
			return;
		}

		if (IsCancelled(callTimer))
			return;

		await SelectAllItemsAsync(callTimer);

		foreach (HeadlessTabView child in ChildViews)
		{
			if (IsCancelled(callTimer))
				return;

			await child.SelectAllItemsRecursiveAsync(callTimer, maxDepth - 1);
		}
	}

	/// <summary>
	/// Traverses only the items selected by <paramref name="tabBookmark"/> rather than expanding
	/// every item. Each matched item is recursed into using its child bookmark, stopping once a
	/// bookmark leaf (a node with no further selected rows) is reached. Unlike
	/// <see cref="SelectAllItemsRecursiveAsync"/>, the <see cref="HeadlessTabOptions.AllowedElementTypes"/>
	/// allowlist is not applied: the bookmark explicitly names what to follow.
	/// </summary>
	/// <param name="call">The call context for logging.</param>
	/// <param name="tabBookmark">The bookmark node describing which rows to select at this level.</param>
	/// <param name="maxDepth">Maximum number of levels to traverse (default 5).</param>
	public async Task SelectBookmarkItemsRecursiveAsync(Call call, TabBookmark tabBookmark, int maxDepth = 5)
	{
		using var callTimer = call.Timer("Loading Tab with Bookmark", new Tag("Label", Label));

		if (maxDepth <= 0)
		{
			// Stopped before following the bookmark's remaining selections.
			DepthTruncated = tabBookmark.SelectedRowViews.Count > 0;
			return;
		}

		// Bookmark leaf: nothing more to follow, so stop rather than expanding everything.
		if (tabBookmark.SelectedRowViews.Count == 0)
			return;

		foreach (IList itemList in Model.ItemLists.ToList())
		{
			List<HeadlessTabItem> listItems = [];
			_listItems[itemList] = listItems;

			// Snapshot so that async child loading cannot invalidate the source enumerator.
			List<object> snapshot = itemList.Cast<object>().ToList();
			for (int rowIndex = 0; rowIndex < snapshot.Count; rowIndex++)
			{
				if (IsCancelled(callTimer))
					return;

				// Match on full SelectedRow identity (label, data key/value, and index) so that
				// rows sharing a label — or identified only by index — are disambiguated.
				var selectedRow = new SelectedRow(snapshot[rowIndex]) { RowIndex = rowIndex };
				if (!tabBookmark.TryGetValue(selectedRow, out TabBookmark? childBookmark))
					continue;

				HeadlessTabView? childView = await TryCreateChildViewAsync(callTimer, snapshot[rowIndex]);
				if (childView == null)
					continue;

				childView.SourceList = itemList;
				ChildViews.Add(childView);
				listItems.Add(new HeadlessTabItem(childView.Label, childView));
				await childView.SelectBookmarkItemsRecursiveAsync(callTimer, childBookmark, maxDepth - 1);
			}
		}
	}

	/// <summary>
	/// Lists and explores the given items using the allowlisted caps
	/// (<see cref="HeadlessTabOptions.MaxAllowedItems"/> / <see cref="HeadlessTabOptions.MaxAllowedChildren"/>).
	/// </summary>
	public Task SelectItemsAsync(Call call, IList items)
	{
		return SelectItemsAsync(call, items, Options.MaxAllowedItems, Options.MaxAllowedChildren);
	}

	/// <summary>
	/// Lists up to <paramref name="maxItems"/> rows of <paramref name="items"/> (by label) and explores
	/// up to <paramref name="maxChildren"/> of them into child views (loaded and recursable). Each cap
	/// uses: negative = unlimited, <c>0</c> = none, positive = cap. When rows are omitted past the item
	/// cap, the list is flagged in <see cref="TruncatedLists"/>.
	/// </summary>
	public async Task SelectItemsAsync(Call call, IList items, int maxItems, int maxChildren)
	{
		List<HeadlessTabItem> listItems = [];
		_listItems[items] = listItems;

		if (maxItems == 0)
		{
			// Nothing listed from this list; flag truncation if it had content to omit.
			if (items.Count > 0)
			{
				_truncatedLists.Add(items);
			}
			return;
		}

		// Snapshot so that async child loading cannot invalidate the source enumerator.
		List<object> snapshot = items.Cast<object>().ToList();
		int childCount = 0;
		for (int i = 0; i < snapshot.Count; i++)
		{
			if (IsCancelled(call))
				break;

			object obj = snapshot[i];

			object? value = obj.GetInnerValue();
			if (value == null || value is bool)
				continue; // not a listable row

			// Leaf values have no navigable content, so list them as labels without consuming the
			// child-exploration budget: scalars (strings, primitives) and empty collections.
			bool isLeaf = value.GetType().IsPrimitive
				|| value is string
				|| value is ICollection { Count: 0 };

			HeadlessTabView? child = null;
			if (!isLeaf)
			{
				if (maxChildren < 0 || childCount < maxChildren)
				{
					child = await TryCreateChildViewAsync(call, obj);
					if (child == null)
						continue; // filtered out or not navigable

					child.SourceList = items;
					ChildViews.Add(child);
					childCount++;
				}
				else if (Options.TabFilter != null && value is ITab or ITabCreatorAsync or ILoadAsync)
				{
					// Navigable but beyond the child budget: don't list (or leak) tab-like rows we
					// can't filter without resolving them.
					continue;
				}
			}

			string label = child?.Label ?? obj.Formatted() ?? '(' + obj.GetType().Name + ')';
			listItems.Add(new HeadlessTabItem(label, child));

			// Flag only if rows remain past the cap, so a list that ends exactly at the limit isn't truncated.
			if (maxItems > 0 && listItems.Count >= maxItems)
			{
				if (i < snapshot.Count - 1)
				{
					_truncatedLists.Add(items);
				}
				break;
			}
		}
	}

	/// <summary>
	/// Tries to create a child <see cref="HeadlessTabView"/> from <paramref name="obj"/> by resolving
	/// its inner value via <c>[InnerValue]</c> attributes.
	/// Handles <see cref="ILoadAsync"/>, <see cref="ITabCreatorAsync"/>, <see cref="ITab"/>, and
	/// plain objects (including <c>[ListItem]</c>-decorated aggregator classes).
	/// Returns <c>null</c> if the item cannot be resolved to a tab.
	/// </summary>
	public async Task<HeadlessTabView?> TryCreateChildViewAsync(Call call, object obj)
	{
		object? value = obj.GetInnerValue();
		if (value == null || value is bool)
			return null;

		string label = obj.Formatted() ?? '(' + obj.GetType().Name + ')';

		// ILoadAsync: wrap in TabInstanceLoadAsync so TabInstance.Load calls LoadAsync
		if (value is ILoadAsync loadAsync)
		{
			var childTabInstance = new TabInstanceLoadAsync(loadAsync)
			{
				Project = Instance.Project,
			};
			childTabInstance.Model.Name = label;
			var childView = new HeadlessTabView(childTabInstance, label) { Options = Options };
			await childView.LoadAsync(call);
			return childView;
		}

		// ITabCreatorAsync: resolve asynchronously to an ITab (mirrors Avalonia TabCreator)
		if (value is ITabCreatorAsync creatorAsync)
		{
			value = await creatorAsync.CreateAsync(call);
		}

		if (value is ITab iTab)
		{
			// Apply the optional tab filter before expanding — e.g. skip [PrivateData] tabs
			// for the public schema view.
			if (Options.TabFilter != null && !Options.TabFilter(iTab))
				return null;

			TabInstance childInstance = Instance.CreateChildTab(iTab);
			var childView = new HeadlessTabView(childInstance, label) { Options = Options };
			await childView.LoadAsync(call);
			return childView;
		}

		// Fallback: plain object (e.g. [ListItem]-decorated aggregator classes such as TabSamples).
		// Delegate to TabModel.AddData so the [ListItem] attribute – or AddObjectMembers for
		// undecorated types – populates the model with navigable items, mirroring the Avalonia
		// TabCreator's TabModel.Create(label, value) path.
		// Note: value may be null here if ITabCreatorAsync.CreateAsync returned null.
		if (value != null && !value.GetType().IsPrimitive && value is not string)
		{
			var childInstance = new PlainObjectTabInstance(Instance.Project, label, value);
			var childView = new HeadlessTabView(childInstance, label) { Options = Options };
			await childView.LoadAsync(call);
			return childView;
		}

		// Leaf value (string or primitive such as int, double, enum): include as a labeled
		// node so that the schema can count and display these items without trying to drill in.
		if (value != null)
		{
			var leafInstance = new LeafTabInstance(Instance.Project, label);
			var leafView = new HeadlessTabView(leafInstance, label) { Options = Options };
			await leafView.LoadAsync(call);
			return leafView;
		}

		return null;
	}

	/// <summary>
	/// <see cref="TabInstance"/> used for plain objects (not <see cref="ITab"/>,
	/// <see cref="ITabCreatorAsync"/>, or <see cref="ILoadAsync"/>).
	/// Delegates to <see cref="TabModel.AddItems(object?)"/> so that <c>[ListItem]</c>-decorated types
	/// and other complex objects are properly expanded into navigable schema items —
	/// mirroring the <c>TabModel.Create</c> path used by the Avalonia <c>TabCreator</c>.
	/// </summary>
	private sealed class PlainObjectTabInstance(Project project, string name, object obj) : TabInstance(project)
	{
		public override void Load(Call call, TabModel model)
		{
			model.Name = name;
			model.AddItems(obj);
		}
	}

	/// <summary>
	/// <see cref="TabInstance"/> for leaf values (strings, ints, enums, etc.) that have no
	/// navigable children. Sets the model name so the label appears correctly in the schema tree.
	/// </summary>
	private sealed class LeafTabInstance(Project project, string name) : TabInstance(project)
	{
		public override void Load(Call call, TabModel model)
		{
			model.Name = name;
		}
	}
}

/// <summary>A single listed row of an item list: its label and the explored child view, if any.</summary>
public class HeadlessTabItem(string label, HeadlessTabView? child)
{
	/// <summary>The row's display label.</summary>
	public string Label => label;

	/// <summary>The explored child view, or <c>null</c> when the row was listed but not explored.</summary>
	public HeadlessTabView? Child => child;

	public override string ToString() => Label;
}
