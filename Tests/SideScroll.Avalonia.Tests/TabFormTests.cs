using Avalonia.Headless.NUnit;
using NUnit.Framework;
using SideScroll.Avalonia.Controls;
using SideScroll.Tabs;
using System.Runtime.CompilerServices;

namespace SideScroll.Avalonia.Tests;

public class TabFormTests
{
	// No visible properties, so the form generates no property controls.
	// Those need an Avalonia Application for the text box context menu keymap
	private class TestItem(string name)
	{
		public override string ToString() => name;
	}

	private static TabFormObject CreateFormObject(object obj)
	{
		return new TabFormObject
		{
			Object = obj,
		};
	}

	[AvaloniaTest]
	public void UpdateReloadsTheForm()
	{
		var formObject = CreateFormObject(new TestItem("A"));
		using var form = new TabForm(formObject);

		var updated = new TestItem("B");
		formObject.Update(null, updated);

		Assert.That(form.Object, Is.SameAs(updated));
	}

	[AvaloniaTest]
	public void DisposedFormStopsReloading()
	{
		var original = new TestItem("A");
		var formObject = CreateFormObject(original);
		var form = new TabForm(formObject);

		form.Dispose();
		formObject.Update(null, new TestItem("B"));

		Assert.That(form.Object, Is.SameAs(original),
			"A disposed form should no longer reload when the form object changes");
	}

	// ─── Lifetime ────────────────────────────────────────────────────────

	// Kept out of the test method so the form has no local still referencing it
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static WeakReference CreateAndRelease(TabFormObject formObject, bool dispose)
	{
		var form = new TabForm(formObject);
		if (dispose)
		{
			form.Dispose();
		}
		return new WeakReference(form);
	}

	[AvaloniaTest]
	[TestCase(true, false, TestName = "Disposed form is collected")]
	[TestCase(false, true, TestName = "Undisposed form is held by the form object")]
	[Description(
		"The TabFormObject lives in the TabModel and outlives the control, so its ObjectChanged and " +
		"OnFocus subscriptions hold the form until Dispose() unsubscribes. The undisposed case " +
		"proves the collection check can actually fail")]
	public void DisposeReleasesFormObject(bool dispose, bool expectedAlive)
	{
		var formObject = CreateFormObject(new TestItem("A"));

		WeakReference reference = CreateAndRelease(formObject, dispose);

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		Assert.That(reference.IsAlive, Is.EqualTo(expectedAlive));

		// The form object has to stay reachable, it's what would be holding the form
		GC.KeepAlive(formObject);
	}
}
