using System.Globalization;
using HCDE.MapLoader.Tests;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class RewindConfigurationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void StartupFlagsConfigureSimulationAndAutomaticHistory(bool rewind, bool lagCompensation)
    {
        WithWad(path =>
        {
            var args = new List<string> { "--iwad", path, "--rewind-interval", "0.25", "--rewind-depth", "2",
                "--rewind-max-mb", "1", "--lag-comp-max-age", "7" };
            if (rewind) args.Add("--rewind");
            if (lagCompensation) args.Add("--lag-compensation");
            Assert.True(DedicatedServerCommandLine.TryParse(args.ToArray(), out var options, out var error), error);
            options.Port = 0;
            using var host = new DedicatedServerHost(options);
            var sim = Assert.IsType<AuthoritySimulation>(host.Simulation);
            Assert.Equal(rewind, sim.RewindEnabled); Assert.Equal(lagCompensation, sim.Rewind.LagCompensationEnabled);
            Assert.Equal(0.25f, sim.Rewind.CaptureIntervalSeconds); Assert.Equal(2, sim.Rewind.Capacity);
            Assert.Equal(1024L * 1024, sim.Rewind.MemoryLimitBytes); Assert.Equal(7, sim.Rewind.MaxLagCompensationAgeTics);
            for (var i = 0; i < 17; i++) sim.Tick();
            Assert.Equal(rewind ? 2 : 0, sim.Rewind.Count);
            Assert.Equal(rewind ? 1 : 0, sim.Rewind.EvictionsForDepth);
        });
    }

    [Theory]
    [InlineData("0.25", 1, 1, 1)]
    [InlineData("30", 64, 512, 70)]
    public void CommandLineAcceptsNativeConfigurationBoundaries(string interval, int depth, int memory, int age)
    {
        WithWad(path =>
        {
            Assert.True(DedicatedServerCommandLine.TryParse(["--iwad", path, "--rewind-interval", interval,
                "--rewind-depth", depth.ToString(CultureInfo.InvariantCulture), "--rewind-max-mb", memory.ToString(CultureInfo.InvariantCulture),
                "--lag-comp-max-age", age.ToString(CultureInfo.InvariantCulture)], out var options, out var error), error);
            Assert.Equal(float.Parse(interval, CultureInfo.InvariantCulture), options.RewindIntervalSeconds);
            Assert.Equal(depth, options.RewindDepth); Assert.Equal(memory, options.RewindMaxMegabytes);
            Assert.Equal(age, options.LagCompensationMaxAgeTics);
        });
    }

    [Fact]
    public void DefaultsKeepBothFeaturesDisabledAndUseNativeServerLimits()
    {
        WithWad(path =>
        {
            Assert.True(DedicatedServerCommandLine.TryParse(["--iwad", path], out var options, out var error), error);
            Assert.False(options.EnableRewind); Assert.False(options.EnableLagCompensation);
            Assert.Equal(1f, options.RewindIntervalSeconds); Assert.Equal(10, options.RewindDepth);
            Assert.Equal(32, options.RewindMaxMegabytes); Assert.Equal(12, options.LagCompensationMaxAgeTics);
        });
    }

    [Theory]
    [InlineData("--rewind-interval", "0.2")]
    [InlineData("--rewind-interval", "31")]
    [InlineData("--rewind-interval", "NaN")]
    [InlineData("--rewind-interval", "Infinity")]
    [InlineData("--rewind-depth", "0")]
    [InlineData("--rewind-depth", "65")]
    [InlineData("--rewind-depth", "many")]
    [InlineData("--rewind-max-mb", "0")]
    [InlineData("--rewind-max-mb", "513")]
    [InlineData("--lag-comp-max-age", "0")]
    [InlineData("--lag-comp-max-age", "71")]
    public void InvalidCommandLineValuesReportTheirOption(string option, string value)
    {
        Assert.False(DedicatedServerCommandLine.TryParse([option, value], out _, out var error));
        Assert.StartsWith(option, error);
    }

    [Theory]
    [InlineData("--rewind-interval")]
    [InlineData("--rewind-depth")]
    [InlineData("--rewind-max-mb")]
    [InlineData("--lag-comp-max-age")]
    public void MissingValuesReportTheirOption(string option)
    {
        Assert.False(DedicatedServerCommandLine.TryParse([option], out _, out var error));
        Assert.StartsWith(option, error);
    }

    [Theory]
    [InlineData(0, "RewindIntervalSeconds")]
    [InlineData(1, "RewindDepth")]
    [InlineData(2, "RewindMaxMegabytes")]
    [InlineData(3, "LagCompensationMaxAgeTics")]
    public void ProgrammaticInvalidConfigurationFailsBeforeOpeningServer(int kind, string parameter)
    {
        var options = new DedicatedServerOptions { Port = 0 };
        switch (kind)
        {
            case 0: options.RewindIntervalSeconds = float.NaN; break;
            case 1: options.RewindDepth = 0; break;
            case 2: options.RewindMaxMegabytes = 513; break;
            default: options.LagCompensationMaxAgeTics = 71; break;
        }
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => new DedicatedServerHost(options));
        Assert.Equal(parameter, error.ParamName);
    }

    [Fact]
    public void FractionalIntervalParsingUsesInvariantCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            WithWad(path =>
            {
                Assert.True(DedicatedServerCommandLine.TryParse(["--iwad", path, "--rewind-interval", "0.5"],
                    out var options, out var error), error);
                Assert.Equal(0.5f, options.RewindIntervalSeconds);
            });
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    private static void WithWad(Action<string> test)
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-rewind-options-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
        try { test(path); }
        finally { File.Delete(path); }
    }
}
