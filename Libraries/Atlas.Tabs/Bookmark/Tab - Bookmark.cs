using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Tabs
{
	public class DataRepoItem
	{
		public string Directory { get; set; }
		public string Key { get; set; }
		public object Value { get; set; }
	}

	// rename to TabInstanceSettings?
	public class TabBookmark
	{
		public Bookmark Bookmark { get; set; }
		public string Name { get; set; }
		public bool IsRoot { get; set; }
		public TabViewSettings ViewSettings = new TabViewSettings(); // list selections, doesn't know about children
		public Dictionary<string, TabBookmark> ChildBookmarks { get; set; } = new Dictionary<string, TabBookmark>(); // doesn't know which tabData to use, maps id to child info
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
					//string address = "";
					if (ViewSettings == null)
						return "";
					//string address = "<" + tabConfiguration.Address + ">";
					string address = ViewSettings.Address;
					return address;
				}
			}
		}
		//public List<DataRepoItem> DataRepoItems { get; set; } = new List<DataRepoItem>();
		public string DataRepoDirectory { get; set; }

		// Temporary, Only FindMatches() uses, refactor these out?
		[NonSerialized]
		public HashSet<object> selectedObjects = new HashSet<object>(); // does this work with multiple TabDatas?
		[NonSerialized]
		public TabModel tabModel;

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
		public static TabBookmark Create(params object[] objs)
		{
			// get TabBookmark.selectedObjects working again and replace?

			TabBookmark tabBookmark = null;
			foreach (object obj in objs)
			{
				string label = obj.ToString();
				var newBookmark = new TabBookmark()
				{
					ViewSettings = new TabViewSettings()
					{
						TabDataSettings = new List<TabDataSettings>()
						{
							new TabDataSettings()
							{
								SelectionType = SelectionType.User,
								SelectedRows = new HashSet<SelectedRow>()
								{
									new SelectedRow()
									{
										label = label,
									},
								},
							},
						},
					},
				};
				if (tabBookmark != null)
					tabBookmark.ChildBookmarks.Add(label, newBookmark);
				else
					tabBookmark = newBookmark;
			}
			return tabBookmark;
		}

		public override string ToString()
		{
			return Name;// string.Join(",", Nodes.Keys);
		}

		public SortedDictionary<string, T> GetSelectedData<T>()
		{
			var items = new SortedDictionary<string, T>();
			if (ViewSettings != null)
			{
				foreach (SelectedRow row in ViewSettings.SelectedRows)
				{
					string dataKey = row.dataKey ?? row.label;
					if (dataKey != null && row.dataValue != null && row.dataValue.GetType() == typeof(T))
						items[dataKey] = (T)row.dataValue;
				}
			}
			return items;
		}

		public void SetData(object obj)
		{
			SetData("default", obj);
		}

		public void SetData(string name, object obj)
		{
			ViewSettings = ViewSettings ?? new TabViewSettings();
			ViewSettings.BookmarkData = ViewSettings.BookmarkData ?? new Dictionary<string, object>();
			ViewSettings.BookmarkData[name] = obj;
		}

		public T GetData<T>(string name = "default")
		{
			if (ViewSettings != null && ViewSettings.BookmarkData != null && ViewSettings.BookmarkData.TryGetValue(name, out object obj) && obj is T t)
				return t;

			return default;
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
				string dataKey = row.dataKey ?? row.label;
				if (dataKey == null || row.dataValue == null)
					continue;

				project.DataApp.Save(DataRepoDirectory, dataKey, row.dataValue);
			}
			foreach (TabBookmark tabBookmark in ChildBookmarks.Values)
				tabBookmark.Import(project);
		}

		public TabBookmark GetLeaf()
		{
			foreach (TabBookmark tabBookmark in ChildBookmarks.Values)
			{
				var leaf = tabBookmark.GetLeaf();
				if (leaf != null)
					return leaf;
			}
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
					if (row.label != null)
						labelsUsed.Add(row.label);
					else
						indicesUsed.Add(row.rowIndex);
				}

				foreach (SelectedRow row in otherSelection)
				{
					if (row.label != null)
					{
						if (!labelsUsed.Contains(row.label))
						{
							currentSelection.Add(row);
						}
					}
					else
					{
						if (!indicesUsed.Contains(row.rowIndex))
						{
							currentSelection.Add(row);
						}
					}
				}
			}
		}
	}
}
