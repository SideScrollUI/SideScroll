using SideScroll.Attributes;
using SideScroll.Tabs.Samples.Exceptions;

namespace SideScroll.Tabs.Samples.Loading;

[ListItem]
public class TabSampleLoading
{
	public static TabSampleLoadModel Load => new();
	public static TabSampleLoadAsync LoadAsync => new();
	public static TabSampleLoadItemProperties LoadItemProperties => new();
	public static TabSampleLoadAsyncItemDelegate LoadAsyncDelegate => new();
	public static TabSampleLoadAsyncItem LoadAsyncItem => new();
	public static TabSampleSkip Skip => new();
	public static TabSampleExceptions Exceptions => new();
	
}
