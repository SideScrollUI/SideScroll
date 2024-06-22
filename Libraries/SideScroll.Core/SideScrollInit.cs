
using SideScroll.Core.Utilities;

namespace SideScroll.Core;

public static class SideScrollInit
{
	public static void Initialize()
	{
		FileUtils.SetUmaskUserOnly();
	}
}
