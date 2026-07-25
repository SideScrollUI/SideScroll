using NUnit.Framework;
using SideScroll.Utilities;

namespace SideScroll.Tests.Utilities;

[Category("Core")]
public class ProcessUtilsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ProcessUtils");
	}

	[Test]
	public void OpenFolder_NonExistentFolder_DoesNotThrow()
	{
		Assert.DoesNotThrow(() => ProcessUtils.OpenFolder("C:\\this_folder_should_not_exist_xyz_123"));
	}

	[Test]
	public void GetDotnetRuntimes_RunsSuccessfully()
	{
		// Since we cannot mock the underlying dotnet process output easily here,
		// we verify that the method executes without throwing exceptions on the current machine.
		Assert.DoesNotThrow(() =>
		{
			var runtimes = ProcessUtils.GetDotnetRuntimes();
			// We should have at least one runtime if tests are running
			Assert.That(runtimes, Is.Not.Null);
		});
	}
}
