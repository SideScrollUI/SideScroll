using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Atlas.Core
{
	public class LogWriterConsole
	{
		private Log log;
		
		private SynchronizationContext context;

		public LogWriterConsole(Log log)
		{
			this.log = log;
			
			context = SynchronizationContext.Current;
			context = context ?? new SynchronizationContext();
			
			log.OnMessage += LogEntry_OnMessage;
		}

		private void LogEntry_OnMessage(object sender, EventLogMessage e)
		{
			string Indendation = "";
			foreach (LogEntry logEntry in e.Entries)
				Indendation += '\t';
			LogEntry newLog = e.Entries[0];
			//string line = log.Created.ToString("yyyy-MM-dd HH:mm:ss") + Indendation + log.ToString();

			Console.WriteLine(Indendation + newLog.Message);
		}

		public override string ToString()
		{
			return "Console";
		}
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