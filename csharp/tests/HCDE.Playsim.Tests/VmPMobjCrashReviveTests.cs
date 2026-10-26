namespace HCDE.Playsim.Tests;

public class VmPMobjCrashReviveTests
{
    [Fact]
    public void Crash_SkipsDontCorpse() =>
        Assert.True(VmPMobjCrashRevive.SkipCrashWhenDontCorpse(VmPMobjCrashRevive.Mf6Dontcorpse));

    [Fact]
    public void Crash_EligibleCorpseNotCrashed() =>
        Assert.True(VmPMobjCrashRevive.CrashEligible(
            VmPMobjCrashRevive.MfCorpse, 0, 0, 0));

    [Fact]
    public void Crash_PicksExtremeTyped() =>
        Assert.True(VmPMobjCrashRevive.PickExtremeTypedCrash(
            new VmPMobjCrashRevive.CrashStatePick(1, 5, 10, true, false, false, false)));

    [Fact]
    public void SpawnHealth_FriendlySkillFloor() =>
        Assert.Equal(1, VmPMobjCrashRevive.SpawnHealth(
            new VmPMobjCrashRevive.SpawnHealthContext(0, 100, VmPMobjCrashRevive.MfFriendly,
                VmPMobjCrashRevive.Mf3Ismonster, 0.001, 1.0)));

    [Fact]
    public void Raise_BlocksPlayerPawn()
    {
        Assert.False(VmPMobjCrashRevive.TryGetRaiseState(
            VmPMobjCrashRevive.MfCorpse, false, true, true, true, out _));
    }

    [Fact]
    public void RunCrash_MarksCrashedEvenWithoutState()
    {
        var pick = new VmPMobjCrashRevive.CrashStatePick(0, 50, 10, false, false, false, true);
        Assert.True(VmPMobjCrashRevive.RunCrash(
            0, VmPMobjCrashRevive.MfCorpse, 0, 0, pick,
            out var choice, out var flags3));
        Assert.Equal(VmPMobjCrashRevive.CrashStateChoice.NormalGeneric, choice);
        Assert.NotEqual(0, flags3 & VmPMobjCrashRevive.Mf3Crashed);
    }

    [Fact]
    public void RunRevive_ClearsPoison()
    {
        var scratch = new VmPMobjCrashRevive.ReviveActorScratch { PoisonDamage = 5 };
        VmPMobjCrashRevive.RunRevive(scratch, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 40, out _);
        Assert.Equal(0, scratch.PoisonDamage);
        Assert.Equal(40, scratch.Health);
    }

    [Fact]
    public void Revive_FlagUnlink() =>
        Assert.True(VmPMobjCrashRevive.ReviveUnlinkNeeded(0, VmPMobjCrashRevive.MfNoblockmap));

    [Fact]
    public void GibHealth_NegatesSpawnHealth() =>
        Assert.Equal(-50, VmPMobjCrashRevive.GibHealthDefault(50));

    [Fact]
    public void Sentient_RequiresSeeState() =>
        Assert.True(VmPMobjCrashRevive.IsSentient(10, true));
}
