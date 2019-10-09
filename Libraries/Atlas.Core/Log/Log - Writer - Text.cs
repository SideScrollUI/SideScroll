using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Atlas.Core
{
	public class LogWriterText : IDisposable
	{
		public string saveFilePath;
		private Log log;

		private StreamWriter txtStreamWriter;
		private SynchronizationContext context;

		public LogWriterText(Log log, string saveFilePath)
		{
			this.log = log;
			this.saveFilePath = saveFilePath + ".log.txt";

			string parentDirectory = Path.GetDirectoryName(this.saveFilePath);
			if (!Directory.Exists(parentDirectory))
				Directory.CreateDirectory(parentDirectory);
			
			txtStreamWriter = new StreamWriter(this.saveFilePath);
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
			string line = log.Created.ToString("yyyy-M-d H:mm:ss") + Indendation + newLog.Message;
			txtStreamWriter.WriteLine(line);
			txtStreamWriter.Flush();
		}

		public virtual void Dispose()
		{
			txtStreamWriter.Close();
		}

		public override string ToString()
		{
			return saveFilePath;
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