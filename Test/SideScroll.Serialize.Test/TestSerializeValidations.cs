using NUnit.Framework;
using SideScroll.Serialize.Atlas;
using System.Text;

namespace SideScroll.Serialize.Test;

[Category("Serialize")]
public class TestSerializeValidations : TestSerializeBase
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
}
