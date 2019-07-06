using Atlas.Core;
using System;

namespace Atlas.GUI.Avalonia.Controls
{
	[Params]
	public class BookmarkParams //: INotifyPropertyChanged
	{
		public string Name { get; set; } = "Sprite";
		public int Amount { get; set; } = 3;
		//public DateTime? Start { get; set; }
		//public DateTime? End { get; set; }
	}
}
