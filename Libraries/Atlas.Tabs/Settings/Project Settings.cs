﻿using Atlas.Core;
using System;

namespace Atlas.Tabs
{
	public class ProjectSettings
	{
		public string Name { get; set; }
		public string LinkType { get; set; }        // for bookmarking
		public Version Version { get; set; } = new Version();
		public Version DataVersion { get; set; } = new Version(); // What Data Repo version to use, bump to current Version when you make a breaking serialization change, (like a breaking NameSpace change, no renaming support yet)
	}
}