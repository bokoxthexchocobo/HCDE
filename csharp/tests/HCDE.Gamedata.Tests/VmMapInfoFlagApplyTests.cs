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
}
