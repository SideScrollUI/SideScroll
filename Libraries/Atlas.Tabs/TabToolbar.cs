using Atlas.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Tabs
{
	public class ToolButton
	{
		public string Tooltip { get; set; }
		public string Label { get; set; }
		public Stream Icon { get; set; }
		public bool ShowTask { get; set; }

		public TaskDelegate.CallAction Action { get; set; }
		public TaskDelegateAsync.CallActionAsync ActionAsync { get; set; }

		public ToolButton(string tooltip, Stream icon, TaskDelegate.CallAction action = null)
		{
			Tooltip = tooltip;
			Icon = icon;
			Action = action;
		}

		public ToolButton(string tooltip, Stream icon, TaskDelegateAsync.CallActionAsync actionAsync)
		{
			Tooltip = tooltip;
			Icon = icon;
			ActionAsync = actionAsync;
		}
	}

	public class TabToolbar
	{
		public List<ToolButton> Buttons { get; set; } = new List<ToolButton>();
	}
}
