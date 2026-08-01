using NUnit.Framework;
using SideScroll.Serialize.Atlas;

namespace SideScroll.Serialize.Tests;

[Category("Serialize")]
public class SerializerFileAtlasTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializerFileAtlas");
	}

	[Test, Description("CreateForFile() uses the path it's given instead of appending the default data filename")]
	public void CreateForFileUsesGivenPath()
	{
		string basePath = Paths.Combine(TestPath, nameof(CreateForFileUsesGivenPath));
		Directory.CreateDirectory(basePath);

		string filePath = Paths.Combine(basePath, "Settings.atlas");

		SerializerFileAtlas serializer = SerializerFileAtlas.CreateForFile(filePath);
		serializer.Save(Call, "value");

		Assert.That(File.Exists(filePath), Is.True,
			"The file should be written where the caller asked, not at Data.atlas.");
		Assert.That(SerializerFileAtlas.CreateForFile(filePath).Load<string>(Call), Is.EqualTo("value"));
	}

	[Test, Description(
		"A bare filename has no directory component, so BasePath has to resolve to a real directory " +
		"or EnsureStorageExists() throws before anything gets written")]
	public void CreateForFileWithBareFilename()
	{
		// Relative to the working directory, so keep the name unique and clean it up
		string fileName = $"{nameof(CreateForFileWithBareFilename)}-{Guid.NewGuid():N}.atlas";
		try
		{
			SerializerFileAtlas serializer = SerializerFileAtlas.CreateForFile(fileName);

			Assert.DoesNotThrow(() => serializer.Save(Call, "value"),
				"An empty BasePath makes EnsureStorageExists() throw before anything is written.");

			Assert.That(serializer.Load<string>(Call), Is.EqualTo("value"));
			Assert.That(Directory.Exists(serializer.BasePath), Is.True,
				"BasePath should resolve to a real directory.");
		}
		finally
		{
			File.Delete(fileName);
		}
	}
}
