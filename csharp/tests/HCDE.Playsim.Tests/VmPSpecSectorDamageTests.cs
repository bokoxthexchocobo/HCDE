namespace HCDE.Playsim.Tests;

public class VmPSpecSectorDamageTests
{
    [Fact]
    public void SkipNonShootable() =>
        Assert.True(VmPSpecSectorDamage.SkipNonShootable(false));

    [Fact]
    public void VavoomSwapHeights_Swaps() =>
        Assert.True(VmPSpecSectorDamage.VavoomSwapHeights(10, 5, out var low, out _) && low == 5);

    [Fact]
    public void DamageInAir3dFloor_WhenFlagSet() =>
        Assert.True(VmPSpecSectorDamage.DamageInAir3dFloor(4, true));

    [Fact]
    public void OrInAirFlag() =>
        Assert.Equal(5, VmPSpecSectorDamage.OrInAirFlag(1));

    [Fact]
    public void TerrainDamageTick() =>
        Assert.True(VmPSpecSectorDamage.TerrainDamageTick(32, 31));

    [Fact]
    public void ShowSecretMessage_WhenLocal() =>
        Assert.True(VmPSpecSectorDamage.ShowSecretMessage(true, true, true));

    [Fact]
    public void PlaySplashWhenDamagePositive() =>
        Assert.True(VmPSpecSectorDamage.PlaySplashWhenDamagePositive(5, 0));

    [Fact]
    public void ActorOnSecretSector() =>
        Assert.True(VmPSpecSectorDamage.ActorOnSecretSector(true, true));

    [Fact]
    public void PrintSecretSector() =>
        Assert.True(VmPSpecSectorDamage.PrintSecretSector(true, 1));

    [Fact]
    public void CountFoundSecret() =>
        Assert.True(VmPSpecSectorDamage.CountFoundSecret());

    [Fact]
    public void GiveSecretVmCallParams() =>
        Assert.True(VmPSpecSectorDamage.GiveSecretVmCallParams(true, false));
}
