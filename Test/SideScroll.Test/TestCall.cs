using NUnit.Framework;

namespace SideScroll.Test;

[Category("Call")]
public class TestCall : TestBase
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
	public async Task FirstOrDefaultAsync()
	{
		var result = await Call.FirstOrDefaultAsync(EchoAsync, _input);

		Assert.That(result, Is.EqualTo(0));
	}

	[Test]
	public async Task SelectAsync()
	{
		var result = await Call.SelectAsync(EchoAsync, _input);

		Assert.That(result, Has.Exactly(2).Items);
		Assert.That(result, Is.EquivalentTo([0, 1]));
	}

	[Test]
	public async Task RunAsync()
	{
		var result = await Call.RunAsync(EchoAsync, _input);

		Assert.That(result, Has.Exactly(3).Items);
		Assert.That(result.Keys, Is.EquivalentTo(_input));
	}
}
