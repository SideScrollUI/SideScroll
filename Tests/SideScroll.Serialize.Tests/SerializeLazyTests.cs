using NUnit.Framework;
using SideScroll.Serialize.Atlas;
using System.Reflection;

namespace SideScroll.Serialize.Tests;

[Category("SerializeLazy")]
public class SerializeLazyTests : SerializeBaseTest
{
	private SerializerFile? _serializerFile;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeLazy");

		string basePath = Paths.Combine(TestPath, "SerializeLazy");

		Directory.CreateDirectory(basePath);

		string filePath = Paths.Combine(basePath, SerializerFileAtlas.DataFileName);
		_serializerFile = new SerializerFileAtlas(filePath);
	}

	[Test, Description("Serialize Lazy Base")]
	public void SerializeLazyBase()
	{
		var input = new Parent
		{
			Child = new Child
			{
				UintTest = 2,
			}
		};

		_serializerFile!.Save(Call, input);
		Parent output = _serializerFile.Load<Parent>(Call, true)!;

		Assert.That(output.Child!.UintTest, Is.EqualTo(input.Child!.UintTest));
	}

	[Test, Description("Serialize Lazy Null Properties")]
	public void SerializeLazyNullProperties()
	{
		var input = new Parent();

		_serializerFile!.Save(Call, input);
		Parent output = _serializerFile.Load<Parent>(Call, true)!;

		Assert.That(output.Child, Is.EqualTo(input.Child));
	}

	[Test, Description("Serialize Lazy Write Then Read")]
	public void SerializeLazyWriteThenRead()
	{
		var input = new WriteRead();

		_serializerFile!.Save(Call, input);
		WriteRead output = _serializerFile.Load<WriteRead>(Call, true)!;
		output.StringTest = "abc";
		string temp = output.StringTest;

		Assert.That(output.StringTest, Is.EqualTo("abc"));
	}

	[Test, Description("Loading twice reuses the generated type instead of emitting a new assembly")]
	public void SerializeLazyReusesGeneratedType()
	{
		var input = new Parent
		{
			Child = new Child(),
		};
		_serializerFile!.Save(Call, input);

		Parent first = _serializerFile.Load<Parent>(Call, true)!;
		Parent second = _serializerFile.Load<Parent>(Call, true)!;

		Assert.That(first.GetType(), Is.SameAs(second.GetType()));
		Assert.That(first.GetType().Assembly, Is.SameAs(second.GetType().Assembly));

		// Both still lazy load correctly
		Assert.That(first.Child!.UintTest, Is.EqualTo(input.Child!.UintTest));
		Assert.That(second.Child!.UintTest, Is.EqualTo(input.Child!.UintTest));
	}

	[Test, Description("Reading a lazy property whose TypeRef was never set returns the current value instead of throwing")]
	public void SerializeLazyMissingTypeRef()
	{
		var input = new Parent
		{
			Child = new Child(),
		};
		_serializerFile!.Save(Call, input);

		Parent output = _serializerFile.Load<Parent>(Call, true)!;

		// LoadObjectData() swallows exceptions, so a partial load can leave a lazy property
		// with neither the Loaded flag nor a TypeRef set
		Type lazyType = output.GetType();
		FieldInfo loadedField = lazyType.GetField("_ChildLoaded", BindingFlags.NonPublic | BindingFlags.Instance)!;
		FieldInfo typeRefField = lazyType.GetField("_ChildTypeRef", BindingFlags.NonPublic | BindingFlags.Instance)!;
		Assert.That(loadedField, Is.Not.Null);
		Assert.That(typeRefField, Is.Not.Null);

		loadedField.SetValue(output, false);
		typeRefField.SetValue(output, null);

		Assert.That(output.Child, Is.Null);
	}

	[Test, Description("Value type properties get unboxed instead of passing the boxed reference to the setter")]
	public void SerializeLazyValueTypes()
	{
		var input = new ValueTypes
		{
			TimeTest = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc),
			GuidTest = Guid.NewGuid(),
			SpanTest = TimeSpan.FromMinutes(90),
			NullableSpanTest = TimeSpan.FromSeconds(30),
		};

		_serializerFile!.Save(Call, input);
		ValueTypes output = _serializerFile.Load<ValueTypes>(Call, true)!;

		Assert.That(output.TimeTest, Is.EqualTo(input.TimeTest));
		Assert.That(output.GuidTest, Is.EqualTo(input.GuidTest));
		Assert.That(output.SpanTest, Is.EqualTo(input.SpanTest));
		Assert.That(output.NullableSpanTest, Is.EqualTo(input.NullableSpanTest));
	}

	[Test, Description("Null value type properties keep their default")]
	public void SerializeLazyNullValueTypes()
	{
		var input = new ValueTypes();

		_serializerFile!.Save(Call, input);
		ValueTypes output = _serializerFile.Load<ValueTypes>(Call, true)!;

		Assert.That(output.TimeTest, Is.EqualTo(default(DateTime)));
		Assert.That(output.NullableSpanTest, Is.Null);
	}

	[Test, Description("Serialize Lazy Constructor")]
	[Ignore("Not Working")]
	public void SerializeLazyConstructor()
	{
		var input = new Container();

		_serializerFile!.Save(Call, input);
		Container output = _serializerFile.Load<Container>(Call, true)!;

		Assert.That(output.Id, Is.Not.Null);
	}

	public class Container
	{
		public virtual string Id { get; set; } = "5";
		public string Result { get; set; }

		public Container()
		{
			Result = Id;
		}
	}

	public class Parent
	{
		public virtual Child? Child { get; set; } //= new Child();
	}

	public class ProtectedGetterParent
	{
		public virtual Child? Child { protected get; set; }
		public Child? GetChild() => Child;
	}

	public class InternalGetterParent
	{
		public virtual Child? Child { internal get; set; }
	}

	[Test, Description(
		"A protected or internal getter on a virtual property is virtual, but HasVirtualProperty " +
		"deliberately checks GetGetMethod(false). Switching to (true) turns lazy loading on for " +
		"these and the generated subclass then loads them as null instead of their saved value")]
	public void SerializeLazyNonPublicGetterIsNotLazyLoaded()
	{
		var input = new ProtectedGetterParent
		{
			Child = new Child { UintTest = 2 },
		};

		_serializerFile!.Save(Call, input);
		ProtectedGetterParent output = _serializerFile.Load<ProtectedGetterParent>(Call, true)!;

		// Lazy loading emits a generated subclass, so the exact type stays put when it isn't applied
		Assert.That(output.GetType(), Is.SameAs(typeof(ProtectedGetterParent)));
		Assert.That(output.GetChild()!.UintTest, Is.EqualTo(2), "The value still round trips.");
	}

	[Test, Description("Control: a public virtual getter is lazy loaded, and its value still round trips")]
	public void SerializeLazyPublicVirtualGetter()
	{
		var input = new Parent
		{
			Child = new Child { UintTest = 3 },
		};

		_serializerFile!.Save(Call, input);
		Parent output = _serializerFile.Load<Parent>(Call, true)!;

		Assert.That(output.GetType(), Is.Not.SameAs(typeof(Parent)));
		Assert.That(output.Child!.UintTest, Is.EqualTo(3));
	}

	public class Child
	{
		public uint UintTest { get; set; } = 1;
		public double DoubleTest { get; set; } = 2.3;
		public string StringTest { get; set; } = "mystring";
	}

	public class WriteRead
	{
		public virtual string StringTest { get; set; } = "mystring";
	}

	public class ValueTypes
	{
		public virtual DateTime TimeTest { get; set; }
		public virtual Guid GuidTest { get; set; }
		public virtual TimeSpan SpanTest { get; set; }
		public virtual TimeSpan? NullableSpanTest { get; set; }
	}
}
