namespace HCDE.Gamedata.Tests;

public class VmDehackedFinishDehPatchTests
{
    [Fact]
    public void ApplyRetargetAfterSlamFlags_SetsMf8Bit()
    {
        Assert.Equal(0x80u, VmDehackedFinishDehPatch.ApplyRetargetAfterSlamFlags(0));
    }

    [Fact]
    public void ShouldCreatePickupWrapper_RequiresSpecial()
    {
        Assert.True(VmDehackedFinishDehPatch.ShouldCreatePickupWrapper(VmDehackedFinishDehPatch.MfSpecial));
    }

    [Fact]
    public void TryAllocatePickupClassName_SkipsExisting()
    {
        Assert.True(VmDehackedFinishDehPatch.TryAllocatePickupClassName(
            0,
            name => name == "DehackedPickup0",
            out var className,
            out var next));
        Assert.Equal("DehackedPickup1", className);
        Assert.Equal(2, next);
    }

    [Fact]
    public void LinkPickupReplacement_ChainsPriorReplacement()
    {
        var chain = new VmDehackedClassReplacementChain();
        VmDehackedFinishDehPatch.LinkPickupReplacement("DehackedPickup0", "HealthBonus", "HealthBonus2", chain);
        Assert.Equal("HealthBonus2", chain.Replacement);
        Assert.Equal("HealthBonus", chain.Replacee);
    }

    [Fact]
    public void MarkStateUseWeapon_SetsFlag()
    {
        Assert.Equal(1u, VmDehackedFinishDehPatch.MarkStateUseWeapon(0));
    }
}
