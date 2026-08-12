using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Logs;
using SideScroll.Serialize.Atlas;
using System.Text;

namespace SideScroll.Serialize.Tests;

/// <summary>
/// A key that hashes on its own members has to be fully loaded before it's added to the collection
/// it belongs to, or every key of its type hashes the same and they collide
/// </summary>
[Category("Serialize")]
public class SerializeDictionaryKeyTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeDictionaryKeys");
	}

	private SerializerMemory _serializer = new SerializerMemoryAtlas();

	[SetUp]
	public void Setup()
	{
		_serializer = new SerializerMemoryAtlas();
	}

	[PublicData]
	public class ComplexKey
	{
		public string? Name { get; set; }

		public override bool Equals(object? obj) => obj is ComplexKey other && other.Name == Name;
		public override int GetHashCode() => Name?.GetHashCode() ?? 0;
	}

	[PublicData]
	public record RecordKey
	{
		public string? Name { get; set; }
	}

	[PublicData]
	public class Holder
	{
		public Dictionary<ComplexKey, string> Map { get; set; } = [];
		public Dictionary<RecordKey, string> Records { get; set; } = [];
		public HashSet<ComplexKey> Set { get; set; } = [];
	}

	private static Holder CreateHolder()
	{
		var holder = new Holder();
		holder.Map[new ComplexKey { Name = "alpha" }] = "1";
		holder.Map[new ComplexKey { Name = "beta" }] = "2";
		holder.Records[new RecordKey { Name = "alpha" }] = "1";
		holder.Records[new RecordKey { Name = "beta" }] = "2";
		holder.Set.Add(new ComplexKey { Name = "alpha" });
		holder.Set.Add(new ComplexKey { Name = "beta" });
		return holder;
	}

	[Test, Description(
		"Keys were added before their members were read, so both hashed as an empty key and the " +
		"second collided with the first, abandoning the rest of the dictionary")]
	public void DictionaryKeysKeepEveryEntry()
	{
		_serializer.Save(Call, CreateHolder());

		var output = _serializer.Load<Holder>(Call);

		Assert.That(output.Map, Has.Count.EqualTo(2));
		Assert.That(output.Map.Values, Is.EquivalentTo(new[] { "1", "2" }));
	}

	[Test, Description("A record compares and hashes on every member, so every record key was affected")]
	public void RecordKeysKeepEveryEntry()
	{
		_serializer.Save(Call, CreateHolder());

		var output = _serializer.Load<Holder>(Call);

		Assert.That(output.Records, Has.Count.EqualTo(2));
	}

	[Test, Description(
		"The surviving entry was stored under the hash an unloaded key had, so looking it up with " +
		"an equal key found nothing")]
	public void DictionaryKeysCanBeLookedUpAfterLoading()
	{
		_serializer.Save(Call, CreateHolder());

		var output = _serializer.Load<Holder>(Call);

		Assert.That(output.Map.ContainsKey(new ComplexKey { Name = "alpha" }), Is.True);
		Assert.That(output.Map.ContainsKey(new ComplexKey { Name = "beta" }), Is.True);
		Assert.That(output.Map[new ComplexKey { Name = "beta" }], Is.EqualTo("2"));
	}

	[Test, Description("Control: a HashSet already preloaded its items, and still does")]
	public void HashSetKeepsEveryItem()
	{
		_serializer.Save(Call, CreateHolder());

		var output = _serializer.Load<Holder>(Call);

		Assert.That(output.Set, Has.Count.EqualTo(2));
		Assert.That(output.Set.Contains(new ComplexKey { Name = "alpha" }), Is.True);
	}

	[Test, Description("Control: keys that don't hash on their members were never affected")]
	public void StringKeyedDictionariesRoundTrip()
	{
		var input = new Dictionary<string, int> { ["alpha"] = 1, ["beta"] = 2 };

		_serializer.Save(Call, input);
		var output = _serializer.Load<Dictionary<string, int>>(Call);

		Assert.That(output, Has.Count.EqualTo(2));
		Assert.That(output["beta"], Is.EqualTo(2));
	}

	/// <summary>Reads back fine, so it saves, and refuses to be assigned, so loading it fails</summary>
	[PublicData]
	public class ThrowingSetter
	{
		public string? Before { get; set; }

		public string? Refuses
		{
			get => "value";
			set => throw new InvalidOperationException("setter refused");
		}
	}

	[Test, Description(
		"A failure part way through loading an object was caught and dropped, so the object came " +
		"back partly filled and reported as loaded with nothing recorded anywhere")]
	public void AFailureWhileLoadingIsReported()
	{
		_serializer.Save(Call, new ThrowingSetter { Before = "read" });

		var output = _serializer.Load<ThrowingSetter>(Call);

		// Still returns whatever was read, rather than failing a file that partly works
		Assert.That(output, Is.Not.Null);

		Assert.That(AllEntriesText(Call.Log), Does.Contain("setter refused"),
			"The failure has to reach the log rather than being swallowed");
	}

	// EntriesText() only renders the top level, and a load logs into the timer's child log
	private static string AllEntriesText(Log log)
	{
		var text = new StringBuilder();
		foreach (LogEntry entry in log.Items)
		{
			Append(entry);
		}
		return text.ToString();

		void Append(LogEntry entry)
		{
			text.AppendLine(entry.ToString());
			if (entry is Log childLog)
			{
				foreach (LogEntry child in childLog.Items)
				{
					Append(child);
				}
			}
		}
	}
}
