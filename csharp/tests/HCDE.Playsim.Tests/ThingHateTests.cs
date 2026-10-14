using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingHateTests
{
    [Theory]
    [InlineData(0, false, false, false)]
    [InlineData(1, false, false, false)]
    [InlineData(2, true, false, false)]
    [InlineData(3, false, true, false)]
    [InlineData(4, true, true, false)]
    [InlineData(5, false, false, true)]
    [InlineData(6, true, false, true)]
    public void ModesConfigureNativeHateFlags(int mode, bool noSight, bool huntPlayers, bool ignorePlayers)
    {
        var sim = Room(); var hater = Hater(sim); hater.TidToHate = 99;
        hater.NoSightCheck = hater.HuntPlayers = hater.NoHatePlayers = true;
        Assert.True(ThingHate.Execute(sim, sim.Players.Single(), 5, 7, mode));
        Assert.Equal(mode == 0 ? 99 : 7, hater.TidToHate);
        Assert.Equal(noSight, hater.NoSightCheck); Assert.Equal(huntPlayers, hater.HuntPlayers); Assert.Equal(ignorePlayers, hater.NoHatePlayers);
        Assert.Equal(mode == 0 ? Hatee(sim).Id : (uint?)null, hater.TargetId);
    }

    [Fact]
    public void DirectHateRemembersPriorEnemyAndEntersSeeState()
    {
        var sim = Room(); var hater = Hater(sim); hater.SetTargetPointer(sim.Players.Single());
        hater.States.Configure(hater, [new(-1, 0), new(-1, 1)], 0); hater.SeeState = 1;
        ThingHate.Execute(sim, null, 5, 7, 0);
        Assert.Equal(Hatee(sim).Id, hater.TargetId); Assert.Equal(sim.Players.Single().Id, hater.Brain!.LastEnemyId);
        Assert.Equal(hater.SeeState, hater.States.Current);
    }

    [Fact]
    public void GroupHateClearsMismatchedMemoryAndSearchCursor()
    {
        var sim = Room(); var hater = Hater(sim); hater.SetTargetPointer(sim.Players.Single()); hater.LastLookActorId = Hatee(sim).Id;
        ThingHate.Execute(sim, null, 5, 7, 2);
        Assert.Null(hater.TargetId); Assert.Null(hater.LastLookActorId);
        hater.Brain!.Tick(sim, hater, MonsterChaseFlags.NoPlayActive); Assert.Equal(Hatee(sim).Id, hater.TargetId);
    }

    [Fact]
    public void ZeroHateePreservesExistingMemoryForGroupMode()
    {
        var sim = Room(); var hater = Hater(sim); hater.SetTargetPointer(Hatee(sim));
        ThingHate.Execute(sim, null, 5, 0, 2);
        Assert.Equal(0, hater.TidToHate); Assert.Equal(Hatee(sim).Id, hater.TargetId); Assert.True(hater.NoSightCheck);
    }

    [Fact]
    public void ZeroHaterRejectsPlayerActivator()
    {
        var sim = Room(); Assert.False(ThingHate.Execute(sim, sim.Players.Single(), 0, 7, 1));
        Assert.Equal(0, sim.Players.Single().TidToHate);
    }

    [Fact]
    public void ZeroHaterUsesMonsterActivatorAndZeroHateeUsesActivator()
    {
        var sim = Room(); var hater = Hater(sim);
        Assert.True(ThingHate.Execute(sim, hater, 0, 7, 0)); Assert.Equal(Hatee(sim).Id, hater.TargetId);
        Assert.True(ThingHate.Execute(sim, sim.Players.Single(), 5, 0, 0)); Assert.Equal(sim.Players.Single().Id, hater.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsDispatchSupportsDirectAndStackCalls(bool stack)
    {
        var sim = Room(); int[] words = stack ? [3, 5, 3, 7, 3, 6, 6, 177, 1] : [11, 177, 5, 7, 6, 1];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single())); sim.Acs.Tick(sim);
        Assert.Equal(7, Hater(sim).TidToHate); Assert.True(Hater(sim).NoSightCheck); Assert.True(Hater(sim).NoHatePlayers);
    }

    [Fact]
    public void MapLineDispatchConsumesSuccessfulOneShot()
    {
        var sim = Room(); var line = new LevelLine { Special = 177, Arg0 = 5, Arg1 = 7, Arg2 = 1, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(0, line.Special); Assert.Equal(7, Hater(sim).TidToHate);
    }

    [Fact]
    public void ActorSpecialDispatchAndSavePreserveConfiguredHate()
    {
        var sim = Room(); var hater = Hater(sim);
        Assert.True(ActorSpecialActions.CallSpecial(hater, 177, 5, 7, 4));
        var saved = SimSavegame.Write(sim, true); ThingHate.Execute(sim, null, 5, 0, 1); SimSavegame.Apply(sim, saved);
        Assert.Equal(7, hater.TidToHate); Assert.True(hater.NoSightCheck); Assert.True(hater.HuntPlayers);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void MissingHateeStillConfiguresGroupAttitude()
    {
        var sim = Room(); Assert.True(ThingHate.Execute(sim, null, 5, 99, 5));
        Assert.Equal(99, Hater(sim).TidToHate); Assert.True(Hater(sim).NoHatePlayers);
    }

    [Fact]
    public void IneligibleHatersAndSelfTargetAreSkipped()
    {
        var sim = Room(); var hater = Hater(sim); hater.Shootable = false;
        ThingHate.Execute(sim, null, 5, 7, 1); Assert.Equal(0, hater.TidToHate);
        hater.Shootable = true; ThingHate.Execute(sim, hater, 0, 0, 0); Assert.Null(hater.TargetId);
    }

    [Fact]
    public void GroupModeCanAssignHateeWhenGoalIsNotCurrentTarget()
    {
        var sim = Room(); var hater = Hater(sim); hater.GoalId = sim.Players.Single().Id;
        ThingHate.Execute(sim, null, 5, 7, 1); Assert.Equal(Hatee(sim).Id, hater.TargetId);
    }

    [Fact]
    public void ActorsWithoutChaseRepresentationAreNotReconfigured()
    {
        var sim = Room(); var hater = Hater(sim); hater.Brain = null; hater.SeeState = -1;
        ThingHate.Execute(sim, null, 5, 7, 2); Assert.Equal(0, hater.TidToHate); Assert.False(hater.NoSightCheck);
    }

    [Fact]
    public void DormantHaterReceivesTargetWithoutEnteringSeeState()
    {
        var sim = Room(); var hater = Hater(sim); hater.Dormant = true;
        hater.States.Configure(hater, [new(-1, 0), new(-1, 1)], 0); hater.SeeState = 1;
        ThingHate.Execute(sim, null, 5, 7, 0); Assert.Equal(Hatee(sim).Id, hater.TargetId); Assert.Equal(0, hater.States.Current);
    }

    [Fact]
    public void DormantHateeDoesNotReplaceDirectTarget()
    {
        var sim = Room(); var hater = Hater(sim); hater.SetTargetPointer(sim.Players.Single()); Hatee(sim).Dormant = true;
        ThingHate.Execute(sim, null, 5, 7, 0); Assert.Equal(sim.Players.Single().Id, hater.TargetId);
    }

    [Fact]
    public void SelfOnlyGroupFinishesWithoutAssigningSelf()
    {
        var sim = Room(); var hater = Hater(sim); hater.GoalId = sim.Players.Single().Id;
        Assert.True(ThingHate.Execute(sim, null, 5, 5, 1)); Assert.Null(hater.TargetId);
    }

    private static Actor Hater(AuthoritySimulation sim) => sim.Actors.Single(a => a.ThingId == 5);
    private static Actor Hatee(AuthoritySimulation sim) => sim.Actors.Single(a => a.ThingId == 7);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, Id = 5 }, new LevelThing { Type = 3001, Id = 7, X = 200 }]
    });
}
