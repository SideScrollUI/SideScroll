using System.Reflection;

namespace SideScroll.Tasks;

public class TaskMethod : TaskCreator
{
	public MethodInfo MethodInfo { get; set; }
	public object Object { get; set; } // object to invoke method for

	public override string ToString() => MethodInfo.Name;

	public TaskMethod(MethodInfo methodInfo, object obj)
	{
		MethodInfo = methodInfo;
		Object = obj;

		Label = methodInfo.Name;
	}

	protected override Action CreateAction(Call call)
	{
		return () => RunMethod(call);
	}

	private void RunMethod(Call call)
	{
		MethodInfo.Invoke(Object, new object[] { call });
	}
}
