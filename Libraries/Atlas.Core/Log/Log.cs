using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Atlas.Core
{
	public class Tag
	{
		public string Name { get; set; }
		[InnerValue]
		public object Value { get; set; }

		public Tag()
		{
		}

		// only one of these works at a time for strings due to ambiguous calls
		/*public Tag(object value, bool verbose = true, [CallerMemberName] string callerMemberName = "")
		{
			this.Name = callerMemberName;
			if (verbose)
				this.Value = value;
			else
				this.Value = value.ToString();
		}*/

		public Tag(object value)
		{
			this.Name = value.ToString();
			this.Value = value;
		}

		public Tag(string name, object value, bool verbose = true)
		{
			this.Name = name;
			if (verbose)
				this.Value = value;
			else
				this.Value = value.ToString();
		}

		public static Tag Add(object value, bool verbose = true)
		{
			Tag tag = new Tag()
			{
				Name = value.ToString(),
			};
			if (verbose)
				tag.Value = value;
			else
				tag.Value = value.ToString();
			return tag;
		}

		public override string ToString()
		{
			return "[ " + Name + " = " + Value.ObjectToString() + " ]";
		}
	}

	public class EventLogMessage : EventArgs
	{
		public List<LogEntry> Entries = new List<LogEntry>();
	}

	[Skippable(false)]
	public class LogEntry : INotifyPropertyChanged
	{
		public enum LogType
		{
			Debug,
			Info,
			Warn,
			Error,
			Alert
		}
		public event PropertyChangedEventHandler PropertyChanged;
		public DateTime Created; // { get; set; }
		public LogType originalType = LogType.Info;
		public LogType Type { get; set; } = LogType.Info;
		public string Text;// { get; set; }
		public string Message
		{
			get
			{
				if (tags == null)
					return Text;
				string tagText = TagText;
				if (tagText == "")
					return Text;
				return Text + " " + tagText;
			}
		}

		[HiddenColumn]
		public virtual string Summary => Text;
		public int Entries { get; set; }

		private float? _Duration;
		public float? Duration
		{
			get
			{
				return _Duration;
			}
			set
			{
				_Duration = value;
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
				if (tags == null)
					return line;

				foreach (Tag tag in tags)
				{
					line += tag.ToString() + " ";
				}
				return line;
			}
		}
		public Tag[] tags;

		public LogEntry()
		{
		}

		public LogEntry(LogType logType, string text, Tag[] tags)
		{
			this.originalType = logType;
			this.Type = logType;
			this.Text = text;
			this.tags = tags;
			this.Created = DateTime.Now;
		}

		public override string ToString()
		{
			return Message;
		}

		protected void CreateEventPropertyChanged([CallerMemberName] String propertyName = "")
		{
			//context.Post(new SendOrPostCallback(this.NotifyPropertyChangedContext), propertyName);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	[Skippable(false)]
	public class Log : LogEntry
	{
		private const int MaxLogItems = 10000;

		[InnerValue]
		public ItemCollection<LogEntry> items = new ItemCollection<LogEntry>(); // change to LRU for performance? No Binding?
		private SynchronizationContext context; // inherited from creator (which can be a Parent Log)
		private int contextRandomId;
		private static object locker = new object(); // todo: replace this with individual ones? (deadlock territory if circular) or a non-blocking version
		private string SummaryText;

		[HiddenColumn]
		public override string Summary => SummaryText;

		public event EventHandler<EventLogMessage> OnMessage;

		public Log()
		{
			this.Created = DateTime.Now;
			InitializeContext();
		}

		public Log(string text = null, SynchronizationContext context = null, Tag[] tags = null)
		{
			this.context = context;
			this.Text = text;
			this.tags = tags;
			this.Created = DateTime.Now;

			InitializeContext();
		}

		private void InitializeContext()
		{
			if (this.context == null)
			{
				this.context = SynchronizationContext.Current;
				if (this.context == null)
				{
					contextRandomId = new Random().Next();
					//throw new Exception("Don't do this");
					this.context = new SynchronizationContext();
				}
			}
		}

		// use caller instead?
		public Log Call(string name, params Tag[] tags)
		{
			return AddChildEntry(LogType.Info, name, tags);
		}

		public LogEntry Add(string text, params Tag[] tags)
		{
			LogEntry logEntry = new LogEntry(LogType.Info, text, tags);
			AddLogEntry(logEntry);
			return logEntry;
		}

		public LogEntry AddWarning(string text, params Tag[] tags)
		{
			LogEntry logEntry = new LogEntry(LogType.Warn, text, tags);
			AddLogEntry(logEntry);
			return logEntry;
		}

		public LogEntry AddError(string text, params Tag[] tags)
		{
			LogEntry logEntry = new LogEntry(LogType.Error, text, tags);
			AddLogEntry(logEntry);
			return logEntry;
		}

		public LogTimer Timer(string text, params Tag[] tags)
		{
			LogTimer logTimer = new LogTimer(text, context);
			logTimer.contextRandomId = contextRandomId;
			logTimer.tags = tags;
			AddLogEntry(logTimer);
			return logTimer;
		}

		public string EntriesText()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (LogEntry logEntry in items)
			{
				stringBuilder.AppendLine(logEntry.ToString());
			}
			return stringBuilder.ToString();
		}

		private Log AddChildEntry(LogType logType, string name, params Tag[] tags)
		{
			Log log = new Log(name, context, tags);
			log.contextRandomId = contextRandomId;
			log.originalType = logType;
			log.Type = logType;
			AddLogEntry(log);
			return log;
		}

		public void AddLogEntry(LogEntry logEntry)
		{
			context.Post(new SendOrPostCallback(this.AddEntryCallback), logEntry);
		}

		// Thread safe callback, only works if the context is the same
		private void AddEntryCallback(object state)
		{
			lock (locker)
			{
				AddEntry((LogEntry)state);
			}
		}

		private void AddEntry(LogEntry logEntry)
		{
			items.Add(logEntry);
			if (items.Count > MaxLogItems)
			{
				items.RemoveAt(0);
				UpdateEntries();
			}
			else
			{
				Entries += logEntry.Entries + 1;
				if (logEntry.Type > Type)
					Type = logEntry.Type;
			}

			CreateEventPropertyChanged(nameof(Entries));
			CreateEventLogMessage(logEntry);

			// Update if there can be child entries
			Log log = logEntry as Log;
			if (log != null)
				log.OnMessage += ChildLog_OnMessage;
		}

		private void UpdateEntries()
		{
			int count = 0;
			Type = originalType;
			foreach (LogEntry logEntry in items)
			{
				count++;
				count += logEntry.Entries;
				if (logEntry.Type > Type)
				{
					Type = logEntry.Type;
					SummaryText = logEntry.Summary;
				}
			}
			Entries = count;
			CreateEventPropertyChanged(nameof(Entries));
		}

		private void CreateEventLogMessage(LogEntry logEntry)
		{
			EventLogMessage eventLogMessage = new EventLogMessage();
			eventLogMessage.Entries.Add(logEntry);
			eventLogMessage.Entries.Add(this);
			OnMessage?.Invoke(this, eventLogMessage);
		}

		private void ChildLog_OnMessage(object sender, EventLogMessage eventLogMessage)
		{
			UpdateEntries();
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
