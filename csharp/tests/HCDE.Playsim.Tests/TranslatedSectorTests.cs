using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TranslatedSectorTests
{
    [Theory]
    [InlineData("Doom", 4, 20)]
    [InlineData("doom", 7, 5)]
    [InlineData("ZDoomTranslated", 69, 10)]
    [InlineData("zdoomtranslated", 32, 5)]
    [InlineData("ZDoom", 4, 0)]
    [InlineData("ZDoom", 69, 10)]
    [InlineData("ZDoom", 32, 0)]
    public void NamespaceControlsDamageNumbering(string ns, int special, int damage)
    {
        var sim = Load(ns, special);
        Assert.Equal(damage, sim.Level.Sectors[0].DamageAmount);
        sim.Tick(); Assert.Equal(100 - damage, sim.Players.Single().Health);
    }

    [Theory]
    [InlineData("Doom", 4)]
    [InlineData("ZDoomTranslated", 4)]
    [InlineData("ZDoom", 68)]
    public void CombinedSpecialStartsLightingBeforeDamageConsumesNumber(string ns, int special)
    {
        var sim = Load(ns, special); var lights = new HashSet<short>();
        for (var i = 0; i < 33; i++) { sim.Tick(); lights.Add(sim.LightOf(0)); }
        Assert.Equal(2, lights.Count); Assert.Equal(60, sim.Players.Single().Health);
    }

    [Theory]
    [InlineData("Doom", 17, false)]
    [InlineData("Doom", 21, false)]
    [InlineData("ZDoomTranslated", 17, true)]
    [InlineData("ZDoomTranslated", 21, true)]
    [InlineData("ZDoom", 17, false)]
    public void DoomBaseTranslatorDisablesExtendedLighting(string ns, int special, bool changes)
    {
        var sim = Load(ns, special); var changed = false;
        for (var i = 0; i < 64; i++) { sim.Tick(); changed |= sim.LightOf(0) != 160; }
        Assert.Equal(changes, changed);
    }

    private static AuthoritySimulation Load(string ns, int special)
    {
        var text = "namespace = \"" + ns + "\"; sector { heightceiling = 128; lightlevel = 160; special = "
            + special + "; } thing { type = 1; skill1 = true; skill2 = true; skill3 = true; skill4 = true; skill5 = true; single = true; coop = true; dm = true; }";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        return AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
    }
}
