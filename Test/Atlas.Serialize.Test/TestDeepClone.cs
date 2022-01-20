using Atlas.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Atlas.Serialize.Test;

[Category("DeepClone")]
public class TestDeepClone : TestSerializeBase
{
	private Log log;
	private Serializer serializer;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("DeepClone");
		log = Call.Log;
	}

	[SetUp]
	public void Setup()
	{
		serializer = new Serializer();
	}

	class StringClass
	{
		public string Value = "value";
	}

	[Test]
	public void DeepCloneStringField()
	{
		var input = new StringClass();

		var output = input.DeepClone();

		Assert.AreSame(input.Value, output.Value);
	}
}
