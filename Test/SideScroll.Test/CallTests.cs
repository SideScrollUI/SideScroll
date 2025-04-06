using NUnit.Framework;

namespace SideScroll.Test;

[Category("Call")]
public class CallTests : BaseTest
{
	private List<int?> _input = [null, 0, 1];

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Call");
	}

	private async Task<int?> EchoAsync(Call call, int? index)
	{
		await Task.Delay(1);
		return index;
	}

	[Test]
	public async Task FirstNonNullAsync()
	{
		var result = await Call.FirstNonNullAsync(EchoAsync, _input);

		Assert.That(result, Is.EqualTo(0));
	}

	[Test]
	public async Task SelectNonNullAsync()
	{
		var result = await Call.SelectNonNullAsync(EchoAsync, _input);

		Assert.That(result, Has.Exactly(2).Items);
		Assert.That(result, Is.EquivalentTo([0, 1]));
	}

	[Test]
	public async Task RunAsync()
	{
		var result = await Call.RunAsync(EchoAsync, _input);

		Assert.That(result, Has.Exactly(3).Items);
		Assert.That(result.Keys, Is.EquivalentTo(_input));
		Assert.That(result.Values, Is.EquivalentTo(_input));
		Assert.That(result.NonNullValues, Is.EquivalentTo([0, 1]));
	}
}
