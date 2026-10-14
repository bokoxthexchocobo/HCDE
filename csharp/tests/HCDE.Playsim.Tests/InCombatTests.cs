using HCDE.MapLoader;
using System.Buffers.Binary;

namespace HCDE.Playsim.Tests;

public class InCombatTests
{
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(0, true, true)]
    [InlineData(2, false, true)]
    [InlineData(-1, false, true)]
    public void PatrolDelaySetsCombatWithoutClearingExistingFlag(int delay, bool initial, bool expected)
    {
        var sim = Room(); var actor = Hunter(sim); actor.InCombat = initial;
        var goal = sim.Actors.Single(a => a.ThingId == 7); actor.GoalId = goal.Id; actor.SetTargetPointer(goal);
        goal.SpecialArgs[0] = 8; sim.Actors.Single(a => a.ThingId == 8).SpecialArgs[1] = delay;
        actor.Brain!.Tick(sim, actor);
        Assert.Equal(expected, actor.InCombat);
    }

    [Fact]
    public void FlagAccessSupportsPrefixAndCase()
    {
        var actor = Hunter(Room()); Assert.True(AcsActorFlags.TrySet(actor, "actor.incombat", true));
        Assert.True(AcsActorFlags.TryGet(actor, "INCOMBAT", out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, "INCOMBAT", false)); Assert.False(actor.InCombat);
    }

    [Fact]
    public void RoundTripAndOlderSaveReset()
    {
        var sim = Room(); var actor = Hunter(sim); var older = SimSavegame.Write(sim, true);
        actor.InCombat = true; var saved = SimSavegame.Write(sim, true);
        actor.InCombat = false; SimSavegame.Apply(sim, saved);
        Assert.True(actor.InCombat); Assert.Equal(saved, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, older); Assert.False(actor.InCombat);
    }

    [Fact]
    public void InvalidSavedFlagIsRejected()
    {
        var sim = Room(); Hunter(sim).InCombat = true; var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static Actor Hunter(AuthoritySimulation sim) => sim.Actors.Single(a => a.ClassDoomEdNum == 3004);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 9024, Id = 7, X = 40 },
            new LevelThing { Type = 9024, Id = 8, X = 300 }]
    });
}
