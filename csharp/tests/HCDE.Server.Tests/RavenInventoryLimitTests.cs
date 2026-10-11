using HCDE.MapLoader.Tests;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class RavenInventoryLimitTests
{
    [Theory]
    [InlineData(16)]
    [InlineData(25)]
    public void ServerLimitReachesRavenHealthGrants(int maximum)
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-raven-limit-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
        try
        {
            Assert.True(DedicatedServerCommandLine.TryParse(["--iwad", path, "--inventory-max", maximum.ToString(), "--raven-health-pickups"], out var options, out var error), error);
            options.Port = 0; using var host = new DedicatedServerHost(options);
            var sim = Assert.IsType<AuthoritySimulation>(host.Simulation); Assert.Equal(maximum, sim.DefaultInventoryMaximum);
            Assert.True(sim.RavenHealthPickups);
            var player = sim.Players.Single(); Assert.True(player.Inventory.GiveHealthPickup("ArtiHealth", int.MaxValue));
            Assert.Equal(maximum, player.Inventory.CountHealthPickup("ArtiHealth"));
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("bad")]
    [InlineData("")]
    public void InvalidOrMissingLimitReportsError(string value)
    {
        string[] args = value.Length == 0 ? ["--inventory-max"] : ["--inventory-max", value];
        Assert.False(DedicatedServerCommandLine.TryParse(args, out _, out var error));
        Assert.Equal("--inventory-max requires a positive integer", error);
    }
}
