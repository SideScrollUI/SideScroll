using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Core
{
	public class TaskInstance : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		//public event EventHandler<EventArgs> OnComplete;
		public Action OnComplete;

		private string _Label;
		public string Label
		{ 
			get => _Label ?? Creator?.Label;
			set => _Label = value;
		}
		public TaskCreator Creator { get; set; }
		[HiddenColumn]
		public Call Call { get; set; } = new Call();

		[InnerValue, HiddenColumn]
		public Log Log => Call.Log;

		[HiddenColumn]
		public bool ShowTask { get; set; }

		public Task Task { get; set; }
		public TaskStatus TaskStatus => (Task?.Status ?? TaskStatus.Created);
		public CancellationTokenSource TokenSource = new CancellationTokenSource();
		public CancellationToken CancelToken => TokenSource.Token;

		public string Status { get; set; } = "Running";
		public string Message { get; set; }

		public long ProgressMax { get; set; } = 0;

		public bool Errored { get; set; }
		public bool Finished { get; set; }

		public TaskInstance ParentTask { get; set; }
		public List<TaskInstance> SubTasks { get; set; } = new List<TaskInstance>();
		private int? _NumSubTasks;
		public int TaskCount
		{
			get
			{
				if (_NumSubTasks != null)
					return (int)_NumSubTasks;
				else
					return (SubTasks != null) ? SubTasks.Count : 0;
			}
			set
			{
				_NumSubTasks = value;
				ProgressMax = 100 * value;
			}
		}

		private Stopwatch stopwatch = new Stopwatch();

		public override string ToString() => Label;

		public TaskInstance()
		{
			Call.TaskInstance = this;
			stopwatch.Start();
		}

		private int _Percent;
		public int Percent
		{
			get => _Percent;
			set
			{
				if (_Percent == value)
					return;
				
				_Percent = value;
				NotifyPropertyChanged(nameof(Percent));
			}
		}

		private long prevPercent;
		private long _Progress;
		public long Progress
		{
			get => _Progress;
			set
			{
				if (_Progress == value)
					return;

				_Progress = Math.Min(value, ProgressMax);
				NotifyPropertyChanged(nameof(Progress));
				UpdatePercent();
				if (ParentTask != null)
				{
					ParentTask.AddProgress(Percent - prevPercent);
					prevPercent = Percent;
				}
			}
		}

		private void AddProgress(long amount)
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
				Percent = (int)(100 * _Progress / ProgressMax);
				NotifyPropertyChanged(nameof(Percent));
			}
		}

		[ButtonColumn("-")]
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
			stopwatch.Stop(); // Both Send and Post adds some delay
			Creator?.context.Post(new SendOrPostCallback(OnFinished), null);
		}

		private void OnFinished(object state)
		{
			/*EventCompleted eventCompleted = new EventCompleted();
			eventCompleted.taskCheckFileSize = this;
			OnComplete?.Invoke(this, eventCompleted);*/
			Finished = true;
			Progress = ProgressMax;

			if (Call.TaskInstance.CancelToken.IsCancellationRequested)
			{
				Status = "Cancelled";
			}
			else if (Call.Log.Type >= LogEntry.LogType.Error)
			{
				Status = Call.Log.Type.ToString();
				Errored = true;
				ShowTask = true;
			}
			else if (Call.Log.Type == LogEntry.LogType.Warn)
			{
				if (!Errored)
					Status = Call.Log.Type.ToString();
				ShowTask = true;
			}
			else if (Task == null || TaskStatus == TaskStatus.RanToCompletion)
			{
				Status = "Complete";
				Message = Message ?? "Success";
			}
			else
			{
				Status = TaskStatus.ToString();
				Message = Log.Summary;
			}
			NotifyPropertyChanged(nameof(Status));
			NotifyPropertyChanged(nameof(TaskStatus));
			NotifyPropertyChanged(nameof(Finished));
			Call.Log.Add("Finished", new Tag("Time", stopwatch.ElapsedMilliseconds / 1000.0));
			Creator?.OnComplete?.Invoke();
			OnComplete?.Invoke();
		}

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			Creator?.context.Post(new SendOrPostCallback(NotifyPropertyChangedContext), propertyName);
		}

		private void NotifyPropertyChangedContext(object state)
		{
			string propertyName = state as string;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

/*
Merge this into Call?

	Call already has AddSubTask
*/
