using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SoundPropagationTests
{
    private static AuthoritySimulation Graph(int count, params (int From, int To, bool Block)[] edges) =>
        AuthoritySimulation.Start(new PlayLevel {
            Sectors = Enumerable.Range(0, count).Select(index => new LevelSector { Index = index, CeilingHeight = 128 }).ToArray(),
            Sides = Enumerable.Range(0, count).Select(index => new LevelSide { Sector = index }).ToArray(),
            Lines = edges.Select(edge => new LevelLine { SideFront = edge.From, SideBack = edge.To,
                Flags = edge.Block ? LevelLine.BlockSoundFlag : 0 }).ToArray(),
        });

    [Fact]
    public void SoundCrossesOneBlockingBoundaryButNotTwo()
    {
        var sim = Graph(4, (0, 1, true), (1, 2, false), (2, 3, true));
        Assert.Equal(new[] { 0, 1, 2 }, SoundPropagation.ReachableSectors(sim, 0).Order().ToArray());
    }

    [Fact]
    public void CheaperAlternatePathCanReachBeyondSecondBoundary()
    {
        // Reach sector 1 first with cost 1; revisiting through 2 with cost 0 is essential.
        var sim = Graph(4, (0, 1, true), (0, 2, false), (2, 1, false), (1, 3, true));
        Assert.Equal(new[] { 0, 1, 2, 3 }, SoundPropagation.ReachableSectors(sim, 0).Order().ToArray());
    }

    [Fact]
    public void CurrentDoorHeightControlsPropagation()
    {
        var sim = Graph(3, (0, 1, false), (1, 2, false));
        var state = sim.CaptureState();
        state.Sectors[1] = (0, 0);
        sim.RestoreState(state);
        Assert.Equal(new[] { 0 }, SoundPropagation.ReachableSectors(sim, 0).ToArray());
        state.Sectors[1] = (0, 128);
        sim.RestoreState(state);
        Assert.Equal(3, SoundPropagation.ReachableSectors(sim, 0).Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WeaponNoiseAlertsHiddenMonstersUnlessAmbush(bool ambush)
    {
        var sim = HiddenMonster(ambush); var player = sim.Players.Single();
        var monster = sim.Actors.Single(actor => actor.DoomEdNum == 3001);
        Assert.False(CombatTrace.HasLineOfSight(sim, monster, player));
        Assert.Null(monster.Brain!.TargetId);
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(ambush ? (uint?)null : player.Id, monster.Brain.TargetId);
    }

    [Fact]
    public void DryFireDoesNotAlertAndDeadMonstersStayUntargeted()
    {
        var sim = HiddenMonster(false); var player = sim.Players.Single();
        var monster = sim.Actors.Single(actor => actor.DoomEdNum == 3001);
        player.Inventory.Bullets = 0;
        Assert.False(HitscanCombat.Fire(sim, player));
        Assert.Null(monster.Brain!.TargetId);
        player.Inventory.Bullets = 10; monster.Health = 0;
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Null(monster.Brain.TargetId);
    }

    [Fact]
    public void DisabledMonsterRemembersNoiseAndAcquiresItWhenEnabled()
    {
        var sim = HiddenMonster(false); var player = sim.Players.Single();
        var monster = sim.Actors.Single(actor => actor.DoomEdNum == 3001);
        monster.Brain!.Enabled = false;
        SoundPropagation.Alert(sim, player);
        Assert.Equal(player.Id, monster.LastHeardTargetId);
        Assert.Null(monster.Brain.TargetId);
        monster.Brain.Enabled = true;
        for (var i = 0; i < 11; i++) sim.Tick();
        Assert.Equal(player.Id, monster.Brain.TargetId);
    }

    [Fact]
    public void RememberedDeadSourceDoesNotWakeMonster()
    {
        var sim = HiddenMonster(false); var player = sim.Players.Single();
        var monster = sim.Actors.Single(actor => actor.DoomEdNum == 3001);
        monster.Brain!.Enabled = false; SoundPropagation.Alert(sim, player);
        player.Health = 0; monster.Brain.Enabled = true;
        for (var i = 0; i < 11; i++) sim.Tick();
        Assert.Equal(player.Id, monster.LastHeardTargetId);
        Assert.Null(monster.Brain.TargetId);
    }

    [Fact]
    public void AmbushMonsterRemembersNoiseButStillRequiresSight()
    {
        var sim = HiddenMonster(true); var player = sim.Players.Single();
        var monster = sim.Actors.Single(actor => actor.DoomEdNum == 3001);
        SoundPropagation.Alert(sim, player);
        for (var i = 0; i < 11; i++) sim.Tick();
        Assert.Equal(player.Id, monster.LastHeardTargetId);
        Assert.Null(monster.Brain!.TargetId);
        player.X = Fixed.FromInt(256); sim.Tick();
        Assert.Equal(player.Id, monster.Brain.TargetId);
    }

    [Fact]
    public void MonstersSpawnedAfterNoiseDoNotInheritOldSectorNoise()
    {
        var sim = HiddenMonster(false);
        SoundPropagation.Alert(sim, sim.Players.Single());
        var newcomer = sim.AddBot(160, 0);
        for (var i = 0; i < 11; i++) sim.Tick();
        Assert.Null(newcomer.LastHeardTargetId);
        Assert.Null(newcomer.Brain!.TargetId);
    }

    [Fact]
    public void LatestNoiseIsRememberedWithoutReplacingCurrentLiveTarget()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Lines = [new LevelLine { X1 = 64, X2 = 64, Y1 = -128, Y2 = 128, SideBack = -1 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = -40 },
                new LevelThing { Type = 3001, X = 128 }],
        });
        var players = sim.Players.OrderBy(player => player.Id).ToArray();
        var monster = sim.Actors.Single(actor => actor.DoomEdNum == 3001);
        SoundPropagation.Alert(sim, players[0]);
        SoundPropagation.Alert(sim, players[1]);
        Assert.Equal(players[0].Id, monster.Brain!.TargetId);
        Assert.Equal(players[1].Id, monster.LastHeardTargetId);
        players[0].Health = 0;
        for (var i = 0; i < 11; i++) sim.Tick();
        Assert.Equal(players[1].Id, monster.Brain.TargetId);
    }

    private static AuthoritySimulation HiddenMonster(bool ambush) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0 }],
        Lines = [new LevelLine { X1 = 64, Y1 = -128, X2 = 64, Y2 = 128, SideBack = -1 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 128, Ambush = ambush }],
    });
}
