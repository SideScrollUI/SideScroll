using NUnit.Framework;
using SideScroll.Serialize.Atlas;

namespace SideScroll.Serialize.Test;

[Category("Serialize")]
public class SerializeSchemaChangeTests : SerializeBaseTest
{
	private SerializerFile? _serializerFile;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Serialize");

		string basePath = Paths.Combine(TestPath, "Serialize");

		Directory.CreateDirectory(basePath);

		string filePath = Paths.Combine(basePath, SerializerFileAtlas.DataFileName);
		_serializerFile = new SerializerFileAtlas(filePath);
	}

	// Todo: Add option to manually construct serializer parts to simplify this
	// Current steps: Save, comment out BoolProperty, and Load
	public class MissingProperty
	{
		public bool BoolProperty { get; set; } // Commenting this out broke loading

		public int IntProperty { get; set; }
	}

	[Test, Description("Serialize Property Type Missing Save")]
	public void SerializePropertyMissingSave()
	{
		MissingProperty input = new()
		{
			BoolProperty = true,
			IntProperty = 1,
		};
		_serializerFile!.Save(Call, input);
		MissingProperty? output = _serializerFile.Load<MissingProperty>(Call);
		Assert.That(output, Is.Not.Null);
		Assert.That(output!.BoolProperty, Is.EqualTo(input.BoolProperty));
		Assert.That(output.IntProperty, Is.EqualTo(input.IntProperty));
	}

	[Test, Description("Serialize Property Type Missing Load"), Ignore("Requires Save first")]
	public void SerializePropertyMissingLoad()
	{
		MissingProperty? output = _serializerFile!.Load<MissingProperty>(Call);
		Assert.That(output, Is.Not.Null);
		Assert.That(output!.IntProperty, Is.EqualTo(1));
	}
}
