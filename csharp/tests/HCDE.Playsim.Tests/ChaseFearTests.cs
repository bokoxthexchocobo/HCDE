using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseFearTests
{
    [Theory]
    [InlineData(false, false, false, 8)]
    [InlineData(true, false, false, -8)]
    [InlineData(false, true, false, -8)]
    [InlineData(true, true, false, -8)]
    [InlineData(true, true, true, 8)]
    public void FearReversesSelectionUnlessNoFear(bool frightened, bool frightening, bool noFear, int x)
    {
        var (sim, actor) = Room(); actor.Frightened = frightened; actor.NoFear = noFear;
        sim.Players.Single().Frightening = frightening;
        Chase(sim, actor); Assert.Equal(Fixed.FromInt(x), actor.X);
    }

    [Fact]
    public void PatrolGoalIsExemptFromFear()
    {
        var (sim, actor) = Room(); actor.Frightened = true;
        actor.GoalId = sim.Players.Single().Id;
        Chase(sim, actor); Assert.Equal(Fixed.FromInt(8), actor.X);
    }

    [Theory]
    [InlineData("NOFEAR")]
    [InlineData("Actor.FRIGHTENED")]
    [InlineData("Actor.frightening")]
    public void FlagsHaveSharedScriptAccess(string name)
    {
        var (_, actor) = Room(); Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, name, false));
        Assert.True(AcsActorFlags.TryGet(actor, name, out value)); Assert.False(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void EveryMaskRoundTripsAndOlderSaveClearsFlags(int mask)
    {
        var (sim, actor) = Room(); var old = SimSavegame.Write(sim, true);
        actor.NoFear = (mask & 1) != 0; actor.Frightened = (mask & 2) != 0; actor.Frightening = (mask & 4) != 0;
        sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum; var saved = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, old); Assert.False(actor.NoFear || actor.Frightened || actor.Frightening);
        SimSavegame.Apply(sim, saved); Assert.Equal(mask, sim.CaptureState().Actors.Single(a => a.Id == actor.Id).FearFlags);
        Assert.Equal(hash, sim.Checksum);
    }

    [Fact]
    public void InvalidMasksRejectStructuredAndBinaryState()
    {
        var (sim, actor) = Room(); actor.Frightened = true;
        var state = sim.CaptureState(); state.Actors[0].FearFlags = 8;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        var bytes = SimSavegame.Write(sim, true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 8);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-fearflags-state", error);
    }

    [Fact]
    public void FearfulSavedChaseReplaysAndRevivalClearsFlags()
    {
        var (sim, actor) = Room(); actor.Frightened = true; var saved = SimSavegame.Write(sim, true);
        Chase(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Chase(sim, actor); Assert.Equal(expected, SimSavegame.Write(sim, true));
        actor.NoFear = actor.Frightening = true; ActorRaise.ReviveSupported(actor);
        Assert.False(actor.NoFear || actor.Frightened || actor.Frightening);
    }

    private static void Chase(AuthoritySimulation sim, Actor actor) => actor.Brain!.Tick(sim, actor,
        MonsterChaseFlags.NoPlayActive, useMelee: false, useMissile: false, nativeMovement: true);

    private static (AuthoritySimulation, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 300 }]
        });
        var actor = sim.AddBot(0, 0); actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.ReactionTime = 0; actor.MovementSpeed = Fixed.FromInt(8); actor.MoveDirection = 8;
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0); actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
