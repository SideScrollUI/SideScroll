using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using Atlas.Core;

namespace Atlas.Tabs.Test.Objects
{
	public class TabTestObjects : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("UriTest", new UriTest("test")),
					new ListItem("Tags", new Tag[] { new Tag("abc", 1.1) }),
					new ListItem("Subclass Property", new TabTestSubClassProperty()),
					new ListItem("Subclass", new ValueSub()),
					new ListItem("Enum", new EnumTest()),
					new ListItem("TimeSpan", new TimeSpan(1, 2, 3)),
				};
			}

			public class MyClass
			{
				public string Name { get; set; } = "Eve";
			}

			public enum EnumTest
			{
				One = 1,
				Two = 2,
				Four = 4,
				Eight = 8,
			}
		}
	}

	public class ValueBase
	{
		public int value = 1;
	}

	public class ValueSub : ValueBase
	{
		public new int value = 2;
	}

	public class UriTest : ISerializable
	{
		public static readonly string SchemeDelimiter;


		public bool IsDefaultPort { get; }
		public string Authority { get; }
		public string DnsSafeHost { get; }
		public string Fragment { get; }
		public string Host { get; }
		public UriHostNameType HostNameType { get; }
		public string IdnHost { get; }
		public bool IsAbsoluteUri { get; }
		public bool IsFile { get; }
		public string[] Segments { get; }
		public bool IsUnc { get; }
		public string LocalPath { get; }
		public string OriginalString { get; }
		public string PathAndQuery { get; }
		public int Port { get; }
		public string Query { get; }
		public string Scheme { get; }
		public string AbsoluteUri { get; }
		public bool IsLoopback { get; }
		public string AbsolutePath { get; }
		public string UserInfo { get; }
		public bool UserEscaped { get; }

		public UriTest(string uriString)
		{

		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException();
		}
	}


	/*public class LogEntry2 : INotifyPropertyChanged
	{
		public enum LogType
		{
			Debug,
			Info,
			Warn,
			Error,
			Alert
		}
		public event PropertyChangedEventHandler PropertyChanged;
		public DateTime Created;// { get; set; }
		public LogType originalType = LogType.Info;
		public LogType Type { get; set; } = LogType.Info;
		public string Text;// { get; set; }
		public string Message
		{
			get
			{
				if (tags == null)
					return Text;
				string tagText = TagText;
				if (tagText == "")
					return Text;
				return Text + " " + tagText;
			}
		}
		public int Entries { get; set; }

		private float? _Duration;
		public float? Duration
		{
			get
			{
				return _Duration;
			}
			set
			{
				_Duration = value;
				CreateEventPropertyChanged();
			}
		}

		//[AttributeName("Tags")]
		//[HiddenColumn]
		private string TagText
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

		public LogEntry2()
		{
		}

		public LogEntry2(LogType logType, string text, Tag[] tags)
		{
			this.originalType = logType;
			this.Type = logType;
			this.Text = text;
			this.tags = tags;
			this.Created = DateTime.Now;
		}

		public override string ToString()
		{
			return Message;
		}

		protected void CreateEventPropertyChanged([CallerMemberName] String propertyName = "")
		{
			//context.Post(new SendOrPostCallback(this.NotifyPropertyChangedContext), propertyName);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}*/
}
