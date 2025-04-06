using SideScroll.Logs;

namespace SideScroll.Serialize.Test;

public class SerializeBaseTest : BaseTest
{
	protected static string TestPath = Environment.CurrentDirectory;

	// Log level can be raised for performance testing
	protected new void Initialize(string name, LogLevel logLevel = LogLevel.Info)
	{
		base.Initialize(name, logLevel);
	}
}
