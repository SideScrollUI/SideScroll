using System;
using Atlas.Core;

namespace Atlas.Start.Avalonia.Tabs
{
	[Params]
	public class MyParams //: INotifyPropertyChanged
	{
		public string Name { get; set; } = "Sprite";
		public int Amount { get; set; } = 3;
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
	}
}
