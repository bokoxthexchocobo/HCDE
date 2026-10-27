namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode913SliceTests
{
    [Fact]
    public void SubScriptVar_DecrementsLocal()
    {
        var locals = new int[] { 0, 100 };
        var stack = new int[] { 7 };
        var sp = 1;
        VmPAcsVmOpcode913Slice.SubScriptVar(locals, 1, stack, ref sp);
        Assert.Equal(0, sp);
        Assert.Equal(93, locals[1]);
    }

    [Fact]
    public void SubGlobalArray_SubtractsIndexedElement()
    {
        var arrays = new[] { new int[4], new int[4] };
        arrays[1][2] = 50;
        var stack = new int[] { 2, 8 };
        var sp = 2;
        VmPAcsVmOpcode913Slice.SubGlobalArray(arrays, 1, stack, ref sp);
        Assert.Equal(42, arrays[1][2]);
    }

    [Fact]
    public void MulWorldVar_MultipliesSlot()
    {
        var world = new int[] { 6 };
        var stack = new int[] { 7 };
        var sp = 1;
        VmPAcsVmOpcode913Slice.MulWorldVar(world, 0, stack, ref sp);
        Assert.Equal(42, world[0]);
    }

    [Fact]
    public void MulMapArray_UsesMapVarAsArrayId()
    {
        var mapCells = new int[] { 0, 4 };
        var mapArrays = new VmPAcsVmOpcode913Slice.AcsMapArrayStore();
        mapArrays.Set(4, 1, 5);
        var stack = new int[] { 1, 6 };
        var sp = 2;
        VmPAcsVmOpcode913Slice.MulMapArray(mapArrays, 1, mapCells, stack, ref sp);
        Assert.Equal(30, mapArrays.Get(4, 1));
    }

    [Fact]
    public void DivGlobalVar_ReturnsDivideByZero()
    {
        var globals = new int[] { 99 };
        var stack = new int[] { 0 };
        var sp = 1;
        Assert.Equal(
            VmPAcsVmOpcode913Slice.ScriptState.DivideBy0,
            VmPAcsVmOpcode913Slice.DivGlobalVar(globals, 0, stack, ref sp));
        Assert.Equal(99, globals[0]);
    }

    [Fact]
    public void DivWorldVar_DividesAndPops()
    {
        var world = new int[] { 84 };
        var stack = new int[] { 7 };
        var sp = 1;
        Assert.Equal(
            VmPAcsVmOpcode913Slice.ScriptState.Running,
            VmPAcsVmOpcode913Slice.DivWorldVar(world, 0, stack, ref sp));
        Assert.Equal(12, world[0]);
        Assert.Equal(0, sp);
    }

    [Fact]
    public void DivScriptArray_DividesCell()
    {
        var arrays = new VmPAcsVmOpcode913Slice.AcsScriptArrayStore();
        arrays.Set(2, 3, 100);
        var stack = new int[] { 3, 4 };
        var sp = 2;
        Assert.Equal(
            VmPAcsVmOpcode913Slice.ScriptState.Running,
            VmPAcsVmOpcode913Slice.DivScriptArray(arrays, 2, stack, ref sp));
        Assert.Equal(25, arrays.Get(2, 3));
    }

    [Fact]
    public void SubMapVar_DecrementsMapSlot()
    {
        var map = new int[] { 20 };
        var stack = new int[] { 5 };
        var sp = 1;
        VmPAcsVmOpcode913Slice.SubMapVar(map, 0, stack, ref sp);
        Assert.Equal(15, map[0]);
    }

    [Fact]
    public void MulScriptArray_MultipliesCell()
    {
        var arrays = new VmPAcsVmOpcode913Slice.AcsScriptArrayStore();
        arrays.Set(0, 1, 9);
        var stack = new int[] { 1, 3 };
        var sp = 2;
        VmPAcsVmOpcode913Slice.MulScriptArray(arrays, 0, stack, ref sp);
        Assert.Equal(27, arrays.Get(0, 1));
    }

    [Fact]
    public void DivMapVar_DetectsZeroDivisor()
    {
        var map = new int[] { 1 };
        var stack = new int[] { 0 };
        var sp = 1;
        Assert.Equal(
            VmPAcsVmOpcode913Slice.ScriptState.DivideBy0,
            VmPAcsVmOpcode913Slice.DivMapVar(map, 0, stack, ref sp));
    }
}
