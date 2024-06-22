using SideScroll.Core;
using NUnit.Framework;

namespace SideScroll.Serialize.Test;

[Category("LargeFile")]
public class TestLargeFile : TestSerializeBase
{
	private const int IntCount = 10_000;
	
	private string? _basePath;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("LargeFile");

		_basePath = Paths.Combine(TestPath, "LargeFile");
		//_basePath = @"S:\SideScroll\Test\LargeFile";

		Directory.CreateDirectory(_basePath);
	}

	[Test, Description("WriteMemoryMappedFile")]
	[Ignore("Todo: For prototyping, handle file cleanup")]
	public void WriteMemoryMappedFile()
	{
		long offset = 0x200000000; // 8 GB
		long length = 0x20000000; // 512 MB

		// Create the memory-mapped file
		string fullPath = Paths.Combine(_basePath, "LargeImage.data");
		using var mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(fullPath, FileMode.OpenOrCreate, "ImgA", offset + length);
		// Create a random access view, from the offset
		// to the 768th megabyte (the offset plus length)
		using var accessor = mmf.CreateViewAccessor(offset, length);
		int intSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MyColor));

		// Make changes to the view
		long mapOffset = 0;
		for (int i = 0; i < IntCount; i++)
		{
			//accessor.Read(i, out color);
			accessor.Write(mapOffset, ref i);
			mapOffset += intSize;
		}
		/*int colorSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MyColor));
		MyColor color;

		// Make changes to the view
		for (long i = 0; i < length; i += colorSize)
		{
			accessor.Read(i, out color);
			color.Brighten(10);
			accessor.Write(i, ref color);
		}*/
	}

	[Test, Description("LargeBinaryWriter")]
	[Ignore("Takes 15+ seconds")]
	public void LargeBinaryWriter()
	{
		long maxOffset = 0x200000000; // 8 GB
		long spacing = 0x20000000; // 512 MB

		// Create the memory-mapped file
		string fullPath = Paths.Combine(_basePath, "LargeImage.data");

		using Stream stream = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
		using BinaryWriter writer = new(stream);
		for (long offset = 0; offset <= maxOffset; offset += spacing)
		{
			long position = stream.Seek(offset, SeekOrigin.Begin);
			Assert.AreEqual(stream.Position, offset);
			for (int i = 0; i < IntCount; i++)
				writer.Write(i);
		}
	}

	[Test, Description("SeekLargeFileReader")]
	[Ignore("Not Working")]
	public void SeekLargeFileReader()
	{
		long offset = 0x200000000; // 4 GB

		// long length = 0x20000000; // 512 MB

		// Create the memory-mapped file
		string fullPath = Paths.Combine(_basePath, "LargeImage.data");

		using Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		long position = stream.Seek(offset, SeekOrigin.Begin);
		Assert.AreEqual(stream.Position, offset);
		using BinaryReader reader = new(stream);
		for (int i = 0; i < IntCount; i++)
			reader.ReadInt32();
	}

	[Test, Description("SeekLargeFileMemoryMapped")]
	[Ignore("Not Working")]
	public void SeekLargeFileMemoryMapped()
	{
		long offset = 0x200000000; // 4 GB
		long length = 0x20000000; // 512 MB

		// Create the memory-mapped file
		string fullPath = Paths.Combine(_basePath, "LargeImage.data");
		using var mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(fullPath, FileMode.Open, "ImgA");
		// Create a random access view, from the offset
		// to the 768th megabyte (the offset plus length)
		using var accessor = mmf.CreateViewAccessor(offset, length);
		int intSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MyColor));

		// Make changes to the view
		long mapOffset = 0;
		for (int i = 0; i < IntCount; i++)
		{
			accessor.Read(i, out int temp);
			//accessor.Write(mapOffset, ref i);
			mapOffset += intSize;
		}
		/*int colorSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MyColor));
		MyColor color;

		// Make changes to the view
		for (long i = 0; i < length; i += colorSize)
		{
			accessor.Read(i, out color);
			color.Brighten(10);
			accessor.Write(i, ref color);
		}*/
	}
}

public struct MyColor
{
	public short Red;
	public short Green;
	public short Blue;
	public short Alpha;

	// Make the view brighter
	public void Brighten(short value)
	{
		Red = (short)Math.Min(short.MaxValue, (int)Red + value);
		Green = (short)Math.Min(short.MaxValue, (int)Green + value);
		Blue = (short)Math.Min(short.MaxValue, (int)Blue + value);
		Alpha = (short)Math.Min(short.MaxValue, (int)Alpha + value);
	}
}
/*
	

Accessing more than 2 GB

	Memory Mapped Files

		Limited to 2 GB
		Can't increase their size while open?
			Have to re-open?

	BinaryReader/BinaryWriter can only access 2 GB

		Can Seek to a 2 GB block in file beforehand?

		Memory Mapped Files are just an official way of doing this
		Length can be used later

	https://docs.microsoft.com/en-us/dotnet/standard/io/memory-mapped-files
	
*/
