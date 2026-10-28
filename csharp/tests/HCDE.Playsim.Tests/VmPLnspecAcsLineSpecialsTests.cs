namespace HCDE.Playsim.Tests;

public class VmPLnspecAcsLineSpecialsTests
{
    [Fact]
    public void AcsExecute_UsesCurrentMapWhenZero()
    {
        var invoke = VmPLnspecAcsLineSpecials.AcsExecute(1, 0, "MAP01", 2, 3, 4, false, _ => null);
        Assert.Equal("MAP01", invoke.MapName);
        Assert.Equal(3, invoke.ArgCount);
    }

    [Fact]
    public void LockedExecute_BlocksWithoutKey()
    {
        Assert.True(VmPLnspecAcsLineSpecials.LockedExecuteRequiresKey(5, false));
    }

    [Fact]
    public void Suspend_UsesCurrentMapWhenZero()
    {
        var ctl = VmPLnspecAcsLineSpecials.ResolveSuspend(7, 0, "MAP02", _ => null);
        Assert.True(ctl.UseCurrentMap);
        Assert.Equal("MAP02", ctl.MapName);
    }

    [Fact]
    public void FsExecute_RejectsBacksideWhenFirstSideOnly()
    {
        Assert.True(VmPLnspecAcsLineSpecials.FsExecuteRejectBackside(true, true, true));
    }

    [Fact]
    public void LowerRaise_RunsBoomFloorWhenCeilingFails()
    {
        Assert.True(VmPLnspecAcsLineSpecials.ShouldRunBrokenBoomFloor(1998, false));
        Assert.False(VmPLnspecAcsLineSpecials.ShouldRunBrokenBoomFloor(1998, true));
    }
}
