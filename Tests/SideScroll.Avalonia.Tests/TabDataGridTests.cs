using NUnit.Framework;
using SideScroll.Avalonia.Controls.DataGrids;

namespace SideScroll.Avalonia.Tests;

/// <summary>
/// Constructing a TabDataGrid needs an Avalonia Application, so these cover the matching rule
/// through the helper it was separated into
/// </summary>
public class TabDataGridTests
{
	/// <summary>Every member throws, so ToUniqueString() finds nothing to identify it by</summary>
	private class Unidentifiable
	{
		public string Throws => throw new InvalidOperationException("no identity");
	}

	private class Named(string name)
	{
		public override string ToString() => name;
	}

	[Test, Description(
		"ToUniqueString() returns null when nothing readable identifies the object, and comparing " +
		"that against the rows matched the first row that was also unidentifiable")]
	public void AnUnidentifiableDefaultMatchesNothing()
	{
		object[] items = [new Unidentifiable(), new Unidentifiable()];

		Assert.That(TabDataGrid.FindMatchingItem(new Unidentifiable(), items), Is.Null);
	}

	[Test, Description("An unidentifiable row doesn't capture an identifiable default either")]
	public void AnUnidentifiableRowIsNotMatched()
	{
		object[] items = [new Unidentifiable(), new Named("b")];

		Assert.That(TabDataGrid.FindMatchingItem(new Named("b"), items), Is.SameAs(items[1]));
	}

	[Test, Description("Control: a default with a unique string still selects the row that matches it")]
	public void AMatchingDefaultSelectsItsRow()
	{
		object[] items = [new Named("a"), new Named("b"), new Named("c")];

		Assert.That(TabDataGrid.FindMatchingItem(new Named("b"), items), Is.SameAs(items[1]));
	}

	[Test, Description("Control: no default and no match both select nothing")]
	public void NoDefaultOrNoMatchSelectsNothing()
	{
		object[] items = [new Named("a"), new Named("b")];

		Assert.That(TabDataGrid.FindMatchingItem(null, items), Is.Null);
		Assert.That(TabDataGrid.FindMatchingItem(new Named("z"), items), Is.Null);
	}
}
