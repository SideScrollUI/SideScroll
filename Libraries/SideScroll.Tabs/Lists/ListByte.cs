using SideScroll.Attributes;

namespace SideScroll.Tabs.Lists;

public class ListByte(int index, byte b)
{
	public static int MaxBytes = 100_000;

	[StyleValue]
	public int Index => index;

	public byte Byte => b;
	public string Hex { get; init; } = BitConverter.ToString([b]);
	public char Char { get; init; } = Convert.ToChar(b);
	public string Bits { get; init; } = Convert.ToString(b, 2).PadLeft(8, '0');

	public override string ToString() => Index.ToString();

	public static List<ListByte> Create(IEnumerable<byte> bytes)
	{
		int i = 0;
		return bytes
			.Take(MaxBytes)
			.Select(b => new ListByte(i++, b))
			.ToList();
	}
}
