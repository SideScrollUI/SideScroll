using SideScroll.Attributes;

namespace SideScroll.Tabs.Lists;

public class ListByte(int index, byte b)
{
	public static int MaxBytes { get; set; } = 100_000;

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

	public static List<ListByte> Load(string path)
	{
		var fileInfo = new FileInfo(path);
		if (fileInfo.Length > MaxBytes)
		{
			using FileStream fileStream = File.OpenRead(path);

			var buffer = new byte[MaxBytes];
			fileStream.ReadExactly(buffer, 0, buffer.Length);
			return Create(buffer);
		}
		
		return Create(File.ReadAllBytes(path));
	}
}
