using NUnit.Framework;
using SideScroll.Resources;
using System.Reflection;

namespace SideScroll.Tests.Resources;

public class ResourceViewTests
{
	[Test]
	public void MissingResourceNamesPathAndAssembly()
	{
		Assembly assembly = typeof(ResourceViewTests).Assembly;
		var resourceView = new ResourceView(assembly, "Missing", "Resources", "DoesNotExist", "txt");

		FileNotFoundException exception = Assert.Throws<FileNotFoundException>(() => _ = resourceView.Stream)!;

		Assert.That(exception.FileName, Is.EqualTo(resourceView.Path));
		Assert.That(exception.Message, Does.Contain(resourceView.Path));
		Assert.That(exception.Message, Does.Contain(assembly.GetName().Name));
	}
}
