using HCDE.MapLoader.Tests;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class StrifeHealthPickupConfigurationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ServerOptionSelectsHealthPickupMapping(bool enabled)
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-strife-health-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
        try
        {
            var args = new List<string> { "--iwad", path };
            if (enabled) args.Add("--strife-health-pickups");
            Assert.True(DedicatedServerCommandLine.TryParse(args.ToArray(), out var options, out var error), error);
            options.Port = 0; using var host = new DedicatedServerHost(options);
            var sim = Assert.IsType<AuthoritySimulation>(host.Simulation); Assert.Equal(enabled, sim.StrifeHealthPickups);
            var player = sim.Players.Single(); player.Health = 40;
            Assert.True(PickupCatalog.TryGive(player, 2011));
            Assert.Equal(enabled ? 40 : 50, player.Health);
            Assert.Equal(enabled ? 1 : 0, player.Inventory.CountHealthPickup("MedPatch"));
        }
        finally { File.Delete(path); }
    }
}
