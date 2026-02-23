
using SideScroll.Utilities;

namespace SideScroll;

/// <summary>
/// Provides initialization functionality for the SideScroll library
/// </summary>
public static class SideScrollInit
{
	/// <summary>
	/// Initializes the SideScroll library by setting file permissions to user-only access
	/// </summary>
	public static void Initialize()
	{
		FileUtils.SetUmaskUserOnly();
	}
}
