using HCDE.Net.Core;

namespace HCDE.Net.Core.Tests;

public class NetRoomCommandResetTests
{
    [Fact]
    public void ResetCommands_AdoptsPendingRoomAndReanchorsSequences()
    {
        var registry = new LivePeerNetRegistry(4);
        registry[1].CurrentSequence = 40;
        var session = new NetRoomCommandResetSession(registry, 3) { PendingAdoptRoomId = 9 };
        session.SetPlayerWaiting(1, true);
        session.ResetCommands(false, 100, [1]);
        Assert.Equal(9, session.CurrentRoomId);
        Assert.False(session.PlayerWaiting[1]);
        Assert.Equal(9, registry[1].ResendId);
        Assert.Equal(99, registry[1].CurrentSequence);
        Assert.Equal(99, registry[1].AppliedSequence);
    }

    [Fact]
    public void CutsceneReady_SoloHuman_LatchesReady()
    {
        var coordinator = new CutsceneReadyCoordinator();
        coordinator.ToggleReady(0, 100, 1, true);
        Assert.True(coordinator.IsParticipantReady(0, true));
    }
}
