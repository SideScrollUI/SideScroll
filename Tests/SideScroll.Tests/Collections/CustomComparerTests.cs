using NUnit.Framework;
using SideScroll.Collections;

namespace SideScroll.Tests.Collections;

[Category("Core")]
public class CustomComparerTests : BaseTest
{
	private readonly CustomComparer _comparer = new();

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	[Test]
	public void Compare_Nulls()
	{
		Assert.That(_comparer.Compare(null, null), Is.EqualTo(0));
		Assert.That(_comparer.Compare(null, 1), Is.LessThan(0));
		Assert.That(_comparer.Compare(1, null), Is.GreaterThan(0));
	}

	[Test]
	public void Compare_SameType()
	{
		Assert.That(_comparer.Compare(1, 2), Is.LessThan(0));
		Assert.That(_comparer.Compare("b", "a"), Is.GreaterThan(0));
		Assert.That(_comparer.Compare(1.5, 1.5), Is.EqualTo(0));
	}

	[Test]
	public void Compare_MixedTypes_IsConsistent()
	{
		// Different runtime types order by type name, and reversing the arguments flips the sign
		int intFive = 5;
		long longThree = 3;

		int comparison = _comparer.Compare(intFive, longThree);
		Assert.That(comparison, Is.Not.EqualTo(0));
		Assert.That(_comparer.Compare(longThree, intFive), Is.EqualTo(-comparison));
	}

	[Test]
	public void Compare_MixedTypes_SortDoesNotThrow()
	{
		// List.Sort throws "IComparer.Compare() method returns inconsistent results" for intransitive comparers
		List<object> items = [5, "b", 3L, 1, "a", 2L, 4, 9L, "c", 7];

		Assert.DoesNotThrow(() => items.Sort((x, y) => _comparer.Compare(x, y)));

		// Items should be grouped by type after sorting, and ordered by value within each type
		List<Type> typeSequence = items.Select(i => i.GetType()).ToList();
		int typeGroups = 1;
		for (int i = 1; i < typeSequence.Count; i++)
		{
			if (typeSequence[i] != typeSequence[i - 1])
			{
				typeGroups++;
			}
		}
		Assert.That(typeGroups, Is.EqualTo(3));

		Assert.That(items.OfType<int>(), Is.Ordered);
		Assert.That(items.OfType<long>(), Is.Ordered);
		Assert.That(items.OfType<string>(), Is.Ordered);
	}
}
