using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;
using System.Collections;

namespace SideScroll.Tabs.Bookmarks;

public class TabLinkCollection(LinkCollection links) : ITab
{
	public LinkCollection Links => links;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; set; } = new("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonDeleteAll { get; set; } = new("Delete All", Icons.Svg.DeleteList)
		{
			Flyout = new ConfirmationFlyoutConfig("Delete all links?", "Delete"),
		};

		[Separator]
		public ToolToggleButton? ToggleButtonShowLinkInfoTab { get; set; }
	}

	public class Instance(TabLinkCollection tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.CustomSettingsPath = tab.Links.GroupId;
			model.MinDesiredWidth = 300;

			if (Project.Data.DataSettings.LinkId == null)
			{
				tab.Links.Load(call, true);
			}

			model.AddData(tab.Links.Items);
		}

		public override void LoadUI(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = Refresh;
			toolbar.ButtonDeleteAll.Action = DeleteAll;
			toolbar.ButtonDeleteAll.IsEnabledBinding = new PropertyBinding(nameof(IList.Count), tab.Links.Items);
			ListProperty listProperty = new(tab.Links, nameof(LinkCollection.ShowLinkInfoTab));
			toolbar.ToggleButtonShowLinkInfoTab = new("Show Link Info Tab", Icons.Svg.PanelLeftContract, Icons.Svg.PanelLeftExpand, listProperty, ShowLinkTab);
			model.AddObject(toolbar);

			if (tab.Links.NewBookmark != null)
			{
				SelectItem(tab.Links.NewBookmark);
				tab.Links.NewBookmark = null;
			}
		}

		private void ShowLinkTab(Call call)
		{
			Reload();
		}

		public override void GetBookmark(TabBookmark tabBookmark)
		{
			base.GetBookmark(tabBookmark);

			// Set links created from this link to always start from the child Link Tab
			foreach (TabBookmark childBookmark in tabBookmark.ChildBookmarks.Values)
			{
				childBookmark.IsRoot = true;
			}
		}

		private void Refresh(Call call)
		{
			Refresh();
		}

		private void DeleteAll(Call call)
		{
			tab.Links.DeleteAll(call);
			tab.Links.Load(call, true);
		}
	}
}
