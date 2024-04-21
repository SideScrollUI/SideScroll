using Atlas.Core;
using Atlas.Tabs.Test.Exceptions;

namespace Atlas.Tabs.Test.Loading;

[ListItem]
public class TabTestLoading
{
	public static TabTestSlowLoad SlowLoad => new();
	public static TabTestSlowModel SlowModel => new();
	public static TabTestSlowAsyncItem SlowAsyncItem => new();
	public static TabTestSlowAsyncModel SlowAsyncModel => new();
	public static TabTestSkip Skip => new();
	public static TabTestExceptions Exceptions => new();
}
