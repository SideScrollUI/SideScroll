using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Atlas.Serialize.Test
{
	public class TestLogBig
	{
		public enum LogType
		{
			Debug,
			Tab,
			Call,
			Info,
			Warn,
			Error,
			Alert
		}
		//public event PropertyChangedEventHandler PropertyChanged;
		// Change everything to tags? const for created/message/childLog? harder to use then
		public DateTime Created;// { get; set; }
		public LogType Type { get; set; }
		public string Text;// { get; set; }
		public string Message
		{
			get
			{
				if (tags == null)
					return Text;
				//if (tags.Count == 0)
				if (TagText == "")
					return Text;
				return Text + " " + TagText;
			}
		}
		public int Entries { get; set; }

		//[AttributeName("Tags")]
		[HiddenColumn]
		public string TagText
		{
			get
			{
				string line = "";
				if (tags == null)
					return line;

				foreach (Tag tag in tags)
				{
					line += tag.ToString() + " ";
				}
				return line;
			}
		}
		public Tag[] tags;

		[InnerValue]
		public ItemCollection<TestLogBig> items; // change to LRU for performance? No Binding?
		//private SynchronizationContext context = null;
		//private int contextID = 0;

		//public event EventHandler<EventLogMessage> OnMessage;

		public TestLogBig()
		{
		}
		
		// Todo: use caller instead
		public void Child(string name)
		{
			TestLogBig logEntry = new TestLogBig();
			//log.Type = logType;
			//logEntry = new Log(context, contextID, settings, "replacing log with local", new Tag[] { });
			if (items == null)
				items = new ItemCollection<TestLogBig>();
			//if (items.Count > settings.MaxLogItems)
			//	items.RemoveAt(0);
			items.Add(logEntry);
		}
	}
}
