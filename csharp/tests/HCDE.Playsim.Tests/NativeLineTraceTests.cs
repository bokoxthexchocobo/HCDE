using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeLineTraceTests
{
    [Theory]
    [InlineData(0, 12, 31, 26)]
    [InlineData(2, 13, 30, 26)]
    [InlineData(1, 3, 10, 20)]
    [InlineData(3, 3, 10, 20)]
    public void OriginModesUseNativeOffsetRotationAndClip(int flags, double x, double y, double z)
    {
        var sim = Room(); var source = sim.Players.Single(); source.SetXYZ(10, 20, 10); source.FloorClip = 4;
        var hit = CombatTrace.LineTrace(sim, source, BamAngle.FromDegrees(90), 8,
            flags: (ActorLineTraceFlags)flags, offsetZ: 20, offsetForward: 3, offsetSide: 2);
        Assert.Null(hit.Victim); Assert.Equal(x, hit.X, 8); Assert.Equal(y, hit.Y, 8); Assert.Equal(z, hit.Z, 8);
    }

    [Fact]
    public void SpeciesAndActorFiltersSkipMatchingIntersections()
    {
        var sim = Room(); var source = sim.Players.Single(); source.Species = "Same";
        var near = sim.AddBot(64, 0); near.Species = "same"; var far = sim.AddBot(128, 0); far.Species = "Other";
        Assert.Same(near, Trace(sim, source).Victim);
        Assert.Same(far, Trace(sim, source, ActorLineTraceFlags.ThroughSpecies).Victim);
        Assert.Null(Trace(sim, source, ActorLineTraceFlags.ThroughActors).Victim);
    }

    [Theory]
    [InlineData(false, ActorLineTraceFlags.AllActors)]
    [InlineData(true, ActorLineTraceFlags.SolidActors)]
    public void ActorMaskCanIncludeNonShootableTargets(bool solid, ActorLineTraceFlags flags)
    {
        var sim = Room(); var source = sim.Players.Single(); var target = sim.AddBot(64, 0);
        target.Shootable = false; target.Solid = solid;
        Assert.Null(Trace(sim, source).Victim); Assert.Same(target, Trace(sim, source, flags).Victim);
    }

    [Theory]
    [InlineData(LevelLine.BlockHitscanFlag, ActorLineTraceFlags.ThroughHitscan)]
    [InlineData(LevelLine.BlockEverythingFlag, ActorLineTraceFlags.ThroughBlock)]
    public void ThroughFlagsRemoveOnlyTheirWallMask(int wallFlag, ActorLineTraceFlags flags)
    {
        var sim = WallRoom(wallFlag); var source = sim.Players.Single(); var target = sim.AddBot(128, 0);
        Assert.NotNull(Trace(sim, source).Wall); Assert.Same(target, Trace(sim, source, flags).Victim);
    }

    [Fact]
    public void BlockUseAddsUseBlockingToTrace()
    {
        var sim = WallRoom(LevelLine.BlockUseFlag); var source = sim.Players.Single(); var target = sim.AddBot(128, 0);
        Assert.Same(target, Trace(sim, source).Victim);
        Assert.NotNull(Trace(sim, source, ActorLineTraceFlags.BlockUse).Wall);
    }

    [Theory]
    [InlineData(4096)]
    [InlineData(2048)]
    public void UnsupportedNativeFlagsRejectWithoutMutation(int flags)
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true);
        Assert.Throws<NotSupportedException>(() => Trace(sim, sim.Players.Single(), (ActorLineTraceFlags)flags));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void NonfiniteOffsetRejectsWithoutMutation()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true);
        Assert.Throws<ArgumentOutOfRangeException>(() => CombatTrace.LineTrace(sim, sim.Players.Single(), default, 100, offsetZ: double.NaN));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    private static CombatTrace.LineAttackHit Trace(AuthoritySimulation sim, Actor actor, ActorLineTraceFlags flags = 0)
        => CombatTrace.LineTrace(sim, actor, default, 256, flags: flags, offsetZ: 28);

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });

    private static AuthoritySimulation WallRoom(int flags) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 256 }, new LevelSector { CeilingHeight = 256 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { X1 = 64, Y1 = -128, X2 = 64, Y2 = 128, SideFront = 0, SideBack = 1, Flags = flags }],
        Things = [new LevelThing { Type = 1 }]
    });
}
