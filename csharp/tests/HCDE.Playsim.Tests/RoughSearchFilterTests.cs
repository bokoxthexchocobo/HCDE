using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RoughSearchFilterTests
{
    [Theory]
    [InlineData(0, 100, 100, 45, true)]
    [InlineData(0, 100, 100, 44, false)]
    [InlineData(0, 100, 0, 0, true)]
    [InlineData(0, 100, 0, -1, false)]
    [InlineData(350, 100, 0, 10, true)]
    [InlineData(350, 100, 0, 9, false)]
    [InlineData(0, -100, 0, 180, true)]
    [InlineData(0, -100, 0, 179, false)]
    public void CheckFovUsesNativeDeviationAndBamNormalization(int yaw, int x, int y, double fov, bool expected)
    {
        var observer = new Actor { Angle = BamAngle.FromDegrees(yaw) }; var target = new Actor(); target.SetXYZ(x, y, 0);
        Assert.Equal(expected, observer.CheckFov(target, fov));
    }

    [Theory]
    [InlineData(0, -50, 0, true)]
    [InlineData(0, 50, 0, false)]
    [InlineData(90, 0, 50, true)]
    [InlineData(90, 0, -50, false)]
    [InlineData(180, 50, 0, true)]
    [InlineData(180, -50, 0, false)]
    [InlineData(270, 0, -50, true)]
    [InlineData(270, 0, 50, false)]
    public void FrontOnlyUsesNativeDivlineRatherThanSymmetricFov(int yaw, int x, int y, bool expected)
    {
        var (sim, observer) = Room(); observer.Angle = BamAngle.FromDegrees(yaw);
        var target = sim.AddBot(192 + x, 192 + y);
        Assert.Equal(expected ? target : null, ActorEnemySearchActions.RoughMonsterSearch(observer, 1, frontOnly: true));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void FrontBoundaryIncludesNativeEqualEpsilon(int rawX, bool expected)
    {
        var (sim, observer) = Room(); var target = sim.AddBot(192, 210);
        target.X = new Fixed(observer.X.Raw + rawX);
        Assert.Equal(expected ? target : null, ActorEnemySearchActions.RoughMonsterSearch(observer, 0, frontOnly: true));
    }

    [Theory]
    [InlineData(45, true)]
    [InlineData(44, false)]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    public void RoughSearchAppliesOnlyPositiveFovWithoutHalving(int fov, bool expected)
    {
        var (sim, observer) = Room(); var target = sim.AddBot(292, 292);
        Assert.Equal(expected ? target : null, ActorEnemySearchActions.RoughMonsterSearch(observer, 1, fieldOfView: fov));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RejectedAngularCandidateDoesNotConsumeSightRandomness(bool front)
    {
        var (sim, observer) = Room(); var target = sim.AddBot(242, 242); target.Invisible = true;
        Assert.Null(ActorEnemySearchActions.RoughMonsterSearch(observer, 1, frontOnly: front, fieldOfView: front ? 0 : 10));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void FiltersContinueToNextEligibleCandidate()
    {
        var (sim, observer) = Room(); var accepted = sim.AddBot(242, 192); sim.AddBot(192, 242);
        Assert.Same(accepted, ActorEnemySearchActions.RoughMonsterSearch(observer, 1, fieldOfView: 10));
    }

    [Fact]
    public void FrontAndFovFiltersBothApply()
    {
        var (sim, observer) = Room(); var target = sim.AddBot(142, 192);
        Assert.Same(target, ActorEnemySearchActions.RoughMonsterSearch(observer, 1, frontOnly: true));
        Assert.Null(ActorEnemySearchActions.RoughMonsterSearch(observer, 1, frontOnly: true, fieldOfView: 90));
    }

    [Fact]
    public void NonfiniteFovFailsBeforeSearch()
    {
        var (sim, observer) = Room(); var target = sim.AddBot(242, 192); target.Invisible = true;
        Assert.Throws<ArgumentOutOfRangeException>(() => observer.CheckFov(target, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorEnemySearchActions.RoughMonsterSearch(observer, 1, fieldOfView: double.PositiveInfinity));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    private static (AuthoritySimulation Sim, Actor Observer) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 5, 5), []), Things = [] });
        return (sim, sim.AddBot(192, 192));
    }
}
