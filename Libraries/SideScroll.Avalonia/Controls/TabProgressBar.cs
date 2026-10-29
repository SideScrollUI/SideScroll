using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace SideScroll.Avalonia.Controls;

/// <summary>
/// A progress bar that stops its indeterminate animation when it leaves the logical tree.
/// </summary>
/// <remarks>
/// Avalonia keeps an indeterminate progress bar's infinite animation subscribed to the global clock after the
/// control is detached (AvaloniaUI/Avalonia#16648), which keeps the bar, and everything reachable from it, alive
/// until <see cref="ProgressBar.IsIndeterminate"/> is turned off. Turning it off here frees a detached bar, and it
/// is turned back on if the bar is attached again
/// </remarks>
public class TabProgressBar : ProgressBar
{
	private bool _resumeIndeterminate;

	/// <inheritdoc/>
	protected override Type StyleKeyOverride => typeof(ProgressBar);

	/// <inheritdoc/>
	protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
	{
		base.OnAttachedToLogicalTree(e);

		if (_resumeIndeterminate)
		{
			_resumeIndeterminate = false;
			IsIndeterminate = true;
		}
	}

	/// <inheritdoc/>
	protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
	{
		if (IsIndeterminate)
		{
			_resumeIndeterminate = true;
			IsIndeterminate = false;
		}

		base.OnDetachedFromLogicalTree(e);
	}
}
