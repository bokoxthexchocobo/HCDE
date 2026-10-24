namespace HCDE.Playsim.Tests;

public class VmPSpecActorInSpecialSectorTests
{
    [Fact]
    public void ActorInSpecialSector_RepairsZeroDamageInterval()
    {
        var sector = new VmPSpecActorInSpecialSector.Sector { DamageInterval = 0 };
        VmPSpecActorInSpecialSector.ActorInSpecialSector(
            new VmPSpecActorInSpecialSector.Actor(),
            sector,
            null,
            new VmPSpecActorInSpecialSector.ActorContext { IsAtZ = (_, _) => true });
        Assert.Equal(32, sector.DamageInterval);
    }

    [Fact]
    public void ActorInSpecialSector_ExitSectorKillsPlayers()
    {
        var killed = false;
        VmPSpecActorInSpecialSector.ActorInSpecialSector(
            new VmPSpecActorInSpecialSector.Actor { Player = new() },
            new VmPSpecActorInSpecialSector.Sector { Flags = VmPSpecActorInSpecialSector.SecfExit1 },
            null,
            new VmPSpecActorInSpecialSector.ActorContext
            {
                IsAtZ = (_, _) => true,
                KillAllPlayers = () => killed = true,
                ExitLevel = _ => { },
            });
        Assert.True(killed);
    }

    [Fact]
    public void UpdateSpecialsTimelimit_WhenExceeded() =>
        Assert.True(VmPSpecActorInSpecialSector.UpdateSpecialsTimelimit(true, 1, 35 * 60));

    [Fact]
    public void ApplySectorFriction_SetsMoveFactor()
    {
        var sector = new VmPSpecActorInSpecialSector.Sector();
        VmPSpecActorInSpecialSector.ApplySectorFriction(sector, 100, false);
        Assert.True(sector.MoveFactor > 0);
    }

    [Fact]
    public void FrictionToMoveFactor_ClampsTiny() =>
        Assert.True(VmPSpecActorInSpecialSector.FrictionToMoveFactor(0) >= 1 / 2048.0);

    [Fact]
    public void SectorFrictionFromAmount_Clamped() =>
        Assert.InRange(VmPSpecActorInSpecialSector.SectorFrictionFromAmount(100), 0, 1);

    [Fact]
    public void SkipUntilGround_WhenAboveFloor() =>
        Assert.True(VmPSpecActorInSpecialSector.ShouldReturnBeforeGround(false, false, false, 0, false));

    [Fact]
    public void SectorFrictionPair_MatchesComponents()
    {
        var sector = new VmPSpecActorInSpecialSector.Sector();
        VmPSpecActorInSpecialSector.ApplySectorFriction(sector, 100, false);
        var f = VmPSpecActorInSpecialSector.SectorFrictionFromAmount(100);
        Assert.Equal(f, sector.Friction);
        Assert.Equal(VmPSpecActorInSpecialSector.FrictionToMoveFactor(f), sector.MoveFactor);
    }

    [Fact]
    public void IronFeetLeakDamage_FullBlocks()
    {
        var damaged = false;
        VmPSpecActorInSpecialSector.ActorInSpecialSector(
            new VmPSpecActorInSpecialSector.Actor { Player = new() },
            new VmPSpecActorInSpecialSector.Sector { DamageAmount = 5, DamageInterval = 32, LeakyDamage = 100 },
            null,
            new VmPSpecActorInSpecialSector.ActorContext
            {
                IsAtZ = (_, _) => true,
                LevelTime = 32,
                IronFeetLevel = () => 2,
                LeakRoll = () => 0,
                DamageActor = (_, _) => damaged = true,
            });
        Assert.False(damaged);
    }

    [Fact]
    public void IronFeetProtectionLevel_FullWins()
    {
        var damaged = false;
        VmPSpecActorInSpecialSector.ActorInSpecialSector(
            new VmPSpecActorInSpecialSector.Actor { Player = new() },
            new VmPSpecActorInSpecialSector.Sector { DamageAmount = 5, DamageInterval = 32, LeakyDamage = 256 },
            null,
            new VmPSpecActorInSpecialSector.ActorContext
            {
                IsAtZ = (_, _) => true,
                LevelTime = 32,
                IronFeetLevel = () => 2,
                LeakRoll = () => 0,
                DamageActor = (_, _) => damaged = true,
            });
        Assert.False(damaged);
    }

    [Fact]
    public void ActorOnSecretSector()
    {
        var secret = 0;
        VmPSpecActorInSpecialSector.ActorInSpecialSector(
            new VmPSpecActorInSpecialSector.Actor { Player = new() },
            new VmPSpecActorInSpecialSector.Sector { Secret = true, Index = 7 },
            null,
            new VmPSpecActorInSpecialSector.ActorContext
            {
                IsAtZ = (_, _) => true,
                ClearSecret = () => { },
                GiveSecret = i => secret = i,
            });
        Assert.Equal(7, secret);
    }
}
