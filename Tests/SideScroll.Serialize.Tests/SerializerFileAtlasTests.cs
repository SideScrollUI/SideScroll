using NUnit.Framework;
using SideScroll.Serialize.Atlas;
using SideScroll.Serialize.Atlas.Schema;
using SideScroll.Tasks;

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

	// ─── Failed saves ────────────────────────────────────────────────────

	// Serializing this throws, standing in for a full disk or an unserializable member.
	// Read-write so the serializer reads it, a computed property is skipped
	public class ThrowsWhenSerialized
	{
		public string Value
		{
			get => throw new InvalidOperationException("Expected");
			set { }
		}
	}

	[Test, Description(
		"The destination used to be truncated before serializing, so a failure destroyed the " +
		"previous save, and exhausting every attempt returned as though it had succeeded")]
	public void FailedSaveThrowsAndKeepsThePreviousFile()
	{
		// Start from an empty directory, the temp file assertion below counts everything in it
		string basePath = Paths.Combine(TestPath, nameof(FailedSaveThrowsAndKeepsThePreviousFile));
		if (Directory.Exists(basePath))
		{
			Directory.Delete(basePath, true);
		}
		Directory.CreateDirectory(basePath);

		string filePath = Paths.Combine(basePath, "Data.atlas");

		SerializerFileAtlas.CreateForFile(filePath).Save(Call, "original");
		long originalLength = new FileInfo(filePath).Length;

		int originalAttempts = SerializerFileAtlas.SaveAttemptsMax;
		try
		{
			SerializerFileAtlas.SaveAttemptsMax = 2; // Keep the retry backoff short

			// Reflection wraps the getter's exception, so assert it arrives rather than its type
			Assert.Catch(
				() => SerializerFileAtlas.CreateForFile(filePath).Save(Call, new ThrowsWhenSerialized()),
				"A save that never succeeds has to reach the caller.");
		}
		finally
		{
			SerializerFileAtlas.SaveAttemptsMax = originalAttempts;
		}

		Assert.That(new FileInfo(filePath).Length, Is.EqualTo(originalLength), "The previous save is untouched.");
		Assert.That(SerializerFileAtlas.CreateForFile(filePath).Load<string>(Call), Is.EqualTo("original"));

		Assert.That(Directory.GetFiles(basePath), Has.Length.EqualTo(1), "No temp files left behind.");
	}

	[TestCase(0)]
	[TestCase(-1)]
	[Description("A non positive maximum skips the loop, which would report success without writing")]
	public void RejectsNonPositiveSaveAttemptsMax(int value)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => SerializerFileAtlas.SaveAttemptsMax = value);
	}

	[Test, Description(
		"Exists checks DataPath, but LoadHeader() read HeaderPath unchecked, so a missing header " +
		"threw a bare FileNotFoundException naming a path the caller never chose")]
	public void LoadHeaderReportsAMissingHeader()
	{
		string basePath = Path.Combine(TestPath, "MissingHeader", Path.GetRandomFileName());
		var serializer = SerializerFileAtlas.Create(basePath, "name");

		var exception = Assert.Throws<SerializerException>(() => serializer.LoadHeader(Call))!;
		Assert.That(exception.Message, Does.Contain("Header"));
	}

	[Test, Description("Control: a saved file's header still loads")]
	public void LoadHeaderReadsASavedHeader()
	{
		string basePath = Path.Combine(TestPath, "SavedHeader", Path.GetRandomFileName());
		var serializer = SerializerFileAtlas.Create(basePath, "name");
		serializer.Save(Call, "value");

		Assert.That(serializer.LoadHeader(Call), Is.Not.Null);
	}

	[Test, Description("Schema loading returns standalone schema data instead of a serializer backed by a closed reader")]
	public void LoadSchemaReturnsStandaloneSchemas()
	{
		string basePath = Path.Combine(TestPath, nameof(LoadSchemaReturnsStandaloneSchemas), Path.GetRandomFileName());
		var serializerFile = new SerializerFileAtlas(basePath, "name");
		serializerFile.Save(Call, new Dictionary<string, int> { ["answer"] = 42 });

		IReadOnlyList<TypeSchema> schemas = serializerFile.LoadSchema(Call);

		Assert.That(schemas, Is.Not.Empty);
		Assert.That(schemas.Select(schema => schema.Name), Does.Contain(typeof(Dictionary<string, int>).ToString()));
		Assert.DoesNotThrow(() => _ = schemas.SelectMany(schema => schema.MemberSchemas).ToList());
	}

	[Test, Description(
		"The Atlas loader only set Percent and never finished its task, so a caller passing one " +
		"waited on it after a load that had already succeeded")]
	public void LoadFinishesTask()
	{
		string basePath = Path.Combine(TestPath, nameof(LoadFinishesTask), Path.GetRandomFileName());
		var serializerFile = new SerializerFileAtlas(basePath, "name");
		serializerFile.Save(Call, new Dictionary<string, int> { ["answer"] = 42 });
		var taskInstance = new TaskInstance();

		Dictionary<string, int>? result = serializerFile.Load<Dictionary<string, int>>(Call, taskInstance: taskInstance);

		Assert.That(result, Is.EqualTo(new Dictionary<string, int> { ["answer"] = 42 }));
		Assert.That(taskInstance.Finished, Is.True);
		Assert.That(taskInstance.Errored, Is.False);
	}

	[Test, Description("A failed Atlas load finishes the task as errored, the same as the other serializers")]
	public void FailedLoadFinishesTaskAsErrored()
	{
		string basePath = Path.Combine(TestPath, nameof(FailedLoadFinishesTaskAsErrored), Path.GetRandomFileName());
		var serializerFile = new SerializerFileAtlas(basePath, "name");
		var taskInstance = new TaskInstance();

		Dictionary<string, int>? result = serializerFile.Load<Dictionary<string, int>>(Call, taskInstance: taskInstance);

		Assert.That(result, Is.Null);
		Assert.That(taskInstance.Finished, Is.True);
		Assert.That(taskInstance.Errored, Is.True);
	}
}
