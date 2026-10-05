using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedMaximumHealthTests
{
    [Fact]
    public void MaximumHealthConfigurationChangesChecksum()
    {
        var level = new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] };
        var baseline = AuthoritySimulation.Start(level);
        var patched = AuthoritySimulation.Start(level,
            dehacked: DehackedPatch.Apply("Misc 0\nMax Health = 150\n"));
        baseline.Tick(); patched.Tick();
        Assert.NotEqual(baseline.Checksum, patched.Checksum);
    }

    [Theory]
    [InlineData(false, 0, 150)]
    [InlineData(true, 0, 100)]
    [InlineData(false, 180, 180)]
    [InlineData(true, 180, 180)]
    public void DehFallbackRespectsCompatibilityAndExplicitPlayerCap(bool compat, int cap, int expected)
    {
        var patch = DehackedPatch.Apply("Misc 0\nMax Health = 150\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
            dehacked: patch, compat: compat ? CompatSurface.DehHealth : CompatSurface.None);
        var player = sim.Players.Single(); player.MaxHealth = cap; player.Health = 80;
        Assert.Equal(expected, player.GetMaxHealth(false));
        Assert.True(player.GiveBody(200)); Assert.Equal(expected, player.Health);
    }

    [Fact]
    public void PatchBaselinePreservesMaximumHealth()
    {
        var first = DehackedPatch.Apply("Misc 0\nMax Health = 150\n");
        Assert.Equal(150, DehackedPatch.Apply("", first).MaxHealth);
    }

    [Fact]
    public void InvalidMaximumReportsParserError()
    {
        Assert.NotEmpty(DehackedPatch.Apply("Misc 0\nMax Health = invalid\n").Errors);
    }
}
