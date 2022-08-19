namespace Atlas.Core;

public class LogWriterConsole
{
	public Log Log;

	public SynchronizationContext Context;

	public override string ToString() => "Console";

	public LogWriterConsole(Log log)
	{
		Log = log;

		Context = SynchronizationContext.Current ?? new SynchronizationContext();

		log.OnMessage += LogEntry_OnMessage;
	}

	private void LogEntry_OnMessage(object? sender, EventLogMessage e)
	{
		string indentation = "";
		for (int i = 1; i < e.Entries.Count; i++)
			indentation += '\t';

		LogEntry newLog = e.Entries[0];
		//string line = log.Created.ToString("yyyy-MM-dd HH:mm:ss") + indentation + log.ToString();

		Console.WriteLine(indentation + newLog.Message);
	}
}

/*
Requirements

	1 per line?

	Parent/Child relationship

	Separate files?
	
	Tags?

	Human readable?
*/
