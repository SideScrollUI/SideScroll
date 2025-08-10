using SideScroll.Attributes;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;
using System.Diagnostics;

namespace SideScroll.Tabs.Samples.Chart;

public class TabProcessMonitor : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonReset { get; set; } = new("Reset", Icons.Svg.Reset);

		[Separator]
		public ToolButton ButtonStop { get; set; } = new("Stop", Icons.Svg.Stop);
	}

	public class Instance : TabInstance
	{
		private ItemQueueCollection<CpuSample> _cpuUsage = [];
		private ItemQueueCollection<MemorySample> _memoryUsage = [];

		private Process? _process;
		private Timer? _timer;

		// Previous Processor Values so we can calculate a delta
		private DateTime? _prevSampleTime;
		private TimeSpan? _prevPrivilegedProcessorTime;
		private TimeSpan? _prevTotalProcessorTime;
		private TimeSpan? _prevUserProcessorTime;

		public override void Load(Call call, TabModel model)
		{
			_process = Process.GetCurrentProcess();

			_cpuUsage = [];
			_memoryUsage = [];
			_prevSampleTime = null;

			AddSample();

			Toolbar toolbar = new();
			toolbar.ButtonReset.Action = Reset;
			toolbar.ButtonStop.Action = StopTask;
			model.AddObject(toolbar);

			ChartView cpuChart = new("CPU Usage")
			{
				ShowNowTime = false,
			};
			cpuChart.AddSeries("Privileged", _cpuUsage, nameof(CpuSample.TimeStamp), nameof(CpuSample.PrivilegedUsage), SeriesType.Average);
			cpuChart.AddSeries("Total", _cpuUsage, nameof(CpuSample.TimeStamp), nameof(CpuSample.TotalUsage), SeriesType.Average);
			cpuChart.AddSeries("User", _cpuUsage, nameof(CpuSample.TimeStamp), nameof(CpuSample.UserUsage), SeriesType.Average);
			model.AddObject(cpuChart);

			ChartView memoryChart = new("Memory Usage")
			{
				ShowNowTime = false,
			};
			memoryChart.AddSeries("Working Set", _memoryUsage, nameof(MemorySample.TimeStamp), nameof(MemorySample.WorkingSet), SeriesType.Average);
			memoryChart.AddSeries("Private", _memoryUsage, nameof(MemorySample.TimeStamp), nameof(MemorySample.PrivateBytes), SeriesType.Average);
			memoryChart.AddSeries("Managed", _memoryUsage, nameof(MemorySample.TimeStamp), nameof(MemorySample.ManagedMemory), SeriesType.Average);
			model.AddObject(memoryChart);

			_timer?.Dispose();
			_timer = new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
		}

		private void StopTask(Call call)
		{
			_timer?.Dispose();
			_timer = null;
		}

		private void Reset(Call call)
		{
			Reload();
		}

		private void Callback(object? state)
		{
			Invoke(AddSample);
		}

		private void AddSample()
		{
			AddCpuSample();
			AddMemorySample();
		}

		private void AddCpuSample()
		{
			DateTime sampleTime = DateTime.Now;
			TimeSpan privilegedProcessorTime = _process!.PrivilegedProcessorTime;
			TimeSpan totalProcessorTime = _process.TotalProcessorTime;
			TimeSpan userProcessorTime = _process.UserProcessorTime;

			if (_prevSampleTime != null)
			{
				TimeSpan duration = sampleTime.Subtract(_prevSampleTime.Value);

				CpuSample cpuSample = new()
				{
					TimeStamp = sampleTime,
					PrivilegedUsage = privilegedProcessorTime.Subtract(_prevPrivilegedProcessorTime!.Value)!.TotalSeconds / duration.TotalSeconds,
					TotalUsage = totalProcessorTime.Subtract(_prevTotalProcessorTime!.Value)!.TotalSeconds / duration.TotalSeconds,
					UserUsage = userProcessorTime.Subtract(_prevUserProcessorTime!.Value)!.TotalSeconds / duration.TotalSeconds,
				};
				_cpuUsage.Add(cpuSample);
			}

			_prevSampleTime = sampleTime;
			_prevPrivilegedProcessorTime = privilegedProcessorTime;
			_prevTotalProcessorTime = totalProcessorTime;
			_prevUserProcessorTime = userProcessorTime;
		}

		private void AddMemorySample()
		{
			DateTime sampleTime = DateTime.Now;
			MemorySample sample = new()
			{
				TimeStamp = sampleTime,
				WorkingSet = _process!.WorkingSet64,         // Physical memory in bytes
				PrivateBytes = _process.PrivateMemorySize64, // Private memory in bytes
				ManagedMemory = GC.GetTotalMemory(false),    // Managed heap only
			};

			_memoryUsage.Add(sample);
		}
	}
}

public class CpuSample
{
	public DateTime TimeStamp { get; set; }
	public double PrivilegedUsage { get; set; }
	public double TotalUsage { get; set; }
	public double UserUsage { get; set; }
}

public class MemorySample
{
	public DateTime TimeStamp { get; set; }
	public long WorkingSet { get; set; }
	public long PrivateBytes { get; set; }
	public long ManagedMemory { get; set; }
}
