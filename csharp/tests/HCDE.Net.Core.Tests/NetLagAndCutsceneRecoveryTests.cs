using HCDE.Net.Core;

namespace HCDE.Net.Core.Tests;

public class NetLagAndCutsceneRecoveryTests
{
    [Fact]
    public void ModifyFrac_WaitingState_ForcesUnity()
    {
        Assert.Equal(1.0, NetLagPresentationPolicy.ModifyFrac(NetLagState.Waiting, 0.25));
        Assert.Equal(0.25, NetLagPresentationPolicy.ModifyFrac(NetLagState.None, 0.25));
    }

    [Fact]
    public void ModifyParticleFrac_WaitingState_ZerosParticles()
    {
        Assert.Equal(0.0, NetLagPresentationPolicy.ModifyParticleFrac(NetLagState.Waiting, 0.5));
        Assert.Equal(0.5, NetLagPresentationPolicy.ModifyParticleFrac(NetLagState.Skipping, 0.5));
    }

    [Fact]
    public void AuthorityWaitWatchdog_ClearsStaleWaitingFlags()
    {
        var registry = new LivePeerNetRegistry(4);
        var room = new NetRoomCommandResetSession(registry);
        room.SetPlayerWaiting(1, true);
        var watchdog = new AuthorityWaitWatchdogSession();
        watchdog.Reset("runtime-resync", 100, room);
        Assert.False(room.PlayerWaiting[1]);
        Assert.True(watchdog.IsWithinGrace(100));
    }

    [Fact]
    public void SoloClientRecovery_ForcesEndAfterDelay()
    {
        var recovery = new CutsceneSoloClientRecoverySession();
        Assert.False(recovery.TryForceEndScreenJob(false, true, false, true, true, 0, true, 1, 100));
        Assert.False(recovery.TryForceEndScreenJob(false, true, false, true, true, 0, true, 1, 500));
        Assert.True(recovery.TryForceEndScreenJob(false, true, false, true, true, 0, true, 1, 2000));
    }
}
