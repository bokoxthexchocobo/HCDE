namespace HCDE.Playsim.Tests;

public class VmFlatVertices1258SliceTests
{
    const string BuildGolden =
"""
v 0 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 1 1.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 2 2.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 3 3.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 4 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 5 0.000000 200.000000 0.000000 0.000000 1.000000 0.000000 0.000000 -1.000000
v 6 320.000000 0.000000 0.000000 1.000000 0.000000 0.000000 0.000000 -1.000000
v 7 320.000000 200.000000 0.000000 1.000000 1.000000 0.000000 0.000000 -1.000000
v 8 -1.000000 -1.000000 0.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 9 3.000000 -1.000000 0.000000 2.000000 0.000000 0.000000 0.000000 -1.000000
v 10 -1.000000 3.000000 0.000000 0.000000 2.000000 0.000000 0.000000 -1.000000
v 11 3.000000 3.000000 0.000000 2.000000 2.000000 0.000000 0.000000 -1.000000
v 12 -32767.000000 32767.000000 -32767.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 13 -32767.000000 32767.000000 32767.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 14 32767.000000 32767.000000 32767.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 15 32767.000000 32767.000000 -32767.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 16 -32767.000000 -32767.000000 -32767.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 17 -32767.000000 -32767.000000 32767.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 18 32767.000000 -32767.000000 32767.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
v 19 32767.000000 -32767.000000 -32767.000000 0.000000 0.000000 0.000000 0.000000 -1.000000
index 4 0
vbo 0 64000000 2
fmt 0 1 3 32 0 0 1 12 1 2 20 6 1
vbo 1 64000000 2
fmt 1 1 3 32 0 0 1 12 1 2 20 6 1
cursor 20 20
copy 0 0 20
map 20
unmap 640 0
upload 0 640
copy 1 0 20
map 20
unmap 640 0
upload 0 640
bind 0

""";

    const string ResizeGolden =
"""
copy 0 4 4
map 20
unmap 640 0
upload 128 128
copy 1 4 4
map 20
unmap 640 0
upload 128 128
bind 0
alloc 20 3
alloc 23 1
fatal 1999500

""";

    [Fact]
    public void Build_FillsTheReservedQuadsAndUploadsBothPipelines()
    {
        var vm = new VmFlatVertices1258(320, 200, 2);
        Assert.Equal(20u, vm.Index);
        Assert.Equal(20u, vm.CurIndex);
        Assert.Equal(20u, vm.NumReserved);
        Assert.Equal(4, vm.IndexBytes);
        Assert.Equal(0, vm.IndexUsage);
        Assert.Same(vm.Pipes[0], vm.Current);
        Assert.Equal(200f, vm.Pipes[0]!.Get(5).Z);
        Assert.Equal(1f, vm.Pipes[0]!.Get(7).U);
        Assert.Equal(1f, vm.Pipes[0]!.Get(7).V);
        Assert.Equal(32767f, vm.Pipes[1]!.Get(19).X);
        Assert.Equal(-32767f, vm.Pipes[1]!.Get(19).Y);
        Assert.Equal(-1f, vm.Shadow[11].LIndex);
        Assert.Equal(0f, vm.Shadow[11].Lu);
        Assert.Equal(BuildGolden, string.Join('\n', vm.Log) + "\n");
        vm.Destroy();
        Assert.Null(vm.Current);
        Assert.False(vm.IndexAlive);
        Assert.Null(vm.Pipes[0]);
        Assert.Null(vm.Pipes[1]);
        Assert.Equal("drop 0", vm.Log[^3]);
        Assert.Equal("drop 1", vm.Log[^2]);
        Assert.Equal("drop index", vm.Log[^1]);
    }

    [Fact]
    public void Resize_UploadsTheLeadingQuadAndAllocStopsAtTheCap()
    {
        var vm = new VmFlatVertices1258(320, 200, 2);
        vm.Log.Clear();
        vm.Shadow[5].Lu = 9f;
        vm.OutputResized(640, 480);
        Assert.Equal(9f, vm.Shadow[5].Lu);
        Assert.Equal(480f, vm.Shadow[5].Z);
        Assert.Equal(1f, vm.Shadow[5].V);
        Assert.Equal(640f, vm.Shadow[6].X);
        Assert.Equal(1f, vm.Shadow[6].U);
        Assert.Equal(640f, vm.Shadow[7].X);
        Assert.Equal(480f, vm.Shadow[7].Z);
        Assert.Equal(0f, vm.Pipes[0]!.Get(4).X);
        Assert.Equal(1f, vm.Pipes[0]!.Get(5).X);
        Assert.Equal(0f, vm.Pipes[0]!.Get(5).Z);
        Assert.Equal(1f, vm.Pipes[1]!.Get(5).X);
        var first = vm.Alloc(3);
        var second = vm.Alloc(1);
        Assert.Equal(20u, first.Index);
        Assert.Equal(23u, second.Index);
        Assert.Equal(24u, vm.CurIndex);
        vm.CurIndex = 1999499;
        var stopped = vm.Alloc(1);
        Assert.True(vm.Fatal);
        Assert.Null(stopped.Slot);
        Assert.Equal(1999499u, stopped.Index);
        Assert.Equal(1999500u, vm.CurIndex);
        Assert.Equal(ResizeGolden, string.Join('\n', vm.Log) + "\n");
    }
}
