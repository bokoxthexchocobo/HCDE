using HCDE.MapLoader.Tests;
using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class InfiniteHealthUseConfigurationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BroaderStoredInventoryOptionControlsArmorUse(bool enabled)
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-infinite-stored-use-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
        try
        {
            var args = new List<string> { "--iwad", path };
            if (enabled) args.Add("--infinite-stored-inventory-use");
            Assert.True(DedicatedServerCommandLine.TryParse(args.ToArray(), out var options, out var error), error);
            options.Port = 0; using var host = new DedicatedServerHost(options);
            var sim = Assert.IsType<AuthoritySimulation>(host.Simulation);
            Assert.Equal(enabled, sim.InfiniteStoredInventoryUse); Assert.Equal(enabled, sim.InfiniteHealthItemUse);
            var inventory = sim.Players.Single().Inventory; inventory.Armor = 10;
            inventory.TryKeepArmorPickup(100, 50, armorType: "StoredSuit");
            var code = new byte[12];
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(code, (int)AcsPcode.PushNumber);
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(8), (int)AcsPcode.UseInventory);
            sim.Acs.Add(new AcsProgram { Number = 1, StringTable = ["StoredSuit"], Code = code });
            Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, sim.Players.Single())); sim.Acs.Tick(sim);
            Assert.Equal(enabled ? 1 : 0, inventory.SpareArmor.Count); Assert.Equal(100, inventory.Armor);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ServerOptionControlsManualUseWithoutChangingPickupLimits(bool enabled)
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-infinite-health-use-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
        try
        {
            var args = new List<string> { "--iwad", path };
            if (enabled) args.Add("--infinite-health-item-use");
            Assert.True(DedicatedServerCommandLine.TryParse(args.ToArray(), out var options, out var error), error);
            Assert.Equal(enabled, options.InfiniteHealthItemUse); options.Port = 0;
            using var host = new DedicatedServerHost(options);
            var sim = Assert.IsType<AuthoritySimulation>(host.Simulation);
            Assert.Equal(enabled, sim.InfiniteHealthItemUse); Assert.False(sim.UnlimitedHealthPickups);
            var player = sim.Players.Single(); player.Health = 30; player.Inventory.GiveHealthPickup("MedPatch", 1);
            Assert.True(player.Inventory.UseHealthPickup("MedPatch")); Assert.Equal(40, player.Health);
            Assert.Equal(enabled ? 1 : 0, player.Inventory.CountHealthPickup("MedPatch"));
            player.Inventory.GiveHealthPickup("MedicalKit", 15);
            Assert.False(player.Inventory.GiveHealthPickup("MedicalKit", 1));
        }
        finally { File.Delete(path); }
    }
}
