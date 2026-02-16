namespace SideScroll.Tabs.Tools.FileViewer;

/// <summary>
/// Interface for probing file types based on file content.
/// Implement this interface to provide custom file type detection logic.
/// </summary>
public interface IFileTypeProbe
{
	/// <summary>
	/// Determines if this probe can handle the given file.
	/// </summary>
	/// <param name="filePath">The path to the file being examined</param>
	/// <param name="headerBytes">The first bytes of the file (typically first 100-512 bytes)</param>
	/// <returns>True if this probe can handle the file, false otherwise</returns>
	bool CanHandle(string filePath, ReadOnlySpan<byte> headerBytes);

	/// <summary>
	/// Gets the tab type that should be used to display this file.
	/// </summary>
	Type GetTabType();

	/// <summary>
	/// Gets the priority of this probe. Higher values are checked first.
	/// Default priority is 0. Extension-based detection has priority -1.
	/// </summary>
	int Priority => 0;
}

/// <summary>
/// Context information for file type probing.
/// </summary>
public class FileProbeContext
{
	public string FilePath { get; }
	public ReadOnlyMemory<byte> HeaderBytes { get; }
	public string Extension { get; }
	public long FileSize { get; }

	public FileProbeContext(string filePath, ReadOnlyMemory<byte> headerBytes, string extension, long fileSize)
	{
		FilePath = filePath;
		HeaderBytes = headerBytes;
		Extension = extension;
		FileSize = fileSize;
	}
}

/// <summary>
/// Delegate for probing file types based on file content.
/// </summary>
/// <param name="context">Context information about the file being probed</param>
/// <returns>The tab type if the probe can handle the file, null otherwise</returns>
public delegate Type? FileTypeProbeDelegate(FileProbeContext context);

/// <summary>
/// Manages file type detection using content-based probes.
/// </summary>
public static class FileTypeDetector
{
	private static readonly List<(IFileTypeProbe probe, int priority)> _probes = [];
	private static readonly List<(FileTypeProbeDelegate probe, int priority)> _delegateProbes = [];

	/// <summary>
	/// Gets or sets the default number of bytes to read from files for probing.
	/// Default is 512 bytes.
	/// </summary>
	public static int DefaultHeaderSize { get; set; } = 512;

	/// <summary>
	/// Registers a file type probe that implements IFileTypeProbe.
	/// Probes are checked in order of priority (highest first).
	/// </summary>
	public static void RegisterProbe(IFileTypeProbe probe)
	{
		lock (_probes)
		{
			_probes.Add((probe, probe.Priority));
			_probes.Sort((a, b) => b.priority.CompareTo(a.priority));
		}
	}

	/// <summary>
	/// Registers a file type probe using a delegate.
	/// Probes are checked in order of priority (highest first).
	/// </summary>
	/// <param name="probe">The probe delegate that returns a Type if it can handle the file</param>
	/// <param name="priority">Priority of this probe. Higher values are checked first. Default is 0.</param>
	public static void RegisterProbe(FileTypeProbeDelegate probe, int priority = 0)
	{
		lock (_delegateProbes)
		{
			_delegateProbes.Add((probe, priority));
			_delegateProbes.Sort((a, b) => b.priority.CompareTo(a.priority));
		}
	}

	/// <summary>
	/// Unregisters all probes of a specific type.
	/// </summary>
	public static void UnregisterProbe<T>() where T : IFileTypeProbe
	{
		lock (_probes)
		{
			_probes.RemoveAll(p => p.probe is T);
		}
	}

	/// <summary>
	/// Clears all registered probes.
	/// </summary>
	public static void ClearProbes()
	{
		lock (_probes)
		{
			_probes.Clear();
		}
		lock (_delegateProbes)
		{
			_delegateProbes.Clear();
		}
	}

	/// <summary>
	/// Probes a file to detect its type using registered probes.
	/// </summary>
	/// <param name="filePath">The path to the file to probe</param>
	/// <returns>The detected tab type, or null if no probe matched</returns>
	public static Type? ProbeFile(string filePath)
	{
		if (!File.Exists(filePath))
			return null;

		try
		{
			// Read header bytes for probing
			byte[] headerBytes;
			long fileSize;
			string extension = Path.GetExtension(filePath).ToLower();

			using (var stream = File.OpenRead(filePath))
			{
				fileSize = stream.Length;
				int bytesToRead = (int)Math.Min(DefaultHeaderSize, fileSize);
				headerBytes = new byte[bytesToRead];
				stream.Read(headerBytes, 0, bytesToRead);
			}

			var context = new FileProbeContext(filePath, headerBytes, extension, fileSize);

			// Check interface-based probes first
			lock (_probes)
			{
				foreach (var (probe, _) in _probes)
				{
					try
					{
						if (probe.CanHandle(filePath, headerBytes))
						{
							return probe.GetTabType();
						}
					}
					catch
					{
						// Ignore probe errors and continue to next probe
					}
				}
			}

			// Check delegate-based probes
			lock (_delegateProbes)
			{
				foreach (var (probe, _) in _delegateProbes)
				{
					try
					{
						Type? result = probe(context);
						if (result != null)
						{
							return result;
						}
					}
					catch
					{
						// Ignore probe errors and continue to next probe
					}
				}
			}
		}
		catch
		{
			// If probing fails, return null
		}

		return null;
	}
}
