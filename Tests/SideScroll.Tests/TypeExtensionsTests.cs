using NUnit.Framework;
using SideScroll.Extensions;

namespace SideScroll.Tests;

[Category("Core")]
public class TypeExtensionsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TypeExtensions");
	}

	[Test]
	public void AssemblyQualifiedShortNameInt()
	{
		string shortName = typeof(int).GetAssemblyQualifiedShortName();

		Assert.That(shortName, Is.EqualTo("System.Int32, System.Private.CoreLib"));
	}

	[Test]
	public void AssemblyQualifiedShortNameList()
	{
		string shortName = typeof(List<Tag>).GetAssemblyQualifiedShortName();

		Assert.That(shortName, Is.EqualTo("System.Collections.Generic.List`1[[SideScroll.Tag, SideScroll]], System.Private.CoreLib"));
	}

	[Test]
	public void AssemblyQualifiedShortNameListOfLists()
	{
		string shortName = typeof(List<List<Tag>>).GetAssemblyQualifiedShortName();

		Assert.That(shortName, Is.EqualTo("System.Collections.Generic.List`1[[System.Collections.Generic.List`1[[SideScroll.Tag, SideScroll]], System.Private.CoreLib]], System.Private.CoreLib"));
	}

	[Test]
	public void AssemblyQualifiedShortNameDictionary()
	{
		string shortName = typeof(Dictionary<string, Tag>).GetAssemblyQualifiedShortName();

		Assert.That(shortName, Is.EqualTo("System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib], [SideScroll.Tag, SideScroll]], System.Private.CoreLib"));
	}
}
