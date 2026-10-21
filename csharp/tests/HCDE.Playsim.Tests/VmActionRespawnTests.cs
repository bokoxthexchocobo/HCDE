namespace HCDE.Playsim.Tests;

public class VmActionRespawnTests
{
    [Fact]
    public void UseTelefragMove_WhenFlagSet() =>
        Assert.True(VmActionRespawn.UseTelefragMove(VmActionRespawn.RsfTelefrag));

    [Fact]
    public void ShouldClearTarget_WhenKeepTargetUnset() =>
        Assert.True(VmActionRespawn.ShouldClearTarget(0));

    [Fact]
    public void ShouldClearSelfTarget_WhenSameReference() =>
        Assert.True(VmActionRespawn.ShouldClearSelfTarget("x", "x"));

    [Fact]
    public void ShouldSpawnFog_WhenFlagSet() =>
        Assert.True(VmActionRespawn.ShouldSpawnFog(VmActionRespawn.RsfFog));

    [Fact]
    public void MergeFriendlyFlags_PreservesFriendlyBit() =>
        Assert.Equal(0x4000000, VmActionRespawn.MergeFriendlyFlags(0, 0x4000000));

    [Fact]
    public void RestoreDefaultHeight_Passthrough() =>
        Assert.Equal(56, VmActionRespawn.RestoreDefaultHeight(56));

    [Fact]
    public void ApplyRespawnSuccess_RestoresHealthWhenOk()
    {
        var r = VmActionRespawn.ApplyRespawnSuccess(true, 100, 0, 0, 1);
        Assert.True(r.ok);
        Assert.Equal(100, r.health);
    }

    [Fact]
    public void ApplySolidFlag_SetsSolid() =>
        Assert.NotEqual(0, VmActionRespawn.ApplySolidFlag(0) & VmActionRespawn.MfSolid);

    [Fact]
    public void ApplyRespawnSuccess_ClearsInvisible() =>
        Assert.Equal(0, VmActionRespawn.ApplyRespawnSuccess(true, 1, 0, 0, VmActionRespawn.RfInvisible).renderFlags);
}
