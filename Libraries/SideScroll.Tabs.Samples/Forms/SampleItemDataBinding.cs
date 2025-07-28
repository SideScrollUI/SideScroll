using SideScroll.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace SideScroll.Tabs.Samples.Forms;

public class SampleItemDataBinding(SynchronizationContext context) : INotifyPropertyChanged
{
	[DataKey, Required, StringLength(30)]
	public string? Value
	{
		get => _value;
		set
		{
			_value = value;
			NotifyPropertyChanged();

			String = _value;
			NotifyPropertyChanged(nameof(String));

			if (int.TryParse(_value, out int i))
			{
				Integer = i;
				NotifyPropertyChanged(nameof(Integer));

				ListItem = ListItems[i % 3];
				NotifyPropertyChanged(nameof(ListItem));

				EnumAttributeTargets = (AttributeTargets)i;
				NotifyPropertyChanged(nameof(EnumAttributeTargets));

				DateTime = new DateTime(2024, 7, i + 1);
				NotifyPropertyChanged(nameof(DateTime));
			}

			if (double.TryParse(_value, out double d))
			{
				Double = d;
				NotifyPropertyChanged(nameof(Double));
			}
		}
	}
	private string? _value;

	[ReadOnly(true)]
	public string? String { get; set; }

	[ReadOnly(true)]
	public bool? Boolean { get; set; }

	[ReadOnly(true)]
	public int? Integer { get; set; }

	[ReadOnly(true), ColumnIndex(2)]
	public double? Double { get; set; }

	public AttributeTargets? EnumAttributeTargets { get; set; }

	public static List<ParamListItem> ListItems { get; } =
	[
		new("One", 1),
		new("Two", 2),
		new("Three", 3),
	];

	[BindList(nameof(ListItems)), ColumnIndex(2)]
	public ParamListItem? ListItem { get; set; }

	[ReadOnly(true)]
	public DateTime? DateTime { get; set; }

	protected SynchronizationContext Context = context;

	public event PropertyChangedEventHandler? PropertyChanged;

	public override string? ToString() => Value;

	public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		Context.Post(NotifyPropertyChangedContext, propertyName);
	}

	private void NotifyPropertyChangedContext(object? state)
	{
		string propertyName = (string)state!;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
