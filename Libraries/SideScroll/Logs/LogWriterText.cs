namespace SideScroll.Logs;

public class LogWriterText : IDisposable
{
	public Log Log { get; }
	public string SaveFilePath { get; }

	private readonly StreamWriter _textStreamWriter;
	private readonly SynchronizationContext _context;

	public override string ToString() => SaveFilePath;

	public LogWriterText(Log log, string saveFilePath)
	{
		Log = log;
		SaveFilePath = saveFilePath + ".log.txt";

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

	public virtual void Dispose()
	{
		_textStreamWriter.Close();
	}
}
