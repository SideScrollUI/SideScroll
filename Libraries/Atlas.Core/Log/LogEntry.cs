using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Atlas.Core
{
	public class EventLogMessage : EventArgs
	{
		public List<LogEntry> Entries = new List<LogEntry>(); // 1st is new log message, last is highest parent log message
	}

	public class LogSettings
	{
		public int MaxLogItems = 10000;

		public LogLevel DebugPrintLogLevel { get; set; } = LogLevel.Warn;

		internal object Lock = new object(); // todo: replace this with individual ones? (deadlock territory if circular) or a non-blocking version

		[HiddenRow]
		public SynchronizationContext Context; // inherited from creator (which can be a Parent Log)

		/*protected void InitializeContext()
		{
			Context = Context ?? SynchronizationContext.Current ?? new SynchronizationContext();
		}*/
	}

	public enum LogLevel
	{
		Debug,
		Info,
		Warn,
		Error,
		Alert
	}

	[Skippable(false)]
	public class LogEntry : INotifyPropertyChanged
	{
		public LogSettings Settings { get; set; }

		[HiddenRow]
		public LogEntry RootLog;

		public event PropertyChangedEventHandler PropertyChanged;

		[HiddenColumn]
		public DateTime Created { get; set; }

		public TimeSpan Time => Created.Subtract(RootLog.Created);

		public LogLevel OriginalLevel = LogLevel.Info;
		public LogLevel Level { get; set; } = LogLevel.Info;

		[Hidden]
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

		[Hidden]
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

		public override string ToString() => Message;

		public LogEntry()
		{
			RootLog = this;

			// Don't initialize for faster deserializing?
		}

		public LogEntry(LogSettings logSettings, LogLevel logLevel, string text, Tag[] tags)
		{
			Settings = logSettings;
			RootLog = this;
			OriginalLevel = logLevel;
			Level = logLevel;
			Text = text;
			Tags = tags;

			Initialize();
		}

		protected void Initialize()
		{
			Created = DateTime.Now;
			Settings = Settings ?? new LogSettings();
		}

		protected void CreateEventPropertyChanged([CallerMemberName] string propertyName = "")
		{
			Settings.Context?.Post(new SendOrPostCallback(NotifyPropertyChangedContext), propertyName);
			//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void NotifyPropertyChangedContext(object state)
		{
			string propertyName = state as string;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			//PropertyChanged?.BeginInvoke(this, new PropertyChangedEventArgs(propertyName), EndAsyncEvent, null);
		}
	}
}
