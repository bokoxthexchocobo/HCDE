using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LineAttackPuffFlagTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(false, 3)]
    [InlineData(false, 4)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    [InlineData(true, 3)]
    [InlineData(true, 4)]
    public void AcsFlagControlsOnlyPuffHeightDraws(bool geometry, int flags)
    {
        var sim = Room(geometry); var player = sim.Players.Single();
        var trace = CombatTrace.TraceLineAttack(sim, player, player.Angle, new BamAngle(0), 256);
        Assert.True(trace.Hit);
        Invoke(sim, player, flags);
        var puff = sim.Actors.OfType<PuffActor>().Single();
        Assert.Equal(trace.Z + ((flags & 1) != 0 ? 0 : 15 / 64.0), puff.Z.ToDouble());
        Assert.Equal(77, puff.ThingId); Assert.Same(player, puff.DamageSource);
        var control = Room(geometry);
        var expected = control.SpawnHitscanPuff(100, 0, trace.Z, noRandomZ: (flags & 1) != 0);
        Assert.Equal(expected.States.RemainingTics, puff.States.RemainingTics);
        Assert.Equal(control.CaptureState().PuffRandomState, sim.CaptureState().PuffRandomState);
        if (!geometry) Assert.Equal(1000, sim.Actors.Single(actor => actor.DoomEdNum == 3004).Health);
    }

    [Fact]
    public void FlagAppliesToEveryTidSourceWithIndependentPuffDraws()
    {
        var sim = Room(true); var player = sim.Players.Single(); player.ThingId = 42;
        var second = sim.AddBot(0, 32); second.Brain = null; second.ThingId = 42;
        Invoke(sim, player, 1, tid: 42);
        var puffs = sim.Actors.OfType<PuffActor>().ToArray(); Assert.Equal(2, puffs.Length);
        var sources = new[] { player.Id, second.Id };
        Assert.Equal(sources.Order(), puffs.Select(puff => puff.DamageSource!.Id).Order());
        foreach (var puff in puffs)
        {
            var source = puff.DamageSource!;
            var hit = CombatTrace.TraceLineAttack(sim, source, new BamAngle(0), new BamAngle(0), 256);
            Assert.Equal(hit.Z, puff.Z.ToDouble());
        }
        var control = Room(true);
        control.SpawnHitscanPuff(0, 0, 32, noRandomZ: true); control.SpawnHitscanPuff(0, 0, 32, noRandomZ: true);
        Assert.Equal(control.CaptureState().PuffRandomState, sim.CaptureState().PuffRandomState);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void HorizonConsumesNoPuffDrawsRegardlessOfFlag(int flags)
    {
        var sim = Room(true); sim.Level.Lines[0].Special = 9;
        Invoke(sim, sim.Players.Single(), flags);
        Assert.Empty(sim.Actors.OfType<PuffActor>()); Assert.Null(sim.CaptureState().PuffRandomState);
    }

    [Fact]
    public void FlaggedAcsAttackReplaysAfterIdentityRestore()
    {
        var sim = Room(true); var player = sim.Players.Single(); var before = SimSavegame.Write(sim, true);
        Invoke(sim, player, 1); sim.Tick(); var expected = SimSavegame.Write(sim, true); var checksum = sim.Checksum;
        SimSavegame.Apply(sim, before); Invoke(sim, player, 1); sim.Tick();
        Assert.Equal(expected, SimSavegame.Write(sim, true)); Assert.Equal(checksum, sim.Checksum);
    }

    private static void Invoke(AuthoritySimulation sim, Actor source, int flags, int tid = 0)
    {
        var stack = new List<int> { tid, 0, 0, 7, 0, 0, 256 << 16, flags, 77 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = source },
            [], new AcsGlobalStrings(), AcsCallFunctions.LineAttack, 9, out var result));
        Assert.Equal(0, result); Assert.Empty(stack);
    }

    private static AuthoritySimulation Room(bool geometry)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = geometry ? [new LevelLine { X1 = 128, X2 = 128, Y1 = 128, Y2 = -128, SideFront = 0, SideBack = -1 }] : [],
            Things = [new LevelThing { Type = 1 }]
        });
        if (!geometry)
        {
            var target = sim.AddBot(128, 0); target.Brain = null; target.Health = 1000; target.NoPain = true; target.Invulnerable = true;
        }
        return sim;
    }
}
