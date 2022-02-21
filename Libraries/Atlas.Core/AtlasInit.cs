
namespace Atlas.Core;

public static class AtlasInit
{
	public static void Initialize()
	{
		FileUtils.SetUmaskUserOnly();
	}
}
