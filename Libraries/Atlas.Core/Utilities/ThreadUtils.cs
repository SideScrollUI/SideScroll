using System;
using System.Threading.Tasks;

namespace Atlas.Core
{
	public class ThreadUtils
	{
		public static void InvokeDelayed(int milliSecondsDelay, Action action)
		{
			Task.Delay(milliSecondsDelay).ContinueWith(t => action());
		}

		public static void InvokeDelayed(TimeSpan timeSpanDelay, Action action)
		{
			Task.Delay(timeSpanDelay).ContinueWith(t => action());
		}
	}
}
