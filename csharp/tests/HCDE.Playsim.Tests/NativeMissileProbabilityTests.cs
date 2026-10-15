using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeMissileProbabilityTests
{
    [Theory]
    [InlineData(3004, 500, 200)]
    [InlineData(3004, 300, 108)]
    [InlineData(3001, 300, 200)]
    [InlineData(16, 500, 154)]
    [InlineData(16, 2000, 160)]
    [InlineData(7, 500, 154)]
    [InlineData(66, 400, 168)]
    [InlineData(3006, 500, 154)]
    public void DoomProfilesUseNativeDistanceMultiplierAndCap(int type, int distance, int threshold)
    {
        var (sim, actor) = Room(type, distance); var (reference, _) = Room(type, distance);
        var attacks = 0;
        for (var i = 0; i < 64; i++)
        {
            var expected = reference.NextCheckMissileByte() >= threshold;
            actor.Brain = MonsterBrain.ForType(type); actor.SetTargetPointer(sim.Players.Single());
            actor.ReactionTime = 0; actor.MoveCount = 0; actor.JustAttacked = false;
            Chase(sim, actor);
            Assert.Equal(expected, actor.Brain!.WindupTics > 0);
            Assert.Equal(reference.CaptureState().CheckMissileRandomState, sim.CaptureState().CheckMissileRandomState);
            if (expected) attacks++;
        }
        Assert.InRange(attacks, 1, 63);
    }

    [Theory]
    [InlineData(64, 2000)]
    [InlineData(66, 200)]
    public void RangeRejectionSkipsProbabilityDraw(int type, int distance)
    {
        var (sim, actor) = Room(type, distance); Chase(sim, actor);
        Assert.Null(sim.CaptureState().CheckMissileRandomState);
        Assert.Equal(0, actor.Brain!.WindupTics);
    }

    [Fact]
    public void RetaliationBypassesProbabilityDraw()
    {
        var (sim, actor) = Room(3004, 500); actor.JustHit = true; Chase(sim, actor);
        Assert.True(actor.Brain!.WindupTics > 0); Assert.False(actor.JustHit);
        Assert.Null(sim.CaptureState().CheckMissileRandomState);
    }

    [Fact]
    public void SaveRestoresStreamChecksumAndOlderSaveClearsStream()
    {
        var (sim, _) = Room(3004, 500); var old = SimSavegame.Write(sim, true);
        sim.NextCheckMissileByte(); sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        var saved = SimSavegame.Write(sim, true); var expected = sim.NextCheckMissileByte();
        SimSavegame.Apply(sim, saved); Assert.Equal(hash, sim.Checksum);
        Assert.Equal(expected, sim.NextCheckMissileByte());
        SimSavegame.Apply(sim, old); Assert.Null(sim.CaptureState().CheckMissileRandomState);
    }

    [Fact]
    public void InvalidArchivePriorVersionRejects()
    {
        var (sim, _) = Room(3004, 500); sim.NextCheckMissileByte(); var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(280, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 280);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-checkmissilerandom-header", error);
    }

    private static void Chase(AuthoritySimulation sim, Actor actor) => actor.Brain!.Tick(sim, actor,
        MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive, useMelee: false, nativeMovement: true);

    private static (AuthoritySimulation, Actor) Room(int type, int distance)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = (short)distance }]
        });
        var actor = sim.AddBot(0, 0); actor.Brain = MonsterBrain.ForType(type);
        actor.ReactionTime = 0; actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0); actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
