using SideScroll.Attributes;
using SideScroll.Tabs.Samples.Exceptions;

namespace SideScroll.Tabs.Samples.Loading;

[ListItem]
public class TabSampleLoading
{
	public static TabSampleSlowLoad SlowLoad => new();
	public static TabSampleSlowModel SlowModel => new();
	public static TabSampleSlowAsyncItem SlowAsyncItem => new();
	public static TabSampleSlowAsyncModel SlowAsyncModel => new();
	public static TabSampleSkip Skip => new();
	public static TabSampleExceptions Exceptions => new();
}
