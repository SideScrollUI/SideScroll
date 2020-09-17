using Atlas.Core;
using System.Collections;

namespace Atlas.Tabs
{
	public class ListToString
	{
		[InnerValue]
		public object Object;
		
		public string Value { get; set; }

		public override string ToString() => Value;

		public ListToString(object obj)
		{
			Object = obj;
			if (obj != null)
				Value = obj.ToString();
		}

		public static ItemCollection<ListToString> Create(IEnumerable enumerable, int limit = 200000)
		{
			var list = new ItemCollection<ListToString>();
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
