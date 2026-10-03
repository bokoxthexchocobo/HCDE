using HCDE.MapLoader.Tests;
using HCDE.Playsim;
namespace HCDE.Server.Tests;
public class PainLimitConfigurationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ParsedCompatibilitySurvivesServerMapBoot(bool limited)
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-pain-limit-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
        try
        {
            var args = new List<string> { "--iwad", path, "--hexen-crush-defaults" };
            if (limited) { args.Add("--limit-pain"); args.Add("--limit-pain"); }
            Assert.True(DedicatedServerCommandLine.TryParse(args.ToArray(), out var options, out var error), error);
            options.Port = 0;
            using var host = new DedicatedServerHost(options);
            Assert.NotNull(host.Simulation);
            Assert.True(host.Simulation.Compat.HasFlag(CompatSurface.HexenCrushDefaults));
            Assert.Equal(limited, host.Simulation.Compat.HasFlag(CompatSurface.LimitPain));
            Assert.Equal(options.Compatibility, host.Simulation.Compat);
        }
        finally { File.Delete(path); }
    }
}