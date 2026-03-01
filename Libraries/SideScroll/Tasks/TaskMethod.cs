using System.Reflection;

namespace SideScroll.Tasks;

/// <summary>
/// A task creator that wraps a method from an object using reflection for dynamic task execution
/// </summary>
public class TaskMethod : TaskCreator
{
	/// <summary>
	/// Gets the method info to invoke
	/// </summary>
	public MethodInfo MethodInfo { get; }

	/// <summary>
	/// Gets the target object to invoke the method on
	/// </summary>
	public object Object { get; }

	public override string ToString() => MethodInfo.Name;

	/// <summary>
	/// Initializes a new task method with the specified method info and target object
	/// </summary>
	public TaskMethod(MethodInfo methodInfo, object obj)
	{
		MethodInfo = methodInfo;
		Object = obj;

		Label = methodInfo.Name;
	}

	/// <summary>
	/// Creates an action that will invoke the method on the target object
	/// </summary>
	public override Action CreateAction(Call call)
	{
		return () => RunMethod(call);
	}

	private void RunMethod(Call call)
	{
		MethodInfo.Invoke(Object, [call]);
	}
}
