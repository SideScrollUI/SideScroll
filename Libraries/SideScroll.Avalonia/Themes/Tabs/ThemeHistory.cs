using Avalonia;
using SideScroll.Serialize;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Avalonia.Themes.Tabs;

public class ThemeHistory : AvaloniaObject
{
	private readonly List<AvaloniaThemeSettings> _items = [];
	private int _index = -1;

	public bool HasPrevious
	{
		get => GetValue(HasPreviousProperty);
		private set => SetValue(HasPreviousProperty, value);
	}

	public bool HasNext
	{
		get => GetValue(HasNextProperty);
		private set => SetValue(HasNextProperty, value);
	}

	public static readonly StyledProperty<bool> HasPreviousProperty =
		AvaloniaProperty.Register<ThemeHistory, bool>(nameof(HasPrevious));

	public static readonly StyledProperty<bool> HasNextProperty =
		AvaloniaProperty.Register<ThemeHistory, bool>(nameof(HasNext));

	private void UpdateState()
	{
		HasPrevious = _index > 0 && _index <= _items.Count;
		HasNext = _index < _items.Count - 1;
	}

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

	public void Replace(AvaloniaThemeSettings themeSettings)
	{
		_items[_index] = themeSettings.DeepClone();
	}

	public bool TryGetPrevious([NotNullWhen(true)] out AvaloniaThemeSettings? themeSettings)
	{
		themeSettings = null;
		if (!HasPrevious) return false;

		_index--;
		themeSettings = _items[_index];
		UpdateState();
		return true;
	}

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
