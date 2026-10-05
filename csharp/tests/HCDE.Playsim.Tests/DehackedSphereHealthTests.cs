using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedSphereHealthTests
{
    private const string Patch = "Misc 0\nMax Soulsphere = 250\nSoulsphere Health = 60\nMegasphere Health = 300\n";

    private static AuthoritySimulation Room(DehackedPatchResult? patch = null) => AuthoritySimulation.Start(
        new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        dehacked: patch);

    [Theory]
    [InlineData(100, 160)]
    [InlineData(230, 250)]
    public void SoulsphereUsesPatchedAmountAndCap(int initial, int expected)
    {
        var player = Room(DehackedPatch.Apply(Patch)).Players.Single(); player.Health = initial;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Soulsphere));
        Assert.Equal(expected, player.Health);
    }

    [Fact]
    public void MegasphereUsesPatchedHealthAndFixedArmor()
    {
        var player = Room(DehackedPatch.Apply(Patch)).Players.Single();
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Megasphere));
        Assert.Equal(300, player.Health);
        Assert.Equal(200, player.Inventory.Armor);
    }

    [Fact]
    public void IncrementalPatchPreservesSphereSettings()
    {
        var patch = DehackedPatch.Apply("", DehackedPatch.Apply(Patch));
        Assert.Equal(250, patch.MaxSoulsphere); Assert.Equal(60, patch.SoulsphereHealth);
        Assert.Equal(300, patch.MegasphereHealth); Assert.Empty(patch.Errors);
    }

    [Theory]
    [InlineData("Max Soulsphere")]
    [InlineData("Soulsphere Health")]
    [InlineData("Megasphere Health")]
    public void InvalidIntegerReportsError(string setting) =>
        Assert.NotEmpty(DehackedPatch.Apply($"Misc 0\n{setting} = invalid\n").Errors);

    [Theory]
    [InlineData("Max Soulsphere", 250)]
    [InlineData("Soulsphere Health", 60)]
    [InlineData("Megasphere Health", 300)]
    public void EachSettingChangesChecksum(string setting, int value)
    {
        var baseline = Room(); var patched = Room(DehackedPatch.Apply($"Misc 0\n{setting} = {value}\n"));
        baseline.Tick(); patched.Tick(); Assert.NotEqual(baseline.Checksum, patched.Checksum);
    }
}
