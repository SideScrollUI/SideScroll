using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Tabs;

public class ListByte
{
	[StyleValue]
	public int Index { get; init; }

	public byte Byte { get; init; }
	public string Hex { get; init; }
	public char Char { get; init; }
	public string Bits { get; init; }

	public override string ToString() => Index.ToString();

	public ListByte(int index, byte b)
	{
		Index = index;
		Byte = b;
		Hex = BitConverter.ToString(new byte[] { b });
		Char = Convert.ToChar(b);
		Bits = Convert.ToString(b, 2).PadLeft(8, '0');
	}

	public static List<ListByte> Create(byte[] bytes)
	{
		int i = 0;
		return bytes
			.Select(b => new ListByte(i++, b))
			.ToList();
	}
}
