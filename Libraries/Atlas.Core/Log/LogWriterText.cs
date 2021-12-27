using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Atlas.Core
{
	public class LogWriterText : IDisposable
	{
		private readonly Log Log;
		public string SaveFilePath;

		private readonly StreamWriter _textStreamWriter;
		private readonly SynchronizationContext _context;

		public override string ToString() => SaveFilePath;

		public LogWriterText(Log log, string saveFilePath)
		{
			Log = log;
			SaveFilePath = saveFilePath + ".log.txt";

			string parentDirectory = Path.GetDirectoryName(SaveFilePath);
			if (!Directory.Exists(parentDirectory))
				Directory.CreateDirectory(parentDirectory);

			_textStreamWriter = new StreamWriter(SaveFilePath);

			_context = SynchronizationContext.Current ?? new SynchronizationContext();

			log.OnMessage += LogEntry_OnMessage;
		}

		private void LogEntry_OnMessage(object sender, EventLogMessage e)
		{
			string Indendation = "";
			foreach (LogEntry logEntry in e.Entries)
				Indendation += '\t';

			LogEntry newLog = e.Entries[0];
			string line = Log.Created.ToString("yyyy-M-d H:mm:ss") + Indendation + newLog.Message;
			_textStreamWriter.WriteLine(line);
			_textStreamWriter.Flush();
		}

		public virtual void Dispose()
		{
			_textStreamWriter.Close();
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
