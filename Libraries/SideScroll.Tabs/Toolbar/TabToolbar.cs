using SideScroll.Extensions;
using System.Reflection;

namespace SideScroll.Tabs.Toolbar;

/// <summary>
/// Container for toolbar buttons and controls displayed in a tab
/// This class can be created outside the UI thread, and the UI controls will be created when loading
/// </summary>
public class TabToolbar : IDisposable
{
	/// <summary>
	/// Additional toolbar buttons appended after any <see cref="ToolButton"/> properties
	/// declared on a subclass. The primary way to define buttons is to add them as typed
	/// properties on a <c>TabToolbar</c> subclass (e.g. <c>public ToolButton ButtonSave { get; } = new(…)</c>);
	/// use this collection only when buttons must be added dynamically at runtime.
	/// </summary>
	public List<ToolButton> AdditionalButtons { get; set; } = [];

	/// <summary>
	/// Releases the bindings held by this toolbar's buttons
	/// </summary>
	/// <remarks>
	/// A <see cref="ToolToggleButton.ListProperty"/> subscribes to the object it's bound to, and
	/// this owns it. The control rendering the button releases only its own subscription, since the
	/// same button can be rendered again and disposing the property would leave the next control
	/// bound to one that no longer observes anything
	/// </remarks>
	public virtual void Dispose()
	{
		foreach (ToolToggleButton toggleButton in GetToggleButtons())
		{
			toggleButton.ListProperty?.Dispose();
			toggleButton.ListProperty = null;
		}

		GC.SuppressFinalize(this);
	}

	private IEnumerable<ToolToggleButton> GetToggleButtons()
	{
		foreach (PropertyInfo propertyInfo in GetType().GetVisibleProperties())
		{
			// A computed property can throw, which shouldn't stop the rest from being released
			object? propertyValue;
			try
			{
				propertyValue = propertyInfo.GetValue(this);
			}
			catch (Exception)
			{
				continue;
			}

			if (propertyValue is ToolToggleButton toggleButton)
			{
				yield return toggleButton;
			}
		}

		foreach (ToolButton toolButton in AdditionalButtons)
		{
			if (toolButton is ToolToggleButton toggleButton)
			{
				yield return toggleButton;
			}
		}
	}
}
