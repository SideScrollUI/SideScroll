using SideScroll.Attributes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SideScroll.Tabs.Samples.Actions;

/// <summary>
/// Bindable state for demonstrating <see cref="Tasks.TaskCreator.IsEnabledBinding"/>.
/// Toggling <see cref="ActionsEnabled"/> enables or disables the bound action buttons.
/// </summary>
public class SampleActionState(SynchronizationContext context) : INotifyPropertyChanged
{
	[Name("Actions Enabled")]
	public bool ActionsEnabled
	{
		get;
		set
		{
			field = value;
			NotifyPropertyChanged();
		}
	} = true;

	public event PropertyChangedEventHandler? PropertyChanged;

	public override string ToString() => ActionsEnabled ? "Enabled" : "Disabled";

	public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		context.Post(NotifyPropertyChangedContext, propertyName);
	}

	private void NotifyPropertyChangedContext(object? state)
	{
		string propertyName = (string)state!;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
