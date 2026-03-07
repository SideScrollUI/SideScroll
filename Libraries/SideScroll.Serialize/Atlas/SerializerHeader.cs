using SideScroll.Extensions;
using SideScroll.Logs;

namespace SideScroll.Serialize.Atlas;

/// <summary>
/// Represents the header information for serialized Atlas files
/// </summary>
public class SerializerHeader
{
	/// <summary>
	/// Magic number identifying Atlas files
	/// SIDE in little endian format
	/// EDIS in big endian format (69, 68, 73, 83)
	/// </summary>
	public const uint SideId = 0x45444953;

	/// <summary>
	/// The latest version number of the serializer format
	/// </summary>
	public const ushort LatestVersion = 2;

	/// <summary>
	/// Gets or sets the version number of the serialized file
	/// </summary>
	public ushort? Version { get; set; }
	
	/// <summary>
	/// Gets or sets the total file size in bytes
	/// </summary>
	public long FileSize { get; set; }
	
	/// <summary>
	/// Gets or sets the name of the serialized object
	/// </summary>
	public string? Name { get; set; }

	public override string ToString() => $"v{Version}: {Name}";

	/// <summary>
	/// Saves the header to a binary writer
	/// </summary>
	public void Save(BinaryWriter writer)
	{
		writer.Write(SideId);
		writer.Write(Version ?? LatestVersion);
		writer.Write(FileSize);
		writer.Write(Name ?? "");
	}

	/// <summary>
	/// Updates the file size in the header after serialization is complete
	/// </summary>
	public void SaveFileSize(BinaryWriter writer)
	{
		FileSize = writer.BaseStream.Length;
		writer.Seek(6, SeekOrigin.Begin);
		writer.Write(FileSize);
	}

	/// <summary>
	/// Loads the header from a binary reader and validates the format
	/// </summary>
	public void Load(Log log, BinaryReader reader, string? requiredName = null)
	{
		uint sideId = reader.ReadUInt32();
		if (sideId != SideId)
		{
			log.Throw(new SerializerException("Invalid header Id",
				new Tag("Expected", SideId),
				new Tag("Found", sideId)));
		}

		Version = reader.ReadUInt16();
		FileSize = reader.ReadInt64();
		Name = reader.ReadString();

		if (!requiredName.IsNullOrEmpty() && Name != requiredName)
		{
			log.Throw(new SerializerException("Loaded name doesn't match required",
				new Tag("Required", requiredName),
				new Tag("Loaded", Name)));
		}

		if (reader.BaseStream.Length != FileSize)
		{
			log.Throw(new SerializerException("File size doesn't match",
				new Tag("Expected", FileSize),
				new Tag("Actual", reader.BaseStream.Length)));
		}

		if (Version != LatestVersion)
		{
			log.AddWarning("Header version doesn't match latest",
				new Tag("Header Version", Version),
				new Tag("Latest Version", LatestVersion));
		}
	}
}
