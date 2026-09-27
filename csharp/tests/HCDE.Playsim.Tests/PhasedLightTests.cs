using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PhasedLightTests
{
    [Theory]
    [InlineData(21, false)]
    [InlineData(53, false)]
    [InlineData(1, true)]
    public void NativeCycleRepeatsEvery64Ticks(short special, bool extended)
    {
        var sim = Room(special, extended, 40); // Low bits encode phase 23.
        int[] pulse = [48, 65, 82, 99, 117, 134, 151, 168, 186, 203, 220, 237,
            237, 220, 203, 186, 168, 151, 134, 117, 99, 82, 65, 48];
        for (var cycle = 0; cycle < 3; cycle++)
        {
            foreach (var light in pulse) { sim.Tick(); Assert.Equal(light, sim.LightOf(0)); }
            for (var i = 0; i < 40; i++) { sim.Tick(); Assert.Equal(48, sim.LightOf(0)); }
        }
    }

    [Theory]
    [InlineData(0, 48)]
    [InlineData(40, 48)]
    [InlineData(51, 237)]
    [InlineData(52, 237)]
    [InlineData(63, 48)]
    [InlineData(115, 237)]
    [InlineData(-13, 237)]
    public void InitialLightSelectsPhaseUsingOnlyLowSixBits(short initial, int first)
    {
        var sim = Room(1, true, initial);
        Assert.Equal(initial, sim.LightOf(0));
        sim.Tick(); Assert.Equal(first, sim.LightOf(0));
        Assert.Equal(initial, sim.Level.Sectors[0].LightLevel);
    }

    [Fact]
    public void StopPreservesBrightnessAndImmediateChangeDoesNotResetPhase()
    {
        var sim = Room(1, true, 40);
        sim.Tick(); Assert.Equal(48, sim.LightOf(0));
        Activate(sim, 112, 5);
        sim.Tick(); Assert.Equal(65, sim.LightOf(0));
        Activate(sim, 117);
        for (var i = 0; i < 128; i++) { sim.Tick(); Assert.Equal(65, sim.LightOf(0)); }
    }

    [Fact]
    public void PhaseAffectsChecksumWhenCurrentBrightnessMatches()
    {
        var a = Room(1, true, 0); var b = Room(1, true, 1);
        a.Tick(); b.Tick();
        Assert.Equal(a.LightOf(0), b.LightOf(0));
        Assert.NotEqual(a.Checksum, b.Checksum);
    }

    private static void Activate(AuthoritySimulation sim, int special, int value = 0) =>
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            new LevelLine { Special = special, Arg0 = 7, Arg1 = value, PlayerUse = true }, true));

    private static AuthoritySimulation Room(short special, bool extended, short light) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = extended ? MapDataFormat.HexenBinary : MapDataFormat.DoomBinary,
        Sectors = [new LevelSector { Index = 0, Special = special, Tag = 7, LightLevel = light, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
