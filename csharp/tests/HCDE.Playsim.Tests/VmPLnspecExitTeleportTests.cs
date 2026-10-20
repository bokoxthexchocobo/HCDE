namespace HCDE.Playsim.Tests;

public class VmPLnspecExitTeleportTests
{
    [Fact]
    public void PolyStopSound_WhenPolyFound() =>
        Assert.True(VmPLnspecExitTeleport.PolyStopSoundWhenFound(new object()));

    [Fact]
    public void ExitSecret_UsesSecretMapLookup()
    {
        var ex = VmPLnspecExitTeleport.BuildExitSecret(true, 1);
        Assert.True(ex.Secret);
    }

    [Fact]
    public void TeleportNewMap_RejectsBacksideUnlessStrife()
    {
        var m = VmPLnspecExitTeleport.BuildTeleportNewMap(true, 4, new object(), true, 0, 0);
        Assert.True(m.AllowSide);
    }

    [Fact]
    public void Teleport_AddsSourceFogWhenArg2Zero() =>
        Assert.Equal(7, VmPLnspecExitTeleport.TeleportBaseFlags(false));

    [Fact]
    public void TeleportNoFog_OrientationCase2UsesBoomRotate() =>
        Assert.Equal(48, VmPLnspecExitTeleport.TeleportNoFogOrientationFlags(2, true));

    [Fact]
    public void ZombieChanger_RequiresActivator() =>
        Assert.False(VmPLnspecExitTeleport.ThrustThingSucceeds(
            VmPLnspecExitTeleport.BuildThrustThing(0, false, false, null)));

    [Fact]
    public void TeleportEndGame_RejectsBackside() =>
        Assert.False(VmPLnspecExitTeleport.TeleportEndGame(true, true));

    [Fact]
    public void TeleportLine_DelegatesSilentLine() =>
        Assert.True(VmPLnspecExitTeleport.TeleportEndGame(false, true));

    [Fact]
    public void ThrustThing_ClampsVelocityWhenNotNoLimit()
    {
        VmPLnspecExitTeleport.ThrustThingHelper(0, 0, 0, 100, false, out var vx, out _);
        Assert.Equal(VmPLnspecExitTeleport.MaxMove, vx);
    }

    [Fact]
    public void ThrustThingZ_AddModeAccumulates()
    {
        var plan = VmPLnspecExitTeleport.BuildThrustZ(0, 8, false, true, new object());
        Assert.Equal(3, VmPLnspecExitTeleport.ApplyThrustZ(1, plan));
    }
}
