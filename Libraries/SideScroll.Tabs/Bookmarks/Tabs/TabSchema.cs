using SideScroll.Attributes;
using SideScroll.Serialize.Json;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Viewer;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SideScroll.Tabs.Bookmarks.Tabs;

/// <summary>
/// A tab that uses a <see cref="HeadlessTabViewer"/> to explore a tab hierarchy and export its structure.
/// Exposed under the "Schema" entry in <see cref="TabLinks"/>.
/// </summary>
/// <remarks>Initializes a new <see cref="TabSchema"/> with an optional root tab to explore.</remarks>
public class TabSchema(ITab? rootTab = null) : ITab
{
	/// <summary>
	/// Gets or sets the default root tab explored when exporting the schema.
	/// Set this from your application startup before navigating to the Schema tab.
	/// </summary>
	public static ITab? DefaultRootTab { get; set; }

	/// <summary>Gets the root tab passed at construction time, if any.</summary>
	public ITab? RootTab => rootTab;

	/// <summary>
	/// Gets or sets the maximum number of levels to traverse when building the schema tree.
	/// Defaults to 3.
	/// </summary>
	public int MaxDepth { get; set; } = 4;

	/// <summary>Creates a new tab instance for <see cref="TabSchema"/>.</summary>
	public TabInstance Create() => new Instance(this);

	/// <summary>Describes a tab node in the exported schema tree.</summary>
	private class SchemaNode
	{
		public string Label { get; set; } = "";

		[JsonIgnore]
		public int ItemCount { get; set; }

		[InnerValue, HiddenColumn]
		public List<SchemaNode>? Children { get; set; }

		public override string ToString() => Label;

		public static SchemaNode From(HeadlessTabView view)
		{
			SchemaNode node = new()
			{
				Label = view.Label,
				ItemCount = view.Model.ItemList.Sum(l => l.Count),
			};
			if (view.ChildViews.Count > 0)
			{
				node.Children = view.ChildViews.Select(From).ToList();
			}
			return node;
		}
	}

	/// <summary>The live tab instance for <see cref="TabSchema"/>.</summary>
	public class Instance(TabSchema tab) : TabInstanceAsync
	{
		/// <summary>
		/// Prevents re-entrant schema generation across async continuations.
		/// <see cref="AsyncLocal{T}"/> is used (rather than <c>[ThreadStatic]</c>) so the flag
		/// is correctly propagated through <c>await</c> continuations even when they resume on
		/// a different thread.
		/// This is needed because <see cref="TabSchema"/> is itself registered inside <c>TabLinks</c>;
		/// when the headless viewer expands <c>TabLinks</c> it would load <c>TabSchema</c>,
		/// which would call <see cref="GetSchemaAsync"/> again, causing infinite recursion.
		/// </summary>
		private static readonly AsyncLocal<bool> IsGenerating = new();

		public override async Task LoadAsync(Call call, TabModel model)
		{
			SchemaNode? schemaNode = await GetSchemaAsync(call);
			if (schemaNode != null)
			{
				JsonSerializerOptions jsonSerializerOptions = new()
				{
					WriteIndented = true
				};
				string json = JsonSerializer.Serialize(schemaNode, JsonConverters.PublicSerializerOptions);

				model.Items = new List<ListItem>
				{
					new("View", schemaNode.Children),
					new("Json", json),
				};
			}
		}

		private async Task<SchemaNode?> GetSchemaAsync(Call call)
		{
			// Guard against re-entrant calls (e.g. the headless viewer expanding TabLinks→TabSchema).
			if (IsGenerating.Value)
			{
				call.Log.AddDebug("Skipping recursive TabSchema expansion during schema generation");
				return null;
			}

			ITab? rootTab = tab.RootTab ?? DefaultRootTab;
			if (rootTab == null)
			{
				call.Log.AddWarning("No root tab configured for schema export",
					new Tag("Hint", $"Set {nameof(TabSchema)}.{nameof(DefaultRootTab)} or pass an ITab to the constructor"));
				return null;
			}

			call.Log.Add("Exporting schema",
				new Tag("Tab", rootTab.GetType().Name),
				new Tag("MaxDepth", tab.MaxDepth));

			IsGenerating.Value = true;
			try
			{
				var viewer = new HeadlessTabViewer(Project);
				HeadlessTabView rootView = await viewer.LoadTabAsync(call, rootTab);
				await rootView.SelectAllItemsRecursiveAsync(call, tab.MaxDepth);

				return SchemaNode.From(rootView);
			}
			finally
			{
				IsGenerating.Value = false;
			}
		}
	}
}
