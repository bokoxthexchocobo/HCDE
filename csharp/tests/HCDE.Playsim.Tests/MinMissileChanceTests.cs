using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MinMissileChanceTests
{
    [Theory]
    [InlineData(3004, 0, 0, 0)]
    [InlineData(3004, 1, 0, 1)]
    [InlineData(3004, 255, 0, 255)]
    [InlineData(16, 100, 0, 100)]
    [InlineData(16, 101, 0.5, 50)]
    [InlineData(0, 100, 0, 100)]
    public void OverrideUsesNativeByteCapAndSkillMultiplier(int type, byte chance, double aggressiveness, int cap)
    {
        var (sim, actor) = Room(aggressiveness); var (reference, _) = Room(aggressiveness);
        actor.MinMissileChance = chance;
        for (var i = 0; i < 64; i++)
        {
            var expected = reference.NextCheckMissileByte() >= cap;
            actor.Brain = type == 0 ? new MonsterBrain(MonsterAttack.Hitscan) : MonsterBrain.ForType(type);
            actor.SetTargetPointer(sim.Players.Single()); actor.ReactionTime = 0;
            actor.MoveCount = 0; actor.JustAttacked = false;
            actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive,
                useMelee: false, nativeMovement: true);
            Assert.Equal(expected, actor.Brain.WindupTics > 0);
            Assert.Equal(reference.CaptureState().CheckMissileRandomState, sim.CaptureState().CheckMissileRandomState);
        }
    }

    [Fact]
    public void SaveRestoresExplicitZeroAndOlderSaveClearsOverride()
    {
        var (sim, actor) = Room(); var old = SimSavegame.Write(sim, true); var original = sim.Checksum;
        actor.MinMissileChance = 0; sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        Assert.NotEqual(original, hash); var saved = SimSavegame.Write(sim, true);
        actor.MinMissileChance = 255; SimSavegame.Apply(sim, saved);
        Assert.Equal((byte)0, actor.MinMissileChance); Assert.Equal(hash, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.Null(actor.MinMissileChance); Assert.Equal(original, sim.Checksum);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(256)]
    public void InvalidStructuredOverrideRejectsBeforeRestoration(int invalid)
    {
        var (sim, actor) = Room(); var state = sim.CaptureState();
        state.Actors.Single(a => a.Id == actor.Id).MinMissileChanceOverride = invalid;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Null(actor.MinMissileChance);
    }

    [Fact]
    public void InvalidBinaryOverrideRejects()
    {
        var (sim, actor) = Room(); actor.MinMissileChance = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(282, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 256);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-minmissilechance-state", error);
    }

    private static (AuthoritySimulation, Actor) Room(double aggressiveness = 0)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 2000 }]
        }, spawnOptions: new SpawnOptions(SkillAggressiveness: aggressiveness));
        var actor = sim.AddBot(0, 0); actor.Brain = MonsterBrain.ForType(3004);
        actor.ReactionTime = 0; actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0); actor.SetTargetPointer(sim.Players.Single());
        sim.RestoreState(sim.CaptureState());
        return (sim, actor);
    }
}
