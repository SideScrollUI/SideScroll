using NUnit.Framework;
using SideScroll.Serialize.Atlas;
using SideScroll.Serialize.KeyValue;
using SideScroll.Tasks;

namespace SideScroll.Serialize.Tests.KeyValue;

/// <summary>
/// Covers the serializer the browser's localStorage one is built from. These paths shipped
/// untested while they lived in a project no test project could reference
/// </summary>
[Category("Serialize")]
public class SerializerKeyValueStoreTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup() => Initialize(nameof(SerializerKeyValueStoreTests));

	private MemoryKeyValueStore _store = null!;

	[SetUp]
	public void SetUp() => _store = new MemoryKeyValueStore();

	private SerializerKeyValueStore CreateSerializer(string path = "Project/Data") => new(_store, path, "name");

	// ─── Saving ───────────────────────────────────────────────────────────

	[Test]
	public void SaveThenLoadRoundTrips()
	{
		SerializerKeyValueStore serializer = CreateSerializer();

		serializer.Save(Call, new Dictionary<string, int> { ["answer"] = 42 });

		Assert.That(serializer.Load<Dictionary<string, int>>(Call),
			Is.EqualTo(new Dictionary<string, int> { ["answer"] = 42 }));
	}

	[Test, Description(
		"A rejected write was logged as a warning and returned normally, so a caller could discard " +
		"the only copy of what it thought it had saved")]
	public void ARejectedWriteThrows()
	{
		SerializerKeyValueStore serializer = CreateSerializer();
		_store.RejectWrites = true;

		Assert.Throws<SerializerException>(() => serializer.Save(Call, "value"));
	}

	[Test, Description("The data is stored by then, so a rejected header is reported as its own failure")]
	public void ARejectedHeaderThrowsAfterTheDataIsStored()
	{
		SerializerKeyValueStore serializer = CreateSerializer();
		_store.RejectWritesToPrefix = StorageKeys.HeaderPrefix;

		var exception = Assert.Throws<SerializerException>(() => serializer.Save(Call, "value"))!;

		Assert.That(exception.Message, Does.Contain("header"));
		Assert.That(_store.Peek(serializer.StorageKey), Is.Not.Null, "the data is still stored");
	}

	[Test, Description("A serialization failure reaches the caller rather than being swallowed")]
	public void AnUnreachableStoreThrowsFromSave()
	{
		SerializerKeyValueStore serializer = CreateSerializer();
		_store.Unreachable = true;

		Assert.Catch(() => serializer.Save(Call, "value"));
	}

	[Test]
	public void SaveWritesBothTheDataAndItsHeader()
	{
		SerializerKeyValueStore serializer = CreateSerializer("Project/Data");

		serializer.Save(Call, "value", "saved name");

		Assert.That(_store.Peek(StorageKeys.DataKey("Project/Data")), Is.Not.Null);
		Assert.That(_store.Peek(StorageKeys.HeaderKey("Project/Data")), Is.Not.Null);
		Assert.That(serializer.LoadHeader(Call).Name, Is.EqualTo("saved name"));
	}

	// ─── Loading ──────────────────────────────────────────────────────────

	[Test, Description("Stored JSON that no longer parses must not be reported as a successful load")]
	public void CorruptDataLoadsAsNullAndErrorsTheTask()
	{
		SerializerKeyValueStore serializer = CreateSerializer();
		_store.Poke(serializer.StorageKey, "{ invalid json");
		var taskInstance = new TaskInstance();

		Assert.That(serializer.Load<Dictionary<string, int>>(Call, taskInstance: taskInstance), Is.Null);
		Assert.That(taskInstance.Finished, Is.True);
		Assert.That(taskInstance.Errored, Is.True);
	}

	[Test, Description("Nothing stored is an empty load rather than a failure, and still finishes the task")]
	public void NoStoredDataLoadsAsNull()
	{
		SerializerKeyValueStore serializer = CreateSerializer();
		var taskInstance = new TaskInstance();

		Assert.That(serializer.Load<string>(Call, taskInstance: taskInstance), Is.Null);
		Assert.That(taskInstance.Finished, Is.True);
		Assert.That(taskInstance.Errored, Is.False);
	}

	[Test]
	public void ASuccessfulLoadFinishesItsTaskWithoutErroring()
	{
		SerializerKeyValueStore serializer = CreateSerializer();
		serializer.Save(Call, "value");
		var taskInstance = new TaskInstance();

		Assert.That(serializer.Load<string>(Call, taskInstance: taskInstance), Is.EqualTo("value"));
		Assert.That(taskInstance.Finished, Is.True);
		Assert.That(taskInstance.Errored, Is.False);
	}

	[Test]
	public void LoadHeaderWithoutAStoredHeaderFallsBackToTheName()
	{
		Assert.That(CreateSerializer().LoadHeader(Call).Name, Is.EqualTo("name"));
	}

	// ─── Existence and removal ────────────────────────────────────────────

	[Test]
	public void ExistsReflectsWhetherDataIsStored()
	{
		SerializerKeyValueStore serializer = CreateSerializer();
		Assert.That(serializer.Exists, Is.False);

		serializer.Save(Call, "value");
		Assert.That(serializer.Exists, Is.True);
	}

	[Test, Description("An unreachable store reads as absent rather than throwing from a property")]
	public void ExistsIsFalseWhenTheStoreIsUnreachable()
	{
		SerializerKeyValueStore serializer = CreateSerializer();
		serializer.Save(Call, "value");
		_store.Unreachable = true;

		Assert.That(serializer.Exists, Is.False);
	}

	[Test]
	public void RemovePathRemovesBothTheDataAndItsHeader()
	{
		SerializerKeyValueStore serializer = CreateSerializer("Project/Data");
		serializer.Save(Call, "value");

		serializer.RemovePath("Project/Data");

		Assert.That(_store.Peek(StorageKeys.DataKey("Project/Data")), Is.Null);
		Assert.That(_store.Peek(StorageKeys.HeaderKey("Project/Data")), Is.Null);
	}

	[Test, Description("An unreachable store lists nothing rather than throwing")]
	public void GetAllKeysIsEmptyWhenTheStoreIsUnreachable()
	{
		SerializerKeyValueStore serializer = CreateSerializer();
		serializer.Save(Call, "value");
		Assert.That(serializer.GetAllKeys(), Is.Not.Empty);

		_store.Unreachable = true;
		Assert.That(serializer.GetAllKeys(), Is.Empty);
	}

	[Test]
	public void GetAllKeysReturnsOnlyDataKeys()
	{
		CreateSerializer("Project/One").Save(Call, "one");
		CreateSerializer("Project/Two").Save(Call, "two");

		IReadOnlyList<string> keys = CreateSerializer().GetAllKeys();

		Assert.That(keys, Has.Count.EqualTo(2));
		Assert.That(keys.All(key => key.StartsWith(StorageKeys.DataPrefix, StringComparison.Ordinal)), Is.True);
	}
}
