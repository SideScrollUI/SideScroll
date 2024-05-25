using Avalonia.Utilities;

namespace Atlas.UI.Avalonia.Utilities;

public class WeakEventSource<TEventArgs> where TEventArgs : EventArgs
{
	public event EventHandler<TEventArgs>? Event;

	public void Raise(object? sender, TEventArgs eventArgs)
	{
		Event?.Invoke(sender, eventArgs);
	}

	public readonly WeakEvent<WeakEventSource<TEventArgs>, TEventArgs> WeakEvent = global::Avalonia.Utilities.WeakEvent.Register<WeakEventSource<TEventArgs>, TEventArgs>(
		(t, s) => t.Event += s,
		(t, s) => t.Event -= s);
}

public delegate void WeakEventAction<TEventArgs>(object? sender, TEventArgs args) where TEventArgs : EventArgs;

public class WeakSubscriber<TEventArgs>(WeakEventAction<TEventArgs> _onEvent) :
	IWeakEventSubscriber<TEventArgs> where TEventArgs : EventArgs
{
	public void OnEvent(object? sender, WeakEvent ev, TEventArgs args)
	{
		_onEvent.Invoke(sender, args);
	}
}
