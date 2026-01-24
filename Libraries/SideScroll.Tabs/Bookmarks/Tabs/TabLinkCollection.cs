using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;
using System.Collections;

namespace SideScroll.Tabs.Bookmarks.Tabs;

public class TabLinkCollection(LinkCollection links) : ITab
{
	public LinkCollection Links => links;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; } = new("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonDeleteAll { get; } = new("Delete All", Icons.Svg.DeleteList)
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

			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = Refresh;
			toolbar.ButtonDeleteAll.Action = DeleteAll;
			toolbar.ButtonDeleteAll.IsEnabledBinding = new PropertyBinding(nameof(IList.Count), tab.Links.Items);
			ListProperty listProperty = new(tab.Links, nameof(LinkCollection.ShowLinkInfoTab));
			toolbar.ToggleButtonShowLinkInfoTab = new("Show Link Info Tab", Icons.Svg.PanelLeftContract, Icons.Svg.PanelLeftExpand, listProperty, ShowLinkTab);
			model.AddObject(toolbar);

			if (Project.Data.DataSettings.LinkId == null)
			{
				tab.Links.Load(call, true);
			}

			model.AddData(tab.Links.Items);

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
			foreach (TabBookmark childBookmark in tabBookmark.SelectedTabViews)
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
