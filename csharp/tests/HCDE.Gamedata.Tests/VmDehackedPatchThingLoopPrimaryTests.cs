namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchThingLoopPrimaryTests
{
    [Fact]
    public void MapFrameLabelPrefix_Spawn() =>
        Assert.Equal("Spawn", VmDehackedPatchThingLoopPrimary.MapFrameLabelPrefix("Initial frame"));

    [Fact]
    public void SkipPlayerMeleeMissile_BlocksPlayer() =>
        Assert.True(VmDehackedPatchThingLoopPrimary.SkipPlayerMeleeMissile(1, "Melee"));

    [Fact]
    public void ClampInfightingGroup_NegativeToZero() =>
        Assert.Equal(0, VmDehackedPatchThingLoopPrimary.ClampInfightingGroup(-1));
}
