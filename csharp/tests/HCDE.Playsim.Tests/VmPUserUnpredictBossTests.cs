namespace HCDE.Playsim.Tests;

public class VmPUserUnpredictBossTests
{
    [Fact]
    public void Unpredict_ClearsCheatWhenAlreadyDisabled() =>
        Assert.True(VmPUserUnpredictBoss.UnpredictAlreadyDisabled(false));

    [Fact]
    public void Unpredict_ConsolePlayerInRange() =>
        Assert.True(VmPUserUnpredictBoss.ClearPredictingCheatInRange(0));

    [Fact]
    public void Unpredict_RestoresWeaponWhenPredicting() =>
        Assert.True(VmPUserUnpredictBoss.RestoreWeaponAfterRollback(true, true));

    [Fact]
    public void BossExit_BlocksNoExitDmflags() =>
        Assert.True(VmPUserUnpredictBoss.BossNoExitDmflags(true, false, 1, 1));

    [Fact]
    public void BossExit_PicksLowerFloorSpec()
    {
        Assert.True(VmPUserUnpredictBoss.TryPickBossSpecAction(
            new VmPUserUnpredictBoss.BossFloorDoorPick(1, 1, 2, 3), out var kind));
        Assert.Equal(1, kind);
    }

    [Fact]
    public void Massacre_SkipsFriendlyWhenBaddiesOnly() =>
        Assert.False(VmPUserUnpredictBoss.MassacreActorEligible(
            new VmPUserUnpredictBoss.MassacreActorGate(0, 1, 1, 0, 1, 1, true)));

    [Fact]
    public void ClearPrediction_UsesAuthoritativeBaseSequence() =>
        Assert.Equal(12, VmPUserUnpredictBoss.ResolveLastPredictedTic(
            new VmPUserUnpredictBoss.ClearPredictionSeatContext(true, true, 12, 99)));

    [Fact]
    public void ClearPrediction_FallsBackToGametic() =>
        Assert.Equal(99, VmPUserUnpredictBoss.ResolveLastPredictedTic(
            new VmPUserUnpredictBoss.ClearPredictionSeatContext(true, false, 0, 99)));
}
