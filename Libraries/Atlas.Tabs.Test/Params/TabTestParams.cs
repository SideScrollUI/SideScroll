using Atlas.Core;

namespace Atlas.Tabs.Test.Params;

[ListItem]
public class TabTestParams
{
	public TabTestParamsTasks Tasks => new();
	public TabTestParamsCollection Collection => new();
	public TabTestParamsDataTabs DataTabs => new();
}
