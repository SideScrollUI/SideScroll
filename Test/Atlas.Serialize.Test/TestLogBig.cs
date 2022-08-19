using Atlas.Core;

namespace Atlas.Serialize.Test;

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
	// Change everything to tags? const for created/message/childLog? harder to use then
	public DateTime Created;// { get; set; }
	public LogType Type { get; set; }
	public string? Text;// { get; set; }
	public string? Message
	{
		get
		{
			if (Tags == null)
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
			if (Tags == null)
				return line;

			foreach (Tag tag in Tags)
			{
				line += tag.ToString() + " ";
			}
			return line;
		}
	}
	public Tag[]? Tags;

	[InnerValue]
	public ItemCollection<TestLogBig>? Items; // change to LRU for performance? No Binding?

	public TestLogBig() { }

	// Todo: use caller instead
	public void Child(string name)
	{
		var logEntry = new TestLogBig();
		//log.Type = logType;
		//logEntry = new Log(context, contextID, settings, "replacing log with local", new Tag[] { });
		if (Items == null)
			Items = new ItemCollection<TestLogBig>();
		//if (Items.Count > settings.MaxLogItems)
		//	Items.RemoveAt(0);
		Items.Add(logEntry);
	}
}
