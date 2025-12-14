using NUnit.Framework;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class FilterTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Filter");
	}

	#region Constructor and Basic Parsing

	[Test]
	public void Constructor_NullFilterText_CreatesEmptyFilter()
	{
		var filter = new Filter(null);

		Assert.That(filter.FilterText, Is.EqualTo(""));
		Assert.That(filter.Depth, Is.EqualTo(0));
	}

	[Test]
	public void Constructor_EmptyFilterText_CreatesEmptyFilter()
	{
		var filter = new Filter("");

		Assert.That(filter.FilterText, Is.EqualTo(""));
		Assert.That(filter.Depth, Is.EqualTo(0));
	}

	[Test]
	public void Constructor_SimpleText_ParsesCorrectly()
	{
		var filter = new Filter("test");

		Assert.That(filter.FilterText, Is.EqualTo("test"));
		Assert.That(filter.Depth, Is.EqualTo(0));
		Assert.That(filter.RootNode, Is.InstanceOf<FilterLeafNode>());
		
		var leafNode = (FilterLeafNode)filter.RootNode!;
		Assert.That(leafNode.TextUppercase, Is.EqualTo("TEST"));
	}

	[Test]
	public void Constructor_QuotedText_ParsesCorrectly()
	{
		var filter = new Filter("\"hello world\"");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterLeafNode>());
		
		var leafNode = (FilterLeafNode)filter.RootNode!;
		Assert.That(leafNode.TextUppercase, Is.EqualTo("HELLO WORLD"));
	}

	[Test]
	public void Constructor_UnclosedQuote_AutoCloses()
	{
		var filter = new Filter("\"hello world");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterLeafNode>());
		
		var leafNode = (FilterLeafNode)filter.RootNode!;
		Assert.That(leafNode.TextUppercase, Is.EqualTo("HELLO WORLD"));
	}

	[Test]
	public void Constructor_UnclosedQuoteWithOperator_TreatsEntireStringAsQuoted()
	{
		// When a quote is unclosed, everything after it is treated as part of the quoted string
		var filter = new Filter("\"hello world & test");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterLeafNode>());
		
		var leafNode = (FilterLeafNode)filter.RootNode!;
		// The entire remainder is treated as quoted text with the opening quote removed
		Assert.That(leafNode.TextUppercase, Is.EqualTo("HELLO WORLD & TEST"));
	}

	[Test]
	public void Constructor_UnclosedQuoteAtStart_WithOperatorAfterSpace()
	{
		// To get operator parsing with an unclosed quote, close it or use space separation
		var filter = new Filter("\"hello world\" & test");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.And));
		Assert.That(operatorNode.Children, Has.Count.EqualTo(2));
		
		var leaf1 = (FilterLeafNode)operatorNode.Children[0];
		var leaf2 = (FilterLeafNode)operatorNode.Children[1];
		Assert.That(leaf1.TextUppercase, Is.EqualTo("HELLO WORLD"));
		Assert.That(leaf2.TextUppercase, Is.EqualTo("TEST"));
	}

	[Test]
	public void Constructor_WithDepth_ParsesCorrectly()
	{
		var filter = new Filter("+3 test");

		Assert.That(filter.FilterText, Is.EqualTo("+3 test"));
		Assert.That(filter.Depth, Is.EqualTo(3));
		Assert.That(filter.RootNode, Is.InstanceOf<FilterLeafNode>());
	}

	[Test]
	public void Constructor_WithLargeDepth_ParsesCorrectly()
	{
		var filter = new Filter("+10 test");

		Assert.That(filter.Depth, Is.EqualTo(10));
	}

	#endregion

	#region AND Operator Tests

	[Test]
	public void Constructor_AndOperatorExplicit_ParsesCorrectly()
	{
		var filter = new Filter("foo & bar");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.And));
		Assert.That(operatorNode.Children, Has.Count.EqualTo(2));
		Assert.That(operatorNode.Children[0], Is.InstanceOf<FilterLeafNode>());
		Assert.That(operatorNode.Children[1], Is.InstanceOf<FilterLeafNode>());
		
		var leaf1 = (FilterLeafNode)operatorNode.Children[0];
		var leaf2 = (FilterLeafNode)operatorNode.Children[1];
		Assert.That(leaf1.TextUppercase, Is.EqualTo("FOO"));
		Assert.That(leaf2.TextUppercase, Is.EqualTo("BAR"));
	}

	[Test]
	public void Constructor_AndOperatorImplicit_ParsesCorrectly()
	{
		var filter = new Filter("foo bar");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.And));
		Assert.That(operatorNode.Children, Has.Count.EqualTo(2));
	}

	[Test]
	public void Constructor_MultipleAndOperators_ParsesCorrectly()
	{
		var filter = new Filter("foo & bar & baz");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.And));
		Assert.That(operatorNode.Children, Has.Count.EqualTo(2));
		
		// First child should be another AND node
		Assert.That(operatorNode.Children[0], Is.InstanceOf<FilterOperatorNode>());
		var nestedAnd = (FilterOperatorNode)operatorNode.Children[0];
		Assert.That(nestedAnd.Operator, Is.EqualTo(FilterOperator.And));
	}

	#endregion

	#region OR Operator Tests

	[Test]
	public void Constructor_OrOperator_ParsesCorrectly()
	{
		var filter = new Filter("foo | bar");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.Or));
		Assert.That(operatorNode.Children, Has.Count.EqualTo(2));
		
		var leaf1 = (FilterLeafNode)operatorNode.Children[0];
		var leaf2 = (FilterLeafNode)operatorNode.Children[1];
		Assert.That(leaf1.TextUppercase, Is.EqualTo("FOO"));
		Assert.That(leaf2.TextUppercase, Is.EqualTo("BAR"));
	}

	[Test]
	public void Constructor_MultipleOrOperators_ParsesCorrectly()
	{
		var filter = new Filter("foo | bar | baz");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.Or));
		Assert.That(operatorNode.Children, Has.Count.EqualTo(3));
	}

	#endregion

	#region Mixed Operator Tests (Precedence)

	[Test]
	public void Constructor_MixedOperators_AndHasPrecedence()
	{
		var filter = new Filter("foo & bar | baz");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.Or));
		Assert.That(operatorNode.Children, Has.Count.EqualTo(2));
		
		// First child should be an AND node
		Assert.That(operatorNode.Children[0], Is.InstanceOf<FilterOperatorNode>());
		var andNode = (FilterOperatorNode)operatorNode.Children[0];
		Assert.That(andNode.Operator, Is.EqualTo(FilterOperator.And));
	}

	[Test]
	public void Constructor_MixedOperators_ReverseOrder()
	{
		var filter = new Filter("foo | bar & baz");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.Or));
		Assert.That(operatorNode.Children, Has.Count.EqualTo(2));
		
		// Second child should be an AND node
		Assert.That(operatorNode.Children[1], Is.InstanceOf<FilterOperatorNode>());
		var andNode = (FilterOperatorNode)operatorNode.Children[1];
		Assert.That(andNode.Operator, Is.EqualTo(FilterOperator.And));
	}

	#endregion

	#region Parentheses Tests

	[Test]
	public void Constructor_Parentheses_ParsesCorrectly()
	{
		var filter = new Filter("(foo | bar) & baz");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.And));
		Assert.That(operatorNode.Children, Has.Count.EqualTo(2));
		
		// First child should be an OR node (from parentheses)
		Assert.That(operatorNode.Children[0], Is.InstanceOf<FilterOperatorNode>());
		var orNode = (FilterOperatorNode)operatorNode.Children[0];
		Assert.That(orNode.Operator, Is.EqualTo(FilterOperator.Or));
	}

	[Test]
	public void Constructor_NestedParentheses_ParsesCorrectly()
	{
		var filter = new Filter("((foo | bar) & baz)");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.And));
	}

	[Test]
	public void Constructor_MultipleParenthesesGroups_ParsesCorrectly()
	{
		var filter = new Filter("(foo | bar) & (baz | qux)");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.And));
		Assert.That(operatorNode.Children, Has.Count.EqualTo(2));
		
		// Both children should be OR nodes
		Assert.That(operatorNode.Children[0], Is.InstanceOf<FilterOperatorNode>());
		Assert.That(operatorNode.Children[1], Is.InstanceOf<FilterOperatorNode>());
	}

	#endregion

	#region FilterLeafNode Matching Tests

	[Test]
	public void FilterLeafNode_Matches_CaseInsensitive()
	{
		var leafNode = new FilterLeafNode { TextUppercase = "TEST" };
		var values = new List<string> { "TEST", "ANOTHER" };

		Assert.That(leafNode.Matches(values), Is.True);
	}

	[Test]
	public void FilterLeafNode_Matches_PartialMatch()
	{
		var leafNode = new FilterLeafNode { TextUppercase = "EST" };
		var values = new List<string> { "TEST" };

		Assert.That(leafNode.Matches(values), Is.True);
	}

	[Test]
	public void FilterLeafNode_NoMatch()
	{
		var leafNode = new FilterLeafNode { TextUppercase = "FOO" };
		var values = new List<string> { "BAR", "BAZ" };

		Assert.That(leafNode.Matches(values), Is.False);
	}

	[Test]
	public void FilterLeafNode_Matches_EmptyValues()
	{
		var leafNode = new FilterLeafNode { TextUppercase = "TEST" };
		var values = new List<string>();

		Assert.That(leafNode.Matches(values), Is.False);
	}

	#endregion

	#region FilterOperatorNode Matching Tests

	[Test]
	public void FilterOperatorNode_And_BothMatch()
	{
		var andNode = new FilterOperatorNode
		{
			Operator = FilterOperator.And,
			Children =
			[
				new FilterLeafNode { TextUppercase = "FOO" },
				new FilterLeafNode { TextUppercase = "BAR" }
			]
		};
		var values = new List<string> { "FOO", "BAR" };

		Assert.That(andNode.Matches(values), Is.True);
	}

	[Test]
	public void FilterOperatorNode_And_OneDoesNotMatch()
	{
		var andNode = new FilterOperatorNode
		{
			Operator = FilterOperator.And,
			Children =
			[
				new FilterLeafNode { TextUppercase = "FOO" },
				new FilterLeafNode { TextUppercase = "BAR" }
			]
		};
		var values = new List<string> { "FOO", "BAZ" };

		Assert.That(andNode.Matches(values), Is.False);
	}

	[Test]
	public void FilterOperatorNode_Or_OneMatches()
	{
		var orNode = new FilterOperatorNode
		{
			Operator = FilterOperator.Or,
			Children =
			[
				new FilterLeafNode { TextUppercase = "FOO" },
				new FilterLeafNode { TextUppercase = "BAR" }
			]
		};
		var values = new List<string> { "FOO", "BAZ" };

		Assert.That(orNode.Matches(values), Is.True);
	}

	[Test]
	public void FilterOperatorNode_Or_NoneMatch()
	{
		var orNode = new FilterOperatorNode
		{
			Operator = FilterOperator.Or,
			Children =
			[
				new FilterLeafNode { TextUppercase = "FOO" },
				new FilterLeafNode { TextUppercase = "BAR" }
			]
		};
		var values = new List<string> { "BAZ", "QUX" };

		Assert.That(orNode.Matches(values), Is.False);
	}

	[Test]
	public void FilterOperatorNode_EmptyChildren_ReturnsTrue()
	{
		var operatorNode = new FilterOperatorNode
		{
			Operator = FilterOperator.And,
			Children = []
		};
		var values = new List<string> { "TEST" };

		Assert.That(operatorNode.Matches(values), Is.True);
	}

	#endregion

	#region ToString Tests

	[Test]
	public void ToString_ReturnsFilterText()
	{
		var filter = new Filter("test filter");

		Assert.That(filter.ToString(), Is.EqualTo("test filter"));
	}

	#endregion

	#region Integration Tests with Test Objects

	public class TestItem
	{
		public string Name { get; set; } = "";
		public int Id { get; set; }
		public string Description { get; set; } = "";
	}

	[Test]
	public void Filter_MatchesObject_SingleProperty()
	{
		var filter = new Filter("apple");
		var item = new TestItem { Name = "Apple", Id = 1, Description = "A fruit" };
		var properties = typeof(TestItem).GetProperties().ToList();

		Assert.That(filter.Matches(item, properties), Is.True);
	}

	[Test]
	public void Filter_MatchesObject_NoMatch()
	{
		var filter = new Filter("banana");
		var item = new TestItem { Name = "Apple", Id = 1, Description = "A fruit" };
		var properties = typeof(TestItem).GetProperties().ToList();

		Assert.That(filter.Matches(item, properties), Is.False);
	}

	[Test]
	public void Filter_MatchesObject_AndOperator_BothMatch()
	{
		var filter = new Filter("apple & fruit");
		var item = new TestItem { Name = "Apple", Id = 1, Description = "A fruit" };
		var properties = typeof(TestItem).GetProperties().ToList();

		Assert.That(filter.Matches(item, properties), Is.True);
	}

	[Test]
	public void Filter_MatchesObject_AndOperator_OneDoesNotMatch()
	{
		var filter = new Filter("apple & vegetable");
		var item = new TestItem { Name = "Apple", Id = 1, Description = "A fruit" };
		var properties = typeof(TestItem).GetProperties().ToList();

		Assert.That(filter.Matches(item, properties), Is.False);
	}

	[Test]
	public void Filter_MatchesObject_OrOperator_OneMatches()
	{
		var filter = new Filter("banana | apple");
		var item = new TestItem { Name = "Apple", Id = 1, Description = "A fruit" };
		var properties = typeof(TestItem).GetProperties().ToList();

		Assert.That(filter.Matches(item, properties), Is.True);
	}

	[Test]
	public void Filter_MatchesObject_ComplexExpression()
	{
		var filter = new Filter("(apple | banana) & fruit");
		var item = new TestItem { Name = "Apple", Id = 1, Description = "A fruit" };
		var properties = typeof(TestItem).GetProperties().ToList();

		Assert.That(filter.Matches(item, properties), Is.True);
	}

	[Test]
	public void Filter_MatchesObject_NumericValue()
	{
		var filter = new Filter("123");
		var item = new TestItem { Name = "Apple", Id = 123, Description = "A fruit" };
		var properties = typeof(TestItem).GetProperties().ToList();

		Assert.That(filter.Matches(item, properties), Is.True);
	}

	#endregion

	#region Edge Cases

	[Test]
	public void Constructor_OnlySpaces_HandlesGracefully()
	{
		var filter = new Filter("   ");

		// Should not crash
		Assert.That(filter.FilterText, Is.EqualTo("   "));
	}

	[Test]
	public void Constructor_OnlyOperators_HandlesGracefully()
	{
		var filter = new Filter("& | &");

		// Should not crash
		Assert.That(filter.FilterText, Is.EqualTo("& | &"));
	}

	[Test]
	public void Constructor_UnclosedParentheses_HandlesGracefully()
	{
		var filter = new Filter("(foo & bar");

		// Should not crash
		Assert.That(filter.FilterText, Is.EqualTo("(foo & bar"));
	}

	[Test]
	public void Constructor_ExtraSpaces_ParsesCorrectly()
	{
		var filter = new Filter("foo    &    bar");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.And));
		Assert.That(operatorNode.Children, Has.Count.EqualTo(2));
	}

	[Test]
	public void Constructor_MixedQuotesAndOperators_ParsesCorrectly()
	{
		var filter = new Filter("\"hello world\" & test");

		Assert.That(filter.RootNode, Is.InstanceOf<FilterOperatorNode>());
		
		var operatorNode = (FilterOperatorNode)filter.RootNode!;
		Assert.That(operatorNode.Operator, Is.EqualTo(FilterOperator.And));
		
		var leaf1 = (FilterLeafNode)operatorNode.Children[0];
		Assert.That(leaf1.TextUppercase, Is.EqualTo("HELLO WORLD"));
	}

	[Test]
	public void Filter_EmptyFilterText_MatchesEverything()
	{
		var filter = new Filter("");
		var item = new TestItem { Name = "Apple", Id = 1, Description = "A fruit" };
		var properties = typeof(TestItem).GetProperties().ToList();

		Assert.That(filter.Matches(item, properties), Is.True);
	}

	[Test]
	public void Filter_NullRootNode_MatchesEverything()
	{
		var filter = new Filter("")
		{
			RootNode = null
		};
		var item = new TestItem { Name = "Apple", Id = 1, Description = "A fruit" };
		var properties = typeof(TestItem).GetProperties().ToList();

		Assert.That(filter.Matches(item, properties), Is.True);
	}

	#endregion
}
