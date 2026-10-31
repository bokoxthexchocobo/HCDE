namespace HCDE.Playsim.Tests;

public class VmShadowMap1265SliceTests
{
    const string ClampGolden =
"""
target 0.25
target 10.00
target 10.00
min 0
min 1024
min 1024
step 1
step 256
step 256
max 0
max 1024
quality 128
quality 512
quality 8192
quality 128
reset rows 1 cap 12/40 adaptive 1
skip
open
lit 1.000000 2.000000 3.000000 4.000000 5.000000 6.000000 1.000000
shadow 1.000000 2.000000 3.000000 4.000000 5.000000 6.000000 0.500000
open
upload=1.20 ms  lights=9  eligible=0  shadowmapped=0  dropped=0  rows=1  cap=12/40  adaptive=on  priority=off

""";

    const string UploadGolden =
"""
reset rows 1 cap 12/40 adaptive 1
clock
buf 4 1 0 1
data nodes 1 24 static
buf 5 1 0 2
data lines 2 12 static
resize 4096
buf 6 1 0 3
data list 3 16384 stream
reset rows 1 cap 12/40 adaptive 1
clock
tree idle
data list 3 16384 stream
reset rows 1 cap 12/40 adaptive 1
clock
sub nodes 1 4 8
sub lines 2 2 6
data list 3 16384 stream
upload=1.20 ms  lights=3  eligible=2  shadowmapped=1  dropped=4  rows=5  cap=12/40  adaptive=on  priority=on
drop list 3
drop nodes 1
drop lines 2
dtor
dtor
reset rows 1 cap 0/0 adaptive 0
clock
fault

""";

    [Fact]
    public void Clamps_SkipTheUploadAndTreatAMissAsLit()
    {
        var vm = new VmShadowMap1265();
        vm.SetTarget(0.1f);
        vm.SetTarget(10f);
        vm.SetTarget(12f);
        vm.SetMinLights(-4);
        vm.SetMinLights(1024);
        vm.SetMinLights(2000);
        vm.SetStep(0);
        vm.SetStep(256);
        vm.SetStep(300);
        vm.SetMaxLights(-1);
        vm.SetMaxLights(1025);
        vm.SetQuality(100);
        vm.SetQuality(512);
        vm.SetQuality(8192);
        vm.SetQuality(4095);
        Assert.Equal(10f, vm.TargetMs);
        Assert.Equal(128, vm.Quality);
        Assert.Equal(256, vm.Step);
        vm.BudgetHardCap = 40;
        vm.BudgetRuntimeCap = 12;
        vm.BudgetAdaptiveEnabled = 1;
        Assert.False(vm.PerformUpdate());
        Assert.Equal(40, vm.BudgetHardCap);
        Assert.Equal(1, vm.LightRowsUpdated);
        Assert.Equal(0, vm.LightPriorityEnabled);
        vm.Enabled = true;
        Assert.True(vm.ShadowTest(1, 2, 3, 4, 5, 6));
        vm.HasTree = true;
        vm.Ray = 1f;
        Assert.True(vm.ShadowTest(1, 2, 3, 4, 5, 6));
        vm.Ray = 0.5f;
        Assert.False(vm.ShadowTest(1, 2, 3, 4, 5, 6));
        vm.Enabled = false;
        Assert.True(vm.ShadowTest(1, 2, 3, 4, 5, 6));
        vm.UpdateMs = 1.2;
        vm.LightsProcessed = 9;
        vm.Stat();
        Assert.Equal(ClampGolden, Dump(vm));
    }

    [Fact]
    public void Upload_ReusesTheLightBufferAndFaultsAMissingTree()
    {
        var vm = new VmShadowMap1265();
        vm.BudgetHardCap = 40;
        vm.BudgetRuntimeCap = 12;
        vm.BudgetAdaptiveEnabled = 1;
        vm.Collect = lights => lights[0] = 7f;
        vm.NewTree = true;
        vm.HasTree = true;
        vm.NodesBytes = 24;
        vm.LinesBytes = 12;
        Assert.True(vm.PerformUpdate());
        Assert.Equal(0, vm.LightsProcessed);
        Assert.Equal(7f, vm.Lights[0]);
        Assert.Equal(4096, vm.Lights.Length);
        Assert.Equal(3, vm.LightList!.Id);
        vm.NewTree = false;
        vm.TreeUpdated = false;
        vm.Collect = lights => lights[1] = 8f;
        Assert.True(vm.PerformUpdate());
        Assert.Equal(7f, vm.Lights[0]);
        Assert.Equal(8f, vm.Lights[1]);
        Assert.Equal(3, vm.LightList.Id);
        vm.TreeUpdated = true;
        vm.DynNodesOffset = 4;
        vm.DynNodesBytes = 8;
        vm.DynLinesOffset = 2;
        vm.DynLinesBytes = 6;
        Assert.True(vm.PerformUpdate());
        vm.UpdateMs = 1.2;
        vm.LightsProcessed = 3;
        vm.LightsEligible = 2;
        vm.LightsShadowmapped = 1;
        vm.LightsBudgetedOut = 4;
        vm.LightRowsUpdated = 5;
        vm.LightPriorityEnabled = 1;
        vm.Stat();
        vm.Destroy();
        vm.Destroy();
        Assert.Null(vm.Nodes);

        var bare = new VmShadowMap1265();
        bare.HasTree = true;
        bare.NewTree = false;
        bare.TreeUpdated = true;
        bare.Collect = _ => { };
        Assert.Throws<NullReferenceException>(() => bare.PerformUpdate());
        bare.Log.Add("fault");
        Assert.Equal(UploadGolden, string.Join('\n', vm.Log.Concat(bare.Log)) + "\n");
    }

    static string Dump(VmShadowMap1265 vm) => string.Join('\n', vm.Log) + "\n";
}
