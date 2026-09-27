using System.Text;
using HCDE.MapLoader;
using HCDE.MapLoader.Tests;

namespace HCDE.Server.Tests;

public class MapsModsTests
{
    [Fact]
    public void CommandLineAcceptsPk3MapAndEmbeddedPatch()
    {
        var basePath = Path.GetTempFileName(); var modPath = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(basePath, TestWadBuilder.BuildMinimalMapWad("MAP01"));
            File.WriteAllBytes(modPath, ModResourcesTests.Zip(("maps/MAP02.wad", TestWadBuilder.BuildMinimalMapWad("MAP01")),
                ("dehacked.txt", Encoding.UTF8.GetBytes("Thing 1\nHit points = 222\n"))));
            Assert.True(DedicatedServerCommandLine.TryParse(["--iwad", basePath, "--file", modPath, "--map", "MAP02"], out var options, out var error), error);
            Assert.NotNull(options.Resources);
            options.Port = 0;
            using var host = new DedicatedServerHost(options);
            Assert.Equal("MAP02", host.Simulation!.Level.MapName);
            Assert.Equal(222, host.Simulation.Players.Single().Health);
        }
        finally { File.Delete(basePath); File.Delete(modPath); }
    }
    [Fact]
    public void CommandLineLoadsRepeatedModsAndHostAppliesPatchesInOrder()
    {
        var basePath = Path.GetTempFileName();
        var first = Path.GetTempFileName();
        var second = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(basePath, TestWadBuilder.BuildMinimalMapWad("MAP01"));
            File.WriteAllBytes(first, HCDE.MapLoader.Tests.MapsModsTests.Wad(("DEHACKED", Encoding.UTF8.GetBytes("Thing 1\nHit points = 123\n"))));
            File.WriteAllBytes(second, HCDE.MapLoader.Tests.MapsModsTests.Wad(("DEHACKED", Encoding.UTF8.GetBytes("Thing 1\nHit points = 234\n"))));
            Assert.True(DedicatedServerCommandLine.TryParse(["--iwad", basePath, "--file", first, "--file", second], out var options, out var error), error);
            options.Port = 0;
            using var host = new DedicatedServerHost(options);
            Assert.Equal(234, Assert.Single(host.Simulation!.Players).Health);
        }
        finally
        {
            File.Delete(basePath);
            File.Delete(first);
            File.Delete(second);
        }
    }

    [Fact]
    public void MissingModIsAParseError()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
            Assert.False(DedicatedServerCommandLine.TryParse(["--iwad", path, "--file", path + ".missing"], out _, out var error));
            Assert.NotNull(error);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void InvalidMapDoesNotStartAnEmptyServer()
    {
        Assert.Throws<InvalidDataException>(() => new DedicatedServerHost(new DedicatedServerOptions { Port = 0, IwadBytes = [1, 2, 3] }));
    }

    [Fact]
    public void InvalidPatchDoesNotStartServer()
    {
        Assert.True(WadLoadOrder.TryMerge([TestWadBuilder.BuildMinimalMapWad("MAP01"),
            HCDE.MapLoader.Tests.MapsModsTests.Wad(("DEHACKED", Encoding.UTF8.GetBytes("Thing 99999\nHit points = 12\n")))], out var bytes, out _));
        Assert.Throws<InvalidDataException>(() => new DedicatedServerHost(new DedicatedServerOptions { Port = 0, IwadBytes = bytes }));
    }
}
