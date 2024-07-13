using SideScroll.Logs;

namespace SideScroll.Serialize.Test;

public class TestSerializeBase : TestBase
{
	protected static string TestPath = Environment.CurrentDirectory;

	protected new void Initialize(string name, LogLevel logLevel = LogLevel.Info)
	{
		base.Initialize(name, logLevel);
	}
}
