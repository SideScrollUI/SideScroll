namespace SideScroll.Core;

public class TestBase
{
	public Call Call { get; set; } = new();

	public virtual void Initialize(string name)
	{
		Call = new Call(name);
		Call.Log.Settings!.DebugPrintLogLevel = LogLevel.Info;
		new LogWriterConsole(Call.Log);
	}
}
