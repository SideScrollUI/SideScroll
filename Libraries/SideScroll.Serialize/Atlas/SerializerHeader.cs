using SideScroll.Extensions;
using SideScroll.Logs;

namespace SideScroll.Serialize.Atlas;

public class SerializerHeader
{
	public const uint SideId = 0x45444953; // SIDE -> EDIS: 69, 68, 73, 83, Start of file (little endian format)

	public const string LatestVersion = "1";

	public string? Version { get; set; }
	public string? Name { get; set; }
	public long FileSize { get; set; }

	private long _schemaPosition;

	public override string ToString() => $"v{Version}: {Name}";

	public void Save(BinaryWriter writer)
	{
		writer.Write(SideId);
		writer.Write(Version ?? LatestVersion);
		writer.Write(Name ?? "");
		_schemaPosition = writer.BaseStream.Position;
		writer.Write(FileSize);
	}

	public void SaveFileSize(BinaryWriter writer)
	{
		FileSize = writer.BaseStream.Length;
		writer.Seek((int)_schemaPosition, SeekOrigin.Begin);
		writer.Write(FileSize);
	}

	public void Load(Log log, BinaryReader reader, string? requiredName = null)
	{
		uint sideId = reader.ReadUInt32();
		if (sideId != SideId)
		{
			log.Throw(new SerializerException("Invalid header Id",
				new Tag("Expected", SideId),
				new Tag("Found", sideId)));
		}

		Version = reader.ReadString();
		Name = reader.ReadString();
		FileSize = reader.ReadInt64();

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
