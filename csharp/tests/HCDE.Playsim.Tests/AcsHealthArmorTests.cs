using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsHealthArmorTests
{
    [Theory]
    [InlineData(1, 73)]
    [InlineData(3001, 37)]
    [InlineData(1, 0)]
    [InlineData(3001, 0)]
    [InlineData(1, 250)]
    public void HealthQueryReadsAnyExistingActivator(int type, int health)
    {
        var sim = Room(type); var actor = sim.Actors.Single(); actor.Health = health;
        Query(sim, actor, 120, health);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(73, false)]
    [InlineData(200, false)]
    [InlineData(73, true)]
    public void ArmorQueryReadsCurrentPlayerInventoryEvenAfterDeath(int armor, bool dead)
    {
        var sim = Room(1); var player = sim.Players.Single(); player.Inventory.Armor = armor;
        if (dead) player.Health = 0;
        Query(sim, player, 121, armor);
    }

    [Theory]
    [InlineData(120, false)]
    [InlineData(120, true)]
    [InlineData(121, false)]
    [InlineData(121, true)]
    public void WorldAndDestroyedActivatorsReturnZero(int opcode, bool destroyed)
    {
        var sim = Room(1); var player = sim.Players.Single(); player.Inventory.Armor = 73;
        if (destroyed) player.Destroy();
        Query(sim, destroyed ? player : null, opcode, 0);
    }

    [Fact]
    public void MonsterWithoutArmorReturnsZero()
    {
        var sim = Room(3001); Query(sim, sim.Actors.Single(), 121, 0);
    }

    [Theory]
    [InlineData(120, 80)]
    [InlineData(121, 90)]
    public void QuerySeesDamageAndArmorAbsorption(int opcode, int expected)
    {
        var sim = Room(1); var player = sim.Players.Single();
        player.Inventory.Armor = 100; player.Inventory.ArmorSavePercent = 33;
        ActorDamage.Apply(player, 30);
        Query(sim, player, opcode, expected);
    }

    [Theory]
    [InlineData(120, 37)]
    [InlineData(121, 73)]
    public void ResumeKeepsActivatorButReadsUpdatedValues(int opcode, int expected)
    {
        var sim = Room(1); var player = sim.Players.Single();
        sim.Acs.Add(Program(1, [2, .. Words(opcode, expected)]));
        Assert.True(sim.Acs.TryExecute(1, [], player)); sim.Acs.Tick(sim);
        player.Health = 37; player.Inventory.Armor = 73;
        Assert.True(sim.Acs.TryExecute(1, [], new PlayerPawn())); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(120, 37)]
    [InlineData(121, 73)]
    public void NestedMapScriptRetainsActivator(int opcode, int expected)
    {
        var sim = Room(1); var player = sim.Players.Single();
        player.Health = 37; player.Inventory.Armor = 73;
        sim.Acs.Add(Program(1, [13, 226, 2, 0, 0, 0, 0, 1]));
        sim.Acs.Add(Program(2, Words(opcode, expected)));
        Assert.True(LineSpecials.ActivateMapLine(sim, player,
            new LevelLine { Special = 80, Arg0 = 1, PlayerUse = true }, true));
        sim.Acs.Tick(sim); sim.Acs.Tick(sim); Assert.Equal(1, sim.LightOf(0));
    }

    private static void Query(AuthoritySimulation sim, Actor? actor, int opcode, int expected)
    {
        sim.Acs.Add(Program(1, Words(opcode, expected)));
        Assert.True(sim.Acs.TryExecute(1, [], actor)); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0));
    }

    // Equality also covers values above the light special's range; the lower tag must survive.
    private static int[] Words(int opcode, int expected) => [3, 7, opcode, 3, expected, 19, 5, 112, 1];
    private static AcsProgram Program(int number, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return new AcsProgram { Number = number, Code = bytes };
    }
    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = type }],
    });
}
