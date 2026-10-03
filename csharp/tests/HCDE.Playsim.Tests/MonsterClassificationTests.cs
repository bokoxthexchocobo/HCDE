using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterClassificationTests
{
    [Theory]
    [InlineData(3004)]
    [InlineData(3001)]
    [InlineData(3006)]
    [InlineData(64)]
    [InlineData(71)]
    public void SupportedMapMonstersHaveNativeClassification(int type)
    {
        var sim = Room(type);
        Assert.True(sim.Actors.Single(actor => actor.DoomEdNum == type).IsMonster);
        Assert.False(sim.Players.Single().IsMonster);
    }

    [Fact]
    public void PickupsAndUnknownThingsAreNotAutomaticallyMonsters()
    {
        Assert.False(Room(2007).Actors.Single(actor => actor.DoomEdNum == 2007).IsMonster);
        Assert.False(Room(9999).Actors.Single(actor => actor.DoomEdNum == 9999).IsMonster);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(4194304, true)]
    public void PatchedCountKillControlsClassification(int bits, bool expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n");
        Assert.Equal(expected, Room(3004, patch).Actors.Single(actor => actor.DoomEdNum == 3004).IsMonster);
    }

    [Fact]
    public void NonFlagPatchPreservesMonsterClassification()
    {
        var patch = DehackedPatch.Apply("Thing 2\nHit points = 88\n");
        Assert.True(Room(3004, patch).Actors.Single(actor => actor.DoomEdNum == 3004).IsMonster);
    }

    [Fact]
    public void SpawnedLostSoulHasMonsterClassification()
    {
        var sim = Room(71);
        var parent = sim.Actors.Single(actor => actor.DoomEdNum == 71);
        var soul = sim.SpawnLostSoul(parent, null, 0);
        Assert.NotNull(soul);
        Assert.True(soul.IsMonster);
    }

    [Fact]
    public void NoInfightingBlocksDamageToMapMonsterFromPlainMonster()
    {
        var sim = Room(3004);
        var victim = sim.Actors.Single(actor => actor.DoomEdNum == 3004);
        victim.NoInfighting = true;
        var source = sim.AddBot(160, 64, doomEdNum: 3001);
        var health = victim.Health;
        ActorDamage.Apply(victim, 1, source: source, inflictor: source);
        Assert.Equal(health, victim.Health);
    }

    private static AuthoritySimulation Room(int type, DehackedPatchResult? patch = null) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 400 }, new LevelThing { Type = type }],
    }, dehacked: patch);
}
