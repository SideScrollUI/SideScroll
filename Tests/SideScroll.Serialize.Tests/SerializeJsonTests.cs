using NUnit.Framework;
using SideScroll.Serialize.Json;
using SideScroll.Tasks;

namespace SideScroll.Serialize.Tests;

public class SerializeJsonTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize(nameof(SerializeJsonTests));
	}

	[Test, Description("A task is completed only after its JSON has deserialized successfully")]
	public void LoadFinishesTaskAfterDeserialization()
	{
		string basePath = Path.Combine(TestPath, nameof(LoadFinishesTaskAfterDeserialization), Path.GetRandomFileName());
		var serializer = new SerializerFileJson(basePath);
		serializer.Save(Call, new Dictionary<string, int> { ["answer"] = 42 });
		var taskInstance = new TaskInstance();

		Dictionary<string, int>? result = serializer.Load<Dictionary<string, int>>(Call, taskInstance: taskInstance);

		Assert.That(result, Is.EqualTo(new Dictionary<string, int> { ["answer"] = 42 }));
		Assert.That(taskInstance.Finished, Is.True);
	}

	[Test, Description(
		"Malformed JSON must not report the load as successfully finished, and must still reach a " +
		"finished state. Nothing else completes the task, so leaving it unfinished left a caller " +
		"waiting on it forever")]
	public void FailedLoadFinishesTaskAsErrored()
	{
		string basePath = Path.Combine(TestPath, nameof(FailedLoadFinishesTaskAsErrored), Path.GetRandomFileName());
		Directory.CreateDirectory(basePath);
		File.WriteAllText(Path.Combine(basePath, SerializerFileJson.DataFileName), "{ invalid json");
		var serializer = new SerializerFileJson(basePath);
		var taskInstance = new TaskInstance();

		Dictionary<string, int>? result = serializer.Load<Dictionary<string, int>>(Call, taskInstance: taskInstance);

		Assert.That(result, Is.Null);
		Assert.That(taskInstance.Finished, Is.True);
		Assert.That(taskInstance.Errored, Is.True);
		Assert.That(taskInstance.Message, Is.Not.Null.And.Not.Empty);
	}

	[Test, Description("A missing file is a failed load too, and finishes the same way")]
	public void MissingFileFinishesTaskAsErrored()
	{
		string basePath = Path.Combine(TestPath, nameof(MissingFileFinishesTaskAsErrored), Path.GetRandomFileName());
		var serializer = new SerializerFileJson(basePath);
		var taskInstance = new TaskInstance();

		Dictionary<string, int>? result = serializer.Load<Dictionary<string, int>>(Call, taskInstance: taskInstance);

		Assert.That(result, Is.Null);
		Assert.That(taskInstance.Finished, Is.True);
		Assert.That(taskInstance.Errored, Is.True);
	}

	[Test, Description("A successful load isn't reported as errored")]
	public void SuccessfulLoadIsNotErrored()
	{
		string basePath = Path.Combine(TestPath, nameof(SuccessfulLoadIsNotErrored), Path.GetRandomFileName());
		var serializer = new SerializerFileJson(basePath);
		serializer.Save(Call, new Dictionary<string, int> { ["answer"] = 42 });
		var taskInstance = new TaskInstance();

		serializer.Load<Dictionary<string, int>>(Call, taskInstance: taskInstance);

		Assert.That(taskInstance.Finished, Is.True);
		Assert.That(taskInstance.Errored, Is.False);
	}

	/*[Test, Description("Serialize Lazy Base")]
	public void SerializeJsonBase()
	{
		var input = new Parent();
		input.child = new Child();
		input.child.uintTest = 2;

		var serializer = new SerializerMemoryJson(
		serializerFile.Save(Call, input);
		Parent output = serializerFile.Load<Parent>(Call, true);

		Assert.AreEqual(input.child.uintTest, output.child.uintTest);
	}*/
}
