using Atlas.Core;

namespace Atlas.Tabs.Test.DataGrid;

public class TabTestMemory : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private byte[]? _bytes;

		public override void Load(Call call, TabModel model)
		{
			//_bytes = new byte[500000000]; // 500 MB, creates 200k strings using ListToString
			_bytes = new byte[128];
			for (int i = 0; i < _bytes.Length; i++)
				_bytes[i] = (byte)i;

			model.Items = new ItemCollection<ListItem>()
			{
				new("Bytes", _bytes),
			};
		}
	}
}
/*
For testing memory usage and leaks?
*/
