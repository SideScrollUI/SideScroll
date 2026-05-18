using SideScroll.Extensions;
using System.Collections;

namespace SideScroll.Tabs.Viewer;

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

	/// <summary>Gets the child tab views added via <see cref="SelectAllItemsAsync"/> or <see cref="SelectItemsAsync"/>.</summary>
	public List<HeadlessTabView> ChildViews { get; } = [];

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
		foreach (IList iList in Model.ItemList)
		{
			await SelectItemsAsync(call, iList);
		}
	}

	/// <summary>
	/// Recursively selects all items up to <paramref name="maxDepth"/> levels deep.
	/// Each level calls <see cref="SelectAllItemsAsync"/> then recurses into every child.
	/// </summary>
	/// <param name="call">The call context for logging.</param>
	/// <param name="maxDepth">Maximum number of levels to traverse (default 5).</param>
	public async Task SelectAllItemsRecursiveAsync(Call call, int maxDepth = 5)
	{
		if (maxDepth <= 0)
			return;

		await SelectAllItemsAsync(call);

		foreach (HeadlessTabView child in ChildViews)
		{
			await child.SelectAllItemsRecursiveAsync(call, maxDepth - 1);
		}
	}

	/// <summary>Creates child views for the given items that resolve to an <see cref="ITab"/>.</summary>
	public async Task SelectItemsAsync(Call call, IEnumerable items)
	{
		foreach (object obj in items)
		{
			HeadlessTabView? childView = await TryCreateChildViewAsync(call, obj);
			if (childView != null)
			{
				ChildViews.Add(childView);
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
			var childView = new HeadlessTabView(childTabInstance, label);
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
			TabInstance childInstance = Instance.CreateChildTab(iTab);
			var childView = new HeadlessTabView(childInstance, label);
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
			var childInstance = new PlainObjectTabInstance(label, value)
			{
				Project = Instance.Project
			};
			var childView = new HeadlessTabView(childInstance, label);
			await childView.LoadAsync(call);
			return childView;
		}

		return null;
	}

	/// <summary>
	/// <see cref="TabInstance"/> used for plain objects (not <see cref="ITab"/>,
	/// <see cref="ITabCreatorAsync"/>, or <see cref="ILoadAsync"/>).
	/// Delegates to <see cref="TabModel.AddData"/> so that <c>[ListItem]</c>-decorated types
	/// and other complex objects are properly expanded into navigable schema items —
	/// mirroring the <c>TabModel.Create</c> path used by the Avalonia <c>TabCreator</c>.
	/// </summary>
	private sealed class PlainObjectTabInstance(string name, object obj) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Name = name;
			model.AddData(obj);
		}
	}

	/// <summary>Returns the label of this tab view.</summary>
	public override string ToString() => Label;
}
