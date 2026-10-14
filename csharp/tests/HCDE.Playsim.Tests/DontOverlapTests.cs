using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DontOverlapTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void CombinedFlagsPreventGroundStepException(bool playerActor, bool bothFlags)
    {
        var sim = Room(); Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(-100, 0);
        actor.X = Fixed.FromInt(-100); actor.OnGround = true; actor.DontOverlap = true;
        var obstacle = sim.AddBot(0, 0); obstacle.Brain = null; obstacle.Height = Fixed.FromInt(16);
        obstacle.ActsLikeBridge = !playerActor; obstacle.DontOverlap = bothFlags;
        Assert.Equal(!bothFlags, ActorPhysics.TryMove(sim, actor, 0, 0, out _));
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void StationaryPlayerProbeUsesUnblockingUnlessCompatibilityDisablesIt(bool playerActor, bool legacy, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }] },
            compat: legacy ? CompatSurface.NoPassMobj : CompatSurface.None);
        Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(0, 0);
        actor.X = default; actor.DontOverlap = true;
        var obstacle = sim.AddBot(0, 0); obstacle.DontOverlap = true;
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked, ActorPhysics.HasPositionBlocker(sim, actor));
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void BothFlagsSuppressHeightPassage(bool moverFlag, bool obstacleFlag)
    {
        var sim = Room(); var actor = sim.AddBot(-100, 0); actor.Brain = null;
        actor.Z = Fixed.FromInt(64); actor.OnGround = false;
        var obstacle = sim.AddBot(0, 0); obstacle.Brain = null;
        ActorPropertyActions.ChangeFlag(actor, "DONTOVERLAP", moverFlag);
        ActorPropertyActions.ChangeFlag(obstacle, "dontoverlap", obstacleFlag);
        Assert.True(AcsActorFlags.TryGet(actor, "DONTOVERLAP", out var flag)); Assert.Equal(moverFlag, flag);
        actor.X = default;
        var before = SimSavegame.Write(sim);
        Assert.Equal(moverFlag && obstacleFlag, ActorPhysics.HasPositionBlocker(sim, actor));
        Assert.Equal(moverFlag && obstacleFlag ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2));
        Assert.Equal(before, SimSavegame.Write(sim));
        actor.X = Fixed.FromInt(-100);
        Assert.Equal(!(moverFlag && obstacleFlag), ActorPhysics.TryMove(sim, actor, 0, 0, out _));
    }

    [Theory]
    [InlineData(30, true)]
    [InlineData(10, false)]
    public void ExistingPlayerOverlapCanMoveAwayButNotFurtherInside(int x, bool moved)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.X = Fixed.FromInt(20);
        actor.Z = Fixed.FromInt(64); actor.OnGround = false; actor.DontOverlap = true;
        var obstacle = sim.AddBot(0, 0); obstacle.Brain = null; obstacle.DontOverlap = true;
        Assert.Equal(moved, ActorPhysics.TryMove(sim, actor, x, 0, out _));
    }

    [Fact]
    public void OptionalArchiveAndChecksumPreserveFlagsAndLegacyDefaults()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var baseline = SimSavegame.Write(sim); var hash = sim.Checksum;
        actor.DontOverlap = true; actor.CanPass = false;
        var bytes = SimSavegame.Write(sim);
        Assert.True(ArchiveLayerFixture.TrailerStart(bytes, 195) >= 16);
        SimSavegame.Apply(sim, bytes); Assert.True(actor.DontOverlap); Assert.False(actor.CanPass);
        Assert.Equal(bytes, SimSavegame.Write(sim)); Assert.NotEqual(hash, sim.Checksum);
        sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, baseline); Assert.False(actor.DontOverlap); Assert.True(actor.CanPass);
        Assert.Equal(baseline, SimSavegame.Write(sim));
    }

    [Fact]
    public void InvalidArchiveFlagIsRejected()
    {
        var sim = Room(); sim.AddBot(0, 0).DontOverlap = true;
        var bytes = SimSavegame.Write(sim);
        var start = ArchiveLayerFixture.TrailerStart(bytes, 195);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-dont-overlap-value", error);
    }

    [Fact]
    public void CurrentModeledResurrectionRestoresUnflaggedDefault()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.DontOverlap = true;
        actor.Health = 0; actor.Corpse = true; ActorRaise.ReviveSupported(actor);
        Assert.False(actor.DontOverlap);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }] });
}
