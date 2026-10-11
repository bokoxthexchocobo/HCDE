using HCDE.MapLoader.Tests;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class MonsterPainConfigurationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ParsedPainSettingReachesServerDamageReactions(bool suppressPain)
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-monster-pain-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
        try
        {
            var args = new List<string> { "--iwad", path };
            if (suppressPain) { args.Add("--no-monster-pain"); args.Add("--no-monster-pain"); }
            Assert.True(DedicatedServerCommandLine.TryParse(args.ToArray(), out var options, out var error), error);
            Assert.Equal(suppressPain, options.NoMonsterPain); options.Port = 0;
            using var host = new DedicatedServerHost(options);
            var sim = Assert.IsType<AuthoritySimulation>(host.Simulation);
            Assert.Equal(suppressPain, sim.NoMonsterPain);
            var target = sim.AddBot(0, 0); target.Brain = null; target.PainChance = 256; target.ReactionTime = 12;
            var initial = target.States.Current;
            var result = ActorDamage.Apply(target, 7, inflictor: new Actor { ForcePain = true });
            Assert.Equal(7, result.HealthLost); Assert.Equal(0, target.ReactionTime);
            Assert.Equal(suppressPain ? initial : target.PainState, target.States.Current);
        }
        finally { File.Delete(path); }
    }
}
