using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Serialize.Json;
using SideScroll.Tasks;

namespace SideScroll.Serialize.Tests.Json;

[Category("Json")]
public class SerializerFileJsonTests : SerializeBaseTest
{
	private string _basePath = null!;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializerFileJson");
	}

	[SetUp]
	public void Setup()
	{
		_basePath = Paths.Combine(TestPath, "SerializerFileJson");

		if (Directory.Exists(_basePath))
		{
			Directory.Delete(_basePath, true);
		}
		Directory.CreateDirectory(_basePath);
	}

	[PublicData]
	public class SampleItem
	{
		public string? Name { get; set; }
	}

	private SerializerFileJson CreateFile()
	{
		return new SerializerFileJson(_basePath, "Test");
	}

	[Test]
	public void SaveAndLoad()
	{
		SampleItem input = new() { Name = "value" };
		CreateFile().Save(Call, input);

		SampleItem? output = CreateFile().Load<SampleItem>(Call);

		Assert.That(output!.Name, Is.EqualTo("value"));
	}

	[Test, Description("Loading reports progress but leaves the task for its owner to finish")]
	public void LoadReportsProgressWithoutFinishingTheTask()
	{
		CreateFile().Save(Call, new SampleItem { Name = "value" });

		TaskInstance taskInstance = new();
		int completed = 0;
		taskInstance.OnComplete = () => completed++;

		SampleItem? output = CreateFile().Load<SampleItem>(Call, false, taskInstance);

		Assert.That(output!.Name, Is.EqualTo("value"));
		Assert.That(taskInstance.Finished, Is.False);
		Assert.That(completed, Is.EqualTo(0));
		Assert.That(taskInstance.Percent, Is.EqualTo(100));
	}
}
