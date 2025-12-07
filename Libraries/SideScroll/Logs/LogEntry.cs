using SideScroll.Attributes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SideScroll.Logs;

public class LogMessageEventArgs : EventArgs
{
	public List<LogEntry> Entries { get; set; } = []; // First is new log message, last is highest parent log message
}

public class LogSettings
{
	public int MaxLogItems { get; set; } = 10_000;

	public LogLevel MinLogLevel { get; set; } = LogLevel.Info; // Logs below this level won't be added

	public LogLevel DebugPrintLogLevel { get; set; } = LogLevel.Warn;

	internal readonly object Lock = new(); // todo: replace this with individual ones? or a non-blocking version

	[Hidden]
	public SynchronizationContext? Context { get; set; } // inherited from creator (which can be a Parent Log)

	public LogSettings Clone()
	{
		return new LogSettings
		{
			MaxLogItems = MaxLogItems,
			MinLogLevel = MinLogLevel,
			DebugPrintLogLevel = DebugPrintLogLevel,
		};
	}

	public LogSettings WithMinLogLevel(LogLevel minLogLevel)
	{
		LogSettings clone = Clone();
		clone.MinLogLevel = minLogLevel;
		return clone;
	}

	/*protected void InitializeContext()
	{
		Context ??= SynchronizationContext.Current ?? new SynchronizationContext();
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
	[Hidden]
	public LogSettings? Settings { get; set; }

	[Hidden]
	public LogEntry RootLog { get; set; }

	public event PropertyChangedEventHandler? PropertyChanged;

	[HiddenColumn]
	public DateTime Created { get; set; }

	public TimeSpan Time => Created.Subtract(RootLog.Created);

	[HiddenColumn]
	public LogLevel OriginalLevel { get; set; } = LogLevel.Info;
	public LogLevel Level { get; set; } = LogLevel.Info;

	[Hidden]
	public string? Text { get; set; }

	[WordWrap, MinWidth(300)]
	public string? Message
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

	public int Entries => _entries;
	protected int _entries;

	[HideRow(null)]
	public TimeSpan? Duration
	{
		get => _duration;
		set
		{
			_duration = value;
			NotifyPropertyChanged();
		}
	}
	private TimeSpan? _duration;

	private string TagText => Tags == null ? "" : string.Join<Tag>(' ', Tags);

	[HiddenColumn]
	public Tag[]? Tags { get; set; }

	public override string ToString() => Message ?? Level.ToString();

	public LogEntry()
	{
		RootLog = this;

		// Don't initialize for faster deserializing?
	}

	public LogEntry(LogSettings? logSettings, LogLevel logLevel, string text, Tag[]? tags)
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
		Settings ??= new LogSettings();
	}

	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		Settings?.Context?.Post(NotifyPropertyChangedContext, propertyName);
	}

	private void NotifyPropertyChangedContext(object? state)
	{
		string propertyName = (string)state!;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
