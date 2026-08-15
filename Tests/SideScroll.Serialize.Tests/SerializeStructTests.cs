using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Serialize;
using SideScroll.Serialize.Atlas;

namespace SideScroll.Serialize.Tests;

/// <summary>
/// A value type is copied into whatever holds it, so it has to be read before the copy is taken.
/// Filling it in afterwards through the load queue updated the boxed original, leaving the holder
/// with every member at its default
/// </summary>
[Category("Serialize")]
public class SerializeStructTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeStructs");
	}

	private T SaveLoad<T>(T input) where T : class
	{
		var serializer = new SerializerMemoryAtlas();
		serializer.Save(Call, input);
		return serializer.Load<T>(Call);
	}

	[PublicData]
	public struct Point
	{
		public int X { get; set; }
		public int Y { get; set; }
	}

	[PublicData]
	public struct PointFields
	{
		public int X;
		public int Y;
	}

	[PublicData]
	public struct WithReference
	{
		public string? Name { get; set; }
		public List<string> Tags { get; set; }
	}

	[PublicData]
	public struct Nested
	{
		public Point Inner { get; set; }
		public int Depth { get; set; }
	}

	[PublicData]
	public class Holder
	{
		public Point Property { get; set; }
		public Point Field;
		public Point? Optional { get; set; }
		public PointFields Fields { get; set; }
		public WithReference Referencing { get; set; }
		public Nested Nested { get; set; }
		public List<Point> InAList { get; set; } = [];
		public Point[] InAnArray { get; set; } = [];
		public object? Boxed { get; set; }
	}

	private static Holder CreateHolder() => new()
	{
		Property = new Point { X = 1, Y = 2 },
		Field = new Point { X = 3, Y = 4 },
		Optional = new Point { X = 5, Y = 6 },
		Fields = new PointFields { X = 7, Y = 8 },
		Referencing = new WithReference { Name = "name", Tags = ["a", "b"] },
		Nested = new Nested { Inner = new Point { X = 9, Y = 10 }, Depth = 11 },
		InAList = [new Point { X = 12, Y = 13 }, new Point { X = 14, Y = 15 }],
		InAnArray = [new Point { X = 16, Y = 17 }],
		Boxed = new Point { X = 18, Y = 19 },
	};

	[Test, Description("A struct property kept none of its members")]
	public void StructPropertyKeepsItsMembers()
	{
		var output = SaveLoad(CreateHolder());

		Assert.That(output.Property.X, Is.EqualTo(1));
		Assert.That(output.Property.Y, Is.EqualTo(2));
	}

	[Test]
	public void StructFieldKeepsItsMembers()
	{
		var output = SaveLoad(CreateHolder());

		Assert.That(output.Field.X, Is.EqualTo(3));
		Assert.That(output.Field.Y, Is.EqualTo(4));
	}

	[Test]
	public void NullableStructKeepsItsMembers()
	{
		var output = SaveLoad(CreateHolder());

		Assert.That(output.Optional!.Value.X, Is.EqualTo(5));
		Assert.That(output.Optional!.Value.Y, Is.EqualTo(6));
	}

	[Test]
	public void NullableStructStaysNull()
	{
		var output = SaveLoad(new Holder { Optional = null });

		Assert.That(output.Optional, Is.Null);
	}

	[Test, Description("Public fields rather than properties")]
	public void StructOfFieldsKeepsItsMembers()
	{
		var output = SaveLoad(CreateHolder());

		Assert.That(output.Fields.X, Is.EqualTo(7));
		Assert.That(output.Fields.Y, Is.EqualTo(8));
	}

	[Test, Description("A reference inside a struct is still filled in through the queue")]
	public void StructKeepsItsReferences()
	{
		var output = SaveLoad(CreateHolder());

		Assert.That(output.Referencing.Name, Is.EqualTo("name"));
		Assert.That(output.Referencing.Tags, Is.EqualTo(new[] { "a", "b" }));
	}

	[Test]
	public void NestedStructKeepsItsMembers()
	{
		var output = SaveLoad(CreateHolder());

		Assert.That(output.Nested.Inner.X, Is.EqualTo(9));
		Assert.That(output.Nested.Depth, Is.EqualTo(11));
	}

	[Test, Description("The count was right and every element was a default")]
	public void StructsInAListKeepTheirMembers()
	{
		var output = SaveLoad(CreateHolder());

		Assert.That(output.InAList.Select(p => p.X), Is.EqualTo(new[] { 12, 14 }));
		Assert.That(output.InAList.Select(p => p.Y), Is.EqualTo(new[] { 13, 15 }));
	}

	[Test]
	public void StructsInAnArrayKeepTheirMembers()
	{
		var output = SaveLoad(CreateHolder());

		Assert.That(output.InAnArray[0].X, Is.EqualTo(16));
		Assert.That(output.InAnArray[0].Y, Is.EqualTo(17));
	}

	[Test, Description("Control: an object member held the box, so it already worked")]
	public void BoxedStructKeepsItsMembers()
	{
		var output = SaveLoad(CreateHolder());

		Assert.That(output.Boxed, Is.InstanceOf<Point>());
		Assert.That(((Point)output.Boxed!).X, Is.EqualTo(18));
	}

	[Test, Description("Reading a value type mid-member restores the position its holder was reading from")]
	public void MembersAfterAStructStillLoad()
	{
		var output = SaveLoad(CreateHolder());

		// Every member is read from one stream, so a struct leaving the position moved would break
		// whichever members follow it rather than the struct itself
		Assert.That(output.Nested.Depth, Is.EqualTo(11));
		Assert.That(output.InAList, Has.Count.EqualTo(2));
		Assert.That(output.Boxed, Is.Not.Null);
	}

	[Test]
	public void CloneKeepsStructMembers()
	{
		var clone = CreateHolder().DeepClone(Call);

		Assert.That(clone.Property.X, Is.EqualTo(1));
		Assert.That(clone.InAList.Select(p => p.X), Is.EqualTo(new[] { 12, 14 }));
	}
}
