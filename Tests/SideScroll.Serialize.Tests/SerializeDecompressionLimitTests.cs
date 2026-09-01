using NUnit.Framework;
using SideScroll.Serialize;
using SideScroll.Serialize.Atlas;
using System.IO.Compression;

namespace SideScroll.Serialize.Tests;

/// <summary>
/// A caller supplies the compressed form, whose size says nothing about how far it expands. An
/// imported bookmark is exactly that, so the expansion is bounded before any of it is validated
/// </summary>
[Category("Serialize")]
public class SerializeDecompressionLimitTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeDecompressionLimit");
	}

	private long _originalMax;

	[SetUp]
	public void Setup()
	{
		_originalMax = SerializerMemory.MaxDecompressedSize;
	}

	[TearDown]
	public void TearDown()
	{
		SerializerMemory.MaxDecompressedSize = _originalMax;
	}

	/// <summary>Compresses highly repetitive data, which gzip shrinks by orders of magnitude</summary>
	private static string CreateExpandingPayload(int decompressedBytes)
	{
		using var outStream = new MemoryStream();
		using (var gzip = new GZipStream(outStream, CompressionMode.Compress))
		{
			var zeros = new byte[81920];
			for (int written = 0; written < decompressedBytes; written += zeros.Length)
			{
				gzip.Write(zeros, 0, Math.Min(zeros.Length, decompressedBytes - written));
			}
		}
		return Convert.ToBase64String(outStream.ToArray());
	}

	[Test, Description(
		"A payload small enough to paste into a link decompressed until the process ran out of " +
		"memory, before any of the data was validated")]
	public void APayloadThatExpandsPastTheLimitIsRejected()
	{
		SerializerMemory.MaxDecompressedSize = 1_000_000;

		string payload = CreateExpandingPayload(20_000_000);
		Assert.That(payload.Length, Is.LessThan(200_000), "The compressed payload stays small");

		var serializer = new SerializerMemoryAtlas();

		Assert.Throws<SerializerException>(() => serializer.LoadBase64String(payload));
	}

	[Test, Description("The limit is reported rather than the load failing somewhere further on")]
	public void TheLimitIsNamedInTheFailure()
	{
		SerializerMemory.MaxDecompressedSize = 1_000_000;

		var serializer = new SerializerMemoryAtlas();

		SerializerException exception = Assert.Throws<SerializerException>(
			() => serializer.LoadBase64String(CreateExpandingPayload(20_000_000)))!;

		Assert.That(exception.Message, Does.Contain("maximum allowed size"));
	}

	[Test, Description("Nothing past the limit is kept, so a rejected payload doesn't cost the memory it asked for")]
	public void ARejectedPayloadDoesNotFillTheStream()
	{
		SerializerMemory.MaxDecompressedSize = 1_000_000;

		var serializer = new SerializerMemoryAtlas();

		Assert.Throws<SerializerException>(() => serializer.LoadBase64String(CreateExpandingPayload(20_000_000)));
		Assert.That(serializer.Stream.Length, Is.LessThanOrEqualTo(SerializerMemory.MaxDecompressedSize));
	}

	[Test, Description("Control: a bookmark sized payload is unaffected")]
	public void AnOrdinaryPayloadStillLoads()
	{
		var input = new List<string>();
		for (int i = 0; i < 1000; i++)
		{
			input.Add("value " + i);
		}

		string base64 = SerializerMemory.ToBase64String(Call, input);

		var serializer = new SerializerMemoryAtlas();
		serializer.LoadBase64String(base64);
		var output = serializer.Load<List<string>>(Call);

		Assert.That(output, Has.Count.EqualTo(1000));
		Assert.That(output[999], Is.EqualTo("value 999"));
	}

	[Test, Description("Control: a payload right at the limit is allowed through")]
	public void APayloadWithinTheLimitLoads()
	{
		SerializerMemory.MaxDecompressedSize = 20_000_000;

		var serializer = new SerializerMemoryAtlas();

		Assert.DoesNotThrow(() => serializer.LoadBase64String(CreateExpandingPayload(1_000_000)));
	}

	[TestCase(0)]
	[TestCase(-1)]
	public void MaxDecompressedSizeRejectsNonPositiveValues(long value)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => SerializerMemory.MaxDecompressedSize = value);
	}
}
