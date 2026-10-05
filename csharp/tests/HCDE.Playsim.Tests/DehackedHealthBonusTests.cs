using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedHealthBonusTests
{
    private static AuthoritySimulation Room(string? patch, bool compat) => AuthoritySimulation.Start(
        new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        dehacked: patch is null ? null : DehackedPatch.Apply(patch),
        compat: compat ? CompatSurface.DehHealth : CompatSurface.None);

    [Theory]
    [InlineData(null, false, 200)]
    [InlineData(null, true, 200)]
    [InlineData("", true, 200)]
    [InlineData("Misc 0\n", true, 100)]
    [InlineData("Misc 0\nMax Health = 150\n", true, 150)]
    [InlineData("Misc 0\nMax Health = 150\n", false, 300)]
    [InlineData("Misc 0\nSoulsphere Health = 60\n", true, 100)]
    public void BonusUsesNativeMiscAndCompatibilityCap(string? patch, bool compat, int cap)
    {
        var player = Room(patch, compat).Players.Single();
        player.MaxHealth = 400; player.Stamina = 30; player.BonusHealth = 20;
        player.Health = cap + player.BonusHealth - 1;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.HealthBonus, pickupAmount: 10));
        Assert.Equal(cap + player.BonusHealth, player.Health);
        Assert.False(PickupCatalog.TryGive(player, PickupCatalog.HealthBonus));
    }

    [Fact]
    public void BaselinePreservesMiscMarker()
    {
        Assert.False(DehackedPatch.Apply("").HealthBonusCapPatched);
        Assert.True(DehackedPatch.Apply("", DehackedPatch.Apply("Misc 0\n")).HealthBonusCapPatched);
    }

    [Theory]
    [InlineData(PickupCatalog.Soulsphere)]
    [InlineData(PickupCatalog.Megasphere)]
    public void SphereExplicitCapIncludesBonusHealthButNotStamina(int pickup)
    {
        var player = Room(null, false).Players.Single();
        player.Health = 199; player.BonusHealth = 20; player.Stamina = 30;
        Assert.True(PickupCatalog.TryGive(player, pickup)); Assert.Equal(220, player.Health);
    }

    [Fact]
    public void MiscMarkerChangesChecksum()
    {
        var baseline = Room(null, true); var patched = Room("Misc 0\n", true);
        baseline.Tick(); patched.Tick(); Assert.NotEqual(baseline.Checksum, patched.Checksum);
    }
}
