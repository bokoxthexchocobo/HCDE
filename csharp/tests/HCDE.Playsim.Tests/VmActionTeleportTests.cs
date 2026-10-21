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
}
