using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloorCarryFlagTests
{
    [Theory]
    [InlineData(true, false, 64, 0, 48)]
    [InlineData(true, true, 64, 0, 64)]
    [InlineData(false, true, 64, 0, 64)]
    [InlineData(true, false, 80, 0, 64)]
    [InlineData(true, false, 80, 2, 64)]
    [InlineData(true, true, 80, 0, 80)]
    [InlineData(false, false, 80, 0, 80)]
    [InlineData(true, false, 64, 2, 64)]
    public void NativeDropBranchOrdering(bool noGravity, bool noLiftDrop, int z, int velocity, int expected)
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.NoGravity = noGravity; actor.Z = Fixed.FromInt(z); actor.VelocityZ = Fixed.FromInt(velocity);
        ActorPropertyActions.ChangeFlag(actor, "NOLIFTDROP", noLiftDrop);
        ActorPropertyActions.ChangeFlag(actor, "RELATIVETOFLOOR", true);
        Drop(sim); LineSpecials.TickMotions(sim);
        Assert.Equal(expected, actor.Z.ToDouble()); Assert.Equal(velocity, actor.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(false, false, 64)]
    [InlineData(true, false, 80)]
    [InlineData(false, true, 80)]
    public void RelativeCarryHonorsMembershipAndBridge(bool noSector, bool bridge, int expected)
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.NoGravity = true; actor.RelativeToFloor = true; actor.Z = Fixed.FromInt(80);
        actor.NoSector = noSector; actor.ActsLikeBridge = bridge;
        Drop(sim); LineSpecials.TickMotions(sim);
        Assert.Equal(expected, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void FlagsRoundTripAndLegacyRestore(bool noLiftDrop, bool relativeToFloor)
    {
        var sim = Room(); var actor = sim.Players.Single();
        var baseline = SimSavegame.Write(sim); SimSavegame.Apply(sim, baseline); var hash = sim.Checksum;
        ActorPropertyActions.ChangeFlag(actor, "Actor.NoLiftDrop", noLiftDrop);
        ActorPropertyActions.ChangeFlag(actor, "relativetofloor", relativeToFloor);
        Assert.True(AcsActorFlags.TryGet(actor, "NOLIFTDROP", out var drop)); Assert.Equal(noLiftDrop, drop);
        Assert.True(AcsActorFlags.TryGet(actor, "Actor.RELATIVETOFLOOR", out var relative)); Assert.Equal(relativeToFloor, relative);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(193, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        SimSavegame.Apply(sim, bytes); Assert.NotEqual(hash, sim.Checksum);
        Assert.Equal(noLiftDrop, actor.NoLiftDrop); Assert.Equal(relativeToFloor, actor.RelativeToFloor);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, baseline); Assert.Equal(hash, sim.Checksum);
        Assert.False(actor.NoLiftDrop); Assert.False(actor.RelativeToFloor);
        Assert.Equal(baseline, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(4)]
    public void ArchiveRejectsUnknownFlagBits(int value)
    {
        var sim = Room(); sim.Players.Single().RelativeToFloor = true;
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-floor-carry-value", error);
    }

    [Fact]
    public void NoLiftDropDoesNotDisableGravityCarry()
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.NoLiftDrop = true; actor.MoveWithSector = true;
        Drop(sim); LineSpecials.TickMotions(sim);
        Assert.Equal(48, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(193)]
    [InlineData(194)]
    public void ArchiveRejectsRecursiveOrFuturePriorVersion(int version)
    {
        var sim = Room(); sim.Players.Single().RelativeToFloor = true;
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), version);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-floor-carry-header", error);
    }

    [Fact]
    public void ResurrectionRestoresSupportedClassDefaults()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] });
        var actor = sim.Actors.Single(); actor.Health = 0; actor.States.Enter(actor, ActorStateMachine.Corpse);
        actor.NoLiftDrop = actor.RelativeToFloor = true;
        Assert.True(ThingRaise.Execute(sim, 17, actor, 0, 2));
        Assert.False(actor.NoLiftDrop); Assert.False(actor.RelativeToFloor);
    }

    [Fact]
    public void RelativeCarrySaveContinuationIsDeterministic()
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.NoGravity = true; actor.RelativeToFloor = true; actor.Z = Fixed.FromInt(80); actor.OnGround = false;
        Drop(sim); sim.Tick(); Assert.Equal(64, actor.Z.ToDouble());
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
    }

    private static void Drop(AuthoritySimulation sim) => sim.Motions.Add(new SectorMotion
    { SectorIndex = 0, Kind = MotionKind.Floor, TargetFloor = 0, FloorDirection = -1, Speed = 16 });

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { FloorHeight = 64, CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
