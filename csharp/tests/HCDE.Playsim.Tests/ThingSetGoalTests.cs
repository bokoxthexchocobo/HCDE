using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingSetGoalTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(-1, true)]
    public void AssignsPatrolPointAndDelay(int mode, bool chase)
    {
        var sim = Room(); var actor = Monster(sim);
        Assert.True(ThingSetGoal.Execute(sim, 5, 7, 3, mode));
        Assert.Equal(sim.Actors.Single(a => a.ClassDoomEdNum == 9024).Id, actor.GoalId);
        Assert.Equal(105, actor.ReactionTime); Assert.Equal(chase, actor.ChaseGoal);
    }

    [Fact]
    public void EnemyPreservesTargetAndReactionTime()
    {
        var sim = Room(); var actor = Monster(sim); actor.SetTargetPointer(sim.Players.Single()); actor.ReactionTime = 9;
        ThingSetGoal.Execute(sim, 5, 7, 3, 1);
        Assert.Equal(sim.Players.Single().Id, actor.TargetId); Assert.Equal(9, actor.ReactionTime);
    }

    [Fact]
    public void OldGoalTargetIsClearedEvenWhenNewGoalIsMissing()
    {
        var sim = Room(); var actor = Monster(sim); var marker = sim.Actors.Single(a => a.ClassDoomEdNum == 9024);
        actor.GoalId = marker.Id; actor.SetTargetPointer(marker); actor.ChaseGoal = true;
        Assert.True(ThingSetGoal.Execute(sim, 5, 999, -2, 0));
        Assert.Null(actor.TargetId); Assert.Null(actor.GoalId); Assert.False(actor.ChaseGoal); Assert.Equal(-70, actor.ReactionTime);
    }

    [Fact]
    public void NonShootableMatchReturnsSuccessWithoutMutation()
    {
        var sim = Room(); var actor = Monster(sim); actor.Shootable = false; actor.ReactionTime = 9;
        Assert.True(ThingSetGoal.Execute(sim, 5, 7, 3, 1));
        Assert.Null(actor.GoalId); Assert.False(actor.ChaseGoal); Assert.Equal(9, actor.ReactionTime);
    }

    [Fact]
    public void ZeroAndMissingTidReturnFalse()
    {
        var sim = Room(); Assert.False(ThingSetGoal.Execute(sim, 0, 7, 1, 1)); Assert.False(ThingSetGoal.Execute(sim, 999, 7, 1, 1));
    }

    [Fact]
    public void ChaseGoalAndGoalPointerRoundTrip()
    {
        var sim = Room(); var actor = Monster(sim); ThingSetGoal.Execute(sim, 5, 7, 3, 1);
        var saved = SimSavegame.Write(sim, true);
        Assert.True(SimSavegame.TryRead(saved, out var state, out var error), error);
        actor.ChaseGoal = false; actor.GoalId = null; SimSavegame.Apply(sim, saved);
        Assert.True(actor.ChaseGoal); Assert.NotNull(actor.GoalId); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ActorActionDispatchesGoalSpecial()
    {
        var sim = Room(); Assert.True(ActorSpecialActions.CallSpecial(Monster(sim), 229, 5, 7, 2, 1));
        Assert.NotNull(Monster(sim).GoalId); Assert.True(Monster(sim).ChaseGoal); Assert.Equal(70, Monster(sim).ReactionTime);
    }

    [Fact]
    public void OlderSnapshotClearsChaseGoal()
    {
        var sim = Room(); var saved = SimSavegame.Write(sim, true); Monster(sim).ChaseGoal = true;
        SimSavegame.Apply(sim, saved); Assert.False(Monster(sim).ChaseGoal);
    }

    [Fact]
    public void InvalidSavedFlagIsRejected()
    {
        var sim = Room(); Monster(sim).ChaseGoal = true; var saved = SimSavegame.Write(sim, true);
        var size = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsDispatchSupportsBothCallForms(bool stack)
    {
        var sim = Room(); int[] words = stack ? [3, 5, 3, 7, 3, 2, 3, 1, 7, 229, 1] : [12, 229, 5, 7, 2, 1, 1];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single())); sim.Acs.Tick(sim);
        Assert.NotNull(Monster(sim).GoalId); Assert.True(Monster(sim).ChaseGoal); Assert.Equal(70, Monster(sim).ReactionTime);
    }

    [Fact]
    public void MapLineDispatchConsumesSuccessfulOneShot()
    {
        var sim = Room(); var line = new LevelLine { Special = 229, Arg0 = 5, Arg1 = 7, Arg2 = 2, Arg3 = 1, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(0, line.Special); Assert.NotNull(Monster(sim).GoalId); Assert.True(Monster(sim).ChaseGoal);
    }

    private static Actor Monster(AuthoritySimulation sim) => sim.Actors.Single(a => a.ThingId == 5);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, Id = 5 },
            new LevelThing { Type = 9047, Id = 7, X = 100 }, new LevelThing { Type = 9024, Id = 7, X = 200 }]
    });
}
