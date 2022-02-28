using Atlas.Core;

namespace Atlas.Tabs.Test.Loading;

[ListItem]
public class TabTestLoading
{
	public static TabTestSlowLoad SlowLoad => new();
	public static TabTestSlowModel SlowModel => new();
	public static TabTestSlowAsyncItem SlowAsyncItem => new();
	public static TabTestSlowAsyncModel SlowAsyncModel => new();
}
