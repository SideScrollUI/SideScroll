using NUnit.Framework;
using SideScroll.Extensions;

namespace SideScroll.Tests.Extensions;

[Category("Core")]
public class StreamExtensionsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("StreamExtensions");
	}

	private static MemoryStream CreateSource(int bytes) => new(new byte[bytes]);

	[Test]
	public void CopiesASourceWithinTheLimit()
	{
		using var source = CreateSource(1000);
		using var destination = new MemoryStream();

		Assert.That(source.TryCopyUpTo(destination, 5000), Is.True);
		Assert.That(destination.Length, Is.EqualTo(1000));
	}

	[Test, Description("A source exactly at the limit is allowed through")]
	public void CopiesASourceExactlyAtTheLimit()
	{
		using var source = CreateSource(1000);
		using var destination = new MemoryStream();

		Assert.That(source.TryCopyUpTo(destination, 1000), Is.True);
		Assert.That(destination.Length, Is.EqualTo(1000));
	}

	[Test, Description("One byte past the limit is what it's there to stop")]
	public void StopsOneBytePastTheLimit()
	{
		using var source = CreateSource(1001);
		using var destination = new MemoryStream();

		Assert.That(source.TryCopyUpTo(destination, 1000), Is.False);
	}

	[Test, Description(
		"The count is checked before each write, so a source past the limit doesn't cost the " +
		"destination the bytes it was going to write anyway")]
	public void WritesNothingPastTheLimit()
	{
		using var source = CreateSource(10_000_000);
		using var destination = new MemoryStream();

		Assert.That(source.TryCopyUpTo(destination, 1000), Is.False);
		Assert.That(destination.Length, Is.LessThanOrEqualTo(1000));
	}

	[Test]
	public void CopiesAnEmptySource()
	{
		using var source = new MemoryStream();
		using var destination = new MemoryStream();

		Assert.That(source.TryCopyUpTo(destination, 1000), Is.True);
		Assert.That(destination.Length, Is.Zero);
	}

	[Test, Description("Larger than one read, so the running total has to carry across buffers")]
	public void CopiesAcrossMultipleReads()
	{
		using var source = CreateSource(500_000);
		using var destination = new MemoryStream();

		Assert.That(source.TryCopyUpTo(destination, 1_000_000), Is.True);
		Assert.That(destination.Length, Is.EqualTo(500_000));
	}

	[TestCase(0)]
	[TestCase(-1)]
	public void RejectsANonPositiveLimit(long maxBytes)
	{
		using var source = CreateSource(10);
		using var destination = new MemoryStream();

		Assert.Throws<ArgumentOutOfRangeException>(() => source.TryCopyUpTo(destination, maxBytes));
	}

	[Test]
	public void RejectsNullStreams()
	{
		using var stream = new MemoryStream();

		Assert.Throws<ArgumentNullException>(() => ((Stream)null!).TryCopyUpTo(stream, 100));
		Assert.Throws<ArgumentNullException>(() => stream.TryCopyUpTo(null!, 100));
	}
}
