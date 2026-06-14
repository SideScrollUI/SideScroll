# Tab Schema

The **tab schema** is an exported, serializable description of a tab hierarchy. It walks a tab
tree *headlessly* (no UI controls) and records each tab's structure — its display objects
(toolbars, forms, charts, actions), its data-grid columns, and a sampled set of navigable child
rows — as a tree of plain data objects that can be rendered in a tab or serialized to JSON.

Typical uses: documenting a project's tab surface, diffing the **public** vs **private** surface,
or giving a tool/agent a machine-readable map of what a tab exposes.

## Components

| Type | Role |
| --- | --- |
| `HeadlessTabViewer` / `HeadlessTabView` | Loads and traverses a tab tree without UI. Produces `HeadlessTabView` nodes with `Model`, `ChildViews`, and per-list `ListItems`. |
| `HeadlessTabOptions` | Traversal configuration (depth, per-list caps, allowlist, tab filter). |
| `SchemaNode` / `SchemaObject` | The exported, serializable schema produced from a `HeadlessTabView`. |
| `TabSchema` | Lazy `ITab` that runs one traversal (one access level) and shows the `View` + `Json`. |
| `TabSchemas` | Entry-point `ITab` exposing **Public** and **Private** sub-tabs. Surfaced under the *Schema* link. |

## Quick start

```csharp
// Headless traversal directly:
var viewer = new HeadlessTabViewer(project)
{
    Options = new HeadlessTabOptions { MaxDepth = 4 },
};
HeadlessTabView root = await viewer.LoadAndTraverseAsync(call, myTab);
SchemaNode schema = SchemaNode.From(root);
string json = JsonSerializer.Serialize(schema, JsonConverters.PublicSerializerOptions);

// Or expose it as a tab:
TabSchemas.DefaultRootTab = new TabAvaloniaSamples();   // app startup
// ...navigating to TabSchemas shows Public + Private schema sub-tabs.
```

## Traversal options (`HeadlessTabOptions`)

- **`MaxDepth`** (5) — how many levels deep to recurse.
- **Per-list caps** — all use *negative = unlimited, 0 = none, positive = cap*:
  - **`MaxAllowedItems`** (50) / **`MaxAllowedChildren`** (50) — for allowlisted element types.
  - **`MaxOtherItems`** (50) / **`MaxOtherChildren`** (1) — for non-allowlisted element types.
- **`AllowedElementTypes`** — element types that are "allowed" (fully explored). `TabSchemas`
  defaults this to `[typeof(IListItem)]`. Lists outside it are only sampled.
- **`TabFilter`** — predicate run on every `ITab`; the **Public** view uses it to skip
  `[PrivateData]` tabs.

### Items vs. children

The two cap axes reflect a cost/safety split:

- **Items** = rows *listed* by label. Cheap — no tab load. The data-grid **columns are always
  shown** (pure reflection), so a list's structure is visible even when nothing is explored.
- **Children** = rows *explored* into sub-tabs (loaded and recursed). Expensive — this is what
  the small `MaxOtherChildren` guards.

Leaf rows (strings, primitives, empty collections) are listed as labels but never consume the
child-exploration budget. When a `TabFilter` is set, unresolved tab-like rows past the child
budget aren't listed, so a public schema can't leak filtered/private tab names.

### `[Explorable]` attribute

`[Explorable]` on an element type **overrides** the allowlist:

- `[Explorable]` ⇒ fully explored even if not in `AllowedElementTypes`.
- `[Explorable(false)]` ⇒ only sampled even if it is (or when no allowlist is set).
- no attribute ⇒ allowlist membership decides.

The attribute is `SideScroll.Attributes.ExplorableAttribute`; resolution lives in
`HeadlessTabView.IsElementTypeAllowed`.

### Bookmark-guided traversal

Passing a `Bookmark` to `LoadAndTraverseAsync` (or `TabSchemas.Bookmark`) follows only the rows
the bookmark selected, stopping at the bookmark's leaf nodes instead of expanding the full tree.
Rows are matched on full `SelectedRow` identity (label, data key/value, and row index), so
duplicate-labelled or index-only rows are disambiguated.

## Schema shape

### `SchemaNode`

| Field | Meaning |
| --- | --- |
| `Label` | The tab name (not serialized; used for display). |
| `TabRoot` | Tab is `[TabRoot]` (bookmarkable/linkable). Omitted when false. |
| `DepthTruncated` | Traversal stopped here at `MaxDepth` with more to expand (vs. a real leaf). |
| `Objects` | The tab's contents in order: display objects and item lists. |

### `SchemaObject` (polymorphic)

`SchemaObject` is serialized polymorphically with a `"Type"` discriminator, so each kind carries
only its own fields (no `$type`/`$value` wrapping). `Type` is also exposed as a C# get property.

| `Type` | Class | Fields |
| --- | --- | --- |
| `Text` | `SchemaText` | `Text` |
| `Chart` | `SchemaChart` | `Name`, `Series` |
| `Form` | `SchemaForm` | `Properties` (`Label`, `Type`, `Value`) |
| `Toolbar` | `SchemaToolbar` | `Controls` (`Name`, `Kind`) |
| `Actions` | `SchemaActions` | `Actions` (`Label`, `Description`) |
| `List` | `SchemaList` | `Columns` (`Label`, `Type`), `Items`, `Truncated` |

A `SchemaList`'s **`Items`** are `SchemaItem`s: a `Label` and an optional **`Child`** (the nested
`SchemaNode` for an explored row; absent for leaf/unexplored rows). **`Truncated`** marks a list
whose rows exceeded `MaxItems`.

### Example JSON

```json
{
  "TabRoot": true,
  "Objects": [
    { "Type": "Toolbar", "Controls": [ { "Name": "Refresh", "Kind": "Button" } ] },
    { "Type": "Form", "Properties": [ { "Label": "Name", "Type": "String", "Value": "Planet X" } ] },
    {
      "Type": "List",
      "Columns": [
        { "Label": "Name", "Type": "String" },
        { "Label": "Distance Km", "Type": "Double" }
      ],
      "Items": [
        { "Label": "Mercury", "Child": { "Objects": [ /* ... */ ] } },
        { "Label": "Venus" }
      ]
    }
  ]
}
```

## Markers summary

- **`TabRoot`** — the tab is independently bookmarkable.
- **`DepthTruncated`** (node) — stopped at the depth limit; there is more below.
- **`Truncated`** (list) — stopped at the item limit; there are more rows.
- An `Item` with no `Child` — a leaf row, or a row not explored within the child budget.
