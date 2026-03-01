using SideScroll.Attributes;

namespace SideScroll.Tabs.Lists;

/// <summary>
/// Represents a single enum value with its name, numeric value, hexadecimal representation, and selection state
/// </summary>
public class ListEnumValue(string name, bool isSelected, object value, string hex)
{
	/// <summary>
	/// Gets whether this enum value is selected/flagged
	/// </summary>
	[StyleValue]
	public bool Selected => isSelected;

	/// <summary>
	/// Gets the name of the enum value
	/// </summary>
	public string Name => name;

	/// <summary>
	/// Gets the numeric value of the enum
	/// </summary>
	public object Value => value;

	/// <summary>
	/// Gets the hexadecimal representation of the enum value
	/// </summary>
	public string Hex => hex;

	public override string ToString() => Name;

	/// <summary>
	/// Creates a list of enum values from an enum instance, showing which flags are selected
	/// </summary>
	public static List<ListEnumValue> Create(Enum enumValue)
	{
		Type enumType = enumValue.GetType();
		List<ListEnumValue> flags = [];

		bool isFlagsEnum = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;

		foreach (Enum flag in Enum.GetValues(enumType))
		{
			bool isSelected = isFlagsEnum ? enumValue.HasFlag(flag) : enumValue.Equals(flag);
			
			string name = flag.ToString();
			long value = Convert.ToInt64(flag);
			string hex = $"{value:X}";

			flags.Add(new ListEnumValue(name, isSelected, value, hex));
		}

		return flags;
	}

	/// <summary>
	/// Creates a list of all possible values for an enum type (no selection state)
	/// </summary>
	public static List<ListEnumValue> Create<T>() where T : Enum
	{
		Type enumType = typeof(T);
		List<ListEnumValue> flags = [];

		foreach (T flag in Enum.GetValues(enumType))
		{
			string name = flag.ToString();
			long value = Convert.ToInt64(flag);
			string hex = $"{value:X}";

			// No selection for static enum display
			flags.Add(new ListEnumValue(name, false, value, hex));
		}

		return flags;
	}
}
