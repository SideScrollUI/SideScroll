using SideScroll.Logs;

namespace SideScroll;

/// <summary>
/// Base class for unit tests with logging and call tracking support
/// </summary>
public class BaseTest
{
	/// <summary>
	/// Gets or sets the Call context for the test
	/// </summary>
	protected Call Call { get; set; } = new();

	/// <summary>
	/// Initializes the test with a call context and logging configuration
	/// </summary>
	/// <param name="name">The name of the test</param>
	/// <param name="logLevel">The minimum log level to print to debug output (default is Info)</param>
	public virtual void Initialize(string name, LogLevel logLevel = LogLevel.Info)
	{
		Call = new Call(name);
		Call.Log.Settings!.DebugPrintLogLevel = logLevel;
		new LogWriterConsole(Call.Log);
	}
}
