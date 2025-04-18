using SideScroll.Logs;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;

namespace SideScroll.Console;

public class Console
{
	private readonly Call _call;
	private readonly LogWriterConsole _logWriterConsole;
	private readonly LogWriterText _logWriterText;

	static void Main(string[] args)
	{
		var console = new Console();
	}

	public Console()
	{
		// setup
		var project = Project.Load(Settings);
		_call = new Call(GetType().Name);
		_logWriterConsole = new LogWriterConsole(_call.Log);
		_logWriterText = new LogWriterText(_call.Log, project.Data.App.GetGroupPath(typeof(Console)) + "/Logs/Main");

		//TestLogWriter();
	}

	public static ProjectSettings Settings => new()
	{
		Name = "SideScroll",
		LinkType = "sidescroll",
		Version = new Version(1, 0),
		DataVersion = new Version(1, 0),
	};

	void TestLogWriter()
	{
		_call.Log.Add("test");
	}
}
/*
This should probably get turned into a library if it ever gets used
*/
