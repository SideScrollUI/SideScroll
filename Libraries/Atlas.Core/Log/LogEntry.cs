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

		protected void InitializeContext()
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
}
