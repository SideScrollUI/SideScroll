namespace SideScroll;

/// <summary>
/// Interface for objects that can be asynchronously loaded. TabViewer will display the loaded object instead of the original object.
/// </summary>
public interface ILoadAsync
{
	/// <summary>
	/// Loads the object asynchronously
	/// </summary>
	/// <param name="call">The call context for logging and cancellation</param>
	/// <returns>The loaded object, or null if loading failed</returns>
	Task<object?> LoadAsync(Call call);
}
