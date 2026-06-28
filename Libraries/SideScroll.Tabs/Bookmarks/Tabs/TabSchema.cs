using SideScroll.Attributes;
using SideScroll.Serialize.Json;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Headless;
using SideScroll.Tabs.Lists;
using System.Text.Json;

namespace SideScroll.Tabs.Bookmarks.Tabs;

/// <summary>
/// Lazy <see cref="ITab"/> wrapper that holds the schema configuration for one access level.
/// The traversal is deferred until the user navigates to this item.
/// </summary>
[PrivateData]
public class TabSchema(ITab tab, HeadlessTabOptions options, Bookmark? bookmark = null) : ITab
{
	/// <summary>
	/// Prevents re-entrant schema generation across async continuations.
	/// <see cref="AsyncLocal{T}"/> is used so the flag is correctly propagated through
	/// <c>await</c> continuations even when they resume on a different thread.
	/// When the headless traversal encounters a <c>TabSchema</c> node it will skip
	/// expanding the nested schema sub-tabs rather than recurse infinitely.
	/// </summary>
	private static readonly AsyncLocal<bool> IsGenerating = new();

	public TabInstance Create() => new Instance(tab, options, bookmark)
	{
		LoadingMessage = "Exporting schema...",
	};

	/// <summary>
	/// Async tab instance that runs the headless traversal for one access level (Public or Private).
	/// </summary>
	private class Instance(ITab tab, HeadlessTabOptions options, Bookmark? bookmark) : TabInstanceAsync
	{
		public override async Task LoadAsync(Call call, TabModel model)
		{
			model.ShowTasks = true;

			// Guard against re-entrant calls (e.g. the headless viewer expanding TabLinks→TabSchemas→TabSchema).
			if (IsGenerating.Value)
			{
				call.Log.AddDebug("Skipping recursive TabSchema expansion during schema generation");
				return;
			}

			using CallTimer schemaCall = call.StartTask("Exporting Schema",
				new Tag("Tab", tab.GetType().Name),
				new Tag("MaxDepth", options.MaxDepth),
				new Tag("Filtered", options.TabFilter != null));

			IsGenerating.Value = true;
			try
			{
				var viewer = new HeadlessTabViewer(Project, options);
				HeadlessTabView rootView = await viewer.LoadAndTraverseAsync(schemaCall, tab, bookmark);
				call.Log.Level = Logs.LogLevel.Info;

				SchemaDocument schemaDocument = SchemaDocument.From(rootView);
				string json = JsonSerializer.Serialize(schemaDocument, JsonConverters.PublicSerializerOptions);
				model.Items = new List<ListItem>
				{
					new("View", schemaDocument),
					new("Json", json),
				};
			}
			finally
			{
				IsGenerating.Value = false;
			}
		}
	}
}
