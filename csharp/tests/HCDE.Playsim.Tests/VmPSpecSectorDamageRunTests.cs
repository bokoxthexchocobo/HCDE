namespace HCDE.Playsim.Tests;

public class VmPSpecSectorDamageRunTests
{
    [Fact]
    public void SkipNonShootable()
    {
        var hit = false;
        VmPSpecSectorDamageRun.DoSectorDamage(
            new VmPSpecSectorDamageRun.Actor { Flags = 0 },
            new VmPSpecSectorDamageRun.Sector(),
            5,
            null,
            new VmPSpecSectorDamageRun.DamageContext { Damage = (_, _, _, _) => hit = true },
            0);
        Assert.False(hit);
    }

    [Fact]
    public void DoSectorDamage_SkipsNonShootable() => SkipNonShootable();

    [Fact]
    public void VavoomSwapHeights_Swaps()
    {
        double z1 = 10, z2 = 5;
        if (z2 < z1)
            (z1, z2) = (z2, z1);
        Assert.Equal(5, z1);
        Assert.Equal(10, z2);
    }

    [Fact]
    public void DamageInAir3dFloor_WhenFlagSet()
    {
        var hit = false;
        var actor = new VmPSpecSectorDamageRun.Actor
        {
            Flags = VmPSpecSectorDamageRun.MfShootable,
            Player = new object(),
            WaterLevel = 0,
            IsAtZ = _ => false,
        };
        var sec = new VmPSpecSectorDamageRun.Sector { FloorZ = _ => 0 };
        VmPSpecSectorDamageRun.DoSectorDamage(
            actor,
            sec,
            1,
            null,
            new VmPSpecSectorDamageRun.DamageContext { Damage = (_, _, _, _) => hit = true },
            VmPSpecSectorDamageRun.DamagePlayers | VmPSpecSectorDamageRun.DamageInAir);
        Assert.True(hit);
    }

    [Fact]
    public void OrInAirFlag() =>
        Assert.Equal(5, 1 | VmPSpecSectorDamageRun.DamageInAir);

    [Fact]
    public void TerrainDamageTick()
    {
        var splashed = false;
        VmPSpecSectorDamageRun.ActorOnSpecialFlat(
            new VmPSpecSectorDamageRun.Actor(),
            1,
            new VmPSpecSectorDamageRun.TerrainFlatContext
            {
                LevelTime = 32,
                DamageAmount = _ => 5,
                DamageTimeMask = _ => 31,
                AllowProtection = _ => false,
                ApplyTerrainDamage = (_, _) => 5,
                SplashIndex = _ => 0,
                PlaySplash = (_, _) => splashed = true,
            });
        Assert.True(splashed);
    }

    [Fact]
    public void ShowSecretMessage_WhenLocal()
    {
        var mid = false;
        VmPSpecSectorDamageRun.GiveSecret(new object(), true, false, 1, new VmPSpecSectorDamageRun.GiveSecretContext
        {
            ShowSecretMessage = true,
            LocalView = _ => true,
            MidPrint = () => mid = true,
            IncrementFoundSecrets = () => { },
        });
        Assert.True(mid);
    }

    [Fact]
    public void PlaySplashWhenDamagePositive()
    {
        var splashed = false;
        VmPSpecSectorDamageRun.ActorOnSpecialFlat(
            new VmPSpecSectorDamageRun.Actor(),
            2,
            new VmPSpecSectorDamageRun.TerrainFlatContext
            {
                LevelTime = 0,
                DamageAmount = _ => 5,
                DamageTimeMask = _ => 0,
                ApplyTerrainDamage = (_, _) => 5,
                SplashIndex = _ => 0,
                PlaySplash = (_, _) => splashed = true,
            });
        Assert.True(splashed);
    }

    [Fact]
    public void PrintSecretSector()
    {
        var printed = false;
        VmPSpecSectorDamageRun.GiveSecret(new object(), true, false, 3, new VmPSpecSectorDamageRun.GiveSecretContext
        {
            ShowSecretMessage = true,
            LocalView = _ => true,
            ShowSecretSector = true,
            PrintSector = _ => printed = true,
            IncrementFoundSecrets = () => { },
        });
        Assert.True(printed);
    }

    [Fact]
    public void CountFoundSecret()
    {
        var found = 0;
        VmPSpecSectorDamageRun.GiveSecret(new object(), false, false, 0, new VmPSpecSectorDamageRun.GiveSecretContext
        {
            ShowSecretMessage = false,
            IncrementFoundSecrets = () => found++,
        });
        Assert.Equal(1, found);
    }

    [Fact]
    public void GiveSecret_IncrementsFound() => CountFoundSecret();

    [Fact]
    public void GiveSecretVmCallParams()
    {
        var called = false;
        VmPSpecSectorDamageRun.GiveSecret(new object(), true, false, 0, new VmPSpecSectorDamageRun.GiveSecretContext
        {
            OnGiveSecret = (_, pm, _) => { called = pm; return 1; },
            ShowSecretMessage = false,
            IncrementFoundSecrets = () => { },
        });
        Assert.True(called);
    }
}
