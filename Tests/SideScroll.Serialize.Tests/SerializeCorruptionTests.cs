using NUnit.Framework;
using SideScroll.Serialize.Atlas;

namespace SideScroll.Serialize.Tests;

/// <summary>
/// Counts, sizes, and dimensions read from a serialized stream decide how much is allocated and
/// where later reads land, so a corrupt or crafted file must fail rather than allocate on its word
/// </summary>
[Category("Serialize")]
public class SerializeCorruptionTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeCorruption");
	}

	private static byte[] Serialize(object obj)
	{
		SerializerMemoryAtlas serializer = new();
		serializer.Save(new Call(), obj);
		return serializer.Stream.ToArray();
	}

	private static object? Load(byte[] bytes)
	{
		SerializerMemoryAtlas serializer = new();
		serializer.Stream.Write(bytes, 0, bytes.Length);
		return serializer.Load(new Call());
	}

	// Finds the one position holding these consecutive little endian ints, failing when the
	// pattern is absent or ambiguous so a passing test can't be patching the wrong bytes
	private static int IndexOfInts(byte[] bytes, params int[] values)
	{
		byte[] pattern = values.SelectMany(BitConverter.GetBytes).ToArray();

		List<int> matches = [];
		for (int i = 0; i + pattern.Length <= bytes.Length; i++)
		{
			if (bytes.AsSpan(i, pattern.Length).SequenceEqual(pattern))
			{
				matches.Add(i);
			}
		}

		Assert.That(matches, Has.Count.EqualTo(1),
			$"Expected exactly one occurrence of [{string.Join(", ", values)}] to patch.");
		return matches[0];
	}

	// 7 x 11 is 77 elements, and the pair is distinctive enough to locate unambiguously
	private static byte[] SerializedMatrix() => Serialize(new int[7, 11]);

	private static byte[] PatchDimensions(int first, int second)
	{
		byte[] bytes = SerializedMatrix();
		int offset = IndexOfInts(bytes, 7, 11);

		BitConverter.GetBytes(first).CopyTo(bytes, offset);
		BitConverter.GetBytes(second).CopyTo(bytes, offset + sizeof(int));
		return bytes;
	}

	[Test, Description("Control: the unpatched matrix loads, so the patch tests start from a working file")]
	public void MatrixRoundTrips()
	{
		var output = Load(SerializedMatrix()) as int[,];

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.GetLength(0), Is.EqualTo(7));
		Assert.That(output.GetLength(1), Is.EqualTo(11));
	}

	[Test, Description(
		"The array is created from the dimensions while only the header count was validated against " +
		"the data size, so a mismatch built a differently sized array than the elements that follow")]
	public void DimensionsMustMatchTheElementCount()
	{
		// 7 x 12 is 84 elements where the header still says 77
		Assert.Throws<SerializerException>(() => Load(PatchDimensions(7, 12)));
	}

	[Test, Description("A negative dimension reaches Array.CreateInstance() as an argument exception")]
	public void NegativeDimensionsAreRejected()
	{
		Assert.Throws<SerializerException>(() => Load(PatchDimensions(-1, 11)));
	}

	[Test, Description("Crafted dimensions whose product exhausts memory are rejected before allocating")]
	public void OversizedDimensionsAreRejected()
	{
		Assert.Throws<SerializerException>(() => Load(PatchDimensions(int.MaxValue, int.MaxValue)));
	}

	// A byte patching sweep over every offset used to live here. It cost ~400ms, and reverting each
	// guard showed it discriminated none of them — the array cases below already fail without their
	// checks, and the object size and member count guards changed nothing it measured. Recover it
	// from history if it's ever wanted as a manual fuzzing tool rather than a suite test
}
