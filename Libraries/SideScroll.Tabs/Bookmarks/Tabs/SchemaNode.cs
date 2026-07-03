using SideScroll.Attributes;
using SideScroll.Charts;
using SideScroll.Extensions;
using SideScroll.Tabs.Headless;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;
using System.Collections;
using System.Reflection;
using System.Text.Json.Serialization;

namespace SideScroll.Tabs.Bookmarks.Tabs;

/// <summary>Describes a tab node in the exported schema tree.</summary>
public class SchemaNode
{
	/// <summary>The tab's display label.</summary>
	[Hidden, JsonIgnore]
	public string Label { get; set; } = "";

	/// <summary>Whether the tab is decorated with <c>[TabRoot]</c>, making it bookmarkable/linkable.</summary>
	[Hide(false)]
	public bool TabRoot { get; set; }

	/// <summary>
	/// True when traversal stopped here due to the max depth limit while more was expandable —
	/// distinguishes a depth-truncated node from a genuine leaf with no children.
	/// </summary>
	[Hide(false)]
	public bool DepthTruncated { get; set; }

	/// <summary>
	/// When set, this node duplicates a type already described earlier in the schema, so it
	/// references it by name instead of repeating its contents.
	/// </summary>
	[Hidden]
	public string? Ref { get; set; }

	/// <summary>Live pointer to the resolved definition for the deduplicated type named by <see cref="Ref"/>, for in-app navigation (not serialized).</summary>
	[Hide(null), Unserialized]
	public SchemaNode? Reference { get; set; }

	/// <summary>
	/// The tab's contents: display objects (toolbars, forms, charts, actions) and item lists,
	/// in order. Item lists carry their columns and navigated items (each with an optional child).
	/// </summary>
	public List<SchemaObject>? Objects { get; set; }

	/// <summary>True when this node has details worth nesting under a parent item (vs. a bare leaf label).</summary>
	[Hidden, JsonIgnore]
	public bool HasDetails => Objects?.Count > 0 || TabRoot || DepthTruncated || Ref != null;

	/// <summary>Returns the tab's <see cref="Label"/>.</summary>
	public override string ToString() => Label;

	/// <summary>Builds the schema for <paramref name="view"/> without a shared export context.</summary>
	public static SchemaNode From(HeadlessTabView view) => From(view, new SchemaDocument());

	/// <summary>
	/// Builds the schema for <paramref name="view"/>, hoisting deduplicated types
	/// (<see cref="HeadlessTabView.DedupType"/>) into <paramref name="schemaDocument"/>'s definitions and
	/// referencing them by name via <see cref="Ref"/>, so a repeated type is described once and
	/// looked up directly. The <see cref="SchemaDocument"/> also carries any other shared traversal state.
	/// </summary>
	internal static SchemaNode From(HeadlessTabView view, SchemaDocument schemaDocument)
	{
		SchemaNode node = new()
		{
			Label = view.Label,
			TabRoot = view.Instance.iTab?.GetType().GetCustomAttribute<TabRootAttribute>() != null,
			DepthTruncated = view.DepthTruncated,
		};

		// Deduplicated type: reference it by name, hoisting its contents into the definitions map
		// the first time (from the canonical, fully-expanded occurrence).
		if (view.DedupType is { } dedupType)
		{
			node.Ref = dedupType.Name;

			Dictionary<string, SchemaNode> definitions = schemaDocument.Definitions!; // non-null while building
			if (view.DuplicateType == null && !definitions.ContainsKey(node.Ref))
			{
				SchemaNode definition = new()
				{
					Label = dedupType.Name,
				};
				definitions[node.Ref] = definition; // reserve before populating (handles self-reference)
				PopulateObjects(definition, view, schemaDocument);
			}

			// Live pointer to the resolved definition for in-app navigation (not serialized).
			node.Reference = definitions.GetValueOrDefault(node.Ref);

			return node;
		}

		PopulateObjects(node, view, schemaDocument);
		return node;
	}

	/// <summary>Fills <paramref name="node"/>'s <see cref="Objects"/> from the view's display objects and item lists.</summary>
	private static void PopulateObjects(SchemaNode node, HeadlessTabView view, SchemaDocument schemaDocument)
	{
		List<SchemaObject> objects = [];

		// Display objects (toolbars, forms, charts, actions) in their original order.
		foreach (TabObject tabObject in view.Model.Objects)
		{
			if (tabObject.Object == null)
				continue;

			objects.Add(SchemaObject.From(tabObject));
		}

		// Item lists become list objects carrying columns and the navigated child items.
		foreach (IList itemList in view.Model.ItemLists)
		{
			objects.Add(SchemaObject.FromList(itemList, view, schemaDocument));
		}

		if (objects.Count > 0)
		{
			node.Objects = objects;
		}
	}
}

/// <summary>
/// The exported schema: the <see cref="Root"/> tree plus a map of deduplicated type definitions
/// referenced from the tree by name (see <see cref="SchemaNode.Ref"/>).
/// </summary>
public class SchemaDocument
{
	/// <summary>Schemas for deduplicated types, keyed by type name and referenced via <see cref="SchemaNode.Ref"/>.</summary>
	[Hide(null)]
	public Dictionary<string, SchemaNode>? Definitions { get; set; } = [];

	/// <summary>The root tab's schema.</summary>
	public SchemaNode Root { get; set; } = new();

	/// <summary>Builds the full schema document for <paramref name="view"/>, populating the deduplicated type definitions or omitting them when nothing was deduplicated.</summary>
	public static SchemaDocument From(HeadlessTabView view)
	{
		SchemaDocument schemaDocument = new();
		schemaDocument.Root = SchemaNode.From(view, schemaDocument);

		// Drop the map entirely when nothing was deduplicated, so it's hidden/omitted.
		if (schemaDocument.Definitions!.Count == 0)
		{
			schemaDocument.Definitions = null;
		}

		return schemaDocument;
	}
}

/// <summary>
/// A single display object in a <see cref="SchemaNode"/>. Serialized polymorphically with a
/// <c>Type</c> discriminator, so each kind (text, chart, form, toolbar, actions, list) carries only
/// its own fields and the JSON stays clean — no <c>$type</c>/<c>$value</c> wrapping.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(SchemaText), "Text")]
[JsonDerivedType(typeof(SchemaChart), "Chart")]
[JsonDerivedType(typeof(SchemaForm), "Form")]
[JsonDerivedType(typeof(SchemaActions), "Actions")]
[JsonDerivedType(typeof(SchemaToolbar), "Toolbar")]
[JsonDerivedType(typeof(SchemaList), "List")]
[Skippable]
public abstract class SchemaObject
{
	/// <summary>
	/// The object kind, matching the polymorphic JSON discriminator. <see cref="JsonIgnoreAttribute"/>
	/// so it isn't serialized twice — the discriminator already emits it as <c>"Type"</c>.
	/// </summary>
	[JsonIgnore, DataKey]
	public abstract string Type { get; }

	//public string? Value => ToString();

	/// <summary>Maps a <see cref="TabObject"/> to its schema representation.</summary>
	public static SchemaObject From(TabObject tabObject)
	{
		object obj = tabObject.Object!;
		return obj switch
		{
			TabToolbar toolbar => SchemaToolbar.From(toolbar),
			ChartView chart => SchemaChart.From(chart),
			IEnumerable<TaskCreator> actions => SchemaActions.From(actions),
			_ when tabObject is TabFormObject => SchemaForm.From(obj),
			_ => new SchemaText { Text = obj.ToString() },
		};
	}

	/// <summary>Builds a list object for <paramref name="list"/> using the child views from <paramref name="view"/>.</summary>
	internal static SchemaObject FromList(IList list, HeadlessTabView view, SchemaDocument schemaDocument) =>
		SchemaList.From(list, view, schemaDocument);

	/// <summary>Returns the object's <see cref="Type"/> discriminator.</summary>
	public override string ToString() => Type;
}

/// <summary>Plain display text, e.g. an object's <c>ToString()</c>.</summary>
public class SchemaText : SchemaObject
{
	/// <inheritdoc/>
	public override string Type => "Text";

	/// <summary>The display text.</summary>
	public string? Text { get; set; }

	//public override string ToString() => Text ?? base.ToString()!;
}

/// <summary>A set of action buttons from a list of <see cref="TaskCreator"/>.</summary>
public class SchemaActions : SchemaObject
{
	/// <inheritdoc/>
	public override string Type => "Actions";

	/// <summary>The action buttons.</summary>
	[InnerValue]
	public List<SchemaAction>? Actions { get; set; }

	//public override string ToString() => $"{Actions?.Count ?? 0}";

	/// <summary>Builds an actions object from a list of <see cref="TaskCreator"/>.</summary>
	public static SchemaActions From(IEnumerable<TaskCreator> actions) => new()
	{
		Actions = actions.Select(SchemaAction.From).ToList(),
	};
}

/// <summary>A single visible property of a form: its display label, type, and current value.</summary>
public class SchemaProperty
{
	/// <summary>The property's display label.</summary>
	public string? Label { get; set; }

	/// <summary>The property's value type name.</summary>
	public string? Type { get; set; }

	/// <summary>The property's current value, formatted as text.</summary>
	public string? Value { get; set; }

	/// <summary>Returns the property's <see cref="Label"/>.</summary>
	public override string ToString() => Label ?? base.ToString()!;

	/// <summary>Builds a property from <paramref name="propertyInfo"/> on <paramref name="obj"/>, capturing its label, type, and value.</summary>
	public static SchemaProperty From(object obj, PropertyInfo propertyInfo)
	{
		return new SchemaProperty
		{
			Label = ReflectionCache.GetPropertyDisplayName(propertyInfo),
			Type = propertyInfo.PropertyType.GetNonNullableType().Name,
			Value = GetValue(obj, propertyInfo),
		};
	}

	private static string? GetValue(object obj, PropertyInfo propertyInfo)
	{
		try
		{
			return propertyInfo.GetValue(obj).Formatted();
		}
		catch (Exception)
		{
			return null;
		}
	}
}

/// <summary>A single action button from a <see cref="TaskCreator"/>: its label and description.</summary>
public class SchemaAction
{
	/// <summary>The action button's label.</summary>
	public string? Label { get; set; }

	/// <summary>The action button's description.</summary>
	[Hide(null)]
	public string? Description { get; set; }

	/// <summary>Returns the action's <see cref="Label"/>.</summary>
	public override string ToString() => Label ?? base.ToString()!;

	/// <summary>Builds an action from a <see cref="TaskCreator"/>.</summary>
	public static SchemaAction From(TaskCreator taskCreator)
	{
		return new SchemaAction
		{
			Label = taskCreator.Label,
			Description = taskCreator.Description,
		};
	}
}

/// <summary>A chart: its name and series names.</summary>
public class SchemaChart : SchemaObject
{
	/// <inheritdoc/>
	public override string Type => "Chart";

	/// <summary>The chart's name.</summary>
	public string? Name { get; set; }

	/// <summary>The chart's series names, in order.</summary>
	public List<string>? Series { get; set; }

	/// <summary>Builds a chart object from a <see cref="ChartView"/>, capturing its name and series names.</summary>
	public static SchemaChart From(ChartView chart)
	{
		List<string> series = chart.Series
			.Select(series => series.Name)
			.OfType<string>()
			.ToList();

		return new SchemaChart
		{
			Name = chart.Name,
			Series = series.Count > 0 ? series : null,
		};
	}
}

/// <summary>A toolbar: its controls in order.</summary>
public class SchemaToolbar : SchemaObject
{
	/// <inheritdoc/>
	public override string Type => "Toolbar";

	/// <summary>The controls in order.</summary>
	public List<SchemaControl>? Controls { get; set; }

	/// <summary>
	/// Mirrors <c>TabControlToolbar.LoadToolbar</c>: collects controls from the toolbar's
	/// reflected properties, then from <see cref="TabToolbar.AdditionalButtons"/>.
	/// </summary>
	public static SchemaToolbar From(TabToolbar toolbar)
	{
		List<SchemaControl> controls = [];

		foreach (PropertyInfo property in toolbar.GetType().GetVisibleProperties())
		{
			if (SchemaControl.From(property.GetValue(toolbar)) is { } control)
			{
				controls.Add(control);
			}
		}

		foreach (ToolButton button in toolbar.AdditionalButtons)
		{
			if (SchemaControl.From(button) is { } control)
			{
				controls.Add(control);
			}
		}

		return new SchemaToolbar
		{
			Controls = controls.Count > 0 ? controls : null,
		};
	}
}

/// <summary>A single toolbar control: its display name and kind (Button, Toggle, ComboBox, Label).</summary>
public class SchemaControl
{
	/// <summary>The control's display name.</summary>
	public string? Name { get; set; }

	/// <summary>The control's kind (Button, Toggle, ComboBox, or Label).</summary>
	public string? Kind { get; set; }

	/// <summary>Returns the control's <see cref="Name"/>.</summary>
	public override string ToString() => Name ?? base.ToString()!;

	/// <summary>
	/// Resolves a control's name and kind so toggle buttons, combo boxes, and labels can be told
	/// apart. Returns <c>null</c> for unsupported control types.
	/// </summary>
	public static SchemaControl? From(object? control) => control switch
	{
		ToolToggleButton toggle => new() { Name = toggle.ToString(), Kind = "Toggle" }, // must precede ToolButton (base type)
		ToolButton button => new() { Name = button.ToString(), Kind = "Button" },
		IToolComboBox comboBox => new() { Name = comboBox.Label, Kind = "ComboBox" },
		string label => new() { Name = label, Kind = "Label" },
		_ => null,
	};
}

/// <summary>A form: its visible properties.</summary>
public class SchemaForm : SchemaObject
{
	/// <inheritdoc/>
	public override string Type => "Form";

	/// <summary>The form's visible properties, in order.</summary>
	public List<SchemaProperty>? Properties { get; set; }

	/// <summary>
	/// Mirrors <c>TabForm.AddObjectRow</c>: lists the form object's visible properties, capturing
	/// the display label, property type, and current value of each.
	/// </summary>
	public static SchemaForm From(object obj)
	{
		List<SchemaProperty> properties = obj.GetType().GetVisibleProperties()
			.Select(property => SchemaProperty.From(obj, property))
			.ToList();

		return new SchemaForm
		{
			Properties = properties.Count > 0 ? properties : null,
		};
	}
}

/// <summary>A data-grid item list: its visible columns and the navigated items.</summary>
public class SchemaList : SchemaObject
{
	/// <inheritdoc/>
	public override string Type => "List";

	/// <summary>The visible columns, mirroring the data grid's columns for the list's element type.</summary>
	public List<SchemaColumn>? Columns { get; set; }

	/// <summary>The navigated rows; each has a label and an optional expanded child.</summary>
	[InnerValue]
	public List<SchemaItem>? Items { get; set; }

	/// <summary>True when the list was not fully expanded (per-list item limit reached).</summary>
	public bool Truncated { get; set; }

	/// <summary>Builds a list object from <paramref name="list"/> using the child views from <paramref name="view"/>.</summary>
	internal static SchemaList From(IList list, HeadlessTabView view, SchemaDocument schemaDocument)
	{
		SchemaList schemaList = new()
		{
			Columns = GetColumns(list),
			Truncated = view.TruncatedLists.Contains(list),
		};

		if (view.ListItems.TryGetValue(list, out List<HeadlessTabItem>? listItems))
		{
			List<SchemaItem> items = [];
			foreach (HeadlessTabItem listItem in listItems)
			{
				SchemaNode? childNode = listItem.Child != null ? SchemaNode.From(listItem.Child, schemaDocument) : null;
				items.Add(new SchemaItem
				{
					Label = listItem.Label,
					Child = childNode?.HasDetails == true ? childNode : null,
				});
			}

			if (items.Count > 0)
			{
				schemaList.Items = items;
			}
		}

		return schemaList;
	}

	/// <summary>Resolves the visible columns for the list's element type, mirroring the data grid.</summary>
	public static List<SchemaColumn>? GetColumns(IList list)
	{
		Type? elementType = list.GetType().GetElementTypeForAll();
		if (elementType == null)
			return null;

		List<SchemaColumn> columns = new TabDataColumns().GetPropertyColumns(elementType)
			.Where(column => column.IsVisible(list))
			.Select(column => new SchemaColumn
			{
				Label = column.Label,
				Type = column.PropertyInfo.PropertyType.GetNonNullableType().Name,
			})
			.ToList();

		return columns.Count > 0 ? columns : null;
	}
}

/// <summary>A single data-grid column: its display label and value type.</summary>
public class SchemaColumn
{
	/// <summary>The column's display label.</summary>
	public string? Label { get; set; }

	/// <summary>The column's value type name.</summary>
	public string? Type { get; set; }

	/// <summary>Returns the column's <see cref="Label"/>.</summary>
	public override string ToString() => Label ?? base.ToString()!;
}

/// <summary>A single navigated row in a list: its label and an optional expanded child node.</summary>
public class SchemaItem
{
	/// <summary>The row's display label.</summary>
	public string? Label { get; set; }

	/// <summary>The row's expanded child node, if any.</summary>
	[Hidden, InnerValue]
	public SchemaNode? Child { get; set; }

	/// <summary>Returns the item's <see cref="Label"/>.</summary>
	public override string ToString() => Label ?? base.ToString()!;
}
