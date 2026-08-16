using NUnit.Framework;
using SideScroll.Logs;
using SideScroll.Utilities;
using System.IO.Compression;

namespace SideScroll.Tests.Utilities;

[Category("Core")]
public class CompressionUtilsTests : BaseTest
{
	private string _testPath = null!;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Compression");

		_testPath = Path.Combine(Environment.CurrentDirectory, "CompressionTests");
	}

	[SetUp]
	public void Setup()
	{
		if (Directory.Exists(_testPath))
		{
			Directory.Delete(_testPath, true);
		}
		Directory.CreateDirectory(_testPath);
	}

	private static LogEntry? FindEntry(Log log, string text)
	{
		foreach (LogEntry logEntry in log.Items)
		{
			if (logEntry.Text == text)
				return logEntry;

			if (logEntry is Log childLog && FindEntry(childLog, text) is { } found)
				return found;
		}
		return null;
	}

	private static object? GetTagValue(Log log, string text, string tagName)
	{
		LogEntry? logEntry = FindEntry(log, text);
		Assert.That(logEntry, Is.Not.Null, $"No '{text}' log entry found");

		Tag? tag = logEntry!.Tags?.FirstOrDefault(t => t.Name == tagName);
		Assert.That(tag, Is.Not.Null, $"No '{tagName}' tag found");
		return tag!.Value;
	}

	private string CreateTextFile(string name, int length)
	{
		string filePath = Path.Combine(_testPath, name);
		File.WriteAllText(filePath, new string('a', length));
		return filePath;
	}

	[Test, Description("The logged size matches the compressed file, the GZipStream has to flush first")]
	public void Compress_LogsCompressedSize()
	{
		string filePath = CreateTextFile("compress.txt", 10_000);
		Call call = new();

		CompressionUtils.Compress(call, new FileInfo(filePath));

		string compressedPath = filePath + ".gz";
		Assert.That(File.Exists(compressedPath));

		long compressedSize = new FileInfo(compressedPath).Length;
		Assert.That(compressedSize, Is.GreaterThan(0));
		Assert.That(GetTagValue(call.Log, "Finished Compressing", "Compressed Size"), Is.EqualTo(compressedSize));
	}

	[Test, Description("Decompressing restores the original contents and logs both sizes")]
	public void Decompress_LogsDecompressedSize()
	{
		string filePath = CreateTextFile("roundtrip.txt", 10_000);
		CompressionUtils.Compress(new Call(), new FileInfo(filePath));

		string compressedPath = filePath + ".gz";
		long compressedSize = new FileInfo(compressedPath).Length;
		File.Delete(filePath);

		Call call = new();
		CompressionUtils.Decompress(call, new FileInfo(compressedPath));

		Assert.That(File.ReadAllText(filePath), Is.EqualTo(new string('a', 10_000)));
		Assert.That(GetTagValue(call.Log, "Finished Decompressing", "Compressed Size"), Is.EqualTo(compressedSize));
		Assert.That(GetTagValue(call.Log, "Finished Decompressing", "Decompressed Size"), Is.EqualTo(10_000L));
	}

	private string CreateZip(string name, string entryName, string contents)
	{
		string zipPath = Path.Combine(_testPath, name);

		using var stream = new FileStream(zipPath, FileMode.Create);
		using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
		using var writer = new StreamWriter(archive.CreateEntry(entryName).Open());
		writer.Write(contents);

		return zipPath;
	}

	[Test, Description("A corrupt zip leaves the previously extracted directory alone")]
	public void ExtractZip_CorruptArchiveKeepsExistingDirectory()
	{
		string targetPath = Path.Combine(_testPath, "corruptzip");
		Directory.CreateDirectory(targetPath);
		File.WriteAllText(Path.Combine(targetPath, "keep.txt"), "original");

		string zipPath = targetPath + ".zip";
		File.WriteAllText(zipPath, "not a zip file");

		Assert.Throws<InvalidDataException>(() => CompressionUtils.Decompress(new Call(), new FileInfo(zipPath)));

		Assert.That(File.ReadAllText(Path.Combine(targetPath, "keep.txt")), Is.EqualTo("original"));
	}

	[Test, Description("Extracting a valid zip replaces the previous directory")]
	public void ExtractZip_ReplacesExistingDirectory()
	{
		string targetPath = Path.Combine(_testPath, "replacezip");
		Directory.CreateDirectory(targetPath);
		File.WriteAllText(Path.Combine(targetPath, "stale.txt"), "stale");

		string zipPath = CreateZip("replacezip.zip", "current.txt", "current");

		CompressionUtils.Decompress(new Call(), new FileInfo(zipPath));

		Assert.Multiple(() =>
		{
			Assert.That(File.ReadAllText(Path.Combine(targetPath, "current.txt")), Is.EqualTo("current"));
			Assert.That(File.Exists(Path.Combine(targetPath, "stale.txt")), Is.False);
			Assert.That(Directory.GetDirectories(_testPath), Has.Exactly(1).Items);
		});
	}

	[Test, Description("A corrupt gzip leaves the previously decompressed file alone")]
	public void ExtractGzip_CorruptArchiveKeepsExistingFile()
	{
		string filePath = Path.Combine(_testPath, "corruptgzip.txt");
		File.WriteAllText(filePath, "original");

		string compressedPath = filePath + ".gz";
		File.WriteAllText(compressedPath, "not a gzip file");

		Assert.Throws<InvalidDataException>(() => CompressionUtils.Decompress(new Call(), new FileInfo(compressedPath)));

		Assert.Multiple(() =>
		{
			Assert.That(File.ReadAllText(filePath), Is.EqualTo("original"));
			Assert.That(Directory.GetFiles(_testPath), Has.Exactly(2).Items);
		});
	}

	[Test, Description("Compressing replaces an existing archive without leaving a temp file")]
	public void Compress_ReplacesExistingArchive()
	{
		string filePath = CreateTextFile("replace.txt", 10_000);
		File.WriteAllText(filePath + ".gz", "stale");

		CompressionUtils.Compress(new Call(), new FileInfo(filePath));
		File.Delete(filePath);
		CompressionUtils.Decompress(new Call(), new FileInfo(filePath + ".gz"));

		Assert.Multiple(() =>
		{
			Assert.That(File.ReadAllText(filePath), Is.EqualTo(new string('a', 10_000)));
			Assert.That(Directory.GetFiles(_testPath), Has.Exactly(2).Items);
		});
	}

	[Test, Description("An uppercase gzip extension is already compressed and must not gain another extension")]
	public void Compress_UppercaseGzipIsNotRecompressed()
	{
		string filePath = CreateTextFile("already.GZ", 100);

		CompressionUtils.Compress(new Call(), new FileInfo(filePath));

		Assert.That(File.Exists(filePath + ".gz"), Is.False);
	}

	[Test, Description("Gzip decompression recognizes an uppercase extension")]
	public void Decompress_UppercaseGzip()
	{
		string filePath = CreateTextFile("uppercase.txt", 1_000);
		CompressionUtils.Compress(new Call(), new FileInfo(filePath));
		string uppercasePath = filePath + ".GZ";
		File.Move(filePath + ".gz", uppercasePath);
		File.Delete(filePath);

		CompressionUtils.Decompress(new Call(), new FileInfo(uppercasePath));

		Assert.That(File.ReadAllText(filePath), Is.EqualTo(new string('a', 1_000)));
	}

	[Test, Description("Zip decompression recognizes an uppercase extension")]
	public void Decompress_UppercaseZip()
	{
		string zipPath = CreateZip("uppercase.ZIP", "contents.txt", "contents");

		CompressionUtils.Decompress(new Call(), new FileInfo(zipPath));

		Assert.That(File.ReadAllText(Path.Combine(_testPath, "uppercase", "contents.txt")), Is.EqualTo("contents"));
	}
	// ─── Expansion limits ────────────────────────────────────────────────

	private long _originalMaxExtracted;
	private int _originalMaxEntries;

	[SetUp]
	public void SetupLimits()
	{
		_originalMaxExtracted = CompressionUtils.MaxExtractedSize;
		_originalMaxEntries = CompressionUtils.MaxEntries;
	}

	[TearDown]
	public void RestoreLimits()
	{
		CompressionUtils.MaxExtractedSize = _originalMaxExtracted;
		CompressionUtils.MaxEntries = _originalMaxEntries;
	}

	/// <summary>Highly repetitive contents, which compress by orders of magnitude</summary>
	private string CreateBombZip(string name, int entryBytes)
	{
		string zipPath = Path.Combine(_testPath, name);

		using var stream = new FileStream(zipPath, FileMode.Create);
		using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
		using var entryStream = archive.CreateEntry("bomb.txt").Open();

		var zeros = new byte[81920];
		for (int written = 0; written < entryBytes; written += zeros.Length)
		{
			entryStream.Write(zeros, 0, Math.Min(zeros.Length, entryBytes - written));
		}
		return zipPath;
	}

	private string CreateBombGzip(string name, int decompressedBytes)
	{
		string gzipPath = Path.Combine(_testPath, name);

		using var stream = new FileStream(gzipPath, FileMode.Create);
		using var gzip = new GZipStream(stream, CompressionMode.Compress);

		var zeros = new byte[81920];
		for (int written = 0; written < decompressedBytes; written += zeros.Length)
		{
			gzip.Write(zeros, 0, Math.Min(zeros.Length, decompressedBytes - written));
		}
		return gzipPath;
	}

	[Test, Description(
		"A small archive can expand until it fills the disk it's extracted onto, so the sizes it " +
		"declares are checked before any of it is written")]
	public void ExtractZip_RejectsAnArchiveThatExpandsPastTheLimit()
	{
		CompressionUtils.MaxExtractedSize = 1_000_000;
		string zipPath = CreateBombZip("bomb.zip", 20_000_000);

		Assert.That(new FileInfo(zipPath).Length, Is.LessThan(200_000), "The archive itself stays small");

		Assert.Throws<InvalidDataException>(
			() => CompressionUtils.Decompress(new Call(), new FileInfo(zipPath)));
	}

	[Test, Description("Rejected before extracting, so nothing reaches the disk")]
	public void ExtractZip_RejectedArchiveExtractsNothing()
	{
		CompressionUtils.MaxExtractedSize = 1_000_000;
		string zipPath = CreateBombZip("bomb2.zip", 20_000_000);

		Assert.Throws<InvalidDataException>(
			() => CompressionUtils.Decompress(new Call(), new FileInfo(zipPath)));

		Assert.That(Directory.Exists(Path.Combine(_testPath, "bomb2")), Is.False);
	}

	[Test, Description("Each entry costs a file and its metadata, however small the archive is")]
	public void ExtractZip_RejectsAnArchiveWithTooManyEntries()
	{
		CompressionUtils.MaxEntries = 10;

		string zipPath = Path.Combine(_testPath, "many.zip");
		using (var stream = new FileStream(zipPath, FileMode.Create))
		using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
		{
			for (int i = 0; i < 50; i++)
			{
				archive.CreateEntry("entry" + i + ".txt");
			}
		}

		Assert.Throws<InvalidDataException>(
			() => CompressionUtils.Decompress(new Call(), new FileInfo(zipPath)));
	}

	[Test, Description(
		"A gzip declares no size, so its bytes are counted as they're read. The entry reporting " +
		"this named only the zip path, and the gzip path had the same unbounded copy")]
	public void ExtractGzip_RejectsAnArchiveThatExpandsPastTheLimit()
	{
		CompressionUtils.MaxExtractedSize = 1_000_000;
		string gzipPath = CreateBombGzip("bomb.txt.gz", 20_000_000);

		Assert.That(new FileInfo(gzipPath).Length, Is.LessThan(200_000));

		Assert.Throws<InvalidDataException>(
			() => CompressionUtils.Decompress(new Call(), new FileInfo(gzipPath)));
	}

	[Test, Description("The partly written temp file is cleaned up rather than left behind")]
	public void ExtractGzip_RejectedArchiveLeavesNoTempFile()
	{
		CompressionUtils.MaxExtractedSize = 1_000_000;
		string gzipPath = CreateBombGzip("bomb2.txt.gz", 20_000_000);

		Assert.Throws<InvalidDataException>(
			() => CompressionUtils.Decompress(new Call(), new FileInfo(gzipPath)));

		Assert.That(Directory.GetFiles(_testPath, "bomb2.txt.*"), Has.Length.EqualTo(1),
			"Only the archive itself should remain");
	}

	[Test, Description("Control: an ordinary archive is unaffected")]
	public void ExtractZip_WithinTheLimitStillExtracts()
	{
		CompressionUtils.MaxExtractedSize = 1_000_000;
		string zipPath = CreateZip("ordinary.zip", "contents.txt", "contents");

		CompressionUtils.Decompress(new Call(), new FileInfo(zipPath));

		Assert.That(File.ReadAllText(Path.Combine(_testPath, "ordinary", "contents.txt")), Is.EqualTo("contents"));
	}

	[TestCase(0)]
	[TestCase(-1)]
	public void LimitsRejectNonPositiveValues(int value)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => CompressionUtils.MaxExtractedSize = value);
		Assert.Throws<ArgumentOutOfRangeException>(() => CompressionUtils.MaxEntries = value);
	}

	[TestCase(".txt")]
	[TestCase(".rar")]
	[TestCase("")]
	[Description(
		"An extension that isn't handled fell out of the if/else having already logged " +
		"\"Decompressing\", so a caller couldn't tell it apart from a successful extraction")]
	public void UnsupportedExtensionIsReported(string extension)
	{
		string filePath = Path.Combine(_testPath, nameof(UnsupportedExtensionIsReported) + extension);
		Directory.CreateDirectory(_testPath);
		File.WriteAllText(filePath, "not an archive");

		var exception = Assert.Throws<InvalidDataException>(
			() => CompressionUtils.Decompress(new Call(), new FileInfo(filePath)))!;

		Assert.That(exception.Message, Does.Contain(".zip").And.Contain(".gz"));
	}

	[Test, Description("Control: a supported extension still extracts")]
	public void SupportedExtensionStillDecompresses()
	{
		Directory.CreateDirectory(_testPath);
		string filePath = Path.Combine(_testPath, nameof(SupportedExtensionStillDecompresses) + ".txt");
		File.WriteAllText(filePath, "contents");

		var call = new Call();
		CompressionUtils.Compress(call, new FileInfo(filePath));
		File.Delete(filePath);

		Assert.DoesNotThrow(() => CompressionUtils.Decompress(call, new FileInfo(filePath + ".gz")));
		Assert.That(File.ReadAllText(filePath), Is.EqualTo("contents"));
	}
}
