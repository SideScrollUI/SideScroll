using Atlas.Core;

namespace Atlas.Tabs.Samples.Params;

[ListItem]
public class TabSampleParams
{
	public static TabSampleParamsDataTabs DataTabs => new();
	public static TabSampleParamsCollection Collection => new();
	public static TabSampleParamsTasks Tasks => new();
}
