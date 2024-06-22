using SideScroll.Core;
using NUnit.Framework;

namespace SideScroll.Serialize.Test;

[Category("Serialize")]
public class TestSerializeSchemaChanges : TestSerializeBase
{
	private SerializerFile? _serializerFile;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Serialize");

		string basePath = Paths.Combine(TestPath, "Serialize");

		Directory.CreateDirectory(basePath);

		string filePath = Paths.Combine(basePath, "Data.atlas");
		_serializerFile = new SerializerFileSideScroll(filePath);
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
		MissingProperty? output = _serializerFile!.Load<MissingProperty>(Call);
		Assert.IsNotNull(output);
		Assert.AreEqual(input.BoolProperty, output!.BoolProperty);
		Assert.AreEqual(input.IntProperty, output.IntProperty);
	}

	[Test, Description("Serialize Property Type Missing Load"), Ignore("Requires Save first")]
	public void SerializePropertyMissingLoad()
	{
		MissingProperty? output = _serializerFile!.Load<MissingProperty>(Call);
		Assert.IsNotNull(output);
		Assert.AreEqual(1, output!.IntProperty);
	}
}
