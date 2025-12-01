using SideScroll.Logs;

namespace SideScroll;

public class BaseTest
{
	protected Call Call { get; set; } = new();

	public virtual void Initialize(string name, LogLevel logLevel = LogLevel.Info)
	{
		Call = new Call(name);
		Call.Log.Settings!.DebugPrintLogLevel = logLevel;
		new LogWriterConsole(Call.Log);
	}
}
