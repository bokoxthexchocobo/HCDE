namespace HCDE.Playsim.Tests;

public class VmPEnemyResurrectionTests
{
    [Fact]
    public void CanResurrect_RejectsNullRaiser() =>
        Assert.False(VmPEnemyResurrection.EvaluateCanResurrect(
            new VmPEnemyResurrection.CanResurrectContext(false, null, null, false, true, false, true)));

    [Fact]
    public void ResurrectionCandidate_AcceptsWhenEligible() =>
        Assert.True(VmPEnemyResurrection.EvaluateResurrectionCandidate(SampleCandidate()));

    [Fact]
    public void ResurrectionCandidate_BlocksFarCorpse() =>
        Assert.False(VmPEnemyResurrection.EvaluateResurrectionCandidate(SampleCandidate(corpseX: 0, probeX: 200)));

    [Fact]
    public void CorpseDimensions_GhostAlphaWhenZeroHeight() =>
        Assert.Equal(0.5, VmPEnemyResurrection.EvaluateCorpseDimensions(
            new VmPEnemyResurrection.CorpseDimensionContext(
                VmPEnemyResurrection.CompatVileGhosts, 0, 56, 20, 1, 0)).Alpha);

    private static VmPEnemyResurrection.ResurrectionCandidateContext SampleCandidate(
        int moveDir = 0,
        double corpseX = 10,
        double probeX = 10) =>
        new(
            moveDir,
            true,
            corpseX,
            10,
            probeX,
            10,
            16,
            16,
            0,
            0,
            true,
            false,
            false,
            true,
            false,
            false,
            0,
            0,
            56,
            true,
            true);
}
