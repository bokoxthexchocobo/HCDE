using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissileChanceMultTests
{
    [Theory]
    [InlineData(3004, 0, 0)]
    [InlineData(3004, 0.5, 54)]
    [InlineData(3004, 1.5, 162)]
    [InlineData(3004, -1, -108)]
    [InlineData(16, 1, 108)]
    [InlineData(0, 0.5, 54)]
    public void OverrideScalesAdjustedDistanceBeforeCap(int type, double multiplier, int threshold)
    {
        var (sim, actor) = Room(); var (reference, _) = Room(); actor.MissileChanceMult = multiplier;
        for (var i = 0; i < 64; i++)
        {
            var expected = reference.NextCheckMissileByte() >= threshold;
            actor.Brain = type == 0 ? new MonsterBrain(MonsterAttack.Hitscan) : MonsterBrain.ForType(type);
            actor.SetTargetPointer(sim.Players.Single()); actor.ReactionTime = 0;
            actor.MoveCount = 0; actor.JustAttacked = false;
            actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive,
                useMelee: false, nativeMovement: true);
            Assert.Equal(expected, actor.Brain.WindupTics > 0);
            Assert.Equal(reference.CaptureState().CheckMissileRandomState, sim.CaptureState().CheckMissileRandomState);
        }
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonfinitePropertyAndStructuredArchivesReject(double invalid)
    {
        var (sim, actor) = Room();
        Assert.Throws<ArgumentOutOfRangeException>(() => actor.MissileChanceMult = invalid);
        Assert.Null(actor.MissileChanceMult);
        var state = sim.CaptureState(); state.Actors[0].MissileChanceMultOverride = invalid;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Fact]
    public void ExplicitZeroRoundTripsAndOlderSaveClearsOverride()
    {
        var (sim, actor) = Room(); var old = SimSavegame.Write(sim, true); var original = sim.Checksum;
        actor.MissileChanceMult = 0; sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        Assert.NotEqual(original, hash); var saved = SimSavegame.Write(sim, true);
        actor.MissileChanceMult = 2; SimSavegame.Apply(sim, saved);
        Assert.Equal(0, actor.MissileChanceMult); Assert.Equal(hash, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.Null(actor.MissileChanceMult); Assert.Equal(original, sim.Checksum);
    }

    [Theory]
    [InlineData(2, 0)]
    [InlineData(0, 1)]
    [InlineData(1, double.NaN)]
    public void InvalidBinaryPresenceOrValueRejects(int present, double value)
    {
        var (sim, actor) = Room(); actor.MissileChanceMult = 0; var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), present);
        BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(start + 12), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-missilechancemult-state", error);
    }

    private static (AuthoritySimulation, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 300 }]
        });
        var actor = sim.AddBot(0, 0); actor.Brain = MonsterBrain.ForType(3004);
        actor.ReactionTime = 0; actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0); actor.SetTargetPointer(sim.Players.Single());
        sim.RestoreState(sim.CaptureState()); return (sim, actor);
    }
}
