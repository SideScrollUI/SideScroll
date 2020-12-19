
namespace Atlas.Core
{
	public class AtlasInit
	{
		public static void Initialize()
		{
			FileUtils.SetUmaskUserOnly();
		}
	}
}
