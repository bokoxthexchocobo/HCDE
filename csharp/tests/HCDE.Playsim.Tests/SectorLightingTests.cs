using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorLightingTests
{
    [Theory]
    [InlineData(8, false)]
    [InlineData(40, false)]
    [InlineData(72, true)]
    public void GlowStartsAutomaticallyAndReversesBeforeEndpoints(short special, bool extended)
    {
        var sim = Room(special, extended);
        Assert.Equal(128, sim.LightOf(0));
        foreach (var expected in new[] { 120, 112, 104, 104, 112, 120, 120, 112 })
        { sim.Tick(); Assert.Equal(expected, sim.LightOf(0)); }
        Assert.Equal(special, sim.Level.Sectors[0].Special);
    }

    [Theory]
    [InlineData(12, false, 35)]
    [InlineData(13, false, 15)]
    [InlineData(76, true, 35)]
    [InlineData(77, true, 15)]
    public void SynchronizedStrobeStartsOnFirstTick(short special, bool extended, int darkTime)
    {
        var sim = Room(special, extended);
        sim.Tick(); Assert.Equal(96, sim.LightOf(0));
        for (var i = 1; i < darkTime; i++) { sim.Tick(); Assert.Equal(96, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(128, sim.LightOf(0));
        for (var i = 1; i < 5; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(96, sim.LightOf(0));
    }

    [Theory]
    [InlineData(2, false, 15)]
    [InlineData(3, false, 35)]
    [InlineData(4, false, 15)]
    [InlineData(66, true, 15)]
    [InlineData(67, true, 35)]
    [InlineData(68, true, 15)]
    [InlineData(104, true, 15)]
    public void OrdinaryStrobeUsesBoundedStartupAndNativeDarkDuration(short special, bool extended, int darkTime)
    {
        var sim = Room(special, extended); var delay = 0;
        while (sim.LightOf(0) == 128 && delay < 8) { sim.Tick(); delay++; }
        Assert.InRange(delay, 1, 8); Assert.Equal(96, sim.LightOf(0));
        for (var i = 1; i < darkTime; i++) { sim.Tick(); Assert.Equal(96, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(13)]
    public void ExtendedFormatDoesNotInterpretDoomSectorNumbers(short special)
    {
        var sim = Room(special, true);
        for (var i = 0; i < 50; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
    }

    [Fact]
    public void StopRemovesInitializedEffectWithoutChangingMapDefinition()
    {
        var sim = Room(72, true); sim.Tick();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            new LevelLine { Special = 117, Arg0 = 7, PlayerUse = true }, true));
        for (var i = 0; i < 50; i++) { sim.Tick(); Assert.Equal(120, sim.LightOf(0)); }
        Assert.Equal(72, sim.Level.Sectors[0].Special);
    }

    [Fact]
    public void NarrowGlowRangeStaysAtInitialLight()
    {
        var sim = Room(8, neighbor: 124);
        for (var i = 0; i < 20; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
    }

    private static AuthoritySimulation Room(short special, bool extended = false, short neighbor = 96) =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Format = extended ? MapDataFormat.HexenBinary : MapDataFormat.DoomBinary,
            Sectors = [new LevelSector { Index = 0, Tag = 7, Special = special, LightLevel = 128, CeilingHeight = 128 },
                new LevelSector { Index = 1, LightLevel = neighbor, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [new LevelLine { SideFront = 0, SideBack = 1 }],
            Things = [new LevelThing { Type = 1 }],
        });
}
