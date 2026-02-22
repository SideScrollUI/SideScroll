using SideScroll.Attributes;
using SideScroll.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace SideScroll.Logs;

/// <summary>
/// A hierarchical log that can contain child log entries
/// </summary>
[Skippable(false)]
public class Log : LogEntry
{
	/// <summary>
	/// Collection of child log entries
	/// </summary>
	[InnerValue]
	public ItemCollection<LogEntry> Items { get; set; } = []; // change to LRU for performance? No Binding?

	/// <summary>
	/// Event raised when a log message is added to this log or any child log
	/// </summary>
	public event EventHandler<LogMessageEventArgs>? OnMessage;

	/// <summary>
	/// Creates a new log with default settings
	/// </summary>
	public Log()
	{
		Initialize();
	}

	/// <summary>
	/// Creates a new log with the specified text, settings, and tags
	/// </summary>
	public Log(string? text = null, LogSettings? logSettings = null, Tag[]? tags = null)
	{
		Text = text;
		Settings = logSettings;
		Tags = tags;

		Initialize();
	}

	/// <summary>
	/// Adds a log entry with Info level
	/// </summary>
	public LogEntry? Add(string text, params Tag[] tags)
	{
		return Add(LogLevel.Info, text, tags);
	}

	/// <summary>
	/// Adds a log entry with Debug level
	/// </summary>
	public LogEntry? AddDebug(string text, params Tag[] tags)
	{
		return Add(LogLevel.Debug, text, tags);
	}

	/// <summary>
	/// Adds a log entry with Warning level
	/// </summary>
	public LogEntry? AddWarning(string text, params Tag[] tags)
	{
		return Add(LogLevel.Warn, text, tags);
	}

	/// <summary>
	/// Adds a log entry with Error level
	/// </summary>
	public LogEntry? AddError(string text, params Tag[] tags)
	{
		return Add(LogLevel.Error, text, tags);
	}

	/// <summary>
	/// Adds a log entry with the specified log level
	/// </summary>
	public LogEntry? Add(LogLevel logLevel, string text, params Tag[] tags)
	{
		if (logLevel < Settings!.MinLogLevel)
			return null;

		var logEntry = new LogEntry(Settings, logLevel, text, tags);
		AddLogEntry(logEntry);
		return logEntry;
	}

	/// <summary>
	/// Adds an exception to the log with appropriate level based on exception type
	/// </summary>
	public LogEntry? Add(Exception e, params Tag[] tags)
	{
		Debug.Print("Exception: " + e.Message);

		var allTags = tags.ToList();
		if (e is TaggedException taggedException)
		{
			allTags.AddRange(taggedException.Tags);
		}
		allTags.Add(new Tag("Exception", e));

		if (e is TaskCanceledException)
		{
			return Add(e.Message, [.. allTags]);
		}
		else if (e is AggregateException ae)
		{
			LogEntry? logEntry = null;
			foreach (Exception ex in ae.InnerExceptions)
			{
				if (ex is TaskCanceledException)
				{
					logEntry = Add(ex.Message, [.. allTags]);
				}
				else
				{
					logEntry = AddError(ex.Message, [.. allTags]);
				}
			}
			return logEntry;
		}
		else
		{
			return AddError(e.Message, [.. allTags]);
		}
	}

	/// <summary>
	/// Creates a child log entry with Info level
	/// </summary>
	public Log AddChild(string name, params Tag[] tags)
	{
		return AddChild(LogLevel.Info, name, tags);
	}

	/// <summary>
	/// Creates a child log entry with the specified log level
	/// </summary>
	public Log AddChild(LogLevel logLevel, string text, params Tag[] tags)
	{
		Log log = new(text, Settings, tags)
		{
			OriginalLevel = logLevel,
			Level = logLevel,
		};
		AddLogEntry(log);
		return log;
	}

	/// <summary>
	/// Logs an exception and then re-throws it
	/// </summary>
	public void Throw(Exception e)
	{
		Add(e);
		throw e;
	}

	/// <summary>
	/// Logs a message as an exception and throws a TaggedException
	/// </summary>
	public void Throw(string text, params Tag[] tags)
	{
		Throw(new TaggedException(text, tags));
	}

	/// <summary>
	/// Logs a message as an exception and throws the specified exception type. 
	/// Attempts to find a constructor that accepts a string message parameter.
	/// </summary>
	/// <typeparam name="T">The exception type to throw</typeparam>
	public void Throw<T>(string text, params Tag[] tags) where T : Exception
	{
		ConstructorInfo[] constructors = typeof(T).GetConstructors();
		foreach (ConstructorInfo constructor in constructors)
		{
			ParameterInfo[] parameters = constructor.GetParameters();
			if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
			{
				var logEntry = new LogEntry(Settings, LogLevel.Error, text, tags);
				T exception = (T)constructor.Invoke([logEntry.ToString() ?? text]);
				Throw(exception);
			}
		}

		Throw(text, tags);
	}

	/// <summary>
	/// Creates a timing log entry that tracks duration
	/// </summary>
	public LogTimer Timer(string text, params Tag[] tags)
	{
		var logTimer = new LogTimer(text, Settings, tags);
		AddLogEntry(logTimer);
		return logTimer;
	}

	/// <summary>
	/// Creates a timing log entry with Debug level
	/// </summary>
	public LogTimer TimerDebug(string text, params Tag[] tags)
	{
		var logTimer = new LogTimer(LogLevel.Debug, text, Settings, tags);
		AddLogEntry(logTimer);
		return logTimer;
	}

	/// <summary>
	/// Returns all log entries as formatted text
	/// </summary>
	public string EntriesText()
	{
		StringBuilder stringBuilder = new();
		foreach (LogEntry logEntry in Items)
		{
			stringBuilder.AppendLine(logEntry.ToString());
		}
		return stringBuilder.ToString();
	}

	/// <summary>
	/// Adds a log entry to this log in a thread-safe manner
	/// </summary>
	public void AddLogEntry(LogEntry logEntry)
	{
		// LogTimer calls this once for a new child message, and once for adding to parent log
		// So only add it for the initial child message
		if (logEntry.Level >= Settings!.DebugPrintLogLevel && logEntry.Entries == 0)
		{
			Debug.Print(logEntry.Level + ": " + logEntry);
		}
		if (logEntry.Level < Settings.MinLogLevel)
			return;

		logEntry.RootLog = RootLog;
		logEntry.Settings = Settings;

		if (Settings.Context != null)
		{
			Settings.Context.Post(AddEntryCallback, logEntry);
		}
		else
		{
			AddEntryCallback(logEntry);
		}
	}

	// Thread safe callback, only works if the context is the same
	private void AddEntryCallback(object? state)
	{
		lock (Settings!.Lock)
		{
			AddEntry((LogEntry)state!);
		}
	}

	private void AddEntry(LogEntry logEntry)
	{
		Items.Add(logEntry);
		if (Items.Count > Settings!.MaxLogItems)
		{
			// subtract entries or leave them?
			Items.RemoveAt(0);
			//UpdateStats();
		}
		else
		{
			UpdateStats(logEntry);
		}

		NotifyLogMessage(logEntry);

		// Update if there can be child entries
		if (logEntry is Log log)
		{
			log.OnMessage += ChildLog_OnMessage;
		}
	}

	// Update stats when a new child log entry gets added at any level below
	private void UpdateStats(LogEntry logEntry)
	{
		Interlocked.Add(ref _entries, logEntry.Entries + 1);

		if (logEntry.Level > Level)
		{
			Level = logEntry.Level;
			NotifyPropertyChanged(nameof(Level));
		}

		NotifyPropertyChanged(nameof(Entries));
	}

	private void NotifyLogMessage(LogEntry logEntry)
	{
		LogMessageEventArgs eventArgs = new()
		{
			Entries = [logEntry, this],
		};
		OnMessage?.Invoke(this, eventArgs);
	}

	private void ChildLog_OnMessage(object? sender, LogMessageEventArgs e)
	{
		UpdateStats(e.Entries[0]);
		e.Entries.Add(this);
		OnMessage?.Invoke(this, e);
	}

	/// <summary>
	/// Updates the minimum log level for this log
	/// </summary>
	public void SetLogLevel(LogLevel logLevel)
	{
		Settings = Settings?.WithMinLogLevel(logLevel);
	}
}

/*
	public class LogStackFrame
	{
		public int line;
		public string file;
		public string method;
	}

	StackTrace stackTrace = new StackTrace();
	for (int i = 1; i < stackTrace.FrameCount; i++)
	{
		StackFrame frame = stackTrace.GetFrame(i);
		LogStackFrame logFrame = new LogStackFrame();
		logFrame.line = frame.GetFileLineNumber();
		logFrame.file = frame.GetFileName();
		logFrame.method = frame.GetMethod().ToString();
		entry.stacktrace.Add(logFrame);
	}
*/
