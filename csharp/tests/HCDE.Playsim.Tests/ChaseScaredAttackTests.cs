using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseScaredAttackTests
{
    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    public void FearUsesNativeThresholdAndExactlyOneNamedDraw(bool frightened, bool frightening, bool noFear)
    {
        var (sim, actor) = Room(); var (reference, _) = Room();
        actor.Frightened = frightened; actor.NoFear = noFear; sim.Players.Single().Frightening = frightening;
        var permitted = 0;
        for (var i = 0; i < 256; i++)
        {
            var expected = reference.NextScaredyCatByte() < 43;
            actor.Brain = new MonsterBrain(MonsterAttack.Hitscan);
            actor.SetTargetPointer(sim.Players.Single()); actor.JustAttacked = false;
            actor.MoveCount = 0; actor.ReactionTime = 0;
            Chase(sim, actor);
            Assert.Equal(expected ? 8 : 0, actor.Brain.WindupTics);
            Assert.Equal(reference.CaptureState().ScaredyCatRandomState, sim.CaptureState().ScaredyCatRandomState);
            if (expected) permitted++;
        }
        Assert.InRange(permitted, 1, 255);
    }

    [Fact]
    public void OrdinaryAndLegacyChaseDoNotInitializeFearStream()
    {
        var (sim, actor) = Room(); Chase(sim, actor);
        Assert.Null(sim.CaptureState().ScaredyCatRandomState);
        actor.Frightened = true; actor.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        actor.SetTargetPointer(sim.Players.Single()); actor.JustAttacked = false;
        actor.Brain.Tick(sim, actor, nativeMovement: false);
        Assert.Null(sim.CaptureState().ScaredyCatRandomState);
    }

    [Fact]
    public void GoalAsTargetSkipsFearDraw()
    {
        var (sim, actor) = Room(); actor.Frightened = true;
        var goal = sim.AddBot(0, 500); actor.ChaseGoal = true; actor.GoalId = goal.Id;
        actor.SetTargetPointer(goal);
        Chase(sim, actor); Assert.Null(sim.CaptureState().ScaredyCatRandomState);
    }

    [Fact]
    public void SeparateChaseGoalStillChecksFearAgainstEnemy()
    {
        var (sim, actor) = Room(); actor.Frightened = true;
        var goal = sim.AddBot(0, 500); actor.ChaseGoal = true; actor.GoalId = goal.Id;
        var enemy = actor.TargetId;
        Chase(sim, actor);
        Assert.NotNull(sim.CaptureState().ScaredyCatRandomState);
        Assert.Equal(enemy, actor.TargetId);
    }

    [Fact]
    public void SavedStreamRestoresChecksumAndOlderSaveClearsIt()
    {
        var (sim, _) = Room(); var old = SimSavegame.Write(sim, true);
        sim.NextScaredyCatByte(); sim.RestoreState(sim.CaptureState());
        var hash = sim.Checksum; var saved = SimSavegame.Write(sim, true);
        var expected = sim.NextScaredyCatByte();
        SimSavegame.Apply(sim, saved); Assert.Equal(hash, sim.Checksum);
        Assert.Equal(expected, sim.NextScaredyCatByte());
        SimSavegame.Apply(sim, old); Assert.Null(sim.CaptureState().ScaredyCatRandomState);
    }

    [Fact]
    public void InvalidRandomArchiveHeaderRejects()
    {
        var (sim, _) = Room(); sim.NextScaredyCatByte(); var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(279, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 279);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-scaredycatrandom-header", error);
    }

    private static void Chase(AuthoritySimulation sim, Actor actor) => actor.Brain!.Tick(sim, actor,
        MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive, useMelee: false, nativeMovement: true);

    private static (AuthoritySimulation, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 300 }]
        });
        var actor = sim.AddBot(0, 0); actor.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        actor.ReactionTime = 0; actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0); actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
