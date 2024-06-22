using SideScroll.Serialize;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.UI.Avalonia.Themes.Tabs;

public class ThemeHistory
{
	private List<AvaloniaThemeSettings> _items = [];
	private int _index = -1;

	public bool HasPrevious => _index > 0 && _index <= _items.Count;
	public bool HasNext => _index < _items.Count - 1;

	public void Add(AvaloniaThemeSettings themeSettings)
	{
		// Remove all entries after the current position
		if (_index + 1 < _items.Count)
		{
			_items.RemoveRange(_index + 1, _items.Count - _index - 1);
		}
		_items.Add(themeSettings.DeepClone()!);
		_index = _items.Count - 1;
	}

	public void Replace(AvaloniaThemeSettings themeSettings)
	{
		_items[_index] = themeSettings.DeepClone()!;
	}

	public bool TryGetPrevious([NotNullWhen(true)] out AvaloniaThemeSettings? themeSettings)
	{
		themeSettings = null;
		if (!HasPrevious) return false;

		_index--;
		themeSettings = _items[_index];
		return true;
	}

	public bool TryGetNext([NotNullWhen(true)] out AvaloniaThemeSettings? themeSettings)
	{
		themeSettings = null;
		if (!HasNext) return false;

		_index++;
		themeSettings = _items[_index];
		return true;
	}
}
