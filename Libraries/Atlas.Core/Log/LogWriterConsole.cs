using System;
using System.Threading;

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

	private void LogEntry_OnMessage(object sender, EventLogMessage e)
	{
		string Indendation = "";
		for (int i = 1; i < e.Entries.Count; i++)
			Indendation += '\t';

		LogEntry newLog = e.Entries[0];
		//string line = log.Created.ToString("yyyy-MM-dd HH:mm:ss") + Indendation + log.ToString();

		Console.WriteLine(Indendation + newLog.Message);
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
