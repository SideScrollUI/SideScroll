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
		private const int MaxLogItems = 10000;

		[InnerValue]
		public ItemCollection<LogEntry> Items { get; set; } = new ItemCollection<LogEntry>(); // change to LRU for performance? No Binding?
		private static object _locker = new object(); // todo: replace this with individual ones? (deadlock territory if circular) or a non-blocking version
		private string SummaryText;

		[HiddenColumn]
		public override string Summary => SummaryText;

		public event EventHandler<EventLogMessage> OnMessage;

		public Log()
		{
			Created = DateTime.Now;
		}

		public Log(string text = null, SynchronizationContext context = null, Tag[] tags = null)
		{
			Context = context;
			Text = text;
			Tags = tags;
			Created = DateTime.Now;
		}

		// use caller instead?
		public Log Call(string name, params Tag[] tags)
		{
			return AddChildEntry(LogType.Info, name, tags);
		}

		public LogEntry Add(string text, params Tag[] tags)
		{
			return Add(LogType.Info, text, tags);
		}

		public LogEntry AddWarning(string text, params Tag[] tags)
		{
			return Add(LogType.Warn, text, tags);
		}

		public LogEntry AddError(string text, params Tag[] tags)
		{
			Debug.Print("Error: " + text);

			return Add(LogType.Error, text, tags);
		}

		public LogEntry Add(LogType logType, string text, params Tag[] tags)
		{
			var logEntry = new LogEntry(logType, text, tags);
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
			var logTimer = new LogTimer(text, Context)
			{
				Tags = tags,
			};
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

		private Log AddChildEntry(LogType logType, string name, params Tag[] tags)
		{
			Log log = new Log(name, Context, tags)
			{
				OriginalType = logType,
				Type = logType,
			};
			AddLogEntry(log);
			return log;
		}

		public void AddLogEntry(LogEntry logEntry)
		{
			if (logEntry.Type >= LogType.Warn)
				Debug.Print(logEntry.ToString());

			logEntry.RootLog = RootLog;
			logEntry.Context = Context;
			if (Context != null)
				Context.Post(new SendOrPostCallback(AddEntryCallback), logEntry);
			else
				AddEntryCallback(logEntry);
		}

		// Thread safe callback, only works if the context is the same
		private void AddEntryCallback(object state)
		{
			lock (_locker)
			{
				AddEntry((LogEntry)state);
			}
		}

		private void AddEntry(LogEntry logEntry)
		{
			Items.Add(logEntry);
			if (Items.Count > MaxLogItems)
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
			if (logEntry.Type > Type)
			{
				Type = logEntry.Type;
				CreateEventPropertyChanged(nameof(Type));
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
