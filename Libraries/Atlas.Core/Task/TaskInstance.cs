using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Atlas.Core;

public class TaskInstance : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;
	//public event EventHandler<EventArgs> OnComplete;

	public Action? OnComplete;

	private string? _label;
	public string? Label
	{
		get => _label ?? Creator?.Label;
		set => _label = value;
	}
	public TaskCreator? Creator { get; set; }

	[HiddenColumn]
	public Call Call { get; set; } = new();

	[InnerValue, HiddenColumn]
	public Log? Log => Call.Log;

	[HiddenColumn]
	public bool ShowTask { get; set; }

	public Task? Task { get; set; }
	public TaskStatus TaskStatus => Task?.Status ?? TaskStatus.Created;

	public CancellationTokenSource TokenSource = new();
	public CancellationToken CancelToken => TokenSource.Token;

	public string Status { get; set; } = "Running";
	public string? Message { get; set; }

	public long ProgressMax { get; set; } = 0;

	public bool Errored { get; set; }
	public bool Finished { get; set; }

	public TaskInstance? ParentTask { get; set; }
	public List<TaskInstance> SubTasks { get; set; } = new();

	private int? _taskCount;
	public int TaskCount
	{
		get
		{
			if (_taskCount != null)
				return (int)_taskCount;
			else
				return (SubTasks != null) ? SubTasks.Count : 0;
		}
		set
		{
			_taskCount = value;
			ProgressMax = 100 * value;
		}
	}

	public DateTime Started = DateTime.UtcNow;

	private readonly Stopwatch _stopwatch = new();

	public override string? ToString() => Label;

	public TaskInstance()
	{
		Initialize();
	}

	public TaskInstance(string label)
	{
		Label = label;

		Initialize();
	}

	private void Initialize()
	{
		Call.TaskInstance = this;

		_stopwatch.Start();
	}

	[Formatted]
	public double Percent
	{
		get => _percent;
		set
		{
			if (_percent == value)
				return;

			_percent = value;
			NotifyPropertyChanged(nameof(Percent));
		}
	}
	private double _percent;

	private double _prevPercent;
	private double _progress;
	public double Progress
	{
		get => _progress;
		set
		{
			if (_progress == value)
				return;

			_progress = Math.Min(value, ProgressMax);
			NotifyPropertyChanged(nameof(Progress));

			UpdatePercent();

			if (ParentTask != null)
			{
				ParentTask.AddProgress(Percent - _prevPercent);
				_prevPercent = Percent;
			}
		}
	}

	private void AddProgress(double amount)
	{
		if (amount <= 0)
			return;

		lock (SubTasks)
		{
			Progress += amount;
		}
	}

	private void UpdatePercent()
	{
		/*if (NumSubTasks > 0)
		{
			int totalPercent = 0;
			lock (SubTasks)
			{
				foreach (TaskInstance subTask in SubTasks)
					totalPercent += subTask.Percent;
			}
			Percent = totalPercent / NumSubTasks;
		}
		else*/
		if (ProgressMax > 0)
		{
			Percent = 100 * _progress / ProgressMax;
			NotifyPropertyChanged(nameof(Percent));
		}
	}

	public bool CancelVisible => !Finished;

	[ButtonColumn("-", nameof(CancelVisible))]
	public void Cancel()
	{
		TokenSource.Cancel();
	}

	// allows having progress broken down into multiple tasks
	public TaskInstance AddSubTask(Call call)
	{
		var subTask = new TaskInstance()
		{
			Label = call.Name,
			Creator = Creator,
			Call = call,
			TokenSource = TokenSource,
			ParentTask = this,
		};
		if (ProgressMax > 0)
			subTask.ProgressMax = 100;

		lock (SubTasks)
		{
			SubTasks.Add(subTask);
		}

		return subTask;
	}

	public void SetFinished()
	{
		if (Finished)
			return;

		_stopwatch.Stop(); // Both Send and Post adds some delay

		if (Creator != null)
			Creator.Context!.Post(OnFinished, null);
		else
			OnFinished(null);
	}

	private void OnFinished(object? state)
	{
		/*var eventCompleted = new EventCompleted();
		eventCompleted.taskCheckFileSize = this;
		OnComplete?.Invoke(this, eventCompleted);*/
		Finished = true;

		if (Call.TaskInstance!.CancelToken.IsCancellationRequested)
		{
			Status = "Cancelled";
		}
		else
		{
			Progress = ProgressMax;

			if (Call.Log.Level >= LogLevel.Error)
			{
				Status = Call.Log.Level.ToString();
				Errored = true;
				ShowTask = true;
			}
			else if (Call.Log.Level == LogLevel.Warn)
			{
				if (!Errored)
					Status = Call.Log.Level.ToString();
				ShowTask = true;
			}
			else if (Task == null || TaskStatus == TaskStatus.RanToCompletion)
			{
				Status = "Complete";
				Message ??= "Success";
			}
			else
			{
				Status = TaskStatus.ToString();
				Message = Log!.Text; // todo: First log entry with highest log level?
			}
		}

		NotifyPropertyChanged(nameof(Status));
		NotifyPropertyChanged(nameof(TaskStatus));
		NotifyPropertyChanged(nameof(Finished));
		NotifyPropertyChanged(nameof(CancelVisible));

		Call.Log.Add("Finished", new Tag("Time", _stopwatch.ElapsedMilliseconds / 1000.0));

		if (ParentTask == null)
			Creator?.OnComplete?.Invoke();
		OnComplete?.Invoke();
	}

	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		Creator?.Context!.Post(NotifyPropertyChangedContext, propertyName);
	}

	private void NotifyPropertyChangedContext(object? state)
	{
		string propertyName = (string)state!;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
