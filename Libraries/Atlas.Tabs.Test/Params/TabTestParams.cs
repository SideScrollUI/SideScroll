using Atlas.Core;

namespace Atlas.Tabs.Test.Params;

[ListItem]
public class TabTestParams
{
	public static TabTestParamsTasks Tasks => new();
	public static TabTestParamsCollection Collection => new();
	public static TabTestParamsDataTabs DataTabs => new();
}
