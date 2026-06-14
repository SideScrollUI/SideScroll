using SideScroll.Attributes;
using SideScroll.Serialize;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Headless;
using SideScroll.Tabs.Lists;
using System.Reflection;

namespace SideScroll.Tabs.Bookmarks.Tabs;

/// <summary>
/// A tab that uses a <see cref="HeadlessTabViewer"/> to explore a tab hierarchy and export its structure.
/// Exposes a "Public" sub-tab (tabs not marked <c>[PrivateData]</c>) and a "Private" sub-tab
/// (full traversal with no filtering), each loaded lazily when navigated to.
/// Exposed under the "Schema" entry in <see cref="TabLinks"/>.
/// </summary>
/// <remarks>Initializes a new <see cref="TabSchemas"/> with an optional root tab to explore.</remarks>
public class TabSchemas(ITab? rootTab = null, HeadlessTabOptions? options = null) : ITab
{
	/// <summary>
	/// Gets or sets the default root tab explored when exporting the schema.
	/// Set this from your application startup before navigating to the Schema tab.
	/// </summary>
	public static ITab? DefaultRootTab { get; set; }

	/// <summary>Gets the root tab passed at construction time, if any.</summary>
	public ITab? RootTab => rootTab;

	/// <summary>
	/// Consolidated traversal options shared by both the Public and Private schema views.
	/// Override individual properties using record <c>with</c>-expression syntax, e.g.:
	/// <c>new TabSchemas { Options = TabSchemas.Options with { MaxDepth = 3 } }</c>.
	/// </summary>
	public HeadlessTabOptions Options { get; set; } = options ?? new()
	{
		AllowedElementTypes = [typeof(IListItem)],
	};

	/// <summary>
	/// Optional bookmark used as the traversal starting point. When set, only the items the
	/// bookmark selects are followed, stopping at its leaf nodes, instead of expanding the
	/// full hierarchy.
	/// </summary>
	public Bookmark? Bookmark { get; set; }

	/// <summary>Creates a new tab instance for <see cref="TabSchemas"/>.</summary>
	public TabInstance Create() => new Instance(this);

	/// <summary>
	/// Filter that accepts only tabs NOT decorated with <c>[PrivateData]</c>.
	/// Used for the "Public" schema view.
	/// </summary>
	private static bool IsPublicTab(ITab iTab) =>
		iTab.GetType().GetCustomAttribute<PrivateDataAttribute>() == null;

	/// <summary>
	/// The live tab instance for <see cref="TabSchemas"/>.
	/// Only validates the root tab and exposes two lazy sub-tabs; the actual traversal
	/// happens inside <see cref="TabSchema"/> when each sub-tab is navigated to.
	/// </summary>
	public class Instance(TabSchemas tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			ITab? effectiveRootTab = tab.RootTab ?? DefaultRootTab?.DeepClone();
			if (effectiveRootTab == null)
			{
				call.Log.AddWarning("No root tab configured for schema export",
					new Tag("Hint", $"Set {nameof(TabSchemas)}.{nameof(DefaultRootTab)} or pass an ITab to the constructor"));
				return;
			}

			model.Items = new List<ListItem>
			{
				new("Public",  new TabSchema(effectiveRootTab, tab.Options with { TabFilter = IsPublicTab }, tab.Bookmark)),
				new("Private", new TabSchema(effectiveRootTab, tab.Options with { TabFilter = null }, tab.Bookmark)),
			};
		}
	}
}
