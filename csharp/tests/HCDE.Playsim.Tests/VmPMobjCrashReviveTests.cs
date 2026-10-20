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
    public void Raise_BlocksPlayerPawn() =>
        Assert.True(VmPMobjCrashRevive.RaiseBlocksPlayerPawn(true));

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
