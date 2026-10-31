namespace HCDE.Playsim.Tests;

public class VmHwClock1266SliceTests
{
    const string Times =
        "BSP = 5.000, Clip=3.000\n" +
        "W: Render=2.000, Setup=1.000\n" +
        "F: Render=0.250, Setup=0.500\n" +
        "S: Render=0.125, Setup=0.250\n" +
        "2D: 1.500 Finish3D: 1.000\n" +
        "Main thread total=18.000, Main thread waiting=10.000 Worker thread total=6.000, Worker thread waiting=4.250\n" +
        "All=2.000, Render=7.000, Setup=1.000, Portal=0.500, Drawcalls=4.000, Postprocess=0.500, Finish=4.000\n";

    const string TimeLog =
"""
clock All
reset
cached
refresh
cached
quiet
active

""";

    const string BenchLog =
"""
bench on 100
hide
bench wait
write MAP Walls: 1 (0 splits, 0 t-splits, 0 vertices)\nFlats: 0 (0 primitives, 0 vertices)\nSprites: 0, Decals=0, Portals: 0, Command buffers: 0\nBSP = 0.000, Clip=0.000\nW: Render=0.000, Setup=0.000\nF: Render=0.000, Setup=0.000\nS: Render=0.000, Setup=0.000\n2D: 0.000 Finish3D: 0.000\nMain thread total=0.000, Main thread waiting=0.000 Worker thread total=0.000, Worker thread waiting=0.000\nAll=0.000, Render=0.000, Setup=0.000, Portal=0.000, Drawcalls=0.000, Postprocess=0.000, Finish=0.000\nDLight - Walls: 0 processed, 0 rendered - Flats: 0 processed, 0 rendered\n\n\n\n
saved
bench keep 8000 1
hide
write fail
saved
bench keep 0 0
hide
bench idle

""";

    [Fact]
    public void Times_CacheForOneSecondAndLeaveFinishAlone()
    {
        var vm = new VmHwClock1266();
        vm.Finish = 4;
        vm.TwoD = 1.5;
        vm.Flush3D = 2.5;
        vm.CommandBuffers = 9;
        vm.IterDLight = 3;
        vm.Active = true;
        vm.ClockSample = 2;
        vm.ResetProfiling();
        Assert.Equal(-2d, vm.All);
        Assert.Equal(4d, vm.Finish);
        Assert.Equal(9, vm.CommandBuffers);
        Assert.Equal(0, vm.RenderedPortals);
        vm.Bsp = 8;
        vm.ClipWall = 3;
        vm.SetupWall = 1;
        vm.RenderWall = 2;
        vm.SetupFlat = 0.5;
        vm.RenderFlat = 0.25;
        vm.SetupSprite = 0.25;
        vm.RenderSprite = 0.125;
        vm.MtWait = 10;
        vm.WtTotal = 6;
        vm.RenderAll = 7;
        vm.ProcessAll = 1;
        vm.PortalAll = 0.5;
        vm.DrawCalls = 4;
        vm.PostProcess = 0.5;
        vm.RenderedLines = 11;
        vm.VertexSplit = 2;
        vm.TexSplit = 3;
        vm.VertexCount = 40;
        vm.RenderedFlats = 5;
        vm.FlatPrimitives = 6;
        vm.FlatVertices = 7;
        vm.RenderedSprites = 8;
        vm.RenderedDecals = 1;
        vm.RenderedPortals = 2;
        vm.DrawDLight = 4;
        vm.IterDLightF = 5;
        vm.DrawDLightF = 6;
        vm.NowMs = 1000;
        Assert.Equal("", vm.RenderTimes());
        vm.NowMs = 2000;
        Assert.Equal(Times, vm.RenderTimes());
        vm.Bsp = 9;
        vm.NowMs = 2001;
        Assert.Equal(Times, vm.RenderTimes());
        Assert.Equal(
            "Walls: 11 (2 splits, 3 t-splits, 40 vertices)\nFlats: 5 (6 primitives, 7 vertices)\nSprites: 8, Decals=1, Portals: 2, Command buffers: 9\n",
            vm.RenderStats());
        Assert.Equal(
            "DLight - Walls: 3 processed, 4 rendered - Flats: 5 processed, 6 rendered\n",
            vm.LightStats());
        vm.StatFound = false;
        vm.StatActive = true;
        vm.CheckActive();
        Assert.False(vm.Active);
        vm.PrintStats = 1;
        vm.CheckActive();
        Assert.True(vm.Active);

        var quiet = new VmHwClock1266();
        quiet.Finish = 4;
        quiet.ClockSample = 2;
        quiet.ResetProfiling();
        Assert.Equal(0d, quiet.All);
        Assert.Equal(4d, quiet.Finish);
        Assert.DoesNotContain("clock", string.Join('\n', quiet.Log));
        Assert.Equal(TimeLog, Dump(vm));
    }

    [Fact]
    public void Bench_WaitsFiveSecondsAndPrintsWhenTheFileFails()
    {
        var bench = new VmHwClock1266();
        bench.NowMs = 100;
        bench.Bench();
        Assert.True(bench.VidFps);
        Assert.True(bench.SwitchFps);
        Assert.Equal(100, bench.WaitStart);
        bench.ConsoleUp = true;
        bench.CheckBench();
        Assert.Equal(1, bench.PrintStats);
        bench.NowMs = 5100;
        bench.Location = "MAP ";
        bench.RenderedLines = 1;
        bench.CheckBench();
        Assert.False(bench.VidFps);
        Assert.Equal(0, bench.PrintStats);
        bench.FileOpens = false;
        bench.VidFps = true;
        bench.NowMs = 8000;
        bench.Bench();
        bench.NowMs = 13000;
        bench.CheckBench();
        Assert.True(bench.VidFps);
        Assert.False(bench.SwitchFps);

        var keep = new VmHwClock1266();
        keep.VidFps = true;
        keep.ConsoleUp = false;
        keep.NowMs = 50;
        keep.Bench();
        Assert.Equal(0, keep.WaitStart);
        Assert.False(keep.SwitchFps);
        keep.CheckBench();
        Assert.Equal(BenchLog, string.Join('\n', bench.Log.Concat(keep.Log)) + "\n");
    }

    static string Dump(VmHwClock1266 vm) => string.Join('\n', vm.Log) + "\n";
}
