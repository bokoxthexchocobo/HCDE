using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingDamageTests
{
    [Theory]
    [InlineData(false, 10, 40)]
    [InlineData(true, 10, 40)]
    [InlineData(false, -100, 100)]
    [InlineData(true, -100, 100)]
    [InlineData(false, 0, 50)]
    [InlineData(true, 0, 50)]
    public void AcsTargetsAllShootableActorsWithMatchingId(bool stack, int amount, int expected)
    {
        var sim = Room();
        foreach (var actor in sim.Actors.Where(a => a.ThingId == 7))
        {
            actor.Health = 50;
            actor.ResurrectionHealth = 100;
        }
        sim.Actors[^1].Shootable = false;
        int[] words = stack ? [3, 7, 3, amount, 3, 0, 6, 119, 1] : [11, 119, 7, amount, 0, 1];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single()));
        sim.Acs.Tick(sim);
        Assert.Equal(expected, sim.Actors[1].Health);
        Assert.Equal(50, sim.Actors[2].Health);
        Assert.Equal(100, sim.Players.Single().Health);
    }

    [Fact]
    public void ZeroIdUsesActivatorAndMissingIdStillSucceeds()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Health = 50; player.MaxHealth = 300;
        Assert.True(ThingDamage.Execute(sim, 119, player, 0, -200, 0));
        Assert.Equal(player.ResurrectionHealth, player.Health);
        Assert.True(ThingDamage.Execute(sim, 119, null, 999, 10, 0));
    }

    [Fact]
    public void MapActionConsumesOneShotWithNoMatchingTargets()
    {
        var sim = Room();
        var line = new LevelLine { Special = 119, Arg0 = 999, Arg1 = 10, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(0, line.Special);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, Id = 7, X = 100 },
            new LevelThing { Type = 3004, Id = 7, X = 200 }],
    });
}
