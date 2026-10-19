using HCDE.Net.Core;

namespace HCDE.Net.Core.Tests;

public class CutsceneAdvanceSessionTests
{
    private static IReadOnlyList<CutsceneParticipant> TwoHumans() =>
    [
        new(0, true, false, false, false),
        new(1, true, false, false, false),
    ];

    [Fact]
    public void StartCutscene_AppliesCountdownForNetGame()
    {
        var session = new CutsceneAdvanceSession();
        session.StartCutscene(new CutsceneStartOptions(false, true, false, 3f, 0f, 0));
        Assert.Equal(105, session.CountdownTics);
    }

    [Fact]
    public void VoteSatisfied_AllHumansReady_AdvancesBeforeCountdown()
    {
        var session = new CutsceneAdvanceSession { RunnerActive = true };
        session.StartCutscene(new CutsceneStartOptions(false, true, false, 10f, 0f, 0));
        var participants = TwoHumans();
        session.PlayerReadiedUp(0, 100, participants, true, false);
        session.PlayerReadiedUp(1, 101, participants, true, false);
        var advanced = session.TryTickAdvance(CutsceneReadyType.Vote, 1f, participants, 0, out var reason);
        Assert.True(advanced);
        Assert.Equal(CutsceneAdvanceReason.ReadyVote, reason);
        Assert.True(session.CountdownTics > 0);
    }

    [Fact]
    public void VoteSatisfied_AnyoneMode_RequiresSingleReadyBit()
    {
        var session = new CutsceneAdvanceSession { RunnerActive = true };
        var participants = TwoHumans();
        session.PlayerReadiedUp(1, 50, participants, true, false);
        bool Ready(int slot) => session.IsPlayerReady(slot, false, CutsceneReadyType.Vote, participants);
        Assert.True(session.IsVoteSatisfied(CutsceneReadyType.Anyone, 1f, participants, 0, Ready));
    }

    [Fact]
    public void CountdownExpired_AdvancesWhenVoteFails()
    {
        var session = new CutsceneAdvanceSession { RunnerActive = true };
        session.StartCutscene(new CutsceneStartOptions(false, true, false, 1f / CutsceneAdvanceSession.TicRate, 0f, 0));
        var participants = TwoHumans();
        var advanced = session.TryTickAdvance(CutsceneReadyType.Vote, 1f, participants, 0, out var reason);
        Assert.True(advanced);
        Assert.Equal(CutsceneAdvanceReason.CountdownExpired, reason);
    }

    [Fact]
    public void AdvanceCutscene_ClearsReadyAndCountdown()
    {
        var session = new CutsceneAdvanceSession();
        session.StartCutscene(new CutsceneStartOptions(false, true, false, 5f, 0f, 0));
        session.PlayerReadiedUp(0, 10, [new(0, true, false, false, false)], true, false);
        var effects = session.AdvanceCutscene();
        Assert.True(effects.AuthorityShouldEndScreenJob);
        Assert.Equal(0, session.CountdownTics);
        Assert.Equal(0ul, session.ReadyMask);
    }

    [Fact]
    public void LateJoinParticipant_ExcludedFromVote()
    {
        var participants = new[]
        {
            new CutsceneParticipant(0, true, false, true, false),
            new CutsceneParticipant(1, true, false, false, false),
        };
        var session = new CutsceneAdvanceSession { RunnerActive = true };
        session.PlayerReadiedUp(1, 20, participants, true, false);
        bool Ready(int slot) => session.IsPlayerReady(slot, false, CutsceneReadyType.Vote, participants);
        Assert.True(session.IsVoteSatisfied(CutsceneReadyType.Vote, 1f, participants, 0, Ready));
    }
}
