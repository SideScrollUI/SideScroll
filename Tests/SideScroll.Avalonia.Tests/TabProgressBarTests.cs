using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using NUnit.Framework;
using SideScroll.Avalonia.Controls;

namespace SideScroll.Avalonia.Tests;

/// <summary>
/// Avalonia leaves an indeterminate progress bar's infinite animation subscribed to the global clock after the
/// control is detached (AvaloniaUI/Avalonia#16648), so a detached bar, and everything reachable from it, is only
/// collected once <see cref="ProgressBar.IsIndeterminate"/> is turned off. <see cref="TabProgressBar"/> turns it
/// off itself when it leaves the tree
/// </summary>
public class TabProgressBarTests
{
	[AvaloniaTest]
	public void Detached_indeterminate_bar_is_collected()
	{
		WeakReference weak = ShowThenDetach(() => new TabProgressBar { IsIndeterminate = true });

		Assert.That(TryCollect(weak), Is.True, "A detached TabProgressBar's indeterminate animation kept its parent alive");
	}

	/// <summary>
	/// The reason the control exists. Inconclusive rather than failed if Avalonia stops leaking, since that would
	/// make <see cref="TabProgressBar"/> unnecessary rather than wrong
	/// </summary>
	[AvaloniaTest]
	public void Detached_plain_indeterminate_bar_still_leaks()
	{
		WeakReference weak = ShowThenDetach(() => new ProgressBar { IsIndeterminate = true });

		Assume.That(TryCollect(weak), Is.False, "Avalonia no longer keeps a detached indeterminate ProgressBar alive, so TabProgressBar may no longer be needed");
	}

	[AvaloniaTest]
	public void Reattached_bar_is_indeterminate_again()
	{
		var bar = new TabProgressBar { IsIndeterminate = true };
		var grid = new Grid();
		grid.Children.Add(bar);
		var window = new Window { Width = 400, Height = 300, Content = grid };
		HeadlessWindow.ShowAndSettle(window);

		window.Content = null;
		HeadlessWindow.Settle(window);
		Assert.That(bar.IsIndeterminate, Is.False, "Detaching should turn the animation off");

		window.Content = grid;
		HeadlessWindow.Settle(window);
		Assert.That(bar.IsIndeterminate, Is.True, "Reattaching should turn it back on");

		window.Close();
	}

	/// <summary>
	/// Shows the bar inside a grid, detaches the grid, and returns a weak reference to the grid, which the
	/// animation reaches through the bar's visual parent
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static WeakReference ShowThenDetach(Func<ProgressBar> createBar)
	{
		var grid = new Grid();
		grid.Children.Add(createBar());
		var window = new Window { Width = 400, Height = 300, Content = grid };
		HeadlessWindow.ShowAndSettle(window);
		Flush(window);

		var weak = new WeakReference(grid);
		grid = null!;
		window.Content = null;
		Flush(window);
		return weak;
	}

	/// <summary>
	/// Runs the layout pass that drops the presenter's old child and the render ticks that commit the compositor
	/// batch still referencing the removed visuals
	/// </summary>
	private static void Flush(Window window)
	{
		HeadlessWindow.Settle(window);
		for (int i = 0; i < 3; i++)
		{
			AvaloniaHeadlessPlatform.ForceRenderTimerTick();
			Dispatcher.UIThread.RunJobs();
		}
	}

	private static bool TryCollect(WeakReference weak)
	{
		for (int i = 0; i < 5 && weak.IsAlive; i++)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			Dispatcher.UIThread.RunJobs();
		}
		return !weak.IsAlive;
	}
}
