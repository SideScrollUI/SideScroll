namespace SideScroll.Logs;

public class LogWriterConsole
{
	public Log Log { get; init; }

	public SynchronizationContext Context { get; set; }

	public override string ToString() => "Console";

	public LogWriterConsole(Log log)
	{
		Log = log;

		Context = SynchronizationContext.Current ?? new();

		log.OnMessage += LogEntry_OnMessage;
	}

	private static void LogEntry_OnMessage(object? sender, LogMessageEventArgs e)
	{
		string indentation = new('\t', e.Entries.Count);

		LogEntry newLog = e.Entries[0];
		//string line = log.Created.ToString("yyyy-MM-dd HH:mm:ss") + indentation + log.ToString();

		Console.WriteLine(indentation + newLog.Message);
	}
}
