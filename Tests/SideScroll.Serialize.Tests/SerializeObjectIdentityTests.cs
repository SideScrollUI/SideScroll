using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Serialize;
using SideScroll.Serialize.Atlas;

namespace SideScroll.Serialize.Tests;

/// <summary>
/// The serializer stores each object once and refers back to it, so what counts as the same object
/// decides both what round-trips intact and how much is written
/// </summary>
[Category("Serialize")]
public class SerializeObjectIdentityTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeObjectIdentity");
	}

	private SerializerMemory _serializer = new SerializerMemoryAtlas();

	[SetUp]
	public void Setup()
	{
		_serializer = new SerializerMemoryAtlas();
	}

	/// <summary>Compares on Id alone, the way an entity keyed by a database id does</summary>
	[PublicData]
	public class KeyedItem
	{
		public int Id { get; set; }
		public string? Name { get; set; }

		public override bool Equals(object? obj) => obj is KeyedItem other && other.Id == Id;
		public override int GetHashCode() => Id.GetHashCode();
	}

	[PublicData]
	public record EqualRecord
	{
		public int Id { get; set; }
		public string? Name { get; set; }
	}

	[PublicData]
	public class Pair<T> where T : class
	{
		public T? First { get; set; }
		public T? Second { get; set; }
	}

	private static Pair<KeyedItem> CreateKeyedPair() => new()
	{
		// Distinct objects that compare equal, differing in a member Equals() doesn't look at
		First = new KeyedItem { Id = 1, Name = "first" },
		Second = new KeyedItem { Id = 1, Name = "second" },
	};

	[Test, Description(
		"Comparing by value made the second object indistinguishable from the first, so it was " +
		"saved as a reference to it and its own values were replaced rather than aliased")]
	public void EqualButDistinctObjectsSurviveSaving()
	{
		Pair<KeyedItem> input = CreateKeyedPair();

		_serializer.Save(Call, input);
		var output = _serializer.Load<Pair<KeyedItem>>(Call);

		Assert.That(output.First!.Name, Is.EqualTo("first"));
		Assert.That(output.Second!.Name, Is.EqualTo("second"));
		Assert.That(output.First, Is.Not.SameAs(output.Second));
	}

	[Test, Description("Cloning tracked the same way, so the second object became the first one's clone")]
	public void EqualButDistinctObjectsSurviveCloning()
	{
		Pair<KeyedItem> input = CreateKeyedPair();

		var clone = input.DeepClone(Call);

		Assert.That(clone.First!.Name, Is.EqualTo("first"));
		Assert.That(clone.Second!.Name, Is.EqualTo("second"));
		Assert.That(clone.First, Is.Not.SameAs(clone.Second));
	}

	[Test, Description("A record compares every member by value, so every record type was affected")]
	public void EqualButDistinctRecordsSurviveSaving()
	{
		Pair<EqualRecord> input = new()
		{
			First = new EqualRecord { Id = 1, Name = "same" },
			Second = new EqualRecord { Id = 1, Name = "same" },
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<Pair<EqualRecord>>(Call);

		Assert.That(output.First, Is.Not.SameAs(output.Second));
	}

	[Test, Description("One object referenced twice is still stored once and reloads as one object")]
	public void SharedReferencesStayShared()
	{
		var shared = new KeyedItem { Id = 1, Name = "shared" };
		Pair<KeyedItem> input = new() { First = shared, Second = shared };

		_serializer.Save(Call, input);
		var output = _serializer.Load<Pair<KeyedItem>>(Call);

		Assert.That(output.First, Is.SameAs(output.Second));
		Assert.That(output.First!.Name, Is.EqualTo("shared"));
	}

	[Test, Description("A shared reference survives cloning as a single clone")]
	public void SharedReferencesStaySharedWhenCloned()
	{
		var shared = new KeyedItem { Id = 1, Name = "shared" };
		Pair<KeyedItem> input = new() { First = shared, Second = shared };

		var clone = input.DeepClone(Call);

		Assert.That(clone.First, Is.SameAs(clone.Second));
		Assert.That(clone.First, Is.Not.SameAs(shared));
	}

	private static List<Pair<string>> CreateRepeatedStrings(int count)
	{
		List<Pair<string>> rows = [];
		for (int i = 0; i < count; i++)
		{
			// Distinct instances holding equal text, which is what loaded or parsed data produces
			rows.Add(new Pair<string>
			{
				First = new string("Completed Successfully".ToCharArray()),
				Second = new string("Network Diagnostics".ToCharArray()),
			});
		}
		return rows;
	}

	[Test, Description(
		"Immutable values stay shared. Storing a repeated string once per occurrence instead " +
		"multiplied the saved size")]
	public void RepeatedStringsAreStoredOnce()
	{
		List<Pair<string>> input = CreateRepeatedStrings(2000);

		_serializer.Save(Call, input);
		var output = _serializer.Load<List<Pair<string>>>(Call);

		Assert.That(output.Select(p => p.First).Distinct(ReferenceEqualityComparer.Instance).Count(),
			Is.EqualTo(1), "Every row's text should load as one shared instance");

		// 2000 rows of two ~20 character strings, well under what storing each separately needs
		Assert.That(_serializer.Stream.Length, Is.LessThan(60_000));
	}

	[Test, Description("Cloning shares repeated strings the same way rather than copying each one")]
	public void RepeatedStringsAreClonedOnce()
	{
		List<Pair<string>> input = CreateRepeatedStrings(500);

		var clone = input.DeepClone(Call);

		Assert.That(clone.Select(p => p.First).Distinct(ReferenceEqualityComparer.Instance).Count(),
			Is.EqualTo(1));
	}

	[Test]
	public void ShareableTypesAreRecognized()
	{
		Assert.Multiple(() =>
		{
			Assert.That(SerializerObjectComparer.IsShareable("text"), Is.True);
			Assert.That(SerializerObjectComparer.IsShareable(typeof(int)), Is.True);
			Assert.That(SerializerObjectComparer.IsShareable(new Version(1, 2)), Is.True);
			Assert.That(SerializerObjectComparer.IsShareable(new Uri("https://example.com")), Is.True);
			Assert.That(SerializerObjectComparer.IsShareable(DayOfWeek.Monday), Is.True);
			Assert.That(SerializerObjectComparer.IsShareable(DateTime.UtcNow), Is.True);

			Assert.That(SerializerObjectComparer.IsShareable(new KeyedItem()), Is.False);
			Assert.That(SerializerObjectComparer.IsShareable(new EqualRecord()), Is.False);
		});
	}

	[Test, Description(
		"Reading a value type boxes it again each time, so the box a member is saved from is never " +
		"the one it was queued as and matching them by reference loses the queued index")]
	public void BoxedValueTypesMatchByValue()
	{
		var comparer = SerializerObjectComparer.Instance;
		DateTime now = DateTime.UtcNow;
		object first = now;
		object second = now;

		Assert.That(ReferenceEquals(first, second), Is.False, "Each boxing allocates its own object");
		Assert.That(comparer.Equals(first, second), Is.True);
		Assert.That(comparer.GetHashCode(first), Is.EqualTo(comparer.GetHashCode(second)));
	}

	[PublicData]
	public class ValueTypeMembers
	{
		public object? BoxedDate { get; set; }
		public object? BoxedNumber { get; set; }
	}

	[Test, Description("A boxed value type stored in an object member still saves and reloads")]
	public void BoxedValueTypeMembersRoundTrip()
	{
		var now = new DateTime(2026, 8, 9, 12, 0, 0, DateTimeKind.Utc);
		var input = new ValueTypeMembers { BoxedDate = now, BoxedNumber = 42L };

		_serializer.Save(Call, input);
		var output = _serializer.Load<ValueTypeMembers>(Call);

		Assert.That(output.BoxedDate, Is.EqualTo(now));
		Assert.That(output.BoxedNumber, Is.EqualTo(42L));
	}

	[Test, Description("Two equal strings are one key, two equal mutable objects are two")]
	public void ComparerSeparatesMutableObjectsButNotStrings()
	{
		var comparer = SerializerObjectComparer.Instance;
		string a = new("shared".ToCharArray());
		string b = new("shared".ToCharArray());

		Assert.That(comparer.Equals(a, b), Is.True);
		Assert.That(comparer.GetHashCode(a), Is.EqualTo(comparer.GetHashCode(b)));

		var first = new KeyedItem { Id = 1, Name = "first" };
		var second = new KeyedItem { Id = 1, Name = "second" };

		Assert.That(first, Is.EqualTo(second), "The type itself still compares them as equal");
		Assert.That(comparer.Equals(first, second), Is.False);
		Assert.That(comparer.Equals(first, first), Is.True);
	}
}
