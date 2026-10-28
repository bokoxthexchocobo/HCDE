namespace HCDE.Playsim.Tests;

public class VmActionRaiseAndCountFlagsTests
{
    [Fact]
    public void ApplyKillCount_DecrementsWhenCountedAlive()
    {
        var (total, _) = VmActionRaiseAndCountFlags.ApplyKillCount(5, true, -1, 0, true);
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

    [Fact]
    public void TrySetViewAngle_InvokesSetterWhenRefPresent()
    {
        var actor = new VmActionRaiseAndCountFlags.AActorRaiseRef(1, 0, 10, 0, 0, false);
        double? set = null;
        Assert.True(VmActionRaiseAndCountFlags.TrySetViewAngle(actor, 90, 0, (a, deg, _) => set = deg));
        Assert.Equal(90, set);
    }

    [Fact]
    public void RunChangeCountFlags_UpdatesLevelTotals()
    {
        var actor = new VmActionRaiseAndCountFlags.AActorRaiseRef(
            1, 0, 10, 0, VmActionRaiseAndCountFlags.Mf5Countsecret, false);
        var totals = new VmActionRaiseAndCountFlags.LevelCountTotals(5, 2, 3);
        var (outTotals, outActor) = VmActionRaiseAndCountFlags.RunChangeCountFlags(totals, actor, -1, -1, 0);
        Assert.Equal(2, outTotals.Secrets);
        Assert.Equal(0, outActor.Flags5 & VmActionRaiseAndCountFlags.Mf5Countsecret);
    }

    [Fact]
    public void DecideMonsterRefire_JumpsWhenSightBlocked()
    {
        var outcome = VmActionRaiseAndCountFlags.DecideMonsterRefire(100, 0, false, false, 50, false);
        Assert.Equal(VmActionRaiseAndCountFlags.MonsterRefireOutcome.Jump, outcome);
    }

    [Fact]
    public void RaiseChildrenOnLevel_RaisesMatchingMaster()
    {
        var parent = new VmActionRaiseAndCountFlags.AActorRaiseRef(9, 0, 10, 0, 0, false);
        var child = new VmActionRaiseAndCountFlags.AActorRaiseRef(2, 9, 0, 0, 0, false);
        var raised = VmActionRaiseAndCountFlags.RaiseChildrenOnLevel(
            parent,
            [child],
            0,
            (_, _, _) => true);
        Assert.Equal(1, raised);
    }
}
