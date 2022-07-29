using Atlas.Core;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Atlas.Serialize.Test;

[Category("Serialize")]
public class SerializeClassConstructor : TestSerializeBase
{
	private SerializerMemory? _serializer;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeClassConstructor");
	}

	[SetUp]
	public void Setup()
	{
		_serializer = new SerializerMemoryAtlas();
	}

	public class NoConstructorBaseClass
	{
		public int A = 1;

		[PrivateData]
		public int B = 0;

		public NoConstructorBaseClass(int a)
		{
			A = a;
		}
	}

	public class DerivedClassWithConstructor : NoConstructorBaseClass
	{
		public DerivedClassWithConstructor() : base(0)
		{
		}

		public DerivedClassWithConstructor(int a) : base(a)
		{
		}
	}

	public class DerivedClassWithConstructorReference
	{
		[Serialized]
		public NoConstructorBaseClass? BaseClass;
	}

	[Test, Description("Serialize No Default Constructor Base Class")]
	public void SerializeNoDefaultConstructorBaseClass()
	{
		var input = new DerivedClassWithConstructor();

		_serializer!.Save(Call, input);
		var output = _serializer.Load<NoConstructorBaseClass>(Call);

		Assert.AreEqual(output.B, input.B);
	}

	[Test, Description("Serialize No Default Constructor Base Class Reference")]
	public void SerializeNoDefaultConstructorBaseClassReference()
	{
		var input = new DerivedClassWithConstructorReference()
		{
			BaseClass = new DerivedClassWithConstructor(1),
		};

		_serializer!.Save(Call, input);
		var output = _serializer.Load<DerivedClassWithConstructorReference>(Call);

		Assert.AreEqual(output.BaseClass!.B, input.BaseClass.B);
	}

	public record CustomConstructorFieldClass
	{
		public int A = 1;

		public CustomConstructorFieldClass(int a)
		{
			A = a;
		}
	}

	public record CustomConstructorPropertyClass
	{
		public int A { get; set; } = 1;

		public CustomConstructorPropertyClass(int a)
		{
			A = a;
		}
	}

	[Test, Description("Serialize Custom Constructor Field Class")]
	public void SerializeCustomConstructorFieldClass()
	{
		var input = new CustomConstructorFieldClass(5);

		_serializer!.Save(Call, input);
		var output = _serializer.Load<CustomConstructorFieldClass>(Call);

		Assert.AreEqual(output.A, input.A);
	}

	[Test, Description("Serialize Custom Constructor Property Class")]
	public void SerializeCustomConstructorPropertyClass()
	{
		var input = new CustomConstructorPropertyClass(5);

		_serializer!.Save(Call, input);
		var output = _serializer.Load<CustomConstructorPropertyClass>(Call);

		Assert.AreEqual(output.A, input.A);
	}

	[Test, Description("Serialize Custom Constructor List Field Class")]
	public void SerializeCustomConstructorListFieldClass()
	{
		var item = new CustomConstructorFieldClass(5);
		var input = new List<CustomConstructorFieldClass>() { item };

		_serializer!.Save(Call, input);
		var output = _serializer.Load<List<CustomConstructorFieldClass>>(Call);

		Assert.AreEqual(output, input);
	}
}
