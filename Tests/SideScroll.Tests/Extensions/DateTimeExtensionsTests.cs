using NUnit.Framework;
using SideScroll.Extensions;

namespace SideScroll.Tests.Extensions;

[Category("Core")]
public class DateTimeExtensionsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	[Test]
	public void Ceil_DefaultSeconds()
	{
		DateTime dateTime = new(2023, 10, 18, 14, 30, 45, 500, DateTimeKind.Utc);

		DateTime result = dateTime.Ceil();

		Assert.That(result, Is.EqualTo(new DateTime(2023, 10, 18, 14, 30, 46, DateTimeKind.Utc)));
		Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
	}

	[Test]
	public void Ceil_Minutes()
	{
		DateTime dateTime = new(2023, 10, 18, 14, 30, 30, DateTimeKind.Utc);

		DateTime result = dateTime.Ceil(TimeSpan.TicksPerMinute);

		Assert.That(result, Is.EqualTo(new DateTime(2023, 10, 18, 14, 31, 0, DateTimeKind.Utc)));
	}

	[Test]
	public void Ceil_AlreadyAligned()
	{
		DateTime dateTime = new(2023, 10, 18, 14, 30, 0, DateTimeKind.Utc);

		Assert.That(dateTime.Ceil(TimeSpan.TicksPerMinute), Is.EqualTo(dateTime));
		Assert.That(dateTime.Ceil(), Is.EqualTo(dateTime));
	}
}
