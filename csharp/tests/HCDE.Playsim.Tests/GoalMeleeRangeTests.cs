using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GoalMeleeRangeTests
{
    [Theory]
    [InlineData(59, true)]
    [InlineData(60, false)]
    [InlineData(61, false)]
    public void GoalArrivalUsesStrictDistanceBeforeAllAttackGates(int x, bool expected)
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var goal = sim.Actors.Single(a => a.DoomEdNum == 3004);
        actor.Friendly = goal.Friendly = true;
        goal.X = Fixed.FromInt(x); goal.Z = Fixed.FromInt(200); goal.Radius = Fixed.FromInt(16);
        actor.Brain!.SetSpecialTarget(goal);
        Assert.False(actor.CheckMeleeRange());
        actor.GoalId = goal.Id;
        Assert.Equal(expected, actor.CheckMeleeRange());
        Assert.Equal(expected, MonsterBrain.CheckMeleeRange(actor, goal, visible: false));
        var bytes = SimSavegame.Write(sim); actor.GoalId = null;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(goal.Id, actor.GoalId); Assert.Equal(expected, actor.CheckMeleeRange());
    }

    [Fact]
    public void GoalPointerRoundTripsAndOlderArchiveClearsIt()
    {
        var sim = Room(); var actor = sim.Actors[0]; var goal = sim.Actors[1];
        var older = SimSavegame.Write(sim); actor.GoalId = goal.Id;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(106, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var loaded = Room(); SimSavegame.Apply(loaded, bytes);
        Assert.Equal(goal.Id, loaded.Actors[0].GoalId); Assert.Null(loaded.Actors[1].GoalId);
        Assert.Equal(bytes, SimSavegame.Write(loaded));
        SimSavegame.Apply(loaded, older); Assert.Null(loaded.Actors[0].GoalId);
        Assert.Equal(older, SimSavegame.Write(loaded));
        loaded.Actors[0].GoalId = 0; Assert.Null(loaded.Actors[0].GoalId);
    }

    [Theory]
    [InlineData(0, "save-goal-size")]
    [InlineData(1, "save-goal-header")]
    [InlineData(2, "save-goal-value")]
    public void MalformedGoalArchiveRejectsBeforeMutation(int corruption, string expected)
    {
        var sim = Room(); sim.Actors[0].GoalId = sim.Actors[1].Id;
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        if (corruption == 0) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), int.MaxValue);
        if (corruption == 1) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), 106);
        if (corruption == 2) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), 0);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
        Assert.Equal(expected, Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes)).Message);
        Assert.Equal(sim.Actors[1].Id, sim.Actors[0].GoalId);
    }

    [Fact]
    public void GoalIdentityChangesChecksum()
    {
        var first = Room(); var second = Room(); first.Actors[0].GoalId = first.Actors[1].Id;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3001 }, new LevelThing { Type = 3004, X = 59 }],
        Sectors = [new LevelSector { CeilingHeight = 512, NoAttack = true }],
        Lines = [new LevelLine { X1 = 25, X2 = 25, Y1 = -100, Y2 = 100, SideBack = -1 }],
    });
}
