using Atlas.Core;

namespace Atlas.Serialize.Test;

public class TestSerializeBase : TestBase
{
	public string TestPath = Environment.CurrentDirectory;

	public new void Initialize(string name)
	{
		base.Initialize(name);
	}
}
