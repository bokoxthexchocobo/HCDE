using HCDE.MapLoader.Tests;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class InfiniteAmmoConfigurationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CommandLineConfiguresSimulationAndEmptyWeaponFiring(bool enabled)
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-infinite-ammo-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
        try
        {
            var args = new List<string> { "--iwad", path };
            if (enabled) args.Add("--infinite-ammo");
            Assert.True(DedicatedServerCommandLine.TryParse(args.ToArray(), out var options, out var error), error);
            Assert.Equal(enabled, options.InfiniteAmmo);
            options.Port = 0;
            using var host = new DedicatedServerHost(options);
            var sim = Assert.IsType<AuthoritySimulation>(host.Simulation);
            Assert.Equal(enabled, sim.InfiniteAmmo);
            var player = sim.Players.Single(); player.Inventory.Bullets = 0;
            Assert.Equal(enabled, HitscanCombat.Fire(sim, player));
            Assert.Equal(0, player.Inventory.Bullets);
        }
        finally { File.Delete(path); }
    }
}
