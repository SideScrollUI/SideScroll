using NUnit.Framework;
using SideScroll.Network.Http;
using System.Text;

namespace SideScroll.Network.Tests;

[Category("HTTP")]
public class HttpCacheTests : BaseTest
{
	private string _cachePath = null!;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("HTTP");
	}

	[SetUp]
	public void Setup()
	{
		_cachePath = Path.Combine(Environment.CurrentDirectory, "HttpCacheTests", TestContext.CurrentContext.Test.Name);

		if (Directory.Exists(_cachePath))
		{
			Directory.Delete(_cachePath, true);
		}
	}

	private static byte[] GetBytes(string text) => Encoding.UTF8.GetBytes(text);

	[Test]
	public void AddAndGetEntry()
	{
		using var cache = new HttpCache(_cachePath, true);

		cache.AddEntry("http://example.com/a", GetBytes("first"));
		cache.AddEntry("http://example.com/b", GetBytes("second"));

		Assert.That(cache.GetString("http://example.com/a"), Is.EqualTo("first"));
		Assert.That(cache.GetString("http://example.com/b"), Is.EqualTo("second"));
		Assert.That(cache.GetBytes("http://example.com/missing"), Is.Null);
	}

	[Test, Description("Entries reload from the index after reopening")]
	public void EntriesPersist()
	{
		using (var cache = new HttpCache(_cachePath, true))
		{
			cache.AddEntry("http://example.com/a", GetBytes("first"));
		}

		using var reopened = new HttpCache(_cachePath, true);

		Assert.That(reopened.Entries, Has.Count.EqualTo(1));
		Assert.That(reopened.GetString("http://example.com/a"), Is.EqualTo("first"));
	}

	[Test, Description("Long uris exceed the single byte length prefix that PeekChar() relied on")]
	public void LongUrisPersist()
	{
		string longUri = "http://example.com/" + new string('a', 500);

		using (var cache = new HttpCache(_cachePath, true))
		{
			cache.AddEntry(longUri, GetBytes("value"));
			cache.AddEntry("http://example.com/short", GetBytes("other"));
		}

		using var reopened = new HttpCache(_cachePath, true);

		Assert.That(reopened.Entries, Has.Count.EqualTo(2));
		Assert.That(reopened.GetString(longUri), Is.EqualTo("value"));
		Assert.That(reopened.GetString("http://example.com/short"), Is.EqualTo("other"));
	}

	[Test, Description("Opening a cache that doesn't exist read only gives an empty cache")]
	public void OpenMissingCacheReadOnly()
	{
		using var cache = new HttpCache(_cachePath, false);

		Assert.That(cache.Entries, Is.Empty);
		Assert.That(cache.GetBytes("http://example.com/a"), Is.Null);
	}

	[Test]
	public void OpenExistingCacheReadOnly()
	{
		using (var cache = new HttpCache(_cachePath, true))
		{
			cache.AddEntry("http://example.com/a", GetBytes("first"));
		}

		using var readOnly = new HttpCache(_cachePath, false);

		Assert.That(readOnly.GetString("http://example.com/a"), Is.EqualTo("first"));
	}

	[Test, Description("Listing entries while adding them shouldn't throw a collection modified exception")]
	public void ConcurrentAddAndList()
	{
		using var cache = new HttpCache(_cachePath, true);
		using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(2));

		Task adding = Task.Run(() =>
		{
			for (int i = 0; i < 200 && !cancellation.IsCancellationRequested; i++)
			{
				cache.AddEntry($"http://example.com/{i}", GetBytes("value"));
			}
		});

		Assert.DoesNotThrow(() =>
		{
			while (!adding.IsCompleted)
			{
				_ = cache.Entries.Count;
				_ = cache.LoadableEntries.Count;
				_ = cache.ContainsKey("http://example.com/1");
				_ = cache.Size;
			}
		});

		Assert.That(adding.Exception, Is.Null);
	}

	// Simulates the process dying partway through appending an entry
	private void AppendPartialEntry(string uri)
	{
		string indexPath = Path.Combine(_cachePath, "http.index");

		using var stream = new FileStream(indexPath, FileMode.Append, FileAccess.Write);
		using var writer = new BinaryWriter(stream);

		writer.Write(uri);
		writer.Write(1234L); // Offset, the Size and Downloaded fields never get written
	}

	[Test, Description("An interrupted write leaves a partial entry, the rest still has to load")]
	public void TruncatedIndexKeepsCompleteEntries()
	{
		using (var cache = new HttpCache(_cachePath, true))
		{
			cache.AddEntry("http://example.com/a", GetBytes("first"));
			cache.AddEntry("http://example.com/b", GetBytes("second"));
		}

		AppendPartialEntry("http://example.com/c");

		using var reopened = new HttpCache(_cachePath, true);

		Assert.That(reopened.Entries, Has.Count.EqualTo(2));
		Assert.That(reopened.GetString("http://example.com/a"), Is.EqualTo("first"));
		Assert.That(reopened.GetString("http://example.com/b"), Is.EqualTo("second"));
	}

	[Test, Description("The partial entry gets dropped so later writes aren't orphaned")]
	public void TruncatedIndexRecoversForWrites()
	{
		using (var cache = new HttpCache(_cachePath, true))
		{
			cache.AddEntry("http://example.com/a", GetBytes("first"));
		}

		AppendPartialEntry("http://example.com/partial");

		using (var recovered = new HttpCache(_cachePath, true))
		{
			recovered.AddEntry("http://example.com/b", GetBytes("second"));
		}

		using var reopened = new HttpCache(_cachePath, true);

		Assert.That(reopened.Entries.Select(e => e.Uri).ToList(), Is.EquivalentTo(new[]
		{
			"http://example.com/a",
			"http://example.com/b",
		}));
		Assert.That(reopened.GetString("http://example.com/b"), Is.EqualTo("second"));
	}

	// Corrupt bytes can still decode as a complete entry, just with a nonsense offset
	private void AppendCorruptEntry(string uri, long offset, int size)
	{
		string indexPath = Path.Combine(_cachePath, "http.index");

		using var stream = new FileStream(indexPath, FileMode.Append, FileAccess.Write);
		using var writer = new BinaryWriter(stream);

		writer.Write(uri);
		writer.Write(offset);
		writer.Write(size);
		writer.Write(DateTime.Now.Ticks);
	}

	[TestCase(-5000, 4, TestName = "CorruptIndexOffset_Negative")]
	[TestCase(0, 1, TestName = "CorruptIndexOffset_BeforeValidData")]
	[Description(
		"A corrupt entry's offset must not be trusted when truncating orphaned data: a negative one " +
		"would throw while opening, and one pointing before real data would delete it.")]
	public void CorruptIndexOffsetKeepsCacheUsable(long offset, int size)
	{
		using (var cache = new HttpCache(_cachePath, true))
		{
			cache.AddEntry("http://example.com/a", GetBytes("first"));
			cache.AddEntry("http://example.com/b", GetBytes("second"));
		}

		AppendCorruptEntry("http://example.com/corrupt", offset, size);

		HttpCache? reopened = null;
		Assert.DoesNotThrow(() => reopened = new HttpCache(_cachePath, true),
			"A corrupt offset shouldn't stop the cache from opening.");

		using (reopened)
		{
			Assert.That(reopened!.GetString("http://example.com/a"), Is.EqualTo("first"));
			Assert.That(reopened.GetString("http://example.com/b"), Is.EqualTo("second"));
		}
	}
}
