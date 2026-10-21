namespace HCDE.Playsim.Tests;

public class VmActionRaiseAndCountFlagsTests
{
    [Fact]
    public void ApplyKillCount_DecrementsWhenCountedAlive()
    {
        var (total, _) = VmActionRaiseAndCountFlags.ApplyKillCount(5, true, -1, VmActionRaiseAndCountFlags.MfCountkill, true);
        Assert.Equal(4, total);
    }

    [Fact]
    public void PlanChangeCountFlags_AdjustsSecretTotal()
    {
        var plan = VmActionRaiseAndCountFlags.PlanChangeCountFlags(1, 2, 3, 0, VmActionRaiseAndCountFlags.Mf5Countsecret, false, true, -1, -1, 0);
        Assert.Equal(2, plan.Secrets);
    }

    [Fact]
    public void SiblingSharesMaster_ExcludesSelf()
    {
        var self = new VmActionRaiseAndCountFlags.AActorRaiseRef(1, 9, 10, 0, 0, false);
        var sib = new VmActionRaiseAndCountFlags.AActorRaiseRef(2, 9, 10, 0, 0, false);
        var master = new VmActionRaiseAndCountFlags.AActorRaiseRef(9, 0, 10, 0, 0, false);
        Assert.True(VmActionRaiseAndCountFlags.SiblingSharesMaster(self, sib, master));
    }

    [Fact]
    public void RefireTargetInvalid_WhenNoSight()
    {
        Assert.True(VmActionRaiseAndCountFlags.RefireTargetInvalid(false, false, 50, false));
    }

    [Fact]
    public void RefireProbabilityFails_WhenRollLow()
    {
        Assert.True(VmActionRaiseAndCountFlags.RefireProbabilityFails(10, 20));
    }

    [Fact]
    public void CheckFlagResult_RequiresOwner()
    {
        Assert.False(VmActionRaiseAndCountFlags.CheckFlagResult(true, true));
    }
}
