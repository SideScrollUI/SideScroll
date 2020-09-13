using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Core
{
	public class Tag
	{
		public string Name { get; set; }
		[InnerValue]
		public object Value { get; set; }

		public override string ToString() => "[ " + Name + " = " + Value.Formatted() + " ]";

		public Tag()
		{
		}

		public Tag(object value)
		{
			Name = value.ToString();
			Value = value;
		}

		public Tag(string name, object value, bool verbose = true)
		{
			Name = name;

			if (verbose)
				Value = value;
			else
				Value = value.ToString();
		}

		public static Tag Add(object value, bool verbose = true)
		{
			return new Tag(value.ToString(), value, verbose);
		}
	}

	public class EventLogMessage : EventArgs
	{
		public List<LogEntry> Entries = new List<LogEntry>(); // 1st is new log message, last is highest parent log message
	}

	[Skippable(false)]
	public class LogEntry : INotifyPropertyChanged
	{
		[HiddenRow]
		public LogEntry RootLog;
		public enum LogType
		{
			Debug,
			Info,
			Warn,
			Error,
			Alert
		}
		public event PropertyChangedEventHandler PropertyChanged;
		[HiddenColumn]
		public DateTime Created { get; set; }
		public TimeSpan Time => Created.Subtract(RootLog.Created);
		public LogType OriginalType = LogType.Info;
		public LogType Type { get; set; } = LogType.Info;
		[HiddenColumn, HiddenRow]
		public string Text { get; set; }
		[WordWrap, MinWidth(300)]
		public string Message
		{
			get
			{
				if (Tags == null)
					return Text;
				string tagText = TagText;
				if (tagText == "")
					return Text;
				return Text + " " + tagText;
			}
		}

		[HiddenColumn, HiddenRow]
		public virtual string Summary => Text;
		protected int _entries;
		public int Entries => _entries;

		private float? _duration;
		public float? Duration
		{
			get
			{
				return _duration;
			}
			set
			{
				_duration = value;
				CreateEventPropertyChanged();
			}
		}

		//[AttributeName("Tags")]
		//[HiddenColumn]
		private string TagText
		{
			get
			{
				string line = "";
				if (Tags == null)
					return line;

				foreach (Tag tag in Tags)
				{
					line += tag.ToString() + " ";
				}
				return line;
			}
		}
		[HiddenColumn]
		public Tag[] Tags { get; set; }
		[HiddenRow]
		public SynchronizationContext Context; // inherited from creator (which can be a Parent Log)

		public override string ToString() => Message;

		public LogEntry()
		{
			RootLog = this;
		}

		public LogEntry(LogType logType, string text, Tag[] tags)
		{
			RootLog = this;
			OriginalType = logType;
			Type = logType;
			Text = text;
			Tags = tags;
			Created = DateTime.Now;
		}

		private void InitializeContext()
		{
			Context = Context ?? SynchronizationContext.Current ?? new SynchronizationContext();
		}

		protected void CreateEventPropertyChanged([CallerMemberName] string propertyName = "")
		{
			Context?.Post(new SendOrPostCallback(NotifyPropertyChangedContext), propertyName);
			//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void NotifyPropertyChangedContext(object state)
		{
			string propertyName = state as string;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			//PropertyChanged?.BeginInvoke(this, new PropertyChangedEventArgs(propertyName), EndAsyncEvent, null);
		}
	}

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
