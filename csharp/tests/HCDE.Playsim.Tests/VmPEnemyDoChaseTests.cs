namespace HCDE.Playsim.Tests;

public class VmPEnemyDoChaseTests
{
    [Fact]
    public void DoChase_SkipsWhenInConversation()
    {
        var s = new VmPEnemyDoChase.DoChaseState { Flags5 = VmPEnemyDoChase.Mf5Inconversation };
        VmPEnemyDoChase.DoChase(s);
        Assert.Equal(0, s.Flags7 & VmPEnemyDoChase.Mf7Inchase);
    }

    [Fact]
    public void DoChase_ClearsNotargetPlayer()
    {
        var s = new VmPEnemyDoChase.DoChaseState
        {
            TargetHasPlayer = true,
            TargetPlayerCheats = VmPEnemyDoChase.CfNotarget,
            TargetFlags = VmPEnemyDoChase.MfShootable,
        };
        VmPEnemyDoChase.DoChase(s);
        Assert.True(s.TargetNull);
        Assert.Equal(0, s.Threshold);
    }

    [Fact]
    public void DoChase_MeleeAttackSetsState()
    {
        var melee = false;
        var s = new VmPEnemyDoChase.DoChaseState
        {
            TargetHealth = 1,
            TargetFlags = VmPEnemyDoChase.MfShootable,
            MeleeStateSet = true,
            CheckMeleeRange = () => true,
            SetMeleeState = () => melee = true,
        };
        VmPEnemyDoChase.DoChase(s);
        Assert.True(melee);
        Assert.True(s.ExitedEarly);
    }

    [Fact]
    public void DoChase_CantLeaveFloorPicRollsBack()
    {
        var tryMove = false;
        var s = new VmPEnemyDoChase.DoChaseState
        {
            TargetHealth = 1,
            TargetFlags = VmPEnemyDoChase.MfShootable,
            Flags2 = VmPEnemyDoChase.Mf2Cantleavefloorpic,
            FloorPic = 2,
            MoveCount = 5,
            PosX = 10,
            PosY = 20,
            TryMove = (x, y) =>
            {
                tryMove = x == 10 && y == 20;
                return true;
            },
        };
        s.SmartMove = () =>
        {
            s.FloorPic = 1;
            return false;
        };
        VmPEnemyDoChase.DoChase(s);
        Assert.True(tryMove);
    }
}
