using SideScroll.Attributes;
using SideScroll.Logs;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SideScroll.Tasks;

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
	public Log Log => Call.Log;

	[HiddenColumn]
	public bool ShowTask { get; set; }

	public Task? Task { get; set; }
	public TaskStatus TaskStatus => Task?.Status ?? TaskStatus.Created;

	public CancellationTokenSource TokenSource = new();
	public CancellationToken CancelToken => TokenSource.Token;

	public string Status { get; set; } = "Running";
	public string? Message { get; set; }

	public bool Errored { get; set; }
	public bool Finished { get; set; }

	public TaskInstance? ParentTask { get; set; }
	public List<TaskInstance> SubTasks { get; set; } = [];

	private int? _taskCount;
	public int TaskCount
	{
		get
		{
			if (_taskCount != null)
				return _taskCount.Value;
			else
				return SubTasks.Count;
		}
		set
		{
			_taskCount = value;
			ProgressMax = 100 * value;
		}
	}

	public DateTime StartTime { get; set; } = DateTime.UtcNow;

	public DateTime? EndTime { get; set; }

	public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;

	private readonly Stopwatch _stopwatch = new();

	public override string? ToString() => Label;

	public TaskInstance()
	{
		Call.TaskInstance = this;

		_stopwatch.Start();
	}

	public TaskInstance(string? label)
	{
		Label = label;

		Call.TaskInstance = this;

		_stopwatch.Start();
	}

	public TaskInstance(Call call, TaskInstance parentTask)
	{
		Label = call.Name;
		Call = call;
		Creator = parentTask.Creator;
		TokenSource = parentTask.TokenSource;
		ParentTask = parentTask;
		
		if (parentTask.ProgressMax > 0)
		{
			_progressMax = 100;
		}
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
			NotifyPropertyChanged();
		}
	}
	private double _percent;
	public TimeSpan Elapsed => DateTime.UtcNow - StartTime;
	public TimeSpan? ETA => Percent > 0.0 ? (Elapsed * 100.0 / Percent) - Elapsed : null;

	public double Progress
	{
		get => _progress;
		set
		{
			if (_progress == value)
				return;

			_progress = Math.Min(value, ProgressMax);
			if (Math.Abs(_lastNotifiedProgress - _progress) > 0.1)
			{
				_lastNotifiedProgress = _progress;
				NotifyPropertyChanged();
			}

			UpdatePercent();

			if (ParentTask != null)
			{
				ParentTask.AddProgress(Percent - _prevPercent);
				_prevPercent = Percent;
			}
		}
	}
	private double _progress;
	private double _prevPercent;
	private double _lastNotifiedProgress;

	[Formatted]
	public double ProgressMax
	{
		get => _progressMax;
		set
		{
			if (_progressMax == value)
				return;

			_progressMax = value;
			NotifyPropertyChanged();

			UpdatePercent();
		}
	}
	private double _progressMax;

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
			if (Duration - _lastNotifiedDuration >= TimeSpan.FromSeconds(1))
			{
				_lastNotifiedDuration = Duration;
				NotifyPropertyChanged(nameof(Duration));
			}
		}
	}
	private TimeSpan _lastNotifiedDuration;

	public bool CancelVisible => !Finished;

	[ButtonColumn("-", nameof(CancelVisible))]
	public void Cancel()
	{
		TokenSource.Cancel();
	}

	// allows having progress broken down into multiple tasks
	public TaskInstance AddSubTask(Call call)
	{
		var subTask = new TaskInstance(call, this);

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
		{
			Creator.Context!.Post(OnFinished, null);
		}
		else
		{
			OnFinished(null);
		}
	}

	private void OnFinished(object? state)
	{
		/*var eventCompleted = new EventCompleted();
		eventCompleted.taskCheckFileSize = this;
		OnComplete?.Invoke(this, eventCompleted);*/
		Finished = true;
		EndTime = DateTime.UtcNow;
		NotifyPropertyChanged(nameof(Finished));
		NotifyPropertyChanged(nameof(Duration));

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
				{
					Status = Call.Log.Level.ToString();
				}
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
				Message = Log.Text; // todo: First log entry with highest log level?
			}
		}

		NotifyPropertyChanged(nameof(Status));
		NotifyPropertyChanged(nameof(TaskStatus));
		NotifyPropertyChanged(nameof(Finished));
		NotifyPropertyChanged(nameof(CancelVisible));

		Call.Log.Add("Finished", new Tag("Time", _stopwatch.ElapsedMilliseconds / 1000.0));

		if (ParentTask == null)
		{
			Creator?.OnComplete?.Invoke();
		}
		OnComplete?.Invoke();
	}

	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		//Debug.WriteLine(propertyName);
		if (Creator != null)
		{
			Creator.Context!.Post(NotifyPropertyChangedContext, propertyName);
		}
		else
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	private void NotifyPropertyChangedContext(object? state)
	{
		string propertyName = (string)state!;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
