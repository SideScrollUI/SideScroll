using Atlas.Core;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs
{
	public class DataRepoItem
	{
		public string Directory { get; set; }
		public string Key { get; set; }
		public object Value { get; set; }
	}

	// rename to TabInstanceSettings?
	[PublicData]
	public class TabBookmark
	{
		private const string DefaultDataName = "default";

		public Bookmark Bookmark { get; set; }
		public string Name { get; set; }
		public bool IsRoot { get; set; }
		public ITab Tab { get; set; } // [TabRoot] will set this to use the serialized tab as the root tab

		public SelectedRow SelectedRow { get; set; } // The parent selection that created this bookmark

		public TabViewSettings ViewSettings = new TabViewSettings(); // list selections, doesn't know about children
		public Dictionary<string, TabBookmark> ChildBookmarks { get; set; } = new Dictionary<string, TabBookmark>(); // doesn't know which tabData to use, maps id to child info
		public Dictionary<string, object> BookmarkData { get; set; }

		public string Address
		{
			get
			{
				if (ChildBookmarks.Count > 0)
				{
					string comma = "";
					string address = "";
					if (ChildBookmarks.Count > 1)
						address += "[";
					//address += Name + "::";
					foreach (var bookmark in ChildBookmarks)
					{
						address += comma;
						address += bookmark.Key + " / " + bookmark.Value.Address;
						comma = ", ";
					}
					if (ChildBookmarks.Count > 1)
						address += "]";
					return address;
				}
				else
				{
					if (ViewSettings == null)
						return "";
					string address = ViewSettings.Address;
					return address;
				}
			}
		}
		//public List<DataRepoItem> DataRepoItems { get; set; } = new List<DataRepoItem>();
		public string DataRepoDirectory { get; set; }

		// Temporary, Only FindMatches() uses, refactor these out?
		[NonSerialized]
		public HashSet<object> SelectedObjects = new HashSet<object>(); // does this work with multiple TabDatas?
		[NonSerialized]
		public TabModel TabModel;

		// too complicated, for now
		/*public void MergeNodes(List<Node> nodes)
		{
			foreach (Node node in nodes)
			{
				foreach (var nodeEntry in node.nodes)
				{
					Node existingNode;
					if (this.nodes.TryGetValue(nodeEntry.Key, out existingNode))
					{

					}
					else
					{
						this.nodes.Add(nodeEntry.Key, nodeEntry.Value);
					}
				}
			}
		}*/

		public override string ToString() => Name;

		public static TabBookmark Create(params object[] objs)
		{
			// get TabBookmark.SelectedObjects working again and replace?

			TabBookmark tabBookmark = null;
			foreach (object obj in objs)
			{
				string label = obj.ToString();
				var newBookmark = new TabBookmark();
				newBookmark.Select(label);
				if (tabBookmark != null)
					tabBookmark.ChildBookmarks.Add(label, newBookmark);
				else
					tabBookmark = newBookmark;
			}
			return tabBookmark;
		}

		// Shallow Clone
		public TabBookmark Clone()
		{
			return new TabBookmark()
			{
				Bookmark = Bookmark,
				Name = Name,
				SelectedRow = SelectedRow,
				DataRepoDirectory = DataRepoDirectory,
				BookmarkData = BookmarkData,
			};
		}

		public void Add(TabBookmark tabBookmark)
		{
			ChildBookmarks.Add(tabBookmark.Name, tabBookmark);

			var selectedRow = tabBookmark.SelectedRow ?? new SelectedRow()
			{
				Label = tabBookmark.Name,
			};
			ViewSettings = ViewSettings ?? new TabViewSettings();
			ViewSettings.TabDataSettings = ViewSettings.TabDataSettings ?? new List<TabDataSettings>();
			if (ViewSettings.TabDataSettings.Count == 0)
				ViewSettings.TabDataSettings.Add(new TabDataSettings());
			ViewSettings.TabDataSettings[0].SelectionType = SelectionType.User;
			ViewSettings.TabDataSettings[0].SelectedRows = ViewSettings.TabDataSettings[0].SelectedRows ?? new HashSet<SelectedRow>();
			ViewSettings.TabDataSettings[0].SelectedRows.Add(selectedRow);
			//Select(ChildBookmarks.Keys);
		}

		public SortedDictionary<string, T> GetSelectedData<T>()
		{
			var items = new SortedDictionary<string, T>();
			if (ViewSettings != null)
			{
				foreach (SelectedRow row in ViewSettings.SelectedRows)
				{
					string dataKey = row.DataKey ?? row.Label;
					if (dataKey != null && row.DataValue != null && row.DataValue.GetType() == typeof(T))
						items[dataKey] = (T)row.DataValue;
				}
			}
			return items;
		}

		public void SetData(object obj)
		{
			SetData(DefaultDataName, obj);
		}

		public void SetData(string name, object obj)
		{
			ViewSettings = ViewSettings ?? new TabViewSettings();
			BookmarkData = BookmarkData ?? new Dictionary<string, object>();
			BookmarkData[name] = obj;
		}

		public T GetData<T>(string name = DefaultDataName)
		{
			if (BookmarkData != null && BookmarkData.TryGetValue(name, out object obj) && obj is T t)
				return t;

			return default;
		}

		public void Select(string label)
		{
			Select(new List<string>() { label });
		}

		public void Select(IEnumerable<string> labels)
		{
			var selectedRows = new HashSet<SelectedRow>();
			foreach (string label in labels)
			{
				var selectedRow = new SelectedRow()
				{
					Label = label,
				};
				selectedRows.Add(selectedRow);
			}
			ViewSettings = new TabViewSettings()
			{
				TabDataSettings = new List<TabDataSettings>()
				{
					new TabDataSettings()
					{
						SelectionType = SelectionType.User,
						SelectedRows = selectedRows,
					},
				},
			};
		}

		public TabBookmark AddChild(string label)
		{
			var childBookmark = new TabBookmark()
			{
				Bookmark = Bookmark,
			};
			ChildBookmarks.Add(label, childBookmark);
			return childBookmark;
		}

		public TabBookmark GetChild(string name)
		{
			if (ChildBookmarks == null)
				return null;

			if (ChildBookmarks.TryGetValue(name, out TabBookmark childBookmark))
				return childBookmark;

			return null;
		}

		public void Import(Project project)
		{
			if (ViewSettings == null)
				return;

			foreach (SelectedRow row in ViewSettings.SelectedRows)
			{
				string dataKey = row.DataKey ?? row.Label;
				if (dataKey == null || row.DataValue == null)
					continue;

				project.DataApp.Save(DataRepoDirectory, dataKey, row.DataValue);
			}
			foreach (TabBookmark tabBookmark in ChildBookmarks.Values)
				tabBookmark.Import(project);
		}

		// Returns the deepest TabBookmark that is rootable
		public TabBookmark GetLeaf()
		{
			TabBookmark lastLeaf = null;
			foreach (TabBookmark tabBookmark in ChildBookmarks.Values)
			{
				var leaf = tabBookmark.GetLeaf();
				if (leaf != null)
				{
					if (lastLeaf != null)
					{
						// If there's > 1 leaf nodes, don't return any of them
						lastLeaf = null;
						break;
					}
				}
				lastLeaf = leaf;
			}
			if (lastLeaf != null)
				return lastLeaf;
			if (IsRoot)
				return this;

			return null;
		}

		public void MergeNode(TabBookmark node)
		{
			foreach (var nodeEntry in node.ChildBookmarks)
			{
				if (ChildBookmarks.TryGetValue(nodeEntry.Key, out TabBookmark existingNode))
				{
					existingNode.MergeNode(nodeEntry.Value);
				}
				else
				{
					ChildBookmarks.Add(nodeEntry.Key, nodeEntry.Value);
				}
			}
			if (ViewSettings == null)
			{
				ViewSettings = node.ViewSettings;
				return;
			}
			Name = " + " + node.Name;
			for (int i = 0; i < ViewSettings.TabDataSettings.Count; i++)
			{
				var currentSelection = ViewSettings.TabDataSettings[i].SelectedRows;
				var otherSelection = node.ViewSettings.TabDataSettings[i].SelectedRows;

				var labelsUsed = new HashSet<string>();
				var indicesUsed = new HashSet<int>();
				foreach (SelectedRow row in currentSelection)
				{
					if (row.Label != null)
						labelsUsed.Add(row.Label);
					else
						indicesUsed.Add(row.RowIndex);
				}

				foreach (SelectedRow row in otherSelection)
				{
					if (row.Label != null)
					{
						if (!labelsUsed.Contains(row.Label))
						{
							currentSelection.Add(row);
						}
					}
					else
					{
						if (!indicesUsed.Contains(row.RowIndex))
						{
							currentSelection.Add(row);
						}
					}
				}
			}
		}
	}
}
