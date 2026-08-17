using NUnit.Framework;
using SideScroll.Logs;
using SideScroll.Tabs.Tools.FileViewer;

namespace SideScroll.Tabs.Tests;

/// <summary>
/// Probes registered at the same priority have to keep their registration order. List.Sort() is
/// unstable, so re-sorting the whole list on every registration could permute them
/// </summary>
[Category("Tabs"), NonParallelizable]
public class FileTypeDetectorTests : BaseTest
{
	// Above the 16 element threshold where List.Sort() switches from insertion sort to introsort
	private const int ProbeCount = 20;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("FileTypeDetector");
	}

	// Registration is process wide, so start from a known state and don't leave one behind
	[SetUp]
	public void Setup() => FileTypeDetector.ClearProbes();

	[TearDown]
	public void TearDown() => FileTypeDetector.ClearProbes();

	private static string CreateFile()
	{
		string path = Path.Combine(Path.GetTempPath(), "FileTypeDetectorTests", Path.GetRandomFileName());
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, "contents");
		return path;
	}

	/// <summary>Records the order it ran in and never claims the file, so every probe is reached</summary>
	private class RecordingProbe(List<int> order, int id, int priority = 0) : IFileTypeProbe
	{
		public int Priority => priority;

		public bool CanHandle(string filePath, ReadOnlySpan<byte> headerBytes)
		{
			order.Add(id);
			return false;
		}

		public Type GetTabType() => typeof(string);
	}

	[Test, Description(
		"Probes sharing a priority have to run in registration order, otherwise which one claims a " +
		"file changes between builds. Re-sorting the whole list on every registration permuted them")]
	public void EqualPriorityProbesRunInRegistrationOrder()
	{
		List<int> order = [];
		for (int i = 0; i < ProbeCount; i++)
		{
			FileTypeDetector.RegisterProbe(new RecordingProbe(order, i));
		}

		FileTypeDetector.ProbeFile(CreateFile());

		Assert.That(order, Is.EqualTo(Enumerable.Range(0, ProbeCount)));
	}

	[Test, Description("The same for delegate probes, which sorted through the same call")]
	public void EqualPriorityDelegateProbesRunInRegistrationOrder()
	{
		List<int> order = [];
		for (int i = 0; i < ProbeCount; i++)
		{
			int id = i;
			FileTypeDetector.RegisterProbe(context =>
			{
				order.Add(id);
				return null;
			});
		}

		FileTypeDetector.ProbeFile(CreateFile());

		Assert.That(order, Is.EqualTo(Enumerable.Range(0, ProbeCount)));
	}

	[Test, Description("Registration order holds within each priority, and the higher one still runs first")]
	public void ProbesRunByPriorityThenRegistrationOrder()
	{
		List<int> order = [];
		for (int i = 0; i < ProbeCount; i++)
		{
			// Alternating, so a stable order interleaves the two tiers back into two runs of evens and odds
			FileTypeDetector.RegisterProbe(new RecordingProbe(order, i, priority: i % 2));
		}

		FileTypeDetector.ProbeFile(CreateFile());

		int[] expected =
		[
			.. Enumerable.Range(0, ProbeCount).Where(i => i % 2 == 1),
			.. Enumerable.Range(0, ProbeCount).Where(i => i % 2 == 0),
		];
		Assert.That(order, Is.EqualTo(expected));
	}

	/// <summary>Claims every file it's asked about</summary>
	private class MatchAll<T>(int priority = 0) : IFileTypeProbe
	{
		public int Priority => priority;
		public bool CanHandle(string filePath, ReadOnlySpan<byte> headerBytes) => true;
		public Type GetTabType() => typeof(T);
	}

	[Test, Description("Control: a higher priority still wins however late it registers")]
	public void HigherPriorityProbesRunFirst()
	{
		FileTypeDetector.RegisterProbe(new MatchAll<int>());
		FileTypeDetector.RegisterProbe(new MatchAll<string>(priority: 5));
		FileTypeDetector.RegisterProbe(new MatchAll<int>(priority: 1));

		Assert.That(FileTypeDetector.ProbeFile(CreateFile()), Is.EqualTo(typeof(string)));
	}

	[Test, Description("Control: a lower priority runs last however early it registers")]
	public void LowerPriorityProbesRunLast()
	{
		FileTypeDetector.RegisterProbe(new MatchAll<int>(priority: -5));
		FileTypeDetector.RegisterProbe(new MatchAll<string>());

		Assert.That(FileTypeDetector.ProbeFile(CreateFile()), Is.EqualTo(typeof(string)));
	}

	/// <summary>Throws instead of answering, standing in for a probe that's broken rather than declining</summary>
	private class ThrowingProbe(int priority = 0) : IFileTypeProbe
	{
		public int Priority => priority;

		public bool CanHandle(string filePath, ReadOnlySpan<byte> headerBytes)
			=> throw new InvalidOperationException("probe unavailable");

		public Type GetTabType() => typeof(string);
	}

	/// <summary>Claims every file, so it detects whether the probes after a broken one still run</summary>
	private class ClaimingProbe(int priority = 0) : IFileTypeProbe
	{
		public int Priority => priority;

		public bool CanHandle(string filePath, ReadOnlySpan<byte> headerBytes) => true;

		public Type GetTabType() => typeof(int);
	}

	[Test, Description("A probe that throws is skipped, and the ones after it still get to claim the file")]
	public void AThrowingProbeDoesNotStopTheOnesAfterIt()
	{
		FileTypeDetector.RegisterProbe(new ThrowingProbe(priority: 10));
		FileTypeDetector.RegisterProbe(new ClaimingProbe(priority: 5));

		Assert.That(FileTypeDetector.ProbeFile(CreateFile()), Is.EqualTo(typeof(int)));
	}

	[Test, Description("A throwing delegate probe is skipped the same way")]
	public void AThrowingDelegateProbeDoesNotStopTheOnesAfterIt()
	{
		FileTypeDetector.RegisterProbe(_ => throw new InvalidOperationException("probe unavailable"), priority: 10);
		FileTypeDetector.RegisterProbe(_ => typeof(int), priority: 5);

		Assert.That(FileTypeDetector.ProbeFile(CreateFile()), Is.EqualTo(typeof(int)));
	}

	[Test, Description("Every probe throwing leaves detection to fall back to the extension, not to fail")]
	public void EveryProbeThrowingReturnsNull()
	{
		FileTypeDetector.RegisterProbe(new ThrowingProbe());
		FileTypeDetector.RegisterProbe(_ => throw new InvalidOperationException("probe unavailable"));

		Assert.That(FileTypeDetector.ProbeFile(CreateFile()), Is.Null);
	}

	[Test, Description(
		"A probe that's broken was indistinguishable from one that declined the file, so the log " +
		"has to name which one it was")]
	public void AThrowingProbeIsReported()
	{
		FileTypeDetector.RegisterProbe(new ThrowingProbe());

		FileTypeDetector.ProbeFile(CreateFile(), Call);

		Assert.That(Call.Log.Level, Is.GreaterThanOrEqualTo(LogLevel.Warn));
		Assert.That(LogText(Call.Log), Does.Contain(nameof(ThrowingProbe)));
	}

	[Test, Description("A throwing delegate is named by the method it points at")]
	public void AThrowingDelegateProbeIsReported()
	{
		FileTypeDetector.RegisterProbe(ThrowingDelegate);

		FileTypeDetector.ProbeFile(CreateFile(), Call);

		Assert.That(Call.Log.Level, Is.GreaterThanOrEqualTo(LogLevel.Warn));
		Assert.That(LogText(Call.Log), Does.Contain(nameof(ThrowingDelegate)));
	}

	/// <summary>Flattens a log's entries and their tags, which its own ToString() doesn't render</summary>
	private static string LogText(Log log)
	{
		var text = new System.Text.StringBuilder();
		void Walk(Log current)
		{
			foreach (LogEntry entry in current.Items)
			{
				text.Append(entry.Text).Append(' ');
				foreach (Tag tag in entry.Tags ?? [])
				{
					text.Append(tag.Name).Append('=').Append(tag.Value).Append(' ');
				}

				if (entry is Log childLog)
				{
					Walk(childLog);
				}
			}
		}

		Walk(log);
		return text.ToString();
	}

	private static Type? ThrowingDelegate(FileProbeContext context)
		=> throw new InvalidOperationException("probe unavailable");

	[Test, Description("A probe that declines is not reported, only one that failed")]
	public void ADecliningProbeIsNotReported()
	{
		List<int> order = [];
		FileTypeDetector.RegisterProbe(new RecordingProbe(order, 0));

		FileTypeDetector.ProbeFile(CreateFile(), Call);

		Assert.That(order, Is.Not.Empty, "precondition: the probe ran");
		Assert.That(Call.Log.Level, Is.LessThan(LogLevel.Warn));
	}
}
