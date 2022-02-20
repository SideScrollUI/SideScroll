using Atlas.Core;

namespace Atlas.Tabs.Test.Loading;

[ListItem]
public class TabTestLoading
{
	public TabTestSlowLoad SlowLoad => new();
	public TabTestSlowModel SlowModel => new();
	public TabTestSlowAsyncItem SlowAsyncItem => new();
	public TabTestSlowAsyncModel SlowAsyncModel => new();
}
