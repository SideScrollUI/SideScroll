using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Serialize.Json;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using System.Text.Json;

namespace SideScroll.Tabs.Bookmarks;

public class TabLinkedBookmark(LinkedBookmark linkedBookmark, LinkCollection linkCollection) : ITab
{
	public event EventHandler<EventArgs>? OnDelete;

	[ButtonColumn("-")]
	public void Delete()
	{
		OnDelete?.Invoke(this, EventArgs.Empty);
	}

	[Hidden]
	public LinkedBookmark LinkedBookmark => linkedBookmark;

	public Bookmark Bookmark => linkedBookmark.Bookmark;

	[Formatted]
	public TimeSpan Age => Bookmark.TimeStamp.Age();

	public override string ToString() => Bookmark.ToString();

	public TabInstance Create()
	{
		if (linkCollection.ShowLinkInfoTab)
		{
			return new Instance(this);
		}

		return TabBookmarkItem.Create(LinkedBookmark, linkCollection.Project, this);
	}

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonCopyLinkToClipboard { get; set; } = new("Copy Link to Clipboard", Icons.Svg.Link);
	}

	public class Instance(TabLinkedBookmark tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonCopyLinkToClipboard.Action = CopyLinkToClipboard;
			model.AddObject(toolbar);

			string json = JsonSerializer.Serialize(tab.Bookmark, JsonConverters.JsonSerializerOptions);

			model.Items = new List<ListItem>()
			{
				new("Link", new TabBookmarkItem(tab.LinkedBookmark, Project)),
				new("Data", tab.LinkedBookmark),
				new("Json", json),
			};
		}

		private void CopyLinkToClipboard(Call call)
		{
			Refresh();
			CopyToClipboard(tab.LinkedBookmark.LinkId);
		}
	}
}
