namespace HCDE.Playsim.Tests;

public class VmClip1268SliceTests
{
    const string DrawLog =
"""
draw 20 40 1
draw 40 80 1
draw 90 110 0
draw 200 300 1

""";

    const string CrunchLog =
"""
draw 20 40 1
draw 60 80 1
draw 40 60 1
draw 80 100 1
draw 20 300 1
draw 10 15 1

""";

    [Fact]
    public void Openings_InsertExtendAndSlideTheRightSentinel()
    {
        var vm = new VmClip1268();
        vm.Clear(10, 300);
        Assert.Equal("clear end=2 [-32767 10] [300 32767]", vm.Dump("clear"));
        Assert.True(vm.Check(11, 20));
        Assert.False(vm.Check(0, 5));
        Assert.False(vm.Check(301, 400));
        Assert.False(vm.Check(10, 10));
        Assert.False(vm.IsVisible(10, 10));
        Assert.True(vm.IsVisible(11, 20));
        Assert.False(vm.IsVisible(300, 310));
        Assert.False(vm.Clip(1, 5, true, (_, _) => true));
        Assert.True(vm.Clip(20, 40, true, (_, _) => true));
        Assert.Equal("insert end=3 [-32767 10] [20 40] [300 32767]", vm.Dump("insert"));
        Assert.True(vm.Clip(40, 80, true, (_, _) => true));
        Assert.Equal("extend end=3 [-32767 10] [20 80] [300 32767]", vm.Dump("extend"));
        Assert.False(vm.Clip(80, 80, true, (_, _) => true));
        Assert.Equal("touch end=3 [-32767 10] [20 80] [300 32767]", vm.Dump("touch"));
        Assert.True(vm.Clip(90, 110, true, (_, _) => false));
        Assert.Equal("reject end=3 [-32767 10] [20 80] [300 32767]", vm.Dump("reject"));
        Assert.True(vm.Clip(200, 300, true, (_, _) => true));
        Assert.Equal("merge-right end=3 [-32767 10] [20 80] [200 32767]", vm.Dump("merge-right"));
        Assert.Throws<NullReferenceException>(() => vm.Clip(150, 180, true, null));
        Assert.Equal("merge-right end=3 [-32767 10] [20 80] [200 32767]", vm.Dump("merge-right"));
        Assert.Equal(DrawLog, string.Join("\n", vm.Log) + "\n");
    }

    [Fact]
    public void Crunch_KeepsTheSlotPastTheCursorAndAPassWallDoesNotClose()
    {
        var vm = new VmClip1268();
        vm.Clear(10, 300);
        vm.Clip(20, 40, true, (_, _) => true);
        vm.Clip(60, 80, true, (_, _) => true);
        Assert.Equal("two end=4 [-32767 10] [20 40] [60 80] [300 32767]", vm.Dump("two"));
        Assert.True(vm.Clip(30, 100, true, (_, _) => true));
        Assert.Equal("crunch end=4 [-32767 10] [20 100] [300 32767] [0 0]", vm.Dump("crunch"));
        vm.Clear(10, 300);
        Assert.True(vm.Clip(20, 350, true, (_, _) => true));
        Assert.Equal("above end=2 [-32767 10] [20 32767]", vm.Dump("above"));
        Assert.True(vm.Clip(0, 15, false, (_, _) => true));
        Assert.Equal("pass end=2 [-32767 10] [20 32767]", vm.Dump("pass"));
        Assert.Equal(CrunchLog, string.Join("\n", vm.Log) + "\n");
    }
}
