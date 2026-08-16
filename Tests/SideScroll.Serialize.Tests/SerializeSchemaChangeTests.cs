using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Serialize.Atlas;
using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using System.Text;

namespace SideScroll.Serialize.Tests;

[Category("Serialize")]
public class SerializeSchemaChangeTests : SerializeBaseTest
{
	private SerializerMemory _serializer = new SerializerMemoryAtlas();

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeSchemaChange");
	}

	[SetUp]
	public void Setup()
	{
		_serializer = new SerializerMemoryAtlas();
	}

	private void ReplaceBytes(string searchText, string replaceText)
	{
		var bytes = _serializer.Stream.GetBuffer();

		var oldBytes = Encoding.UTF8.GetBytes(searchText);
		var newBytes = Encoding.UTF8.GetBytes(replaceText);

		Assert.That(oldBytes.Length, Is.EqualTo(newBytes.Length));

		for (int i = 0; i <= bytes.Length - oldBytes.Length; i++)
		{
			if (bytes.Skip(i).Take(oldBytes.Length).SequenceEqual(oldBytes))
			{
				Array.Copy(newBytes, 0, bytes, i, newBytes.Length);
			}
		}
	}

	public class MissingPropertyOld
	{
		public bool BoolProperty { get; set; }

		public int IntProperty { get; set; }
	}

	public class MissingPropertyNew
	{
		public int IntProperty { get; set; }
	}

	[Test, Description("Serialize Property Missing Save")]
	public void SerializePropertyMissingSave()
	{
		MissingPropertyOld input = new()
		{
			BoolProperty = true,
			IntProperty = 1,
		};
		_serializer.Save(Call, input);

		ReplaceBytes(nameof(MissingPropertyOld), nameof(MissingPropertyNew));

		var output = _serializer.Load<MissingPropertyNew>(Call);
		Assert.That(output, Is.Not.Null);
		Assert.That(output.IntProperty, Is.EqualTo(input.IntProperty));
	}

	public class Class1
	{
		public int Integer = 1;
	}

	public class Class2
	{
		public int Integer { get; set; } = 1;
	}

	[Test]
	public void RenameFieldToProperty()
	{
		Class1 input = new()
		{
			Integer = 2,
		};

		_serializer.Save(Call, input);

		ReplaceBytes(nameof(Class1), nameof(Class2));

		var output = _serializer.Load<Class2>(Call);

		Assert.That(output.Integer, Is.EqualTo(input.Integer));
	}

	[Test]
	public void RenamePropertyToField()
	{
		Class2 input = new()
		{
			Integer = 2,
		};

		_serializer.Save(Call, input);

		ReplaceBytes(nameof(Class2), nameof(Class1));

		var output = _serializer.Load<Class1>(Call);

		Assert.That(output.Integer, Is.EqualTo(input.Integer));
	}

	public class OldClass
	{
		public int OldField = 1;
		public int OldProperty { get; set; } = 1;
	}

	public class NewClass
	{
		[DeprecatedName(nameof(OldClass.OldField))]
		public int NewField = 2;

		[DeprecatedName(nameof(OldClass.OldProperty))]
		public int NewProperty { get; set; } = 2;
	}

	[Test]
	public void DeprecatedName()
	{
		OldClass input = new()
		{
			OldField = 4,
			OldProperty = 5,
		};

		_serializer.Save(Call, input);

		ReplaceBytes(nameof(OldClass), nameof(NewClass));

		var output = _serializer.Load<NewClass>(Call);

		Assert.That(output.NewField, Is.EqualTo(input.OldField));
		Assert.That(output.NewProperty, Is.EqualTo(input.OldProperty));
	}

	public class RenamedClassNew
	{
		public int IntField = 1;
		public int IntProperty { get; set; } = 2;
	}

	[Test, Description("Rename a type in the TypeSchema using RegisterDeprecatedType")]
	public void RenameTypeWithDeprecatedName()
	{
		TypeSchema.RegisterDeprecatedType(typeof(RenamedClassNew), "RenamedClassOld");

		RenamedClassNew input = new()
		{
			IntField = 4,
			IntProperty = 5,
		};

		_serializer.Save(Call, input);

		// Simulate data serialized before the rename by replacing the type name with one that no longer exists
		ReplaceBytes(nameof(RenamedClassNew), "RenamedClassOld");

		var output = _serializer.Load<RenamedClassNew>(Call);

		Assert.That(output.IntField, Is.EqualTo(input.IntField));
		Assert.That(output.IntProperty, Is.EqualTo(input.IntProperty));
	}

	public class TypeReferenceClass
	{
		public Type? Type { get; set; }
	}

	[Test, Description("Rename a serialized Type value in the TypeRepoType using RegisterDeprecatedType")]
	public void RenameTypeValueWithDeprecatedName()
	{
		TypeSchema.RegisterDeprecatedType(typeof(RenamedClassNew), "RenamedClassOld");

		TypeReferenceClass input = new()
		{
			Type = typeof(RenamedClassNew),
		};

		_serializer.Save(Call, input);

		// Simulate data serialized before the rename by replacing the type name with one that no longer exists
		ReplaceBytes(nameof(RenamedClassNew), "RenamedClassOld");

		var output = _serializer.Load<TypeReferenceClass>(Call);

		Assert.That(output.Type, Is.EqualTo(typeof(RenamedClassNew)));
	}

	public class NullableOldClass
	{
		public int? IntegerField;
		public int? IntegerProperty { get; set; }

		public string? StringField;
		public string? StringProperty { get; set; }
	}

	public class NullableNewClass
	{
		public int IntegerField = 1;
		public int IntegerProperty { get; set; } = 2;

		public string StringField = "field";
		public string StringProperty { get; set; } = "property";
	}

	[Test]
	public void MemberNullToNonNull()
	{
		NullableOldClass input = new();

		_serializer.Save(Call, input);

		ReplaceBytes(nameof(NullableOldClass), nameof(NullableNewClass));

		var output = _serializer.Load<NullableNewClass>(Call);

		Assert.That(output.IntegerField, Is.EqualTo(1));
		Assert.That(output.IntegerProperty, Is.EqualTo(2));
		Assert.That(output.StringField, Is.EqualTo("field"));
		Assert.That(output.StringProperty, Is.EqualTo("property"));
	}

	public class NullableDefaults
	{
		public string? StringField = "field default";
		public string? StringProperty { get; set; } = "property default";
		public List<int>? ListProperty { get; set; } = [1, 2, 3];
		public object? ObjectProperty { get; set; } = "object default";
		public int? NumberProperty { get; set; } = 7;
	}

	[Test, Description(
		"Reference type nullability is erased, so string and string? are the same Type and the " +
		"Nullable<T> check discarded every serialized null, leaving the constructor's default. " +
		"MemberNullToNonNull above covers the other half, where a non-nullable member keeps its default")]
	public void NullableMembersRoundTripNull()
	{
		NullableDefaults input = new()
		{
			StringField = null,
			StringProperty = null,
			ListProperty = null,
			ObjectProperty = null,
			NumberProperty = null,
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<NullableDefaults>(Call);

		Assert.That(output.StringField, Is.Null);
		Assert.That(output.StringProperty, Is.Null);
		Assert.That(output.ListProperty, Is.Null);
		Assert.That(output.ObjectProperty, Is.Null);
		Assert.That(output.NumberProperty, Is.Null);
	}

	[Test, Description("Control: values still round trip, so the null handling isn't clearing everything")]
	public void NullableMembersRoundTripValues()
	{
		NullableDefaults input = new()
		{
			StringField = "saved field",
			StringProperty = "saved property",
			ListProperty = [4, 5],
			ObjectProperty = "saved object",
			NumberProperty = 9,
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<NullableDefaults>(Call);

		Assert.That(output.StringField, Is.EqualTo("saved field"));
		Assert.That(output.StringProperty, Is.EqualTo("saved property"));
		Assert.That(output.ListProperty, Is.EqualTo(new[] { 4, 5 }));
		Assert.That(output.ObjectProperty, Is.EqualTo("saved object"));
		Assert.That(output.NumberProperty, Is.EqualTo(9));
	}
	// Same member name, different field type, so loading one as the other reaches SetValue() with a
	// value the field can't take
	public class FieldTypeOldXX
	{
		public string Value = "text";
		public int Kept = 7;
	}

	public class FieldTypeNewXX
	{
		public int Value = 0;
		public int Kept = 0;
	}

	[Test, Description(
		"A field whose type no longer matches what was saved is skipped at schema initialization, " +
		"so the object around it still loads and the skip is reported")]
	public void ChangedFieldTypeIsSkippedAndReported()
	{
		FieldTypeOldXX input = new();

		_serializer.Save(Call, input);
		ReplaceBytes(nameof(FieldTypeOldXX), nameof(FieldTypeNewXX));

		var output = _serializer.Load<FieldTypeNewXX>(Call);

		// The rest of the object still loads
		Assert.That(output.Kept, Is.EqualTo(7));
		Assert.That(output.Value, Is.EqualTo(0), "The mismatched field is left at its default");

		Assert.That(AllEntriesText(Call.Log), Does.Contain("type has changed"),
			"InitializeField() already detects the mismatch and reports it");
	}

	private static string AllEntriesText(Log log)
	{
		var text = new StringBuilder();
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
		foreach (LogEntry entry in log.Items)
		{
			Append(entry);
		}
		return text.ToString();
	}
}
