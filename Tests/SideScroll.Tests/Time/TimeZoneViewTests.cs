using NUnit.Framework;
using SideScroll.Time;

namespace SideScroll.Tests.Time;

[Category("Core")]
public class TimeZoneViewTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TimeZoneView");
	}

	[Test]
	public void ConvertTimeToUtc_CustomSource_UsesItsTimeZone()
	{
		DateTime sourceTime = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Unspecified);
		TimeSpan localOffset = TimeZoneInfo.Local.GetUtcOffset(sourceTime);
		TimeSpan customOffset = localOffset == TimeSpan.FromHours(12)
			? TimeSpan.FromHours(-12)
			: TimeSpan.FromHours(12);
		TimeZoneInfo customZone = TimeZoneInfo.CreateCustomTimeZone(
			"SideScroll-Test-Zone",
			customOffset,
			"SideScroll Test Zone",
			"SideScroll Test Zone");
		var view = new TimeZoneView("Test", "Test Zone", customZone);

		DateTime result = view.ConvertTimeToUtc(sourceTime);

		DateTime expected = TimeZoneInfo.ConvertTimeToUtc(sourceTime, customZone);
		Assert.That(result, Is.EqualTo(expected));
		Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
	}

	[Test]
	public void Convert_LocalSourceToCustomDestination_PreservesInstant()
	{
		DateTime localTime = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Local);
		TimeSpan localOffset = TimeZoneInfo.Local.GetUtcOffset(localTime);
		TimeSpan customOffset = localOffset == TimeSpan.FromHours(12)
			? TimeSpan.FromHours(-12)
			: TimeSpan.FromHours(12);
		TimeZoneInfo customZone = TimeZoneInfo.CreateCustomTimeZone(
			"SideScroll-Test-Destination",
			customOffset,
			"SideScroll Test Destination",
			"SideScroll Test Destination");
		var view = new TimeZoneView("Test", "Test Destination", customZone);

		DateTime result = view.Convert(localTime);

		DateTime expected = TimeZoneInfo.ConvertTimeFromUtc(localTime.ToUniversalTime(), customZone);
		Assert.That(result, Is.EqualTo(expected));
	}

	[Test]
	public void Equals_EqualNames_HaveEqualHashCodes()
	{
		var first = new TimeZoneView("A", "Shared", TimeZoneInfo.Utc);
		var second = new TimeZoneView("B", "Shared", TimeZoneInfo.Local);

		Assert.That(first, Is.EqualTo(second));
		Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
		Assert.That(new HashSet<TimeZoneView> { first, second }, Has.Count.EqualTo(1));
	}

	[Test]
	public void CompareTo_UsesAscendingOrder()
	{
		var alpha = new TimeZoneView("Alpha", "Alpha", TimeZoneInfo.Utc);
		var zulu = new TimeZoneView("Zulu", "Zulu", TimeZoneInfo.Utc);

		Assert.That(alpha.CompareTo(zulu), Is.LessThan(0));
		Assert.That(zulu.CompareTo(alpha), Is.GreaterThan(0));
	}
}
