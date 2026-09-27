using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BoomSectorDamageTests
{
    [Theory]
    [InlineData(MapDataFormat.DoomBinary, 32, 5, "Fire")]
    [InlineData(MapDataFormat.DoomBinary, 64, 10, "Slime")]
    [InlineData(MapDataFormat.DoomBinary, 96, 20, "Slime")]
    [InlineData(MapDataFormat.HexenBinary, 256, 5, "Fire")]
    [InlineData(MapDataFormat.HexenBinary, 512, 10, "Slime")]
    [InlineData(MapDataFormat.UdmfText, 768, 20, "Slime")]
    public void InitializesBoomDamageBits(MapDataFormat format, short special, int damage, string type)
    {
        var sim = Start(format, special); var sector = sim.Level.Sectors[0];
        Assert.Equal(damage, sector.DamageAmount); Assert.Equal(type, sector.DamageType);
        Assert.Equal(32, sector.DamageInterval); Assert.Equal(damage == 20 ? 5 : 0, sector.Leakiness);
        sim.Tick(); Assert.Equal(100 - damage, sim.Players.Single().Health);
    }

    [Theory]
    [InlineData(MapDataFormat.DoomBinary, 165, 160)]
    [InlineData(MapDataFormat.UdmfText, 1349, 1280)]
    public void DamageBitsPrecedeLowSpecialAndPreserveOtherBits(MapDataFormat format, short special, short remaining)
    {
        var sim = Start(format, special); var sector = sim.Level.Sectors[0];
        Assert.Equal(5, sector.DamageAmount); Assert.Equal("Fire", sector.DamageType);
        Assert.Equal(remaining, sector.Special);
    }

    [Fact]
    public void ExplicitDamagePrecedesBoomBits()
    {
        var sim = Start(MapDataFormat.UdmfText, 256, explicitDamage: -3);
        Assert.Equal(-3, sim.Level.Sectors[0].DamageAmount);
        Assert.Equal(8, sim.Level.Sectors[0].DamageInterval);
        Assert.Equal("Ice", sim.Level.Sectors[0].DamageType);
    }

    [Theory]
    [InlineData(false, 5)]
    [InlineData(true, 0)]
    public void Mbf21DeathFlagIsNotMistakenForOrdinaryDamage(bool mbf21, int expected)
    {
        var sim = Start(MapDataFormat.DoomBinary, 4128, mbf21: mbf21);
        Assert.Equal(expected, sim.Level.Sectors[0].DamageAmount);
    }

    private static AuthoritySimulation Start(MapDataFormat format, short special, int explicitDamage = 0, bool mbf21 = false)
        => AuthoritySimulation.Start(new PlayLevel
        {
            Format = format,
            Sectors = [new LevelSector { Special = special, CeilingHeight = 128, DamageAmount = explicitDamage,
                DamageInterval = 8, DamageType = "Ice" }],
            Things = [new LevelThing { Type = 1 }],
        }, compat: mbf21 ? CompatSurface.Mbf21 : CompatSurface.None);
}
