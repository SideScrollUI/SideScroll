using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Bookmarks.Tabs;

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

	[WordWrap]
	public Bookmark Bookmark => linkedBookmark.Bookmark;

	[Formatted]
	public TimeSpan? Age => Bookmark.CreatedTime?.Age();

	public override string ToString() => Bookmark.ToString();

	public TabInstance Create()
	{
		if (linkCollection.ShowLinkInfoTab)
		{
			return new Instance(this);
		}

		return TabLinkView.Create(LinkedBookmark, linkCollection.Project, this);
	}

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonCopyLinkToClipboard { get; } = new("Copy Link to Clipboard", Icons.Svg.Link);
	}

	public class Instance(TabLinkedBookmark tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonCopyLinkToClipboard.Action = CopyLinkToClipboard;
			model.AddObject(toolbar);

			string json = tab.Bookmark.ToJson();

			model.Items = new List<ListItem>
			{
				new("Link", new TabLinkView(tab.LinkedBookmark, Project)),
				new("Data", tab.LinkedBookmark),
				new("Json", json),
			};
		}

		private void CopyLinkToClipboard(Call call)
		{
			CopyToClipboard(tab.LinkedBookmark.LinkId);
		}
	}
}
