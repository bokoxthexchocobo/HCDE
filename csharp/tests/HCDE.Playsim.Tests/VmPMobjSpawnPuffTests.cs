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

    [Fact]
    public void Puff_CrashPriorityPrefersSky() =>
        Assert.True(VmPMobjSpawnPuff.PickSkyCrash(
            new VmPMobjSpawnPuff.CrashStatePickContext(
                VmPMobjSpawnPuff.PfHitsky, true, true, true, true)));

    [Fact]
    public void Puff_SkipEffectsWhenTemporary() =>
        Assert.True(VmPMobjSpawnPuff.SkipEffectsWhenTemporary(VmPMobjSpawnPuff.PfTemporary));

    [Fact]
    public void BuildPuffPostSpawnOutcome_PrefersSkyCrash()
    {
        var plan = VmPMobjSpawnPuff.BuildPuffPostSpawnOutcome(
            VmPMobjSpawnPuff.PfHitsky,
            0,
            0,
            0,
            45,
            4,
            0,
            false,
            true,
            true,
            true,
            true,
            new VmPMobjSpawnPuff.PuffParticleContext(0, 1, 2, VmPMobjSpawnPuff.Mf4Allowparticles),
            true,
            true,
            false);
        Assert.Equal(VmPMobjSpawnPuff.PuffCrashSelection.SkyDeath, plan.Crash);
        Assert.Equal(225, plan.YawDegrees);
    }

    [Fact]
    public void SelectPuffCrashState_WallWhenMiss()
    {
        var ctx = new VmPMobjSpawnPuff.CrashStatePickContext(0, false, true, false, false);
        Assert.Equal(VmPMobjSpawnPuff.PuffCrashSelection.WallCrash, VmPMobjSpawnPuff.SelectPuffCrashState(ctx));
    }
}
