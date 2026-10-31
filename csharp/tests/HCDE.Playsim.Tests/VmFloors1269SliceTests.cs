namespace HCDE.Playsim.Tests;

public class VmFloors1269SliceTests
{
    const string ClipLog =
"""
enter 1
floor new 3
clip 4 3
floor reuse 3
free 2
leave 0
free clip 4
free 1
cleanup

""";

    [Fact]
    public void Heights_InsertInOrderAndASpareLeaveDropsTheList()
    {
        var vm = new VmFloors1269();
        vm.AddHeight(100, 0, 100);
        vm.AddHeight(0, 0, 100);
        Assert.False(vm.FakeActive);
        Assert.Equal(-1, vm.HeightMax);
        Assert.Equal("", vm.Chain());

        vm.AddHeight(10, 0, 100);
        vm.AddHeight(30, 0, 100);
        vm.AddHeight(20, 0, 100);
        vm.FakeActive = false;
        vm.AddHeight(10, 0, 100);
        Assert.True(vm.FakeActive);
        Assert.Equal(2, vm.HeightMax);
        Assert.Equal("10,20,30", vm.Chain());
        vm.AddHeight(5, 0, 100);
        Assert.Equal("5,10,20,30", vm.Chain());
        Assert.Equal(3, vm.HeightMax);
        var back = "";
        for (var node = vm.Tail; node != null; node = node.Prev)
            back += (back.Length == 0 ? "" : ",") + node.Height.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal("30,20,10,5", back);

        vm.EnterSkybox();
        Assert.Equal("", vm.Chain());
        Assert.Equal(-1, vm.HeightMax);
        vm.AddHeight(40, 0, 100);
        vm.LeaveSkybox();
        Assert.Equal(0, vm.CurrentSkybox);
        Assert.Equal("5,10,20,30", vm.Chain());
        Assert.Equal(3, vm.HeightMax);
        var leaked = vm.Top;
        vm.LeaveSkybox();
        Assert.Equal(-1, vm.CurrentSkybox);
        Assert.Null(vm.Top);
        Assert.Equal(-1, vm.HeightMax);
        Assert.False(leaked!.Freed);
        Assert.DoesNotContain(vm.Log, line => line.StartsWith("free"));
    }

    [Fact]
    public void Clips_CopyTheBuffersAndCleanupFreesBothLists()
    {
        Assert.Throws<NullReferenceException>(() => new VmFloors1269().NewClip(new short[] { 1 }, new short[] { 2 }));
        var vm = new VmFloors1269();
        vm.AddHeight(10, 0, 100);
        vm.EnterSkybox();
        vm.AddHeight(40, 0, 100);
        vm.SetFakeFloor(7);
        var floor = new short[] { 1, 2 };
        var ceiling = new short[] { 3, 4 };
        vm.NewClip(floor, ceiling);
        floor[0] = 9;
        Assert.Equal((short)1, vm.FakeFloor!.Floor![0]);
        Assert.Equal((short)3, vm.FakeFloor.Ceiling![0]);
        vm.SetFakeFloor(7);
        vm.Cleanup();
        Assert.False(vm.FakeActive);
        Assert.Equal(0, vm.CurrentSkybox);
        Assert.Null(vm.Top);
        Assert.Null(vm.FakeFloor!.Floor);
        Assert.Equal(ClipLog, string.Join("\n", vm.Log) + "\n");
    }
}
