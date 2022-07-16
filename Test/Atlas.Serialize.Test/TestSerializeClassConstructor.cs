using Atlas.Core;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Atlas.Serialize.Test;

[Category("Serialize")]
public class SerializeClassConstructor : TestSerializeBase
{
	private SerializerMemory _serializer;

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
		public NoConstructorBaseClass BaseClass;
	}

	[Test, Description("Serialize No Default Constructor Base Class")]
	public void SerializeNoDefaultConstructorBaseClass()
	{
		var input = new DerivedClassWithConstructor();

		_serializer.Save(Call, input);
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

		_serializer.Save(Call, input);
		var output = _serializer.Load<DerivedClassWithConstructorReference>(Call);

		Assert.AreEqual(output.BaseClass.B, input.BaseClass.B);
	}

	public class NoEmptyConstructorFieldClass
	{
		public int A = 1;

		public NoEmptyConstructorFieldClass(int a)
		{
			A = a;
		}
	}

	public class NoEmptyConstructorPropertyClass
	{
		public int A { get; set; } = 1;

		public NoEmptyConstructorPropertyClass(int a)
		{
			A = a;
		}
	}

	[Test, Description("Serialize No Empty Constructor Field Class")]
	public void SerializeNoEmptyConstructorFieldClass()
	{
		var input = new NoEmptyConstructorFieldClass(5);

		_serializer.Save(Call, input);
		var output = _serializer.Load<NoEmptyConstructorFieldClass>(Call);

		Assert.AreEqual(output.A, input.A);
	}

	[Test, Description("Serialize No Empty Constructor Property Class")]
	public void SerializeNoEmptyConstructorPropertyClass()
	{
		var input = new NoEmptyConstructorPropertyClass(5);

		_serializer.Save(Call, input);
		var output = _serializer.Load<NoEmptyConstructorPropertyClass>(Call);

		Assert.AreEqual(output.A, input.A);
	}
}
