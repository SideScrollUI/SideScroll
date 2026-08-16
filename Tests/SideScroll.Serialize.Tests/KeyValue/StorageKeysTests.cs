using NUnit.Framework;
using SideScroll.Serialize.DataRepos;
using SideScroll.Serialize.KeyValue;

namespace SideScroll.Serialize.Tests.KeyValue;

/// <summary>
/// Covers the path to key conversion the localStorage repositories navigate with
/// </summary>
[Category("Serialize")]
public class StorageKeysTests
{
	[Test]
	public void APathRoundTripsThroughItsDataKey()
	{
		Assert.That(StorageKeys.ToPath(StorageKeys.DataKey("Project/Group/Item")), Is.EqualTo("Project/Group/Item"));
	}

	[Test, Description("Keys are stored with forward slashes, so a path written on Windows finds what another wrote")]
	public void BackslashesNormalizeToForwardSlashes()
	{
		Assert.That(StorageKeys.DataKey(@"Project\Group"), Is.EqualTo(StorageKeys.DataKey("Project/Group")));
	}

	[Test, Description("Paths go through Uri escaping, so separators and spaces survive the round trip")]
	public void AwkwardCharactersSurviveTheRoundTrip()
	{
		foreach (string path in new[] { "Group/Item With Spaces", "Group/Item#1", "Group/a=b&c", "Group/100%" })
		{
			Assert.That(StorageKeys.ToPath(StorageKeys.DataKey(path)), Is.EqualTo(path), path);
		}
	}

	[Test, Description(
		"The header prefix also starts with SideScroll_, so trimming the data prefix off one would " +
		"leave part of it in the path instead of failing")]
	public void AHeaderKeyIsNotAcceptedAsADataKey()
	{
		Assert.Throws<ArgumentException>(() => StorageKeys.ToPath(StorageKeys.HeaderKey("Project/Data")));
	}

	[Test]
	public void DataAndHeaderKeysDifferForTheSamePath()
	{
		Assert.That(StorageKeys.DataKey("Project"), Is.Not.EqualTo(StorageKeys.HeaderKey("Project")));
	}

	[Test]
	public void AnItemDirectlyInTheGroupIsInIt()
	{
		Assert.That(StorageKeys.IsDataKeyInGroup(StorageKeys.DataKey("Group/Item"), "Group"), Is.True);
	}

	[Test, Description("Only items directly in the group count, not ones nested below it")]
	public void ANestedItemIsNotInTheGroup()
	{
		Assert.That(StorageKeys.IsDataKeyInGroup(StorageKeys.DataKey("Group/Sub/Item"), "Group"), Is.False);
	}

	[Test]
	public void AnItemInAnotherGroupIsNotInIt()
	{
		Assert.That(StorageKeys.IsDataKeyInGroup(StorageKeys.DataKey("Other/Item"), "Group"), Is.False);
		Assert.That(StorageKeys.IsDataKeyInGroup(StorageKeys.DataKey("GroupExtra/Item"), "Group"), Is.False);
	}

	[Test, Description("The index isn't one of the group's items")]
	public void ThePrimaryIndexIsNotAnItemInTheGroup()
	{
		string indexKey = StorageKeys.DataKey("Group/" + DataRepo.PrimaryIndexFileName);

		Assert.That(StorageKeys.IsDataKeyInGroup(indexKey, "Group"), Is.False);
	}

	[Test, Description("A group path written either way, with or without a trailing separator, matches the same items")]
	public void GroupPathsMatchRegardlessOfSeparatorStyle()
	{
		string key = StorageKeys.DataKey("Group/Item");

		Assert.That(StorageKeys.IsDataKeyInGroup(key, "Group/"), Is.True);
		Assert.That(StorageKeys.IsDataKeyInGroup(key, @"Group\"), Is.True);
	}
}
