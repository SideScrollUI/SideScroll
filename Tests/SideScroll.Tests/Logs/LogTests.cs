using NUnit.Framework;
using SideScroll.Logs;

namespace SideScroll.Tests.Logs;

[Category("Core")]
public class LogTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	[Test, Description("Level keeps rising after the log fills up and starts trimming")]
	public void Level_RisesAfterMaxLogItems()
	{
		Log log = new()
		{
			Settings = new LogSettings { MaxLogItems = 3 },
		};

		for (int i = 0; i < 5; i++)
		{
			log.Add("Info " + i);
		}
		Assert.That(log.Level, Is.EqualTo(LogLevel.Info));

		log.AddError("Failed");

		Assert.That(log.Level, Is.EqualTo(LogLevel.Error));
	}

	[Test, Description("Entries counts every entry added, including trimmed ones")]
	public void Entries_CountsTrimmedItems()
	{
		Log log = new()
		{
			Settings = new LogSettings { MaxLogItems = 3 },
		};

		for (int i = 0; i < 6; i++)
		{
			log.Add("Info " + i);
		}

		Assert.That(log.Items, Has.Count.EqualTo(3));
		Assert.That(log.Entries, Is.EqualTo(6));
	}

	[Test]
	public void CloneSettings_KeepsContext()
	{
		SynchronizationContext context = new();
		LogSettings settings = new()
		{
			Context = context,
			MaxLogItems = 5,
		};

		LogSettings clone = settings.Clone();

		Assert.That(clone.Context, Is.SameAs(context));
		Assert.That(clone.MaxLogItems, Is.EqualTo(5));
	}

	[Test]
	public void WithMinLogLevel_KeepsContext()
	{
		SynchronizationContext context = new();
		LogSettings settings = new()
		{
			Context = context,
		};

		LogSettings clone = settings.WithMinLogLevel(LogLevel.Debug);

		Assert.That(clone.Context, Is.SameAs(context));
		Assert.That(clone.MinLogLevel, Is.EqualTo(LogLevel.Debug));
	}

	[Test]
	public void SetLogLevel_KeepsContext()
	{
		SynchronizationContext context = new();
		Log log = new()
		{
			Settings = new LogSettings { Context = context },
		};

		log.SetLogLevel(LogLevel.Debug);

		Assert.That(log.Settings!.Context, Is.SameAs(context));
	}

	[Test]
	public void LogWriterText_UsesEachEntriesTimestamp()
	{
		string path = Path.GetTempFileName();
		try
		{
			Log log = new()
			{
				Created = new DateTime(2000, 1, 1),
			};

			LogEntry entry;
			using (new LogWriterText(log, path))
			{
				entry = log.Add("Message")!;
			}

			string line = File.ReadAllText(path);
			Assert.That(line, Does.StartWith(entry.Created.ToString("yyyy-M-d H:mm:ss")));
			Assert.That(line, Does.Not.StartWith(log.Created.ToString("yyyy-M-d H:mm:ss")));
		}
		finally
		{
			File.Delete(path);
		}
	}

	[Test]
	public void LogWriterText_AcceptsFilenameWithoutDirectory()
	{
		string fileName = $"SideScroll-{Guid.NewGuid():N}.log";
		try
		{
			using (new LogWriterText(new Log(), fileName))
			{
			}

			Assert.That(File.Exists(fileName), Is.True);
		}
		finally
		{
			File.Delete(fileName);
		}
	}

	[Test]
	public void LogTimer_DisposeIsIdempotent()
	{
		Log log = new();
		LogTimer timer = log.Timer("Work");

		Assert.DoesNotThrow(() =>
		{
			timer.Dispose();
			timer.Dispose();
		});
		Assert.That(timer.Items.Count(entry => entry.Text == "Finished"), Is.EqualTo(1));
	}

	[Test]
	public void PropertyChangedWithoutContextIsRaised()
	{
		var entry = new LogEntry(new LogSettings(), LogLevel.Info, "Test", null);
		string? changedProperty = null;
		entry.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

		entry.Duration = TimeSpan.FromSeconds(1);

		Assert.That(changedProperty, Is.EqualTo(nameof(LogEntry.Duration)));
	}

	[Test, Description("Logging and rethrowing an exception preserves its original failure location")]
	public void ThrowPreservesOriginalStackTrace()
	{
		Exception original;
		try
		{
			ThrowOriginalException();
			throw new AssertionException("Expected an exception");
		}
		catch (InvalidOperationException e)
		{
			original = e;
		}

		InvalidOperationException rethrown = Assert.Throws<InvalidOperationException>(() =>
			new Log().Throw(original))!;

		Assert.That(rethrown.StackTrace, Does.Contain(nameof(ThrowOriginalException)));
	}

	private static void ThrowOriginalException()
	{
		throw new InvalidOperationException("Expected");
	}

	[Test, Description("A child removed immediately by a zero retention limit no longer updates its parent")]
	public void ZeroRetentionDoesNotKeepChildSubscription()
	{
		Log parent = new()
		{
			Settings = new LogSettings { MaxLogItems = 0 },
		};
		Log child = parent.AddChild("Child");
		int entriesAfterRemoval = parent.Entries;

		child.Add("Hidden child entry");

		Assert.That(parent.Items, Is.Empty);
		Assert.That(parent.Entries, Is.EqualTo(entriesAfterRemoval));
	}

	[Test]
	public void LoweredRetentionLimitTrimsAllExcessItems()
	{
		var settings = new LogSettings { MaxLogItems = 5 };
		Log log = new() { Settings = settings };
		for (int i = 0; i < 5; i++)
			log.Add($"Entry {i}");

		settings.MaxLogItems = 2;
		log.Add("Newest");

		Assert.That(log.Items, Has.Count.EqualTo(2));
		Assert.That(log.Items[^1].Text, Is.EqualTo("Newest"));
	}
}
