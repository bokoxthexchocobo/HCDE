namespace HCDE.Playsim.Tests;

public class VmPEnemyResurrectTests
{
    [Fact]
    public void CanResurrect_RejectsNullRaiser() =>
        Assert.False(VmPEnemyResurrect.EvaluateCanResurrect(
            new VmPEnemyResurrect.CanResurrectContext(false, null, null, false, true, false, true)));

    [Fact]
    public void CanResurrect_BlocksActiveVirtual() =>
        Assert.False(VmPEnemyResurrect.EvaluateCanResurrect(
            new VmPEnemyResurrect.CanResurrectContext(true, new object(), new object(), true, false, false, true)));

    [Fact]
    public void CanResurrect_SelfSkipsPassive()
    {
        var self = new object();
        Assert.True(VmPEnemyResurrect.EvaluateCanResurrect(
            new VmPEnemyResurrect.CanResurrectContext(true, self, self, false, true, false, false)));
    }

    [Fact]
    public void ResurrectionCandidate_BlocksNoMoveDir() =>
        Assert.False(VmPEnemyResurrect.EvaluateResurrectionCandidate(SampleCandidate(moveDir: 8)));

    [Fact]
    public void ResurrectionCandidate_BlocksFarCorpse() =>
        Assert.False(VmPEnemyResurrect.EvaluateResurrectionCandidate(SampleCandidate(corpseX: 0, probeX: 200)));

    [Fact]
    public void ResurrectionCandidate_RequiresSightAcrossPortals() =>
        Assert.False(VmPEnemyResurrect.EvaluateResurrectionCandidate(
            SampleCandidate(corpsePortal: 1, raiserPortal: 2, sightOk: false)));

    [Fact]
    public void ResurrectionCandidate_AcceptsWhenEligible() =>
        Assert.True(VmPEnemyResurrect.EvaluateResurrectionCandidate(SampleCandidate()));

    [Fact]
    public void FriendlyTargetCleanup_ClearsStaleEnemy() =>
        Assert.True(VmPEnemyResurrect.EvaluateFriendlyTargetCleanup(
            new VmPEnemyResurrect.FriendlyTargetCleanupContext(true, true, false, false)));

    [Fact]
    public void HealStatePick_UsesArchvileFallback() =>
        Assert.True(VmPEnemyResurrect.EvaluateHealStateEntry(
            new VmPEnemyResurrect.HealStatePickContext(false, false, true, true)));

    [Fact]
    public void CorpseDimensions_RestoreWhenNoGhostCompat() =>
        Assert.Equal(56, VmPEnemyResurrect.EvaluateCorpseDimensions(
            new VmPEnemyResurrect.CorpseDimensionContext(0, 10, 56, 20, 1, 0)).Height);

    [Fact]
    public void CorpseDimensions_GhostAlphaWhenZeroHeight() =>
        Assert.Equal(0.5, VmPEnemyResurrect.EvaluateCorpseDimensions(
            new VmPEnemyResurrect.CorpseDimensionContext(
                VmPEnemyResurrect.CompatVileGhosts, 0, 56, 20, 1, 0)).Alpha);

    private static VmPEnemyResurrect.ResurrectionCandidateContext SampleCandidate(
        int moveDir = 0,
        double corpseX = 10,
        double probeX = 10,
        int corpsePortal = 0,
        int raiserPortal = 0,
        bool sightOk = true) =>
        new(
            moveDir,
            true,
            corpseX,
            10,
            probeX,
            10,
            16,
            16,
            corpsePortal,
            raiserPortal,
            sightOk,
            false,
            false,
            true,
            false,
            false,
            0,
            0,
            40,
            true,
            true);
}
