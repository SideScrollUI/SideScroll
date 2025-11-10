using Avalonia.Utilities;

namespace SideScroll.Avalonia.Utilities;

/// <summary>
/// Provides a weak event source that wraps a standard event and exposes it through Avalonia's WeakEvent mechanism.
/// Use this when you need to share events across multiple components while preventing memory leaks from event subscriptions.
/// For example, TabChart uses this to synchronize the selection cursor across multiple charts,
/// allowing charts to communicate with each other without creating strong references that would prevent garbage collection.
/// </summary>
public class WeakEventSource<TEventArgs> where TEventArgs : EventArgs
{
	/// <summary>
	/// The underlying event that subscribers can attach to
	/// </summary>
	public event EventHandler<TEventArgs>? Event;

	/// <summary>
	/// Raises the event with the specified sender and event arguments
	/// </summary>
	public void Raise(object? sender, TEventArgs eventArgs)
	{
		Event?.Invoke(sender, eventArgs);
	}

	/// <summary>
	/// Avalonia's weak event wrapper that prevents memory leaks from event subscriptions.
	/// Subscribers are held with weak references, allowing them to be garbage collected even if they forget to unsubscribe.
	/// </summary>
	public readonly WeakEvent<WeakEventSource<TEventArgs>, TEventArgs> WeakEvent = global::Avalonia.Utilities.WeakEvent.Register<WeakEventSource<TEventArgs>, TEventArgs>(
		(t, s) => t.Event += s,
		(t, s) => t.Event -= s);
}

/// <summary>
/// Delegate for handling weak events
/// </summary>
public delegate void WeakEventAction<TEventArgs>(object? sender, TEventArgs args) where TEventArgs : EventArgs;

/// <summary>
/// Implements a weak event subscriber that wraps an action delegate.
/// This allows subscribing to weak events using lambda expressions or method references.
/// </summary>
public class WeakSubscriber<TEventArgs>(WeakEventAction<TEventArgs> _onEvent) :
	IWeakEventSubscriber<TEventArgs> where TEventArgs : EventArgs
{
	/// <summary>
	/// Called when the weak event is raised
	/// </summary>
	public void OnEvent(object? sender, WeakEvent ev, TEventArgs args)
	{
		_onEvent.Invoke(sender, args);
	}
}
