using SideScroll.Attributes;

namespace SideScroll.Tabs.Lists;

/// <summary>
/// Represents a single byte with its index and various representations (hex, char, binary)
/// </summary>
public class ListByte(int index, byte b)
{
	/// <summary>
	/// Gets or sets the maximum number of bytes to display (default: 100,000)
	/// </summary>
	public static int MaxBytes { get; set; } = 100_000;

	/// <summary>
	/// Gets the zero-based index of the byte
	/// </summary>
	[StyleValue]
	public int Index => index;

	/// <summary>
	/// Gets the byte value
	/// </summary>
	public byte Byte => b;

	/// <summary>
	/// Gets the hexadecimal representation of the byte
	/// </summary>
	public string Hex { get; } = BitConverter.ToString([b]);

	/// <summary>
	/// Gets the character representation of the byte
	/// </summary>
	public char Char { get; } = Convert.ToChar(b);

	/// <summary>
	/// Gets the 8-bit binary representation of the byte
	/// </summary>
	public string Bits { get; } = Convert.ToString(b, 2).PadLeft(8, '0');

	public override string ToString() => Index.ToString();

	/// <summary>
	/// Creates a list of ListByte objects from a byte sequence, limited to MaxBytes
	/// </summary>
	public static List<ListByte> Create(IEnumerable<byte> bytes)
	{
		int i = 0;
		return bytes
			.Take(MaxBytes)
			.Select(b => new ListByte(i++, b))
			.ToList();
	}

	/// <summary>
	/// Loads a file and creates a list of ListByte objects from its contents, limited to MaxBytes
	/// </summary>
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
