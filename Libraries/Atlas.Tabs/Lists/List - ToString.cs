using Atlas.Core;
using System.Collections;

namespace Atlas.Tabs
{
	public class ListToString
	{
		[InnerValue]
		public object obj;
		
		public string Value { get; set; }

		public ListToString(object obj)
		{
			this.obj = obj;
			if (obj != null)
				Value = obj.ToString();
		}

		public override string ToString()
		{
			return Value;
		}

		public static ItemCollection<ListToString> Create(IEnumerable enumerable, int limit = 200000)
		{
			ItemCollection<ListToString> list = new ItemCollection<ListToString>();
			foreach (object obj in enumerable)
			{
				list.Add(new ListToString(obj));
				if (list.Count > limit)
					break;
			}
			return list;
		}
	}
}

/*
*/