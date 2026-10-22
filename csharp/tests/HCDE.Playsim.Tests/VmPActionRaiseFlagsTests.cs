namespace HCDE.Playsim.Tests;

public class VmPActionRaiseFlagsTests
{
    [Fact]
    public void ChangeCountFlags_TogglesKillCounter()
    {
        var actor = new VmPActionRaiseFlags.CountFlagsActor { CountsAsKill = true, Health = 10 };
        var level = new VmPActionRaiseFlags.LevelCounters { Monsters = 5 };
        VmPActionRaiseFlags.ChangeCountFlags(actor, level, kill: 1, item: -1, secret: -1);
        Assert.Equal(5, level.Monsters);
    }

    [Fact]
    public void RaiseMaster_SkipsNull()
    {
        var raised = false;
        VmPActionRaiseFlags.RaiseMaster(null, (_, _, _) => raised = true, new(), 0);
        Assert.False(raised);
    }

    [Fact]
    public void MonsterRefire_JumpsWhenTargetDead()
    {
        Assert.True(VmPActionRaiseFlags.MonsterRefire(0, 0, new(), 0, null, null, null));
    }
}
