using Atlas.Core;

namespace Atlas.Tabs.Test;

[ListItem]
public class TabTestParams
{
	public TabTestParamsTasks Tasks => new();
	public TabTestParamsCollection Collection => new();
	public TabTestParamsDataTabs DataTabs => new();
}
