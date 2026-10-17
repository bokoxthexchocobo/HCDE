using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlockedBySolidActorsTests
{
    [Theory]
    [InlineData(false, true, false, true)]
    [InlineData(true, true, false, false)]
    [InlineData(true, false, false, true)]
    [InlineData(true, true, true, true)]
    public void NonsolidMoverFlagControlsBlockingWithoutChangingTargetRules(bool flag, bool solid, bool noclip, bool moves)
    {
        var (sim, mover, target) = Room(); mover.BlockedBySolidActors = flag;
        target.Solid = solid; target.NoClip = noclip;
        Assert.Equal(moves, ActorPhysics.TryMove(sim, mover, 8, 0, out _));
        Assert.Equal(moves ? 8 : 0, mover.X.ToDouble()); Assert.False(mover.Solid);
    }

    [Fact]
    public void BlockingFlagDoesNotMakeMoverSolidEnoughToDetonateTouchy()
    {
        var (sim, mover, target) = Room(); mover.BlockedBySolidActors = true;
        target.Touchy = target.Armed = true; target.SeeState = -1;
        Assert.False(ActorPhysics.TryMove(sim, mover, 8, 0, out _));
        Assert.False(target.IsDead); Assert.True(target.Armed);
    }

    [Fact]
    public void BlockingStillTransfersPushBeforeRefusingMovement()
    {
        var (sim, mover, target) = Room(); mover.BlockedBySolidActors = target.Pushable = true;
        mover.VelocityX = Fixed.FromInt(8); ActorPhysics.Step(sim, mover);
        Assert.Equal(0, mover.X.Raw); Assert.Equal(2, target.VelocityX.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SquareProbeIncludesNonsolidMoverOnlyWithFlag(bool flag)
    {
        var (sim, mover, target) = Room(); mover.BlockedBySolidActors = flag; mover.X = Fixed.FromInt(8);
        Assert.Same(flag ? target : null, ActorPhysics.FindPositionBlocker(sim, mover, out _, squareBounds: true));
    }

    [Fact]
    public void AcsFlagSupportsCaseInsensitiveReadAndWrite()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "Actor.BlockedBySolidActors", true));
        Assert.True(AcsActorFlags.TryGet(actor, "blockedbysolidactors", out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, "BLOCKEDBYSOLIDACTORS", false)); Assert.False(actor.BlockedBySolidActors);
    }

    [Fact]
    public void OptionalArchiveAndChecksumRestoreFlagAndOldSaveDefault()
    {
        var (sim, mover, _) = Room(); sim.RestoreState(sim.CaptureState());
        var older = SimSavegame.Write(sim, true); var baseline = sim.Checksum;
        mover.BlockedBySolidActors = true; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
        var saved = SimSavegame.Write(sim, true); Assert.Equal(321, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var (restored, loaded, _) = Room(); SimSavegame.Apply(restored, saved);
        Assert.True(loaded.BlockedBySolidActors); Assert.Equal(saved, SimSavegame.Write(restored, true));
        Assert.False(ActorPhysics.TryMove(restored, loaded, 8, 0, out _));
        SimSavegame.Apply(restored, older); Assert.False(loaded.BlockedBySolidActors);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void InvalidArchiveFlagIsRejected(int value)
    {
        var (sim, mover, _) = Room(); mover.BlockedBySolidActors = true;
        var bytes = SimSavegame.Write(sim, true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-blockedbysolidactors-value", error);
    }

    [Fact]
    public void ResurrectionRestoresSupportedClassDefault()
    {
        var (_, mover, _) = Room(); mover.Health = 0; mover.BlockedBySolidActors = true;
        ActorRaise.ReviveSupported(mover); Assert.False(mover.BlockedBySolidActors);
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var mover = sim.AddBot(0, 0, 3001); var target = sim.AddBot(40, 0, 3004);
        mover.Brain = target.Brain = null; mover.Solid = false;
        return (sim, mover, target);
    }
}
