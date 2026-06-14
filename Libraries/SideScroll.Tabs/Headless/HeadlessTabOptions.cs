namespace SideScroll.Tabs.Headless;

/// <summary>
/// Consolidated options for a headless tab traversal.
/// Passed to <see cref="HeadlessTabViewer"/> and propagated automatically to every
/// descendant <see cref="HeadlessTabView"/> so callers only need to configure one object.
/// </summary>
public record HeadlessTabOptions
{
	/// <summary>
	/// Maximum number of levels to traverse recursively.
	/// Defaults to 5.
	/// </summary>
	public int MaxDepth { get; init; } = 5;

	// Per-list caps. All use: negative = unlimited, 0 = none, positive = cap.
	// "Items" are rows listed by label (cheap, no deep load — visible columns are always shown).
	// "Children" are items explored into sub-tabs (loaded and recursed) — the expensive part.
	// Children are a subset of items, so an effective cap is min(Items, Children).

	/// <summary>Max rows listed for a list whose element type is in <see cref="AllowedElementTypes"/>. Defaults to 50.</summary>
	public int MaxAllowedItems { get; init; } = 50;

	/// <summary>Max rows explored into child sub-tabs for an allowlisted list. Defaults to 50.</summary>
	public int MaxAllowedChildren { get; init; } = 50;

	/// <summary>
	/// Max rows listed for a list whose element type is NOT in <see cref="AllowedElementTypes"/>.
	/// Listing is just labels (the columns are always shown), so this can be generous. Defaults to 50.
	/// </summary>
	public int MaxOtherItems { get; init; } = 50;

	/// <summary>
	/// Max rows explored into child sub-tabs for a non-allowlisted list. Kept low to avoid expensive
	/// loads of unknown element types. <c>0</c> avoids loading even one. Defaults to 1.
	/// </summary>
	public int MaxOtherChildren { get; init; } = 1;

	/// <summary>
	/// Optional filter applied to every <see cref="ITab"/> encountered during traversal.
	/// Tabs for which the predicate returns <c>false</c> are not expanded.
	/// </summary>
	public Func<ITab, bool>? TabFilter { get; init; }

	/// <summary>
	/// Optional allowlist of element types that may be fully expanded (up to
	/// <see cref="MaxAllowedItems"/> / <see cref="MaxAllowedChildren"/>). Lists whose element type is
	/// outside the allowlist use the lower <see cref="MaxOtherItems"/> / <see cref="MaxOtherChildren"/> caps.
	/// When <c>null</c> (the default), all item lists are treated as allowed.
	/// </summary>
	public IReadOnlyList<Type>? AllowedElementTypes { get; init; }
}
