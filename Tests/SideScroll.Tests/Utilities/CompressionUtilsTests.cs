using NUnit.Framework;
using SideScroll.Logs;
using SideScroll.Utilities;

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
}
