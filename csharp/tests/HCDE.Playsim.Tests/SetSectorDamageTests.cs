using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SetSectorDamageTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void MapAndScriptRoutesChangeTaggedSectorsAndGameplay(int route)
    {
        var sim = Room();
        Execute(sim, route, 7, 10, 14, 1, 128);
        foreach (var sector in sim.Level.Sectors.Take(2))
        {
            Assert.Equal(10, sector.DamageAmount); Assert.Equal("Fire", sector.DamageType);
            Assert.Equal(1, sector.DamageInterval); Assert.Equal(128, sector.Leakiness);
            Assert.True(sector.HarmInAir); Assert.Equal(26, sector.Special);
        }
        Assert.Equal(0, sim.Level.Sectors[2].DamageAmount);
        sim.Tick(); Assert.Equal(90, sim.Players.Single().Health);
        Execute(sim, route, 7, 0, 24, 4, 5);
        sim.Tick(); Assert.Equal(90, sim.Players.Single().Health);
        Assert.Equal("Ice", sim.Level.Sectors[0].DamageType);
    }

    [Theory]
    [InlineData(-10, 32, 0)]
    [InlineData(19, 32, 0)]
    [InlineData(20, 32, 5)]
    [InlineData(49, 32, 5)]
    [InlineData(50, 1, 256)]
    [InlineData(65536, 1, 256)]
    public void LegacyDefaultsUseAmountBeforeNarrowing(int amount, int interval, int leakiness)
    {
        var sim = Room(); Execute(sim, 1, 7, amount, 0, -1, 99);
        Assert.Equal(unchecked((short)amount), sim.Level.Sectors[0].DamageAmount);
        Assert.Equal(interval, sim.Level.Sectors[0].DamageInterval);
        Assert.Equal(leakiness, sim.Level.Sectors[0].Leakiness);
    }

    [Fact]
    public void ExplicitFieldsNarrowAndInvalidIntervalRepairsOnContact()
    {
        var sim = Room(); Execute(sim, 2, 7, 65541, 999, 32768, 65538);
        var sector = sim.Level.Sectors[0];
        Assert.Equal(5, sector.DamageAmount); Assert.Equal(-32768, sector.DamageInterval);
        Assert.Equal(2, sector.Leakiness); Assert.Equal("None", sector.DamageType);
        sim.Tick(); Assert.Equal(32, sector.DamageInterval); Assert.Equal(95, sim.Players.Single().Health);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public void ZeroTagTargetsUntaggedAndMissingTagStillSucceeds(int tag)
    {
        var sim = Room(); Execute(sim, 0, tag, 5, 0, 1, 0);
        Assert.Equal(0, sim.Level.Sectors[0].DamageAmount);
        Assert.Equal(tag == 0 ? 5 : 0, sim.Level.Sectors[2].DamageAmount);
    }

    private static void Execute(AuthoritySimulation sim, int route, int tag, int amount, int type, int interval, int leakiness)
    {
        if (route == 0)
        {
            var line = new LevelLine { Special = 214, PlayerUse = true, Arg0 = tag, Arg1 = amount,
                Arg2 = type, Arg3 = interval, Arg4 = leakiness };
            Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
            Assert.Equal(0, line.Special);
            return;
        }
        int[] words = route == 1 ? [13, 214, tag, amount, type, interval, leakiness, 1]
            : [3, tag, 3, amount, 3, type, 3, interval, 3, leakiness, 8, 214, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sides = [new LevelSide { Sector = 0 }],
        Lines = [new LevelLine { X1 = -64, Y1 = -64, X2 = 64, Y2 = -64, SideFront = 0, SideBack = -1 },
            new LevelLine { X1 = 64, Y1 = -64, X2 = 64, Y2 = 64, SideFront = 0, SideBack = -1 },
            new LevelLine { X1 = 64, Y1 = 64, X2 = -64, Y2 = 64, SideFront = 0, SideBack = -1 },
            new LevelLine { X1 = -64, Y1 = 64, X2 = -64, Y2 = -64, SideFront = 0, SideBack = -1 }],
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, Special = 26, HarmInAir = true },
            new LevelSector { Index = 1, Tag = 7, CeilingHeight = 128, Special = 26, HarmInAir = true },
            new LevelSector { Index = 2, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
