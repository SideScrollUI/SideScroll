using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.Utilities;
using SideScroll.Extensions;
using SideScroll.Logs;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SideScroll.Avalonia.Tabs;

/// <summary>
/// Displays a <see cref="TaskInstance"/>'s log, with a toolbar button to copy the Call.Log as JSON.
/// </summary>
public class TabTaskInstance(TaskInstance taskInstance) : ITab
{
	public TaskInstance TaskInstance => taskInstance;

	public override string ToString() => taskInstance.Label ?? nameof(TaskInstance);

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonCopy { get; } = new("Copy Log as JSON", Icons.Svg.Copy);
	}

	private class Instance(TabTaskInstance tab) : TabInstance
	{
		private static readonly JsonSerializerOptions JsonSerializerOptions = new()
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		};

		public override void Load(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonCopy.ActionAsync = CopyAsync;
			model.AddObject(toolbar);

			model.AddItems(tab.TaskInstance.Log.Items);
		}

		private async Task CopyAsync(Call call)
		{
			Dictionary<string, object?> entry = ToJson(tab.TaskInstance.Call.Log);
			string json = JsonSerializer.Serialize(entry, JsonSerializerOptions);

			await ClipboardUtils.SetTextAsync(TabViewer.Instance, json);

			call.TaskInstance!.ShowMessage("Copied Log to Clipboard");
		}

		// Project the log into a clean tree of just the message, tags, and child entries
		private static Dictionary<string, object?> ToJson(LogEntry entry)
		{
			var result = new Dictionary<string, object?>
			{
				["Message"] = entry.Text,
				["Level"] = entry.Level.ToString(),
			};

			if (entry.Duration is { } duration)
			{
				result["Duration"] = duration;
			}

			if (entry.Tags is { Length: > 0 })
			{
				var tags = new Dictionary<string, object?>();
				foreach (Tag tag in entry.Tags)
				{
					tags[tag.Name ?? ""] = FormatTagValue(tag.Value);
				}
				result["Tags"] = tags;
			}

			if (entry is Log log && log.Items.Count > 0)
			{
				result["Items"] = log.Items.Select(ToJson).ToList();
			}

			return result;
		}

		// Keep simple values as-is so they serialize naturally; format anything else to a readable string
		private static object? FormatTagValue(object? value)
		{
			if (value is null or string || value.GetType().IsPrimitive || value is decimal)
			{
				return value;
			}
			return value.Formatted();
		}
	}
}
