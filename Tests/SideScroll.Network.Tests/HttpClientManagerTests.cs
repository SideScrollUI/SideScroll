using NUnit.Framework;
using SideScroll.Network.Http;

namespace SideScroll.Network.Tests;

[Category("Network")]
public class HttpClientManagerTests
{
	private int _originalMaxClients;

	[SetUp]
	public void Setup()
	{
		_originalMaxClients = HttpClientManager.MaxClients;
	}

	[TearDown]
	public void TearDown()
	{
		HttpClientManager.MaxClients = _originalMaxClients;
	}

	[Test]
	public void DefaultConfigReturnsTheSharedClient()
	{
		HttpClient first = HttpClientManager.GetClient(new HttpClientConfig());
		HttpClient second = HttpClientManager.GetClient(new HttpClientConfig());

		Assert.That(second, Is.SameAs(first));
	}

	[Test]
	public void MatchingConfigsReuseTheSameClient()
	{
		HttpClientConfig config = new(Accept: "application/reuse-test");

		HttpClient first = HttpClientManager.GetClient(config);
		HttpClient second = HttpClientManager.GetClient(config);

		Assert.That(second, Is.SameAs(first));
		Assert.That(first.Timeout, Is.EqualTo(new HttpClient().Timeout), "No timeout was configured");
	}

	[Test]
	public void ConfiguredTimeoutIsApplied()
	{
		TimeSpan timeout = TimeSpan.FromSeconds(37);

		HttpClient client = HttpClientManager.GetClient(new HttpClientConfig(Timeout: timeout));

		Assert.That(client.Timeout, Is.EqualTo(timeout));
	}

	[Test]
	[Description(
		"Timeout is part of the cache key and has unlimited distinct values, so the pool has to " +
		"stop growing. Past the cap each config still gets a working client, it just isn't reused")]
	public void ClientsPastTheCapAreNotCached()
	{
		HttpClientManager.MaxClients = 0;

		HttpClientConfig config = new(Accept: "application/cap-test");
		HttpClient first = HttpClientManager.GetClient(config);
		HttpClient second = HttpClientManager.GetClient(config);

		Assert.That(first, Is.Not.Null);
		Assert.That(second, Is.Not.SameAs(first),
			"Nothing should be cached once the cap is reached");
	}

	[Test]
	public void NegativeMaxClientsIsRejected()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => HttpClientManager.MaxClients = -1);
	}
}
