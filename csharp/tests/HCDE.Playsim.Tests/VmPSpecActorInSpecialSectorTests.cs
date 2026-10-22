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
}
