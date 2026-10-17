using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RoughMonsterSearchTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void CurrentCellIsAlwaysSearched(int distance)
    {
        var (sim, observer) = Room(); var target = sim.AddBot(210, 210);
        Assert.Same(target, ActorEnemySearchActions.RoughMonsterSearch(observer, distance));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public void DistanceCountsBlockRings(int distance, bool found)
    {
        var (sim, observer) = Room(); var target = sim.AddBot(448, 192);
        Assert.Equal(found ? target : null, ActorEnemySearchActions.RoughMonsterSearch(observer, distance));
    }

    [Fact]
    public void EarlierRingWinsOverActorCreationOrder()
    {
        var (sim, observer) = Room(); var near = sim.AddBot(320, 192); sim.AddBot(448, 192);
        Assert.Same(near, ActorEnemySearchActions.RoughMonsterSearch(observer, 3));
    }

    [Fact]
    public void FirstRowScansLeftToRightBeforeOtherEdges()
    {
        var (sim, observer) = Room(); var first = sim.AddBot(64, 64);
        sim.AddBot(320, 64); sim.AddBot(320, 320);
        Assert.Same(first, ActorEnemySearchActions.RoughMonsterSearch(observer, 1));
    }

    [Fact]
    public void SameCellUsesManagedReverseInsertionOrder()
    {
        var (sim, observer) = Room(); sim.AddBot(210, 210); var last = sim.AddBot(215, 215);
        Assert.Same(last, ActorEnemySearchActions.RoughMonsterSearch(observer, 0));
    }

    [Fact]
    public void RadiusLinksActorIntoAdjacentSearchCell()
    {
        var (sim, observer) = Room(); var target = sim.AddBot(260, 192);
        Assert.Same(target, ActorEnemySearchActions.RoughMonsterSearch(observer, 0));
        target.NoBlockmap = true; Assert.Null(ActorEnemySearchActions.RoughMonsterSearch(observer, 1));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SeekabilityIsAnOptionalFilterBeforeSight(bool onlySeekable)
    {
        var (sim, observer) = Room(); var target = sim.AddBot(210, 210); target.CantSeek = true;
        Assert.Equal(onlySeekable ? null : target, ActorEnemySearchActions.RoughMonsterSearch(observer, 0, onlySeekable));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void OutsideMapOriginReachesFirstCellAtNativeRing()
    {
        var (sim, observer) = Room(); observer.SetXYZ(-192, 64, 0); var target = sim.AddBot(64, 64);
        Assert.Null(ActorEnemySearchActions.RoughMonsterSearch(observer, 1));
        Assert.Same(target, ActorEnemySearchActions.RoughMonsterSearch(observer, 2));
    }

    [Fact]
    public void DuplicateCornerChecksRetainNativeSightDraws()
    {
        var (sim, observer) = Room(); var (reference, _) = Room(); var target = sim.AddBot(320, 64); target.Invisible = true;
        var visible = reference.NextCheckSightByte() <= 50;
        if (!visible) visible = reference.NextCheckSightByte() <= 50;
        Assert.Equal(visible ? target : null, ActorEnemySearchActions.RoughMonsterSearch(observer, 1));
        Assert.Equal(reference.CaptureState().CheckSightRandomState, sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void ProjectileSearchRejectsOwnerAndFindsMonster()
    {
        var (sim, owner) = Room(); var target = sim.AddBot(210, 210);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall); missile.SetXYZ(192, 192, 0);
        Assert.Same(target, ActorEnemySearchActions.RoughMonsterSearch(missile, 0, true));
    }

    [Fact]
    public void RepeatedSearchAndVisibilityStateResumeAfterSaveRestore()
    {
        var (sim, observer) = Room(); var target = sim.AddBot(320, 64); target.Invisible = true;
        ActorEnemySearchActions.RoughMonsterSearch(observer, 1); var bytes = SimSavegame.Write(sim, true);
        var expected = Enumerable.Range(0, 16).Select(_ => ActorEnemySearchActions.RoughMonsterSearch(observer, 1)?.Id).ToArray();
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(expected, Enumerable.Range(0, 16).Select(_ => ActorEnemySearchActions.RoughMonsterSearch(observer, 1)?.Id).ToArray());
    }

    private static (AuthoritySimulation Sim, Actor Observer) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 5, 5), []), Things = [] });
        return (sim, sim.AddBot(192, 192));
    }
}
