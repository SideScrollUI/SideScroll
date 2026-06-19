using SideScroll.Collections;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Forms;

public class TabSampleFormValidation : ITab
{
	public TabInstance Create() => new Instance();

	private class Toolbar : TabToolbar
	{
		public ToolButton ButtonSave { get; } = new("Save", Icons.Svg.Save, isDefault: true);
	}

	private class Instance : TabInstance
	{
		private readonly ItemCollectionUI<SampleValidationItem> _items = [];
		private SampleValidationItem? _item;
		private TabFormObject? _formObject;

		public override void Load(Call call, TabModel model)
		{
			_item = new SampleValidationItem();
			_formObject = model.AddForm(_item);

			Toolbar toolbar = new();
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);

			model.Items = _items;
		}

		private void Save(Call call)
		{
			// Validate() flags any empty [Required] field, and any [RequiredGroup] where
			// every member is empty, throwing if validation fails before reaching here.
			Validate();

			_items.Add(_item!.DeepClone(call));

			_item = new SampleValidationItem();
			_formObject!.Update(this, _item);
		}
	}
}
