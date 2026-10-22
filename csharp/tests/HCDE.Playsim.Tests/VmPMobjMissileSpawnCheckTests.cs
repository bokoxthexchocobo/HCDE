namespace HCDE.Playsim.Tests;

public class VmPMobjMissileSpawnCheckTests
{
    [Fact]
    public void GetTics_FastHalves()
    {
        Assert.Equal(5, VmPMobjMissileSpawnCheck.GetTics(false, 10, 0, true, true, false, false));
    }

    [Fact]
    public void RandomizeMissileTics_ClampedToOne()
    {
        Assert.Equal(1, VmPMobjMissileSpawnCheck.RandomizeMissileTics(2, 3));
    }

    [Fact]
    public void AdvanceSpawnPosition_HalvesUntilInsideRadius()
    {
        var adv = VmPMobjMissileSpawnCheck.AdvanceSpawnPosition(10, 0, 0, 5);
        Assert.True(adv.X * adv.X < 25);
    }

    [Fact]
    public void ShouldDestroyOnHorizon_WhenBlockedByHorizonLine()
    {
        var ctx = new VmPMobjMissileSpawnCheck.MissileSpawnBlockContext(
            true, false, 0, VmPMobjMissileSpawnCheck.LineHorizon, false, true);
        Assert.True(VmPMobjMissileSpawnCheck.ShouldDestroyOnHorizon(ctx));
    }

    [Fact]
    public void PlaySpawnSoundAtPosition_WhenNoSpawnerAndNotSilent()
    {
        Assert.True(VmPMobjMissileSpawnCheck.PlaySpawnSoundAtPosition(true, true, false));
    }
}
