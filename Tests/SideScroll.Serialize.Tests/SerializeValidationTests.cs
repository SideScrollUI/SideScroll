using NUnit.Framework;
using SideScroll.Serialize.Atlas;
using System.Text;

namespace SideScroll.Serialize.Tests;

[Category("Serialize")]
public class SerializeValidationTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Validate");
	}

	[SetUp]
	public void Setup()
	{
	}

	[Test, Description("Validate null base64 data")]
	public void ValidateNullBase64()
	{
		Assert.That(() => SerializerMemory.ValidateBase64(Call, null!), Throws.Exception.TypeOf<ArgumentNullException>());
	}

	[Test, Description("Validate invalid base64 data")]
	public void ValidateInvalidBase64()
	{
		Assert.That(() => SerializerMemory.ValidateBase64(Call, "base64"), Throws.Exception.TypeOf<FormatException>());
	}

	[Test, Description("Validate invalid gzip data")]
	public void ValidateInvalidGzipData()
	{
		string base64 = Convert.ToBase64String(new byte[] { 0, 1, 2, 3 });
		Assert.That(() => SerializerMemory.ValidateBase64(Call, base64), Throws.Exception.TypeOf<InvalidDataException>());
	}

	[Test, Description("Validate invalid atlas data")]
	public void ValidateInvalidAtlasData()
	{
		byte[] bytes = [0, 1, 2, 3];
		string base64 = SerializerMemory.ConvertStreamToBase64String(Call, new MemoryStream(bytes));

		Assert.That(() => SerializerMemory.ValidateBase64(Call, base64), Throws.Exception.TypeOf<SerializerException>());
	}

	[Test, Description("Validate atlas data")]
	public void ValidateAtlasData()
	{
		byte[] sideId = Encoding.ASCII.GetBytes("SIDE");

		SerializerMemoryAtlas serializer = new();
		serializer.Save(Call, "input");
		byte[] bytes = serializer.Stream.ToArray();

		Assert.That(bytes.Take(4), Is.EqualTo(sideId));
	}

	[Test, Description("Load leaves the stream open so it can be loaded multiple times")]
	public void LoadTwice()
	{
		SerializerMemoryAtlas serializer = new();
		serializer.Save(Call, "input");

		var first = serializer.Load<string>(Call);
		var second = serializer.Load<string>(Call);

		Assert.That(first, Is.EqualTo("input"));
		Assert.That(second, Is.EqualTo("input"));
	}

	[Test, Description("TryLoad should catch exceptions from invalid data and return false")]
	public void TryLoadCatchesExceptions()
	{
		SerializerMemoryAtlas serializer = new();
		serializer.Stream.Write([0, 1, 2, 3]); // Invalid atlas data

		bool success = serializer.TryLoad(out string? result, Call);

		Assert.That(success, Is.False);
		Assert.That(result, Is.Null);
	}

	[Test, Description("Validate leaves the stream open so the data can still be loaded")]
	public void ValidateThenLoad()
	{
		SerializerMemoryAtlas serializer = new();
		serializer.Save(Call, "input");

		serializer.Validate(Call);
		var output = serializer.Load<string>(Call);

		Assert.That(output, Is.EqualTo("input"));
	}

	[Test, Description("Writing an unregistered object throws SerializerException")]
	public void WriteUnregisteredObjectThrowsSerializerException()
	{
		Serializer serializer = new();
		using var stream = new MemoryStream();
		using var writer = new BinaryWriter(stream);
		
		// String was not registered via AddObjectRef
		Assert.That(() => serializer.WriteObjectRef(typeof(string), "unregistered", writer), Throws.Exception.TypeOf<SerializerException>());
	}
}
