using Avalonia;
using SideScroll.Serialize;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Avalonia.Themes.Tabs;

/// <summary>
/// Maintains a navigable history of <see cref="AvaloniaThemeSettings"/>, enabling undo and redo of theme changes.
/// </summary>
public class ThemeHistory : AvaloniaObject
{
	private readonly List<AvaloniaThemeSettings> _items = [];
	private int _index = -1;

	/// <summary>Gets whether there is a previous theme state to navigate back to.</summary>
	public bool HasPrevious
	{
		get => GetValue(HasPreviousProperty);
		private set => SetValue(HasPreviousProperty, value);
	}

	/// <summary>Gets whether there is a next theme state to navigate forward to.</summary>
	public bool HasNext
	{
		get => GetValue(HasNextProperty);
		private set => SetValue(HasNextProperty, value);
	}

	/// <summary>Avalonia styled property backing <see cref="HasPrevious"/>.</summary>
	public static readonly StyledProperty<bool> HasPreviousProperty =
		AvaloniaProperty.Register<ThemeHistory, bool>(nameof(HasPrevious));

	/// <summary>Avalonia styled property backing <see cref="HasNext"/>.</summary>
	public static readonly StyledProperty<bool> HasNextProperty =
		AvaloniaProperty.Register<ThemeHistory, bool>(nameof(HasNext));

	private void UpdateState()
	{
		HasPrevious = _index > 0 && _index <= _items.Count;
		HasNext = _index < _items.Count - 1;
	}

	/// <summary>Adds a deep copy of the given theme settings to the history, discarding any redoable states.</summary>
	public void Add(AvaloniaThemeSettings themeSettings)
	{
		// Remove all entries after the current position
		if (_index + 1 < _items.Count)
		{
			_items.RemoveRange(_index + 1, _items.Count - _index - 1);
		}
		_items.Add(themeSettings.DeepClone());
		_index = _items.Count - 1;

		UpdateState();
	}

	/// <summary>Replaces the current history entry with a deep copy of the given theme settings.</summary>
	public void Replace(AvaloniaThemeSettings themeSettings)
	{
		_items[_index] = themeSettings.DeepClone();
	}

	/// <summary>Moves back in history and returns the previous theme settings, or <c>false</c> if at the start.</summary>
	public bool TryGetPrevious([NotNullWhen(true)] out AvaloniaThemeSettings? themeSettings)
	{
		themeSettings = null;
		if (!HasPrevious) return false;

		_index--;
		themeSettings = _items[_index];
		UpdateState();
		return true;
	}

	/// <summary>Moves forward in history and returns the next theme settings, or <c>false</c> if at the end.</summary>
	public bool TryGetNext([NotNullWhen(true)] out AvaloniaThemeSettings? themeSettings)
	{
		themeSettings = null;
		if (!HasNext) return false;

		_index++;
		themeSettings = _items[_index];
		UpdateState();
		return true;
	}
}
