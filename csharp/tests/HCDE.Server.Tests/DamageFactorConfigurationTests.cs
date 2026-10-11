using HCDE.MapLoader.Tests;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class DamageFactorConfigurationTests
{
    [Fact]
    public void ParsedFactorsReachServerGameplay()
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-damage-factors-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
        try
        {
            Assert.True(DedicatedServerCommandLine.TryParse(["--iwad", path, "--player-damage-factor", "0.5",
                "--monster-damage-factor", "2", "--friendly-damage-factor", "0.25", "--skill-damage-factor", "0.5"], out var options, out var error), error);
            options.Port = 0;
            using var host = new DedicatedServerHost(options);
            var sim = Assert.IsType<AuthoritySimulation>(host.Simulation);
            Assert.Equal(0.5, sim.PlayerDamageFactor); Assert.Equal(2, sim.MonsterDamageFactor);
            Assert.Equal(0.25, sim.FriendlyDamageFactor);
            Assert.Equal(0.5, sim.SkillDamageFactor);
            var target = sim.AddBot(0, 0); target.Brain = null;
            Assert.Equal(16, ActorDamage.Apply(target, 8).HealthLost);
            target.Friendly = true;
            Assert.Equal(2, ActorDamage.Apply(target, 8).HealthLost);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("--player-damage-factor", "NaN")]
    [InlineData("--monster-damage-factor", "-1")]
    [InlineData("--friendly-damage-factor", "Infinity")]
    [InlineData("--player-damage-factor", "")]
    [InlineData("--skill-damage-factor", "NaN")]
    [InlineData("--skill-damage-factor", "-0.5")]
    public void InvalidFactorsReportExplicitErrors(string option, string value)
    {
        Assert.False(DedicatedServerCommandLine.TryParse([option, value], out _, out var error));
        Assert.Equal($"{option} requires a finite nonnegative number", error);
    }
}
