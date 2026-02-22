using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Logs;
using SideScroll.Tasks;
using SideScroll.Utilities;
using System.Runtime.CompilerServices;

namespace SideScroll;

/// <summary>
/// Represents a call context for tracking execution flow, logging, and task management throughout the application
/// </summary>
[Unserialized]
public class Call
{
	/// <summary>
	/// Gets or sets the name of this call context
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the log associated with this call context
	/// </summary>
	public Log Log { get; set; }

	/// <summary>
	/// Gets or sets the task instance for tracking task status and enabling task cancellation
	/// </summary>
	public TaskInstance? TaskInstance { get; set; } // Shows the Task Status and let's you stop them

	public override string? ToString() => Name;

	/// <summary>
	/// Initializes a new instance of the Call class with a new log
	/// </summary>
	protected Call()
	{
		Log = new();
	}

	/// <summary>
	/// Initializes a new instance of the Call class with an optional name
	/// </summary>
	public Call(string? name = null)
	{
		Name = name;
		Log = new();
	}

	/// <summary>
	/// Initializes a new instance of the Call class with an existing log
	/// </summary>
	public Call(Log log)
	{
		Log = log;
	}

	/// <summary>
	/// Creates a child call context that inherits from this call's log and task instance
	/// </summary>
	public Call Child([CallerMemberName] string name = "", params Tag[] tags)
	{
		Call call = new()
		{
			Name = name,
			TaskInstance = TaskInstance,
			Log = Log.AddChild(name, tags),
		};
		return call;
	}

	/// <summary>
	/// Creates a child call with debug-level logging enabled for all log messages
	/// </summary>
	public Call DebugLogAll()
	{
		var child = Child("LogDebug");
		child.Log.Settings = child.Log.Settings!.Clone();
		child.Log.Settings.MinLogLevel = LogLevel.Debug;
		child.Log.Settings.DebugPrintLogLevel = LogLevel.Debug;
		return child;
	}

	/// <summary>
	/// Creates a timer for measuring execution time with Info-level logging
	/// </summary>
	/// <example>
	/// <code>
	/// using (var timer = call.Timer())
	/// {
	///     // Perform work here
	///     // Timer automatically logs elapsed time on dispose
	/// }
	/// </code>
	/// </example>
	public CallTimer Timer([CallerMemberName] string? name = null, params Tag[] tags)
	{
		return Timer(LogLevel.Info, name, tags);
	}

	/// <summary>
	/// Creates a timer for measuring execution time with the specified log level
	/// </summary>
	/// <example>
	/// <code>
	/// using (var timer = call.Timer(LogLevel.Debug, "ProcessData", new Tag("Source", "API")))
	/// {
	///     // Perform work here
	///     // Timer logs at Debug level with custom name and tags
	/// }
	/// </code>
	/// </example>
	public CallTimer Timer(LogLevel logLevel, [CallerMemberName] string? name = null, params Tag[] tags)
	{
		CallTimer call = new()
		{
			Name = name,
		};
		call.TaskInstance = TaskInstance?.AddSubTask(call);
		call.Log = Log.AddChild(logLevel, name ?? "Timer", tags);

		return call;
	}

	/// <summary>
	/// Starts a new task with Info-level logging for tracking progress and cancellation
	/// </summary>
	/// <example>
	/// <code>
	/// using (var task = call.StartTask("DownloadFiles"))
	/// {
	///     // Task progress can be monitored via task.TaskInstance
	///     // Task can be cancelled via task.TaskInstance.CancelToken
	/// }
	/// </code>
	/// </example>
	public CallTimer StartTask([CallerMemberName] string? name = null, params Tag[] tags)
	{
		return StartTask(LogLevel.Info, name, tags);
	}

	/// <summary>
	/// Starts a new task with the specified log level for tracking progress and cancellation
	/// </summary>
	/// <example>
	/// <code>
	/// using (var task = call.StartTask(LogLevel.Warn, "CriticalOperation"))
	/// {
	///     // Perform critical work
	///     // Logs at Warning level for higher visibility
	/// }
	/// </code>
	/// </example>
	public CallTimer StartTask(LogLevel logLevel, [CallerMemberName] string? name = null, params Tag[] tags)
	{
		CallTimer call = new()
		{
			Name = name,
			IsTask = true,
		};
		call.TaskInstance = TaskInstance?.AddSubTask(call) ?? new TaskInstance(name)
		{
			TaskCount = 1,
		};
		call.Log = Log.AddChild(logLevel, name ?? "Task", tags);

		return call;
	}

	/// <summary>
	/// Starts a new task with a specific count for tracking multiple sub-operations
	/// </summary>
	/// <param name="taskCount">The number of sub-tasks to track within this task</param>
	/// <example>
	/// <code>
	/// var items = new[] { "file1.txt", "file2.txt", "file3.txt" };
	/// using (var task = call.StartTask(items.Length, "ProcessFiles"))
	/// {
	///     foreach (var item in items)
	///     {
	///         // Process each item
	///         // Progress is tracked as taskCount/items.Length
	///     }
	/// }
	/// </code>
	/// </example>
	public CallTimer StartTask(int taskCount, [CallerMemberName] string name = "", params Tag[] tags)
	{
		TaskInstance ??= new TaskInstance();
		if (TaskInstance.TaskCount == 0)
		{
			TaskInstance.TaskCount = 1;
		}

		var allTags = tags.ToList();
		allTags.Add(new Tag("Count", taskCount));

		CallTimer timer = StartTask(name, allTags.ToArray());
		timer.TaskInstance!.TaskCount = taskCount;
		return timer;
	}

	/// <summary>
	/// Adds a sub-task to break down progress tracking into multiple tasks
	/// </summary>
	public TaskInstance AddSubTask(string name = "")
	{
		TaskInstance = TaskInstance!.AddSubTask(Child(name));
		return TaskInstance;
	}

	/// <summary>
	/// Adds a sub-call to break down progress tracking into multiple calls
	/// </summary>
	public Call AddSubCall(string name = "")
	{
		return AddSubTask(name).Call;
	}

	private static async Task<TResult?> RunFuncAsync<TItem, TResult>(
		Call call,
		Func<Call, TItem, Task<TResult>> func,
		TItem item)
	{
		using CallTimer callTimer = call.StartTask(item?.ToString());

		try
		{
			TResult result = await func(callTimer, item);
			if (result == null)
			{
				callTimer.Log.Add("No result");
			}
			return result;
		}
		catch (Exception e)
		{
			callTimer.Log.Add(e);
			return default;
		}
	}

	private static async Task<TResult?> RunFuncAsync<TItem, TParam1, TResult>(
		Call call,
		Func<Call, TItem, TParam1, Task<TResult>> func,
		TItem item,
		TParam1 param1)
	{
		using CallTimer callTimer = call.StartTask(item?.ToString());

		try
		{
			TResult result = await func(callTimer, item, param1);
			if (result == null)
			{
				callTimer.Log.Add("No result");
			}
			return result;
		}
		catch (Exception e)
		{
			callTimer.Log.Add(e);
			return default;
		}
	}

	private static async Task<TResult?> RunFuncAsync<TItem, TParam1, TParam2, TResult>(
		Call call,
		Func<Call, TItem, TParam1, TParam2, Task<TResult>> func,
		TItem item,
		TParam1 param1,
		TParam2 param2)
	{
		using CallTimer callTimer = call.StartTask(item?.ToString());

		try
		{
			TResult result = await func(callTimer, item, param1, param2);
			if (result == null)
			{
				callTimer.Log.Add("No result");
			}
			return result;
		}
		catch (Exception e)
		{
			callTimer.Log.Add(e);
			return default;
		}
	}

	/// <summary>
	/// Executes an async function on a collection of items and returns the first non-null result
	/// </summary>
	public async Task<TResult?> FirstNonNullAsync<TItem, TResult>(
		Func<Call, TItem, Task<TResult?>> func,
		ICollection<TItem> items,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		// todo: Migrate FirstOrDefault() deeper
		return (await RunAsync(
			func,
			items,
			maxConcurrentRequests,
			maxRequestsPerSecond)).NonNullValues.FirstOrDefault();
	}

	/// <summary>
	/// Executes an async function with one parameter on a collection of items and returns the first non-null result
	/// </summary>
	public async Task<TResult?> FirstNonNullAsync<TItem, TParam1, TResult>(
		Func<Call, TItem, TParam1, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			func,
			items,
			param1,
			maxConcurrentRequests,
			maxRequestsPerSecond)).NonNullValues.FirstOrDefault();
	}

	/// <summary>
	/// Executes an async function with two parameters on a collection of items and returns the first non-null result
	/// </summary>
	public async Task<TResult?> FirstNonNullAsync<TItem, TParam1, TParam2, TResult>(
		Func<Call, TItem, TParam1, TParam2, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		TParam2 param2,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			func,
			items,
			param1,
			param2,
			maxConcurrentRequests,
			maxRequestsPerSecond)).NonNullValues.FirstOrDefault();
	}

	/// <summary>
	/// Executes an async function on a collection of items and returns all non-null results
	/// </summary>
	public async Task<List<TResult>> SelectNonNullAsync<TItem, TResult>(
		Func<Call, TItem, Task<TResult?>> func,
		ICollection<TItem> items,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			func,
			items,
			maxConcurrentRequests,
			maxRequestsPerSecond)).NonNullValues.ToList();
	}

	/// <summary>
	/// Executes an async function with one parameter on a collection of items and returns all non-null results
	/// </summary>
	public async Task<List<TResult>> SelectNonNullAsync<TItem, TParam1, TResult>(
		Func<Call, TItem, TParam1, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			func,
			items,
			param1,
			maxConcurrentRequests,
			maxRequestsPerSecond)).NonNullValues.ToList();
	}

	/// <summary>
	/// Executes an async function with two parameters on a collection of items and returns all non-null results
	/// </summary>
	public async Task<List<TResult>> SelectNonNullAsync<TItem, TParam1, TParam2, TResult>(
		Func<Call, TItem, TParam1, TParam2, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		TParam2 param2,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			func,
			items,
			param1,
			param2,
			maxConcurrentRequests,
			maxRequestsPerSecond)).NonNullValues.ToList();
	}

	/// <summary>
	/// Executes a named async function on a collection of items and returns all non-null results
	/// </summary>
	public async Task<List<TResult>> SelectNonNullAsync<TItem, TResult>(
		string name,
		Func<Call, TItem, Task<TResult?>> func,
		ICollection<TItem> items,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			name,
			func,
			items,
			maxConcurrentRequests,
			maxRequestsPerSecond)).NonNullValues.ToList();
	}

	/// <summary>
	/// Executes a named async function with one parameter on a collection of items and returns all non-null results
	/// </summary>
	public async Task<List<TResult>> SelectNonNullAsync<TItem, TParam1, TResult>(
		string name,
		Func<Call, TItem, TParam1, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			name,
			func,
			items,
			param1,
			maxConcurrentRequests,
			maxRequestsPerSecond)).NonNullValues.ToList();
	}

	/// <summary>
	/// Executes a named async function with two parameters on a collection of items and returns all non-null results
	/// </summary>
	public async Task<List<TResult>> SelectNonNullAsync<TItem, TParam1, TParam2, TResult>(
		string name,
		Func<Call, TItem, TParam1, TParam2, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		TParam2 param2,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return (await RunAsync(
			name,
			func,
			items,
			param1,
			param2,
			maxConcurrentRequests,
			maxRequestsPerSecond)).NonNullValues.ToList();
	}

	/// <summary>
	/// Executes an async function on a collection of items with concurrency and rate limiting, returning all results including nulls
	/// </summary>
	public async Task<ItemResultCollection<TItem, TResult>> RunAsync<TItem, TResult>(
		Func<Call, TItem, Task<TResult?>> func,
		ICollection<TItem> items,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return await RunAsync(
			func.Method.Name.TrimEnd("Async").WordSpaced(),
			func,
			items,
			maxConcurrentRequests,
			maxRequestsPerSecond);
	}

	/// <summary>
	/// Executes an async function with one parameter on a collection of items with concurrency and rate limiting, returning all results including nulls
	/// </summary>
	public async Task<ItemResultCollection<TItem, TResult>> RunAsync<TItem, TParam1, TResult>(
		Func<Call, TItem, TParam1, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return await RunAsync(
			func.Method.Name.TrimEnd("Async").WordSpaced(),
			func,
			items,
			param1,
			maxConcurrentRequests,
			maxRequestsPerSecond);
	}

	/// <summary>
	/// Executes an async function with two parameters on a collection of items with concurrency and rate limiting, returning all results including nulls
	/// </summary>
	public async Task<ItemResultCollection<TItem, TResult>> RunAsync<TItem, TParam1, TParam2, TResult>(
		Func<Call, TItem, TParam1, TParam2, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		TParam2 param2,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		return await RunAsync(
			func.Method.Name.TrimEnd("Async").WordSpaced(),
			func,
			items,
			param1,
			param2,
			maxConcurrentRequests,
			maxRequestsPerSecond);
	}

	/// <summary>
	/// Executes a named async function on a collection of items with concurrency and rate limiting, returning all results including nulls
	/// </summary>
	public async Task<ItemResultCollection<TItem, TResult>> RunAsync<TItem, TResult>(
		string name,
		Func<Call, TItem, Task<TResult?>> func,
		ICollection<TItem> items,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		using CallTimer callTimer = StartTask(items.Count, name);

		using var rateLimiter = new ConcurrentRateLimiter(maxConcurrentRequests, maxRequestsPerSecond);

		var tasks = new List<Task>();
		var results = new KeyValuePair<TItem, TResult?>[items.Count];

		foreach (var (index, item) in items.WithIndex())
		{
			var limitToken = await rateLimiter.WaitAsync();

			if (TaskInstance?.CancelToken.IsCancellationRequested == true)
			{
				limitToken.Dispose();
				Log.Add("Cancelled");
				break;
			}

			tasks.Add(Task.Run(async () =>
			{
				try
				{
					TResult? result = await RunFuncAsync(callTimer, func, item);
					results[index] = new(item, result);
				}
				catch (Exception e)
				{
					results[index] = new(item, default);
					Log.Add(e);
				}
				finally
				{
					limitToken.Dispose();
				}
			}));
		}
		await Task.WhenAll(tasks);

		return new ItemResultCollection<TItem, TResult>(results);
	}

	/// <summary>
	/// Executes a named async function with one parameter on a collection of items with concurrency and rate limiting, returning all results including nulls
	/// </summary>
	public async Task<ItemResultCollection<TItem, TResult>> RunAsync<TItem, TParam1, TResult>(
		string name,
		Func<Call, TItem, TParam1, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		using CallTimer callTimer = StartTask(items.Count, name);

		using var rateLimiter = new ConcurrentRateLimiter(maxConcurrentRequests, maxRequestsPerSecond);

		var tasks = new List<Task>();
		var results = new KeyValuePair<TItem, TResult?>[items.Count];
		foreach (var (index, item) in items.WithIndex())
		{
			var limitToken = await rateLimiter.WaitAsync();

			if (TaskInstance?.CancelToken.IsCancellationRequested == true)
			{
				limitToken.Dispose();
				Log.Add("Cancelled");
				break;
			}

			tasks.Add(Task.Run(async () =>
			{
				try
				{
					TResult? result = await RunFuncAsync(callTimer, func, item, param1);
					results[index] = new(item, result);
				}
				catch (Exception e)
				{
					results[index] = new(item, default);
					Log.Add(e);
				}
				finally
				{
					limitToken.Dispose();
				}
			}));
		}
		await Task.WhenAll(tasks);

		return new ItemResultCollection<TItem, TResult>(results);
	}

	/// <summary>
	/// Executes a named async function with two parameters on a collection of items with concurrency and rate limiting, returning all results including nulls
	/// </summary>
	public async Task<ItemResultCollection<TItem, TResult>> RunAsync<TItem, TParam1, TParam2, TResult>(
		string name,
		Func<Call, TItem, TParam1, TParam2, Task<TResult?>> func,
		ICollection<TItem> items,
		TParam1 param1,
		TParam2 param2,
		int? maxConcurrentRequests = null,
		int? maxRequestsPerSecond = null)
	{
		using CallTimer callTimer = StartTask(items.Count, name);

		using var rateLimiter = new ConcurrentRateLimiter(maxConcurrentRequests, maxRequestsPerSecond);

		var tasks = new List<Task>();
		var results = new KeyValuePair<TItem, TResult?>[items.Count];
		foreach (var (index, item) in items.WithIndex())
		{
			var limitToken = await rateLimiter.WaitAsync();

			if (TaskInstance?.CancelToken.IsCancellationRequested == true)
			{
				limitToken.Dispose();
				Log.Add("Cancelled");
				break;
			}

			tasks.Add(Task.Run(async () =>
			{
				try
				{
					TResult? result = await RunFuncAsync(callTimer, func, item, param1, param2);
					results[index] = new(item, result);
				}
				catch (Exception e)
				{
					results[index] = new(item, default);
					Log.Add(e);
				}
				finally
				{
					limitToken.Dispose();
				}
			}));
		}
		await Task.WhenAll(tasks);

		return new ItemResultCollection<TItem, TResult>(results);
	}
}

/// <summary>
/// Collection of item-result pairs with helper properties for accessing keys, values, and non-null values
/// </summary>
public class ItemResultCollection<TItem, TResult>(IEnumerable<KeyValuePair<TItem, TResult?>> enumerable) :
	List<KeyValuePair<TItem, TResult?>>(enumerable)
{
	/// <summary>
	/// Gets all input items from the collection
	/// </summary>
	public IEnumerable<TItem> Keys => this.Select(p => p.Key);

	/// <summary>
	/// Gets all result values from the collection, including nulls
	/// </summary>
	public IEnumerable<TResult?> Values => this.Select(p => p.Value);

	/// <summary>
	/// Gets all non-null result values from the collection
	/// </summary>
	public IEnumerable<TResult> NonNullValues => this.Select(p => p.Value).OfType<TResult>();
}
