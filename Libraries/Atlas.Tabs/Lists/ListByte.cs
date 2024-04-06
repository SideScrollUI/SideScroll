using Atlas.Core;

namespace Atlas.Tabs;

public class ListByte(int index, byte b)
{
	[StyleValue]
	public int Index { get; init; } = index;

	public byte Byte { get; init; } = b;
	public string Hex { get; init; } = BitConverter.ToString(new byte[] { b });
	public char Char { get; init; } = Convert.ToChar(b);
	public string Bits { get; init; } = Convert.ToString(b, 2).PadLeft(8, '0');

	public override string ToString() => Index.ToString();

	public static List<ListByte> Create(byte[] bytes)
	{
		int i = 0;
		return bytes
			.Select(b => new ListByte(i++, b))
			.ToList();
	}
}
