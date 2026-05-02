namespace SideScroll.Logs;

/// <summary>
/// Subscribes to a <see cref="Log"/> and writes each log entry as a timestamped line to a text file.
/// </summary>
public class LogWriterText : IDisposable
{
	/// <summary>Gets the log that is being written to the file.</summary>
	public Log Log { get; }

	/// <summary>Gets the path of the output text file.</summary>
	public string SaveFilePath { get; }

	private readonly StreamWriter _textStreamWriter;
	private readonly SynchronizationContext _context;
	private bool _disposed;

	public override string ToString() => SaveFilePath;

	/// <summary>Initializes a new instance, creates the output file, and subscribes to log events.</summary>
	public LogWriterText(Log log, string saveFilePath)
	{
		Log = log;
		SaveFilePath = saveFilePath;

		string parentDirectory = Path.GetDirectoryName(SaveFilePath)!;
		if (!Directory.Exists(parentDirectory))
		{
			Directory.CreateDirectory(parentDirectory);
		}

		_textStreamWriter = new StreamWriter(SaveFilePath);

		_context = SynchronizationContext.Current ?? new();

		log.OnMessage += Log_OnMessage;
	}

	private void Log_OnMessage(object? sender, LogMessageEventArgs e)
	{
		string indentation = new('\t', e.Entries.Count);

		LogEntry newLog = e.Entries[0];
		string line = Log.Created.ToString("yyyy-M-d H:mm:ss") + indentation + newLog.Message;
		_textStreamWriter.WriteLine(line);
		_textStreamWriter.Flush();
	}

	/// <summary>Releases managed resources when <paramref name="disposing"/> is <c>true</c>.</summary>
	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			// Dispose managed resources
			// Unsubscribe from event
			Log.OnMessage -= Log_OnMessage;

			// Close and dispose writer
			_textStreamWriter.Close();
		}

		_disposed = true;
	}

	/// <summary>Disposes the writer, unsubscribes from log events, and closes the output file.</summary>
	public virtual void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
