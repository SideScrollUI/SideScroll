using Atlas.Core.Time;
using NUnit.Framework;
using System;

namespace Atlas.Serialize.Test
{
	[Category("Serialize")]
	public class SerializeTypes : TestSerializeBase
	{
		private SerializerMemory _serializer;
		
		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("Serialize");
		}

		[SetUp]
		public void Setup()
		{
			_serializer = new SerializerMemoryAtlas();
		}

		[Test, Description("Serialize Nullable int")]
		public void SerializeNullableInt()
		{
			int? input = 1;

			_serializer.Save(Call, input);
			int? output = _serializer.Load<int?>(Call);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Serialize Nullable Primitive")]
		public void SerializeNullablePrimitive()
		{
			int? input = 5;

			_serializer.Save(Call, input);
			int? output = _serializer.Load<int?>(Call);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Serialize int")]
		public void SerializeInt()
		{
			int input = 5;

			_serializer.Save(Call, input);
			int output = _serializer.Load<int>(Call);

			Assert.AreEqual(input, output);
		}

		[Test, Description("Serialize Enum")]
		public void SerializeEnum()
		{
			var input = new EnumTest
			{
				TestEnum = MyEnum.b
			};

			_serializer.Save(Call, input);
			EnumTest output = _serializer.Load<EnumTest>(Call);

			Assert.AreEqual(output.TestEnum, input.TestEnum);
		}

		[Test, Description("Serialize Nullable Enum")]
		public void SerializeNullableEnum()
		{
			MyEnum? input = MyEnum.b;

			_serializer.Save(Call, input);
			MyEnum? output = _serializer.Load<MyEnum?>(Call);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Serialize Type")]
		public void SerializeType()
		{
			Type type = typeof(string);

			_serializer.Save(Call, type);
			Type output = _serializer.Load<Type>(Call);

			Assert.AreEqual(type, output);
		}

		public struct StructTest
		{
			public int value;
		}

		[Test, Description("Serialize Struct")]
		public void SerializeStruct()
		{
			var input = new StructTest()
			{
				value = 5
			};

			_serializer.Save(Call, input);
			StructTest output = _serializer.Load<StructTest>(Call);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize DateTime Local")]
		public void SerializeDateTimeLocal()
		{
			DateTime input = DateTime.Now;

			_serializer.Save(Call, input);
			DateTime output = _serializer.Load<DateTime>(Call);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize DateTime UTC")]
		public void SerializeDateTimeUtc()
		{
			DateTime input = DateTime.UtcNow;

			_serializer.Save(Call, input);
			DateTime output = _serializer.Load<DateTime>(Call);

			Assert.AreEqual(input, output);
		}

		// DateTimeOffset has no set operators and relies on constructor
		[Test, Description("Serialize DateTimeOffset Local")]
		public void SerializeDateTimeOffsetLocal()
		{
			DateTime dateTime = DateTime.Now;
			DateTimeOffset input = new DateTimeOffset(dateTime);

			_serializer.Save(Call, input);
			DateTimeOffset output = _serializer.Load<DateTimeOffset>(Call);

			Assert.AreEqual(input, output);
		}

		// DateTimeOffset has no set operators and relies on constructor
		[Test, Description("Serialize DateTimeOffset UTC")]
		public void SerializeDateTimeOffsetUtc()
		{
			DateTime dateTime = DateTime.UtcNow;
			DateTimeOffset input = new DateTimeOffset(dateTime);

			_serializer.Save(Call, input);
			DateTimeOffset output = _serializer.Load<DateTimeOffset>(Call);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize TimeZoneInfo")]
		public void SerializeTimeZoneInfo()
		{
			TimeZoneInfo input = TimeZoneInfo.Local;

			_serializer.Save(Call, input);
			TimeZoneInfo output = _serializer.Load<TimeZoneInfo>(Call);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize TimeZoneView")]
		public void SerializeTimeZoneView()
		{
			TimeZoneView input = TimeZoneView.Local;

			_serializer.Save(Call, input);
			TimeZoneView output = _serializer.Load<TimeZoneView>(Call);

			Assert.AreEqual(input.Abbreviation, output.Abbreviation);
			Assert.AreEqual(input.Name, output.Name);
			Assert.AreEqual(input.TimeZoneInfo, output.TimeZoneInfo);
		}

		public class NullableDateTime
		{
			public long Long { get; set; }
			public DateTime? TimeStamp { get; set; }
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize Long and DateTime")]
		public void SerializeLongAndDateTime()
		{
			var input = new NullableDateTime()
			{
				TimeStamp = DateTime.UtcNow,
			};

			_serializer.Save(Call, input);
			var output = _serializer.Load<NullableDateTime>(Call);

			Assert.AreEqual(input.TimeStamp, output.TimeStamp);
		}

		[Test, Description("Serialize Objects")]
		public void SerializeObjects()
		{
			var input = new Objects();

			_serializer.Save(Call, input);
			var output = _serializer.Load<Objects>(Call);
			Assert.NotNull(output);
		}

		public class Objects
		{
			public object Object = 2;
		}

		public enum MyEnum
		{
			a,
			b
		}

		public class EnumTest
		{
			public MyEnum TestEnum = MyEnum.a;
		}

		[Test, Description("Serialize Version")]
		public void SerializeVersion()
		{
			var input = new Version("2.4");

			_serializer.Save(Call, input);
			var output = _serializer.Load<Version>(Call);

			Assert.AreEqual(input, output);
		}
	}
}
