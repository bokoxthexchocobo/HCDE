namespace HCDE.Playsim.Tests;

public class VmPLnspecBlockingAutomapCamera931SliceTests
{
    [Fact]
    public void Blocking_RejectsZeroLineId()
    {
        Assert.True(VmPLnspecBlockingAutomapCamera931Slice.BlockingRejectZeroId(0));
    }

    [Fact]
    public void Blocking_DecodesSetAndClearWords()
    {
        var (set, clear) = VmPLnspecBlockingAutomapCamera931Slice.DecodeBlockingArgs(3, 1);
        Assert.Equal(3, set[0]);
        Assert.Equal(1, clear[0]);
    }

    [Fact]
    public void AutomapFlags_RejectsZeroId()
    {
        Assert.True(VmPLnspecBlockingAutomapCamera931Slice.AutomapFlagsRejectZeroId(0));
    }

    [Fact]
    public void AutomapStyle_RejectsOutOfRange()
    {
        Assert.False(VmPLnspecBlockingAutomapCamera931Slice.AutomapStyleInRange(99, 5));
    }

    [Fact]
    public void ChangeCamera_SetsRevertCheatWhenArg2()
    {
        var cheats = VmPLnspecBlockingAutomapCamera931Slice.UpdateCheatsForCamera(
            0,
            new VmPLnspecBlockingAutomapCamera931Slice.ChangeCameraState(new object(), null, 0, 1, true),
            true);
        Assert.Equal(VmPLnspecBlockingAutomapCamera931Slice.CfRevertPlease, cheats);
    }

    [Fact]
    public void ChangeCamera_IteratesAllPlayersWhenWhoSet()
    {
        var state = VmPLnspecBlockingAutomapCamera931Slice.BeginChangeCamera(1, new object(), new object(), 1, 0, true);
        Assert.True(VmPLnspecBlockingAutomapCamera931Slice.ShouldIterateAllPlayers(state));
    }

    [Fact]
    public void ApplyBlocking_MergesFlags()
    {
        var (f, f2) = VmPLnspecBlockingAutomapCamera931Slice.ApplyBlocking(0, 0, [2], [1]);
        Assert.Equal(2, f);
        Assert.Equal(0, f2);
    }
}
