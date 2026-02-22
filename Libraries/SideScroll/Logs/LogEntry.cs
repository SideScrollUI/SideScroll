using SideScroll.Attributes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SideScroll.Logs;

/// <summary>
/// Event arguments containing log entries when a log message is raised
/// </summary>
public class LogMessageEventArgs : EventArgs
{
	/// <summary>
	/// List of log entries, where the first is the new log message and the last is the highest parent log message
	/// </summary>
	public List<LogEntry> Entries { get; set; } = []; // First is new log message, last is highest parent log message
}

/// <summary>
/// Configuration settings for log behavior
/// </summary>
public class LogSettings
{
	/// <summary>
	/// Maximum number of log items to retain
	/// </summary>
	public int MaxLogItems { get; set; } = 10_000;

	/// <summary>
	/// Minimum log level to add to the log. Logs below this level won't be added
	/// </summary>
	public LogLevel MinLogLevel { get; set; } = LogLevel.Info; // Logs below this level won't be added

	/// <summary>
	/// Minimum log level to output to debug print
	/// </summary>
	public LogLevel DebugPrintLogLevel { get; set; } = LogLevel.Warn;

	internal readonly object Lock = new(); // todo: replace this with individual ones? or a non-blocking version

	[Hidden]
	public SynchronizationContext? Context { get; set; } // inherited from creator (which can be a Parent Log)

	/// <summary>
	/// Creates a copy of the log settings
	/// </summary>
	public LogSettings Clone()
	{
		return new LogSettings
		{
			MaxLogItems = MaxLogItems,
			MinLogLevel = MinLogLevel,
			DebugPrintLogLevel = DebugPrintLogLevel,
		};
	}

	/// <summary>
	/// Creates a copy of the log settings with a new minimum log level
	/// </summary>
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

/// <summary>
/// Severity level of a log entry
/// </summary>
public enum LogLevel
{
	Debug,
	Info,
	Warn,
	Error,
	Alert
}

/// <summary>
/// Represents a single log entry with message, level, and timing information
/// </summary>
[Skippable(false)]
public class LogEntry : INotifyPropertyChanged
{
	/// <summary>
	/// Log settings that control behavior
	/// </summary>
	[Hidden]
	public LogSettings? Settings { get; set; }

	/// <summary>
	/// The root log that this entry belongs to
	/// </summary>
	[Hidden]
	public LogEntry RootLog { get; set; }

	/// <summary>
	/// Event raised when a property value changes
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Timestamp when the log entry was created
	/// </summary>
	[HiddenColumn]
	public DateTime Created { get; set; }

	/// <summary>
	/// Time elapsed since the root log was created
	/// </summary>
	public TimeSpan Time => Created.Subtract(RootLog.Created);

	/// <summary>
	/// The original log level before any updates
	/// </summary>
	[HiddenColumn]
	public LogLevel OriginalLevel { get; set; } = LogLevel.Info;

	/// <summary>
	/// Current log level, may be updated based on child entries
	/// </summary>
	public LogLevel Level { get; set; } = LogLevel.Info;

	/// <summary>
	/// The raw log message text without tags
	/// </summary>
	[Hidden]
	public string? Text { get; set; }

	/// <summary>
	/// The complete log message including text and tags
	/// </summary>
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

	/// <summary>
	/// Total number of entries in this log
	/// </summary>
	public int Entries => _entries;
	protected int _entries;

	/// <summary>
	/// Duration for timed log entries
	/// </summary>
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

	/// <summary>
	/// Additional metadata tags attached to the log entry
	/// </summary>
	[HiddenColumn]
	public Tag[]? Tags { get; set; }

	public override string ToString() => Message ?? Level.ToString();

	/// <summary>
	/// Creates a new log entry with default values
	/// </summary>
	public LogEntry()
	{
		RootLog = this;

		// Don't initialize for faster deserializing?
	}

	/// <summary>
	/// Creates a new log entry with the specified settings, level, text, and tags
	/// </summary>
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

	/// <summary>
	/// Initializes the log entry with current timestamp and default settings
	/// </summary>
	protected void Initialize()
	{
		Created = DateTime.Now;
		Settings ??= new LogSettings();
	}

	/// <summary>
	/// Raises the PropertyChanged event for the specified property
	/// </summary>
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
