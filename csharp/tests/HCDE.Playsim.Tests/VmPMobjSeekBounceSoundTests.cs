namespace HCDE.Playsim.Tests;

public class VmPMobjSeekBounceSoundTests
{
    [Fact]
    public void PlayBounceSound_SkipsQuiet()
    {
        var called = false;
        Assert.False(VmPMobjSeekBounceSound.RunPlayBounceSound(true, VmPMobjSeekBounceSound.BounceQuiet, true, _ => called = true, _ => { }, _ => { }, 1));
        Assert.False(called);
    }

    [Fact]
    public void PlayBounceSound_UsesSeeSound()
    {
        var see = false;
        VmPMobjSeekBounceSound.RunPlayBounceSound(true, VmPMobjSeekBounceSound.BounceUseSeeSound, false, _ => see = true, _ => { }, _ => { }, 1);
        Assert.True(see);
    }

    [Fact]
    public void FaceMobjDir_ClockwiseWhenPositive()
    {
        var (dir, delta) = VmPMobjSeekBounceSound.FaceMobjDir(0, 10);
        Assert.Equal(1, dir);
        Assert.Equal(10, delta);
    }

    [Fact]
    public void CanSeek_BlocksCantSeek() =>
        Assert.False(VmPMobjSeekBounceSound.CanSeek(true, false, false, false, false, true));

    [Fact]
    public void SeekerMissile_TracksWhenEligible()
    {
        double yaw = 0;
        var st = new VmPMobjSeekBounceSound.SeekerMissileState(new object(), true, 1, 5, 0, 5, 1, 2, false, 0, 0, 0, 0);
        Assert.True(VmPMobjSeekBounceSound.SeekerMissile(ref st, ref yaw, null));
    }

    [Fact]
    public void SeekerMissile_ClearsTracerWhenUnshootable()
    {
        double yaw = 0;
        var cleared = false;
        var st = new VmPMobjSeekBounceSound.SeekerMissileState(new object(), true, 0, 5, 0, 5, 1, 2, false, 0, 0, 0, 0);
        Assert.False(VmPMobjSeekBounceSound.SeekerMissile(ref st, ref yaw, () => cleared = true));
        Assert.True(cleared);
    }
}
