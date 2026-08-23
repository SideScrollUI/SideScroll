namespace SideScroll.Tabs.Lists;

/// <summary>
/// Collapses members that reflection returns more than once under the same name, keeping the
/// most derived declaration at the position of the one it hides
/// </summary>
/// <remarks>
/// Reflection collapses a same signature <c>new</c> itself, returning a single member. It returns
/// both declarations for a <c>new</c> that changes the type and for any <c>new</c> field, which
/// otherwise show as two rows or two columns with the same name, one of them bound to the hidden
/// declaration.
/// <para>
/// The surviving member keeps the hidden declaration's position rather than moving to its own, so
/// a grid's rows and columns stay where they were when a subclass redeclares a member. Members
/// arrive sorted by <c>MetadataToken</c>, which follows declaration order, so the derived
/// declaration is the later of the two
/// </para>
/// </remarks>
/// <typeparam name="T">The list element type, such as <see cref="ListMember"/> or a member info</typeparam>
/// <param name="items">The list to add to, which the merger writes through</param>
/// <param name="capacity">Expected member count, used to size the name lookup</param>
public class MemberNameMerger<T>(IList<T> items, int capacity = 0)
{
	private readonly Dictionary<string, int> _nameToIndex = new(capacity);

	/// <summary>
	/// Appends <paramref name="item"/>, or replaces in place the item already added under
	/// <paramref name="name"/>
	/// </summary>
	/// <param name="name">The member name to merge on, not the display label</param>
	/// <param name="item">The item to add, which wins over an earlier one of the same name</param>
	public void AddOrReplace(string name, T item)
	{
		if (_nameToIndex.TryGetValue(name, out int index))
		{
			items[index] = item;
		}
		else
		{
			_nameToIndex[name] = items.Count;
			items.Add(item);
		}
	}
}
