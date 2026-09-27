using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RandomSectorLightingTests
{
    [Theory]
    [InlineData(1, false, 0)]
    [InlineData(33, false, 42)]
    [InlineData(65, true, 0)]
    [InlineData(65, true, 1234)]
    public void LightFlashHasNativeCountdownWithoutFlickersExtraTick(short special, bool extended, int seed)
    {
        var sim = Room(special, extended, seed);
        Assert.Contains(Until(sim, 40, 65), new[] { 1, 65 });
        for (var i = 0; i < 12; i++)
        {
            Assert.InRange(Until(sim, 128, 8), 1, 8);
            Assert.Contains(Until(sim, 40, 65), new[] { 1, 65 });
        }
    }

    [Theory]
    [InlineData(17, false)]
    [InlineData(49, false)]
    [InlineData(81, true)]
    public void FireFlickerOnlyUpdatesEveryFourthTick(short special, bool extended)
    {
        var sim = Room(special, extended);
        var seen = new HashSet<int>();
        for (var cycle = 0; cycle < 40; cycle++)
        {
            var before = sim.LightOf(0);
            for (var i = 0; i < 3; i++) { sim.Tick(); Assert.Equal(before, sim.LightOf(0)); }
            sim.Tick();
            Assert.Contains((int)sim.LightOf(0), new[] { 56, 80, 96, 112, 128 });
            seen.Add(sim.LightOf(0));
        }
        Assert.True(seen.Count > 1);
    }

    [Fact]
    public void FireFlickerUsesCurrentLightForMinimumDecision()
    {
        var sim = Room(81, true);
        Activate(sim, 112, 0); // Below captured neighbor minimum + 16 regardless of the next random amount.
        for (var i = 0; i < 3; i++) { sim.Tick(); Assert.Equal(0, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(56, sim.LightOf(0));
    }

    [Fact]
    public void FireMinimumCanExceedInitialLightAndClampsToSignedShort()
    {
        var sim = Room(81, true, light: 32760, neighbor: 32760);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(short.MaxValue, sim.LightOf(0));
    }

    [Theory]
    [InlineData(65)]
    [InlineData(81)]
    public void StopFreezesLightAndDoesNotConsumeCombatRandomness(short special)
    {
        var sim = Room(special, true); var control = Room(0, true);
        for (var i = 0; i < 100; i++) sim.Tick();
        Activate(sim, 117); var light = sim.LightOf(0);
        for (var i = 0; i < 100; i++)
        {
            sim.Tick(); Assert.Equal(light, sim.LightOf(0));
            Assert.Equal(control.NextCombatRandom(), sim.NextCombatRandom());
        }
    }

    [Theory]
    [InlineData(65)]
    [InlineData(81)]
    public void SameSeedReplaysLightingAndChecksums(short special)
    {
        var a = Room(special, true, 42); var b = Room(special, true, 42);
        Assert.Equal(a.Checksum, b.Checksum);
        for (var i = 0; i < 200; i++) { a.Tick(); b.Tick(); Assert.Equal(a.Checksum, b.Checksum); }
    }

    private static int Until(AuthoritySimulation sim, int target, int limit)
    {
        for (var i = 1; i <= limit; i++) { sim.Tick(); if (sim.LightOf(0) == target) return i; }
        Assert.Fail($"Light did not reach {target} in {limit} ticks."); return 0;
    }

    private static void Activate(AuthoritySimulation sim, int special, int value = 0) =>
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            new LevelLine { Special = special, Arg0 = 7, Arg1 = value, PlayerUse = true }, true));

    private static AuthoritySimulation Room(short special, bool extended, int seed = 0, short light = 128, short neighbor = 40) =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Format = extended ? MapDataFormat.HexenBinary : MapDataFormat.DoomBinary,
            Sectors = [new LevelSector { Index = 0, Tag = 7, Special = special, LightLevel = light, CeilingHeight = 128 },
                new LevelSector { Index = 1, LightLevel = neighbor, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [new LevelLine { SideFront = 0, SideBack = 1 }],
            Things = [new LevelThing { Type = 1 }],
        }, seed);
}
