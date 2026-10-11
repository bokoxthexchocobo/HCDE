using HCDE.MapLoader.Tests;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class UnlimitedHealthConfigurationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ServerSettingControlsMatchingHealthStackLimit(bool enabled)
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-unlimited-health-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
        try
        {
            var args = new List<string> { "--iwad", path };
            if (enabled) args.Add("--unlimited-health-pickups");
            Assert.True(DedicatedServerCommandLine.TryParse(args.ToArray(), out var options, out var error), error);
            options.Port = 0; using var host = new DedicatedServerHost(options);
            var sim = Assert.IsType<AuthoritySimulation>(host.Simulation); Assert.Equal(enabled, sim.UnlimitedHealthPickups);
            var player = sim.Players.Single(); player.Inventory.GiveHealthPickup("MedicalKit", 15);
            Assert.Equal(enabled, player.Inventory.GiveHealthPickup("MedicalKit", 1));
            Assert.Equal(enabled ? 16 : 15, player.Inventory.CountHealthPickup("MedicalKit"));
        }
        finally { File.Delete(path); }
    }
}
