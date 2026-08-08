using NUnit.Framework;
using SideScroll.Tabs.Samples.Charts;
using System.Runtime.CompilerServices;

namespace SideScroll.Tabs.Tests.Charts;

[Category("Tabs")]
public class TabProcessMonitorTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TabProcessMonitor");
	}

	// Kept out of the test method so nothing local still references the instance or its model.
	// The toolbar actions are delegates over the instance, and the model holds the toolbar
	[MethodImpl(MethodImplOptions.NoInlining)]
	private WeakReference LoadAndRelease(bool dispose)
	{
		TabInstance instance = new TabProcessMonitor().Create();
		TabModel model = new();
		instance.Load(Call, model);

		if (dispose)
		{
			instance.Dispose();
		}
		return new WeakReference(instance);
	}

	[TestCase(true, false, TestName = "Disposed process monitor is collected")]
	[TestCase(false, true, TestName = "Undisposed process monitor is held by its timer")]
	[Description(
		"Load() starts a sampling Timer, which the runtime roots and whose callback holds the tab " +
		"instance, so navigating away would leave it sampling forever. The undisposed case proves " +
		"the collection check can actually fail")]
	public void DisposeStopsSampling(bool dispose, bool expectedAlive)
	{
		WeakReference reference = LoadAndRelease(dispose);

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		Assert.That(reference.IsAlive, Is.EqualTo(expectedAlive));
	}

	[Test]
	public void DisposeIsSafeToCallTwice()
	{
		TabInstance instance = new TabProcessMonitor().Create();
		instance.Load(Call, new TabModel());

		instance.Dispose();

		Assert.DoesNotThrow(instance.Dispose);
	}
}
