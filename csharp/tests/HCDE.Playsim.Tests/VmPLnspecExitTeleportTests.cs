namespace HCDE.Playsim.Tests;

public class VmPLnspecExitTeleportTests
{
    [Fact]
    public void PolyStopSound_WhenPolyFound()
    {
        Assert.True(VmPLnspecExitTeleport.PolyStopSoundFound(new object()));
    }

    [Fact]
    public void ExitSecret_UsesSecretMapLookup()
    {
        Assert.True(VmPLnspecExitTeleport.ExitSecretUsesSecretMap(true));
    }

    [Fact]
    public void TeleportNewMap_RejectsBacksideUnlessStrife()
    {
        Assert.True(VmPLnspecExitTeleport.TeleportNewMapAllowBackside(4));
    }

    [Fact]
    public void Teleport_AddsSourceFogWhenArg2Zero()
    {
        Assert.Equal(7, VmPLnspecExitTeleport.TeleportFlags(false));
    }

    [Fact]
    public void TeleportNoFog_OrientationCase2UsesBoomRotate()
    {
        Assert.Equal(48, VmPLnspecExitTeleport.TeleportNoFogFlags(2, true));
    }

    [Fact]
    public void ZombieChanger_RequiresActivator()
    {
        Assert.False(VmPLnspecExitTeleport.ZombieChangerNeedsActivator(null));
    }

    [Fact]
    public void TeleportLine_DelegatesSilentLine()
    {
        Assert.True(VmPLnspecExitTeleport.TeleportLineUsesSilent(true));
    }
}
