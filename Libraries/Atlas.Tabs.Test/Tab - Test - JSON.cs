﻿using System;
using System.Collections.Generic;
using System.Text;
using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabTestJson : ITab
	{
		public readonly static string json1 =
@"
{
""id"":""abc"",
""value"": 123,
""map"":
  {
    ""list"":
    [
	  {""type"": ""Cat"", ""Age"": 3},
	  {""type"": ""Dog"", ""Age"": 5},
  	  {""type"": ""Frog"", ""Age"": 7},
	  {""type"": ""Turtle"", ""Age"": 11}
    ]
  }
}
";
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Sample Text", LazyJsonNode.Parse(json1)),
				};
			}
		}
	}
}
/*

*/
