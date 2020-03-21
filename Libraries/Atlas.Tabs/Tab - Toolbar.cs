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

		public TaskDelegate.CallAction Action { get; set; }

		public ToolButton(string label, Stream icon)
		{
			Label = label;
			Icon = icon;
		}
	}

	public class TabToolbar
	{
	}
}
