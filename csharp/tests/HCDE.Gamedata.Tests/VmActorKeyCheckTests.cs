namespace HCDE.Gamedata.Tests;

public class VmActorKeyCheckTests
{
    [Fact]
    public void OneKeyMatches_FindsKeyInInventory()
    {
        var owner = new VmInventoryItem { ClassName = "Player" };
        owner.Inventory.Add(new VmInventoryItem { ClassName = "RedCard" });
        Assert.True(VmActorKeyCheck.OneKeyMatches(owner, "RedCard"));
    }

    [Fact]
    public void LockMatches_EmptyKeyListRequiresAnyKey()
    {
        var owner = new VmInventoryItem();
        owner.Inventory.Add(new VmInventoryItem { ClassName = "BlueKey" });
        Assert.True(VmActorKeyCheck.LockMatches(owner, [], anyKeyIfEmpty: true));
    }

    [Fact]
    public void LockKeywords_IncludesAny()
    {
        Assert.Equal("ANY", VmLockDefKeywords.LockKeywords[0]);
    }
}
