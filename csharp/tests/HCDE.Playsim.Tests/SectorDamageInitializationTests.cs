using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorDamageInitializationTests
{
    [Theory]
    [InlineData(MapDataFormat.DoomBinary, 4, 20)]
    [InlineData(MapDataFormat.DoomBinary, 5, 10)]
    [InlineData(MapDataFormat.DoomBinary, 7, 5)]
    [InlineData(MapDataFormat.DoomBinary, 16, 20)]
    [InlineData(MapDataFormat.HexenBinary, 68, 20)]
    [InlineData(MapDataFormat.HexenBinary, 69, 10)]
    [InlineData(MapDataFormat.HexenBinary, 71, 5)]
    [InlineData(MapDataFormat.HexenBinary, 80, 20)]
    [InlineData(MapDataFormat.HexenBinary, 85, 4)]
    [InlineData(MapDataFormat.UdmfText, 104, 5)]
    [InlineData(MapDataFormat.UdmfText, 196, -1)]
    public void InitializesDamageAndConsumesSpecialWithoutChangingTemplate(MapDataFormat format, short special, int amount)
    {
        var level = Level(format, special); var sim = AuthoritySimulation.Start(level);
        var sector = sim.Level.Sectors[0];
        Assert.Equal(amount, sector.DamageAmount); Assert.Equal(32, sector.DamageInterval);
        Assert.Equal(amount == 20 ? 5 : 0, sector.Leakiness);
        Assert.Equal(amount < 0 ? "None" : "Slime", sector.DamageType);
        Assert.Equal(0, sector.Special); Assert.Equal(special, level.Sectors[0].Special);
        Assert.Equal(0, level.Sectors[0].DamageAmount);
        var player = sim.Players.Single(); player.Health = 50;
        sim.Tick(); Assert.Equal(50 - amount, player.Health);
    }

    [Theory]
    [InlineData(-3)]
    [InlineData(7)]
    public void ExplicitDamageTakesPrecedence(int amount)
    {
        var level = Level(MapDataFormat.UdmfText, 69); var sector = level.Sectors[0];
        sector.DamageAmount = amount; sector.DamageInterval = 8; sector.DamageType = "Fire"; sector.Leakiness = 128;
        var sim = AuthoritySimulation.Start(level); var actual = sim.Level.Sectors[0];
        Assert.Equal(amount, actual.DamageAmount); Assert.Equal(8, actual.DamageInterval);
        Assert.Equal("Fire", actual.DamageType); Assert.Equal(128, actual.Leakiness);
        Assert.Equal(0, actual.Special);
    }

    [Theory]
    [InlineData(MapDataFormat.HexenBinary, 5)]
    [InlineData(MapDataFormat.DoomBinary, 6)]
    [InlineData(MapDataFormat.DoomBinary, 10)]
    public void UnconvertedSpecialsAreNotMisinterpreted(MapDataFormat format, short special)
    {
        var sim = AuthoritySimulation.Start(Level(format, special));
        Assert.Equal(0, sim.Level.Sectors[0].DamageAmount);
        Assert.Equal(special, sim.Level.Sectors[0].Special);
    }

    private static PlayLevel Level(MapDataFormat format, short special) => new()
    {
        Format = format,
        Sectors = [new LevelSector { CeilingHeight = 128, Special = special, LightLevel = 160 }],
        Things = [new LevelThing { Type = 1 }],
    };
}
