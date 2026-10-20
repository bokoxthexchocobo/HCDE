namespace HCDE.Playsim.Tests;

public class VmPSpecFrictionSectorTests
{
    [Fact]
    public void TimelimitExit_WhenTimeExceeded() =>
        Assert.True(VmPSpecFrictionSector.TimelimitExit(true, 1, 35 * 60));

    [Fact]
    public void FrictionToMoveFactor_ClampsTiny() =>
        Assert.True(VmPSpecFrictionSector.ClampMoveFactor(0) >= 1 / 2048.0);

    [Fact]
    public void SectorFrictionFromAmount_Clamped() =>
        Assert.InRange(VmPSpecFrictionSector.SectorFrictionFromAmount(100), 0, 1);

    [Fact]
    public void DefaultDamageInterval_RepairsZero() =>
        Assert.Equal(32, VmPSpecFrictionSector.DefaultDamageInterval(0));

    [Fact]
    public void IronFeetProtectionLevel_FullWins() =>
        Assert.Equal(2, VmPSpecFrictionSector.IronFeetProtectionLevel(true, true, 0));

    [Fact]
    public void SkipUntilGround_WhenAboveFloor() =>
        Assert.True(VmPSpecFrictionSector.SkipUntilGround(false, false, false, 0, false));

    [Fact]
    public void IronFeetLeakDamage_FullBlocks() =>
        Assert.False(VmPSpecFrictionSector.IronFeetLeakDamage(2, 0, 256));

    [Fact]
    public void SectorFrictionPair_MatchesComponents()
    {
        var (f, m) = VmPSpecFrictionSector.SectorFrictionPair(100);
        Assert.Equal(VmPSpecFrictionSector.SectorFrictionFromAmount(100), f);
        Assert.Equal(VmPSpecFrictionSector.ClampMoveFactor(VmPSpecFrictionSector.FrictionToMoveFactor(f)), m);
    }
}
