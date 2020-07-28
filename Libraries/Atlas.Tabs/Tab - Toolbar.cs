using Atlas.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Tabs
{
	public class ToolButton
	{
		public string Label { get; set; }
		public Stream Icon { get; set; }
		public bool ShowTask { get; set; }

		public TaskDelegate.CallAction Action { get; set; }
		public TaskDelegateAsync.CallActionAsync ActionAsync { get; set; }

		public ToolButton(string label, Stream icon, TaskDelegate.CallAction action = null)
		{
			Label = label;
			Icon = icon;
			Action = action;
		}

		public ToolButton(string label, Stream icon, TaskDelegateAsync.CallActionAsync actionAsync)
		{
			Label = label;
			Icon = icon;
			ActionAsync = actionAsync;
		}
	}

	public class TabToolbar
	{
		public List<ToolButton> Buttons { get; set; } = new List<ToolButton>();
	}
}
