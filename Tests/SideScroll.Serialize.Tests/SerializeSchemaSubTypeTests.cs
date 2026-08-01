using NUnit.Framework;
using SideScroll.Serialize.Atlas;
using SideScroll.Serialize.Atlas.Schema;
using System.Text;

namespace SideScroll.Serialize.Tests;

[Category("Serialize")]
public class SerializeSchemaSubTypeTests : SerializeBaseTest
{
	private string _basePath = null!;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeSchemaSubType");
	}

	[SetUp]
	public void Setup()
	{
		_basePath = Paths.Combine(TestPath, "SerializeSchemaSubType");

		if (Directory.Exists(_basePath))
		{
			Directory.Delete(_basePath, true);
		}
		Directory.CreateDirectory(_basePath);
	}

	public class UnsealedItem
	{
		public string? Name { get; set; } = "value";
	}

	// SerializerFileAtlas appends the data filename to the path it's given
	private string SaveFile(object obj)
	{
		new SerializerFileAtlas(_basePath).Save(Call, obj);
		return Paths.Combine(_basePath, SerializerFileAtlas.DataFileName);
	}

	private static int IndexOf(byte[] haystack, byte[] needle)
	{
		for (int i = 0; i <= haystack.Length - needle.Length; i++)
		{
			int j = 0;
			while (j < needle.Length && haystack[i + j] == needle[j])
			{
				j++;
			}
			if (j == needle.Length) return i;
		}
		return -1;
	}

	private static TypeSchema LoadSchema(Call call, byte[] bytes, Type type)
	{
		Serializer serializer = new();
		using var reader = new BinaryReader(new MemoryStream(bytes));
		serializer.Load(call, reader, null, loadData: false);

		return serializer.TypeSchemas.Single(schema => schema.Type == type);
	}

	// HasSubType is written right after the AssemblyQualifiedName
	private static int GetHasSubTypeIndex(byte[] bytes, Type type)
	{
		byte[] nameBytes = Encoding.UTF8.GetBytes(type.AssemblyQualifiedName!);
		int index = IndexOf(bytes, nameBytes);

		Assert.That(index, Is.GreaterThan(0), "Couldn't find the type's schema in the file");
		return index + nameBytes.Length;
	}

	[Test]
	public void UnsealedTypeSavesHasSubType()
	{
		byte[] bytes = File.ReadAllBytes(SaveFile(new UnsealedItem()));

		Assert.That(bytes[GetHasSubTypeIndex(bytes, typeof(UnsealedItem))], Is.EqualTo(1));
		Assert.That(LoadSchema(Call, bytes, typeof(UnsealedItem)).HasSubType, Is.True);
	}

	[Test, Description("HasSubType decides the object reference format, so the saved value has to win over the current type")]
	public void LoadUsesSavedHasSubType()
	{
		byte[] bytes = File.ReadAllBytes(SaveFile(new UnsealedItem()));

		// Rewrite it as if the type had been sealed when this was saved
		bytes[GetHasSubTypeIndex(bytes, typeof(UnsealedItem))] = 0;

		TypeSchema schema = LoadSchema(Call, bytes, typeof(UnsealedItem));

		Assert.That(schema.HasSubType, Is.False, "HasSubType was recomputed from the type instead of read from the file");
	}
}
