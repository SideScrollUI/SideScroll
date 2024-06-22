
using SideScroll.Utilities;

namespace SideScroll;

public static class SideScrollInit
{
	public static void Initialize()
	{
		FileUtils.SetUmaskUserOnly();
	}
}
