namespace HCDE.Playsim.Tests;

public class VmPMobjSpawnPuffTests
{
    [Fact]
    public void Index_BeginsInvasionOnZero() =>
        Assert.True(VmPMobjSpawnPuff.BeginInvasionRegistration(0));

    [Fact]
    public void Puff_RequiresType() =>
        Assert.False(VmPMobjSpawnPuff.PuffTypeValid(true));

    [Fact]
    public void Puff_SkyCrashState() =>
        Assert.True(VmPMobjSpawnPuff.UseSkyCrashState(VmPMobjSpawnPuff.PfHitsky, true));

    [Fact]
    public void Puff_MeleeStateWhenClose() =>
        Assert.True(VmPMobjSpawnPuff.UseMeleeState(VmPMobjSpawnPuff.PfMeleerange, true));

    [Fact]
    public void Puff_DrawSplashWhenAllowed() =>
        Assert.True(VmPMobjSpawnPuff.DrawSplashParticles(
            new VmPMobjSpawnPuff.PuffParticleContext(0, 1, 2, VmPMobjSpawnPuff.Mf4Allowparticles)));

    [Fact]
    public void Puff_PlaySeeSoundOnHit() =>
        Assert.True(VmPMobjSpawnPuff.PlaySeeSoundOnHit(VmPMobjSpawnPuff.PfHitthing, true));
}
