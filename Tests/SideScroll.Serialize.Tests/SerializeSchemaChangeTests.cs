using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Serialize.Atlas;
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
}
