using NUnit.Framework;
using SideScroll.Tabs.Bookmarks;

namespace SideScroll.Tabs.Tests.Bookmarks;

[Category("Tabs")]
public class LinkManagerTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("LinkManager");
	}

	[TearDown]
	public void TearDown()
	{
		LinkManager.Instance = null;
	}

	[Test]
	public void InitializeSetsTheInstanceForTheProject()
	{
		var project = new Project();

		project.Initialize();

		Assert.That(LinkManager.Instance, Is.Not.Null);
		Assert.That(LinkManager.Instance!.Project, Is.SameAs(project));
	}

	[Test]
	[Description("The manager holds the project through both link collections, so leaving the static set keeps a closed project alive")]
	public void ReleaseClearsTheInstanceForItsOwnProject()
	{
		var project = new Project();
		project.Initialize();

		LinkManager.Release(project);

		Assert.That(LinkManager.Instance, Is.Null);
	}

	[Test]
	[Description("Tearing down one project shouldn't clear a manager that a later project replaced it with")]
	public void ReleaseLeavesAnotherProjectsInstance()
	{
		var closed = new Project();
		closed.Initialize();

		var current = new Project();
		current.Initialize();

		LinkManager.Release(closed);

		Assert.That(LinkManager.Instance, Is.Not.Null);
		Assert.That(LinkManager.Instance!.Project, Is.SameAs(current));
	}

	[Test]
	public void ReleaseIsSafeWithNoInstance()
	{
		LinkManager.Instance = null;

		Assert.DoesNotThrow(() => LinkManager.Release(new Project()));
	}
}
