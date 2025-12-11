using SideScroll.Attributes;

namespace SideScroll.Tabs.Lists;

public class ListEnumValue(string name, bool isSelected, object value)
{
	[StyleValue]
	public bool Selected => isSelected;

	public string Name => name;
	public object Value => value;

	public override string ToString() => Name;

	public static List<ListEnumValue> Create(Enum enumValue)
	{
		Type enumType = enumValue.GetType();
		List<ListEnumValue> flags = [];

		bool isFlagsEnum = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;

		foreach (Enum flag in Enum.GetValues(enumType))
		{
			bool isSelected = isFlagsEnum ? enumValue.HasFlag(flag) : enumValue.Equals(flag);
			
			string name = flag.ToString();
			object value = Convert.ToInt64(flag);

			flags.Add(new ListEnumValue(name, isSelected, value));
		}

		return flags;
	}

	public static List<ListEnumValue> Create<T>() where T : Enum
	{
		Type enumType = typeof(T);
		List<ListEnumValue> flags = [];

		bool isFlagsEnum = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;

		foreach (T flag in Enum.GetValues(enumType))
		{
			string name = flag.ToString();
			object value = Convert.ToInt64(flag);

			// No selection for static enum display
			flags.Add(new ListEnumValue(name, false, value));
		}

		return flags;
	}
}
