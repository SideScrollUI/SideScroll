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

		public string Label => Creator?.Label; // used for Task Label
		public TaskCreator Creator { get; set; }
		[HiddenColumn]
		public Call call { get; set; } = new Call();

		[InnerValue, HiddenColumn]
		public Log Log => call.log;

		[HiddenColumn]
		public bool ShowTask { get; set; }

		public Task Task { get; set; }
		public TaskStatus TaskStatus => (Task?.Status ?? TaskStatus.Created);
		public CancellationTokenSource tokenSource = new CancellationTokenSource();
		public CancellationToken CancelToken => tokenSource.Token;

		public string Status { get; set; } = "Running";
		public string Message { get; set; }

		public long ProgressMax { get; set; }

		public bool Errored { get; set; }
		public bool Finished { get; set; }

		public TaskInstance ParentTask { get; set; }
		public List<TaskInstance> SubTasks { get; set; } = new List<TaskInstance>();
		private int? _NumSubTasks;
		public int NumSubTasks
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
			}
		}

		private Stopwatch stopwatch = new Stopwatch();

		public override string ToString() => Label;

		public TaskInstance()
		{
			call.taskInstance = this;
			stopwatch.Start();
		}

		private int _Percent;
		public int Percent
		{
			get { return _Percent; }
			set
			{
				if (_Percent == value)
					return;
				
				_Percent = value;
				NotifyPropertyChanged(nameof(Percent));
				if (ParentTask != null)
					ParentTask.UpdatePercent();
			}
		}

		private long _Progress;
		public long Progress
		{
			get { return _Progress; }
			set
			{
				if (_Progress == value)
					return;

				_Progress = value;
				NotifyPropertyChanged(nameof(Progress));
				UpdatePercent();
			}
		}

		private void UpdatePercent()
		{
			if (NumSubTasks > 0)
			{
				int totalPercent = 0;
				lock (SubTasks)
				{
					foreach (TaskInstance subTask in SubTasks)
						totalPercent += subTask.Percent;
				}
				Percent = totalPercent / NumSubTasks;
			}
			else if (ProgressMax > 0)
			{
				Percent = (int)(100 * _Progress / ProgressMax);
			}
		}

		public void Cancel()
		{
			tokenSource.Cancel();
		}

		// allows having progress broken down into multiple tasks
		public TaskInstance AddSubTask(Call call)
		{
			TaskInstance subTask = new TaskInstance();
			subTask.Creator = Creator;
			subTask.call = call;
			subTask.tokenSource = tokenSource;
			subTask.ParentTask = this;
			
			lock (SubTasks)
			{
				SubTasks.Add(subTask);
			}

			return subTask;
		}

		public void SetFinished()
		{
			stopwatch.Stop(); // Both Send and Post adds some delay
			Creator?.context.Post(new SendOrPostCallback(OnFinished), null);
		}

		private void OnFinished(object state)
		{
			/*EventCompleted eventCompleted = new EventCompleted();
			eventCompleted.taskCheckFileSize = this;
			OnComplete?.Invoke(this, eventCompleted);*/
			Finished = true;
			Percent = 100; // calls NotifyPropertyChanged

			if (call.log.Type >= LogEntry.LogType.Error)
			{
				Status = call.log.Type.ToString();
				Errored = true;
				ShowTask = true;
			}
			else if (call.log.Type == LogEntry.LogType.Warn)
			{
				if (!Errored)
					Status = call.log.Type.ToString();
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
			call.log.Add("Finished", new Tag("Time", stopwatch.ElapsedMilliseconds / 1000.0));
			Creator?.OnComplete?.Invoke();
			OnComplete?.Invoke();
		}

		protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			Creator?.context.Post(new SendOrPostCallback(this.NotifyPropertyChangedContext), propertyName);
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
