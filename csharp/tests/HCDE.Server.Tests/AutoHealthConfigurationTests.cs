using HCDE.MapLoader.Tests;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class AutoHealthConfigurationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ServerSettingControlsStrifeItemsButPreservesEmergencyItems(bool disabled)
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-auto-health-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
        try
        {
            var args = new List<string> { "--iwad", path };
            if (disabled) { args.Add("--disable-auto-health"); args.Add("--disable-auto-health"); }
            Assert.True(DedicatedServerCommandLine.TryParse(args.ToArray(), out var options, out var error), error);
            Assert.Equal(disabled, options.DisableAutoHealth); options.Port = 0;
            using var host = new DedicatedServerHost(options);
            var sim = Assert.IsType<AuthoritySimulation>(host.Simulation);
            Assert.Equal(disabled, sim.DisableAutoHealth);
            var player = sim.Players.Single(); player.Health = 60;
            player.Inventory.AddHealthPickup(20, 2, 3);
            ActorDamage.Apply(player, 20);
            Assert.Equal(disabled ? 40 : 60, player.Health);
            Assert.Equal(disabled ? 2 : 1, Assert.Single(player.Inventory.HealthPickups).Amount);
            player.Inventory.AddHealthPickup(25, 2, 2);
            Assert.Equal(25, player.Inventory.AutoUseHealth(20, false));
        }
        finally { File.Delete(path); }
    }
}
