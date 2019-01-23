using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Core;
using Atlas.Tabs;
using Avalonia.Logging.Serilog;
using Serilog;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabSeriLog : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private ItemCollection<ListItem> items = new ItemCollection<ListItem>();

			public override void Load()
			{
				//tabModel.Items = items;
				/*SerilogLogger serilogLogger = new SerilogLogger()
				{

				};*/
				var logger = CreateLogger();
				logger.Debug("test");

				ItemCollection<TaskCreator> actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Add Log Entry", AddEntry),
					new TaskDelegate("Task Instance Progress", SubTaskInstances),
				};
				tabModel.Actions = actions;
			}

			private Serilog.Core.Logger CreateLogger()
			{
				var logConfig = new LoggerConfiguration()
					.MinimumLevel.Warning()
					.WriteTo.Trace(outputTemplate: "{Area}: {Message}");
				Serilog.Core.Logger logger = logConfig.CreateLogger();
				SerilogLogger.Initialize(logger);
				return logger;
			}

			private void AddEntry(Call call)
			{
				call.log.Add("New Log entry", new Tag("name", "value"));
			}

			private void SubTaskInstances(Call call)
			{
				List<int> downloads = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				Parallel.ForEach(downloads, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, i =>
				{
					using (CallTimer sleepCall = call.Timer(i.ToString()))
					{
						sleepCall.AddSubTask();
						sleepCall.taskInstance.ProgressMax = i;
						for (int j = 0; j < i; j++)
						{
							System.Threading.Thread.Sleep(1000);
							sleepCall.taskInstance.Progress = j + 1;
						}
					}
				});
			}
		}
	}
}
/*
Todo: test out logging from other threads, while showing in GUI
*/
