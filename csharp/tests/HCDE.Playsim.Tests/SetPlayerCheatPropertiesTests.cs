using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SetPlayerCheatPropertiesTests
{
    public static TheoryData<int, int> PropertiesAndValues => new()
    {
        { 2, 0 }, { 2, 1 }, { 2, -5 }, { 16, 0 }, { 16, 1 }, { 16, -5 },
        { 17, 0 }, { 17, 1 }, { 17, -5 }, { 18, 0 }, { 18, 1 }, { 18, -5 },
        { 21, 0 }, { 21, 1 }, { 21, -5 }, { 22, 0 }, { 22, 1 }, { 22, -5 },
    };

    [Theory]
    [MemberData(nameof(PropertiesAndValues))]
    public void LocalActionSetsAndClearsOnlySelectedProperty(int property, int value)
    {
        var sim = Room(); var player = sim.Players.First();
        Assert.True(ActorSpecialActions.CallSpecial(player, 191, 0, value, property));
        foreach (var candidate in new[] { 2, 16, 17, 18, 21, 22 })
            Assert.Equal(candidate == property && value != 0, Get(player, candidate));
        Assert.All(sim.Players.Skip(1), p => Assert.False(Get(p, property)));
        Assert.True(ActorSpecialActions.CallSpecial(player, 191, 0, 0, property));
        Assert.False(Get(player, property));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    [InlineData(21)]
    [InlineData(22)]
    public void AllPlayerActionPersistsAndClearsProperty(int property)
    {
        var sim = Room();
        Assert.True(PlayerPropertyActions.ExecuteSpecial(sim, 191, null, -1, 1, property));
        Assert.All(sim.Players, p => Assert.True(Get(p, property)));
        var saved = SimSavegame.Write(sim, true);
        Assert.True(PlayerPropertyActions.ExecuteSpecial(sim, 191, null, 1, 0, property));
        Assert.All(sim.Players, p => Assert.False(Get(p, property)));
        SimSavegame.Apply(sim, saved);
        Assert.All(sim.Players, p => Assert.True(Get(p, property)));
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(21)]
    [InlineData(22)]
    public void ScriptedProtectionAffectsOrdinaryDamage(int property)
    {
        var sim = Room(); var player = sim.Players.First(); player.Health = 100;
        Assert.True(ActorSpecialActions.CallSpecial(player, 191, 0, 1, property));
        ActorDamage.Apply(player, 200);
        Assert.Equal(property is 16 or 17 ? 1 : 100, player.Health);
        Assert.False(player.IsDead);
    }

    [Fact]
    public void OlderSaveClearsPlayerAndMonsterBuddha()
    {
        var sim = Room(); var monster = sim.AddBot(64, 64);
        var older = SimSavegame.Write(sim, true);
        monster.Buddha = true; sim.Players.First().Buddha = true;
        var saved = SimSavegame.Write(sim, true);
        monster.Buddha = false; SimSavegame.Apply(sim, saved); Assert.True(monster.Buddha);
        SimSavegame.Apply(sim, older); Assert.All(sim.Actors, a => Assert.False(a.Buddha));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidBuddhaStateRejectsBeforeMutation(int value)
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].BuddhaFlag = value;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.All(sim.Actors, a => Assert.False(a.Buddha));
    }

    [Fact]
    public void BuddhaArchiveRequiresGeometry()
    {
        var sim = Room(); var state = sim.CaptureState();
        state.GeometryHealth = null; state.Actors[0].BuddhaFlag = 1;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    private static bool Get(PlayerPawn player, int property) => property switch
    {
        2 => player.InstantWeaponSwitch, 16 => player.Buddha, 17 => player.Buddha2,
        18 => player.Frightening, 21 => player.GodMode, 22 => player.GodMode2,
        _ => throw new ArgumentOutOfRangeException(nameof(property)),
    };

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 128 }],
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
