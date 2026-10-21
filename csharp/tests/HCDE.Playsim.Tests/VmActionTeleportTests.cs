namespace HCDE.Playsim.Tests;

public class VmActionTeleportTests
{
    [Fact]
    public void BlockedByNoteleport_WithoutOverride()
    {
        Assert.True(VmActionTeleport.BlockedByNoteleport(VmActionTeleport.Mf2Noteleport, 0));
        Assert.False(VmActionTeleport.BlockedByNoteleport(VmActionTeleport.Mf2Noteleport, VmActionTeleport.TfOverride));
    }

    [Fact]
    public void RandomDecideIndex_Clamps()
    {
        Assert.Equal(8, VmActionTeleport.RandomDecideIndex(9999, 1));
    }

    [Fact]
    public void ComputeFinalZ_CeilingClamp()
    {
        var z = VmActionTeleport.ComputeFinalZ(10, 0, 12, 4, 20);
        Assert.Equal(8, z);
    }

    [Fact]
    public void ShouldAbortRandomDecide_WhenRollFails()
    {
        Assert.True(VmActionTeleport.ShouldAbortRandomDecide(VmActionTeleport.TfRandomdecide, 200, 0));
    }

    [Fact]
    public void SourceFogUsesSpawnedClass_WhenNotActorFog()
    {
        Assert.True(VmActionTeleport.SourceFogUsesSpawnedClass(0, false));
        Assert.False(VmActionTeleport.SourceFogUsesSpawnedClass(VmActionTeleport.TfUseactorfog, false));
    }
}
