namespace SideScroll.Utilities;

/// <summary>
/// Provides utilities for working with delayed task execution
/// </summary>
public static class ThreadUtils
{
	/// <summary>
	/// Invokes an action after a specified delay in milliseconds
	/// </summary>
	public static void InvokeDelayed(int milliSecondsDelay, Action action)
	{
		Task.Delay(milliSecondsDelay).ContinueWith(_ => action());
	}

	/// <summary>
	/// Invokes an action after a specified TimeSpan delay
	/// </summary>
	public static void InvokeDelayed(TimeSpan timeSpanDelay, Action action)
	{
		Task.Delay(timeSpanDelay).ContinueWith(_ => action());
	}
}
