using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Atlas.Core;

namespace Atlas.Tabs.Test.Objects
{
	public class TabTestObjects : ITab
	{
		public TabInstance Create() => new Instance();

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
}
