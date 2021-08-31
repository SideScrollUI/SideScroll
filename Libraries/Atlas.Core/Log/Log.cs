using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Core
{
	[Skippable(false)]
	public class Log : LogEntry
	{
		[InnerValue]
		public ItemCollection<LogEntry> Items { get; set; } = new ItemCollection<LogEntry>(); // change to LRU for performance? No Binding?
	
		private string SummaryText;

		[HiddenColumn]
		public override string Summary => SummaryText;

		public event EventHandler<EventLogMessage> OnMessage;

		public Log()
		{
			Initialize();
		}

		public Log(string text = null, LogSettings logSettings = null, Tag[] tags = null)
		{
			Text = text;
			Settings = logSettings;
			Tags = tags;

			Initialize();
		}

		// use caller instead?
		public Log Call(string name, params Tag[] tags)
		{
			return AddChildEntry(LogLevel.Info, name, tags);
		}

		public Log Call(LogLevel logLevel, string name, params Tag[] tags)
		{
			return AddChildEntry(logLevel, name, tags);
		}

		public LogEntry Add(string text, params Tag[] tags)
		{
			return Add(LogLevel.Info, text, tags);
		}

		public LogEntry AddDebug(string text, params Tag[] tags)
		{
			return Add(LogLevel.Debug, text, tags);
		}

		public LogEntry AddWarning(string text, params Tag[] tags)
		{
			return Add(LogLevel.Warn, text, tags);
		}

		public LogEntry AddError(string text, params Tag[] tags)
		{
			return Add(LogLevel.Error, text, tags);
		}

		public LogEntry Add(LogLevel logLevel, string text, params Tag[] tags)
		{
			if (logLevel < Settings.MinLogLevel)
				return null;

			var logEntry = new LogEntry(Settings, logLevel, text, tags);
			AddLogEntry(logEntry);
			return logEntry;
		}

		public LogEntry Add(Exception e)
		{
			Debug.Print("Exception: " + e.Message);

			if (e is TaskCanceledException)
			{
				return Add(e.Message, new Tag(e));
			}
			else if (e is AggregateException ae)
			{
				LogEntry logEntry = null;
				foreach (Exception ex in ae.InnerExceptions)
				{
					if (ex is TaskCanceledException)
						logEntry = Add(ex.Message, new Tag(ex));
					else
						logEntry = AddError(ex.Message, new Tag(ex));
				}
				return logEntry;
			}
			else
			{
				return AddError(e.Message, new Tag(e));
			}
		}

		public LogTimer Timer(string text, params Tag[] tags)
		{
			var logTimer = new LogTimer(text, Settings, tags);
			AddLogEntry(logTimer);
			return logTimer;
		}

		public LogTimer TimerDebug(string text, params Tag[] tags)
		{
			var logTimer = new LogTimer(LogLevel.Debug, text, Settings, tags);
			AddLogEntry(logTimer);
			return logTimer;
		}

		public string EntriesText()
		{
			var stringBuilder = new StringBuilder();
			foreach (LogEntry logEntry in Items)
			{
				stringBuilder.AppendLine(logEntry.ToString());
			}
			return stringBuilder.ToString();
		}

		private Log AddChildEntry(LogLevel logLevel, string name, params Tag[] tags)
		{
			Log log = new Log(name, Settings, tags)
			{
				OriginalLevel = logLevel,
				Level = logLevel,
			};
			AddLogEntry(log);
			return log;
		}

		public void AddLogEntry(LogEntry logEntry)
		{
			// LogTimer calls this once for a new child message, and once for adding to parent log
			// So only add it for the initial child message
			if (logEntry.Level >= Settings.DebugPrintLogLevel && logEntry.Entries == 0)
			{
				Debug.Print(logEntry.Level + ": " + logEntry.ToString());
			}

			logEntry.RootLog = RootLog;
			logEntry.Settings = Settings;

			if (Settings.Context != null)
				Settings.Context.Post(new SendOrPostCallback(AddEntryCallback), logEntry);
			else
				AddEntryCallback(logEntry);
		}

		// Thread safe callback, only works if the context is the same
		private void AddEntryCallback(object state)
		{
			lock (Settings.Lock)
			{
				AddEntry((LogEntry)state);
			}
		}

		private void AddEntry(LogEntry logEntry)
		{
			Items.Add(logEntry);
			if (Items.Count > Settings.MaxLogItems)
			{
				// subtract entries or leave them?
				Items.RemoveAt(0);
				//UpdateStats();
			}
			else
			{
				UpdateStats(logEntry);
			}

			CreateEventLogMessage(logEntry);

			// Update if there can be child entries
			if (logEntry is Log log)
				log.OnMessage += ChildLog_OnMessage;
		}

		// Update stats when a new child log entry gets added at any level below
		private void UpdateStats(LogEntry logEntry)
		{
			Interlocked.Add(ref _entries, logEntry.Entries + 1);
			if (logEntry.Level > Level)
			{
				Level = logEntry.Level;
				CreateEventPropertyChanged(nameof(Level));
			}
			CreateEventPropertyChanged(nameof(Entries));
		}

		private void CreateEventLogMessage(LogEntry logEntry)
		{
			var eventLogMessage = new EventLogMessage();
			eventLogMessage.Entries.Add(logEntry);
			eventLogMessage.Entries.Add(this);
			OnMessage?.Invoke(this, eventLogMessage);
		}

		private void ChildLog_OnMessage(object sender, EventLogMessage eventLogMessage)
		{
			UpdateStats(eventLogMessage.Entries[0]);
			eventLogMessage.Entries.Add(this);
			OnMessage?.Invoke(this, eventLogMessage);
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

	Special interface for outside threads

	Object Creator Logs / Constructor Log
		History of Object level creation
	Method Caller Logs / Method Log
		Map Clicks

	How to make only one function call with both of them?


Options

	Data has to get copied
		lock it always (too many locks)
		serialize it

	CallContext.LogicalSetData("time", DateTime.Now);
		Loses Messages, Allows multiple writes?
	
	delegates

		No unsafe code can be accessed within the anonymous-method-block. 

	Receive unsafe, lock, serialize, unlock, deserialize

	Serialize, queue


	public class LogStackFrame
	{
		public int line;
		public string file;
		public string method;
	}


	StackTrace stackTrace = new StackTrace();
	for (int i = 1; i<stackTrace.FrameCount; i++)
	{
		StackFrame frame = stackTrace.GetFrame(i);
		LogStackFrame logFrame = new LogStackFrame();
		logFrame.line = frame.GetFileLineNumber();
		logFrame.file = frame.GetFileName();
		logFrame.method = frame.GetMethod().ToString();
		entry.stacktrace.Add(logFrame);
	}
*/
