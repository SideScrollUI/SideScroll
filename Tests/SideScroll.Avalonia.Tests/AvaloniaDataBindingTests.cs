using Avalonia.Controls;
using Avalonia.Data;
using NUnit.Framework;

namespace SideScroll.Avalonia.Tests;

public class AvaloniaDataBindingTests
{
	[SetUp]
	public void Setup()
	{
	}

	private class TestItem
	{
		public string? Name { get; set; }
	}

	[Test]
	public void Test1()
	{
		TestBind(1);
	}

	[Test]
	public void Test10()
	{
		TestBind(10);
	}

	/*
	[Test]
	public void Test10000()
	{
		TestBind(10_000);
	}*/

	private static void TestBind(int count)
	{
		Binding binding = new()
		{
			Path = nameof(TestItem.Name),
			Mode = BindingMode.OneWay, // copying a value to the clipboard triggers an infinite loop without this?
		};

		for (int i = 0; i < count; i++)
		{
			TextBlock textBlock = new();
			//var testItem = new TestItem();

			textBlock.Bind(TextBlock.TextProperty, binding);
		}
	}
}
