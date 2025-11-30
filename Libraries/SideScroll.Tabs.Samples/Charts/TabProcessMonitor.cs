using SideScroll.Attributes;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;
using SideScroll.Time;
using System.Diagnostics;

namespace SideScroll.Tabs.Samples.Charts;

public class TabProcessMonitor : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonReset { get; } = new("Reset", Icons.Svg.Reset);

		[Separator]
		public ToolButton ButtonStop { get; } = new("Stop", Icons.Svg.Stop);
	}

	public class Instance : TabInstance
	{
		private ItemQueueCollection<CpuSample> _cpuUsage = [];

		private ItemQueueCollection<TimeRangeValue> _memoryUsageWorkingSet = [];
		private ItemQueueCollection<TimeRangeValue> _memoryUsagePrivate = [];
		private ItemQueueCollection<TimeRangeValue> _memoryUsageManaged = [];

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

			_memoryUsageWorkingSet = [];
			_memoryUsagePrivate = [];
			_memoryUsageManaged = [];

			_prevSampleTime = null;

			AddSample();

			Toolbar toolbar = new();
			toolbar.ButtonReset.Action = Reset;
			toolbar.ButtonStop.Action = StopTask;
			model.AddObject(toolbar);

			// Uses one collection for all series, and maps object properties to the values
			ChartView cpuChart = new("CPU Usage")
			{
				ShowNowTime = false,
				DefaultPeriodDuration = TimeSpan.FromSeconds(1),
			};
			cpuChart.AddSeries("Privileged", _cpuUsage, nameof(CpuSample.TimeStamp), nameof(CpuSample.PrivilegedUsage), SeriesType.Average);
			cpuChart.AddSeries("Total", _cpuUsage, nameof(CpuSample.TimeStamp), nameof(CpuSample.TotalUsage), SeriesType.Average);
			cpuChart.AddSeries("User", _cpuUsage, nameof(CpuSample.TimeStamp), nameof(CpuSample.UserUsage), SeriesType.Average);
			model.AddObject(cpuChart);

			// Uses separate collections per series, and each series uses separate TimeRangeValue records
			ChartView memoryChart = new("Memory Usage")
			{
				ShowNowTime = false,
				DefaultPeriodDuration = TimeSpan.FromSeconds(1),
			};
			memoryChart.AddSeries("Working Set", _memoryUsageWorkingSet, seriesType: SeriesType.Average);
			memoryChart.AddSeries("Private", _memoryUsagePrivate, seriesType: SeriesType.Average);
			memoryChart.AddSeries("Managed", _memoryUsageManaged, seriesType: SeriesType.Average);
			model.AddObject(memoryChart);

			_timer?.Dispose();
			_timer = new Timer(Callback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
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
			Post(AddSample);
		}

		private void AddSample()
		{
			AddCpuSample();
			AddMemorySample();
		}

		private void AddCpuSample()
		{
			DateTime sampleTime = DateTime.Now.Trim();
			TimeSpan privilegedProcessorTime = _process!.PrivilegedProcessorTime;
			TimeSpan totalProcessorTime = _process.TotalProcessorTime;
			TimeSpan userProcessorTime = _process.UserProcessorTime;

			if (_prevSampleTime != null)
			{
				TimeSpan duration = sampleTime.Subtract(_prevSampleTime.Value);

				CpuSample cpuSample = new()
				{
					TimeStamp = sampleTime,
					PrivilegedUsage = privilegedProcessorTime.Subtract(_prevPrivilegedProcessorTime!.Value) / duration,
					TotalUsage = totalProcessorTime.Subtract(_prevTotalProcessorTime!.Value) / duration,
					UserUsage = userProcessorTime.Subtract(_prevUserProcessorTime!.Value) / duration,
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
			DateTime sampleTime = DateTime.Now.Trim();

			long workingSet = _process!.WorkingSet64;          // Physical memory in bytes
			long privateMemory = _process.PrivateMemorySize64; // Private memory in bytes
			long managedMemory = GC.GetTotalMemory(false);     // Managed heap only

			_memoryUsageWorkingSet.Add(new TimeRangeValue(sampleTime, sampleTime, workingSet));
			_memoryUsagePrivate.Add(new TimeRangeValue(sampleTime, sampleTime, privateMemory));
			_memoryUsageManaged.Add(new TimeRangeValue(sampleTime, sampleTime, managedMemory));
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
