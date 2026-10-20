namespace HCDE.Gamedata.Tests;

public class VmMapInfoFlagApplyTests
{
    [Fact]
    public void ApplySetFlag_TogglesWithAssign()
    {
        var level = new VmMapInfoLevelFlags { Flags = 0xff };
        VmMapInfoFlagApply.ApplySetFlag(level, 0x10, hasAssign: true, number: 0, orMask: 0);
        Assert.Equal(0xefu, level.Flags);
    }

    [Fact]
    public void ApplyScFlags2_MergesTeamplayOn()
    {
        var flags2 = 0u;
        VmMapInfoFlagApply.ApplyScFlags2(ref flags2, 0x100, 0xfffffeff);
        Assert.Equal(0x100u, flags2);
    }

    [Fact]
    public void ApplySetFlag2_TogglesWithAssign()
    {
        var level = new VmMapInfoLevelFlags();
        VmMapInfoFlagApply.ApplySetFlag2(level, 0x4, hasAssign: true, number: 1, orMask: 0);
        Assert.Equal(0x4u, level.Flags2);
    }

    [Fact]
    public void ApplyClearCompat_SetsMask()
    {
        var level = new VmMapInfoLevelFlags { CompatFlags = 0xff, CompatFlags2 = 0xff };
        VmMapInfoFlagApply.ApplyClearCompat(level, 0x10, 0x01);
        Assert.Equal(0xefu, level.CompatFlags);
        Assert.Equal(0x10u, level.CompatMask);
    }
}

