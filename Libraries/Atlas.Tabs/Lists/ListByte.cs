using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Tabs;

public class ListByte
{
	[StyleValue]
	public int Index { get; set; }

	public byte Byte { get; set; }
	public string Hex { get; set; }
	public char Char { get; set; }

	public override string ToString() => Index.ToString();

	public ListByte(int index, byte b)
	{
		Index = index;
		Byte = b;
		Hex = BitConverter.ToString(new byte[] { b });
		Char = Convert.ToChar(b);
	}

	public static List<ListByte> Create(byte[] bytes)
	{
		int i = 0;
		return bytes
			.Select(b => new ListByte(i++, b))
			.ToList();
	}
}
