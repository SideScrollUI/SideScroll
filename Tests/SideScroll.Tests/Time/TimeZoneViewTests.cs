using NUnit.Framework;
using SideScroll.Extensions;
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
	public void Equals_EqualNamesWithDifferentZoneIds_AreNotEqual()
	{
		var first = new TimeZoneView("A", "Shared", TimeZoneInfo.Utc);
		var second = new TimeZoneView("B", "Shared", TimeZoneInfo.Local);

		Assert.That(first, Is.Not.EqualTo(second));
		Assert.That(new HashSet<TimeZoneView> { first, second }, Has.Count.EqualTo(2));
	}

	[Test]
	public void Convert_CustomZoneNamedUtc_UsesItsOffset()
	{
		TimeZoneInfo customZone = TimeZoneInfo.CreateCustomTimeZone(
			"SideScroll-Named-Utc",
			TimeSpan.FromHours(4),
			"Utc",
			"Utc");
		var view = new TimeZoneView("Utc", "Utc", customZone);
		DateTime utc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

		DateTime result = view.Convert(utc);

		Assert.That(result, Is.EqualTo(TimeZoneInfo.ConvertTimeFromUtc(utc, customZone)));
	}

	[Test]
	public void Convert_UnconfiguredZone_DoesNotChangeValue()
	{
		var view = new TimeZoneView();
		DateTime value = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Unspecified);

		Assert.That(view.Convert(value), Is.EqualTo(value));
		Assert.That(view.ConvertTimeToUtc(value), Is.EqualTo(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
	}

	[Test]
	public void DateTimeFormat_UnconfiguredCurrentZone_DoesNotThrow()
	{
		TimeZoneView original = TimeZoneView.Current;
		TimeZoneView.Current = new TimeZoneView();
		try
		{
			Assert.DoesNotThrow(() => DateTime.UtcNow.Format());
		}
		finally
		{
			TimeZoneView.Current = original;
		}
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
