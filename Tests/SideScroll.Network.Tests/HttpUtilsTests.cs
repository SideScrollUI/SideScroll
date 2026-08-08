using NUnit.Framework;
using SideScroll.Network.Http;
using System.Text;

namespace SideScroll.Network.Tests;

[Category("HTTP")]
[NonParallelizable] // These set static properties
public class HttpUtilsTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("HttpUtils");
	}

	[Test]
	public void DecodeStringPreservesUtf8AndRemovesBom()
	{
		byte[] bytes = Encoding.UTF8.GetPreamble()
			.Concat(Encoding.UTF8.GetBytes("café"))
			.ToArray();

		Assert.That(HttpUtils.DecodeString(bytes), Is.EqualTo("café"));
	}

	[TestCase(0)]
	[TestCase(-1)]
	public void RejectsNonPositiveReadBufferSize(int value)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => HttpUtils.ReadBufferSize = value);
	}

	[TestCase(0)]
	[TestCase(-1)]
	public void RejectsNonPositiveAttemptCounts(int value)
	{
		Assert.Multiple(() =>
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => HttpUtils.MaxAttempts = value);
			Assert.Throws<ArgumentOutOfRangeException>(() => HttpCall.MaxAttempts = value);
		});
	}

	[Test]
	public void RejectsNegativeRetryDelays()
	{
		Assert.Multiple(() =>
		{
			Assert.Throws<ArgumentOutOfRangeException>(
				() => HttpUtils.BaseRetryDelay = TimeSpan.FromTicks(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => HttpCall.SleepMilliseconds = -1);
		});
	}
}
