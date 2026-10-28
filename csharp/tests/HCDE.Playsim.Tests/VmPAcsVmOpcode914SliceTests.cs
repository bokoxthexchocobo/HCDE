namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode914SliceTests
{
    [Fact]
    public void DivWorldArray_DividesElement()
    {
        var arrays = new[] { new int[8], new int[8] };
        arrays[1][3] = 100;
        var stack = new int[] { 3, 4 };
        var sp = 2;
        Assert.Equal(
            VmPAcsVmOpcode914Slice.ScriptState.Running,
            VmPAcsVmOpcode914Slice.DivWorldArray(arrays, 1, stack, ref sp));
        Assert.Equal(25, arrays[1][3]);
    }

    [Fact]
    public void DivMapArray_ReturnsDivideByZero()
    {
        var mapCells = new int[] { 0, 2 };
        var mapArrays = new VmPAcsVmOpcode914Slice.AcsMapArrayStore();
        var stack = new int[] { 1, 0 };
        var sp = 2;
        Assert.Equal(
            VmPAcsVmOpcode914Slice.ScriptState.DivideBy0,
            VmPAcsVmOpcode914Slice.DivMapArray(mapArrays, 1, mapCells, stack, ref sp));
    }

    [Fact]
    public void ModScriptVar_ReturnsModulusByZero()
    {
        var locals = new int[] { 10 };
        var stack = new int[] { 0 };
        var sp = 1;
        Assert.Equal(
            VmPAcsVmOpcode914Slice.ScriptState.ModulusBy0,
            VmPAcsVmOpcode914Slice.ModScriptVar(locals, 0, stack, ref sp));
    }

    [Fact]
    public void ModGlobalVar_AppliesModulo()
    {
        var globals = new int[] { 29 };
        var stack = new int[] { 5 };
        var sp = 1;
        Assert.Equal(
            VmPAcsVmOpcode914Slice.ScriptState.Running,
            VmPAcsVmOpcode914Slice.ModGlobalVar(globals, 0, stack, ref sp));
        Assert.Equal(4, globals[0]);
    }

    [Fact]
    public void ModScriptArray_UpdatesCell()
    {
        var arrays = new VmPAcsVmOpcode914Slice.AcsScriptArrayStore();
        arrays.Set(1, 2, 17);
        var stack = new int[] { 2, 5 };
        var sp = 2;
        VmPAcsVmOpcode914Slice.ModScriptArray(arrays, 1, stack, ref sp);
        Assert.Equal(2, arrays.Get(1, 2));
    }

    [Fact]
    public void ModMapArray_UsesMapVarCellAsArrayId()
    {
        var mapCells = new int[] { 0, 7 };
        var mapArrays = new VmPAcsVmOpcode914Slice.AcsMapArrayStore();
        mapArrays.Set(7, 0, 22);
        var stack = new int[] { 0, 7 };
        var sp = 2;
        VmPAcsVmOpcode914Slice.ModMapArray(mapArrays, 1, mapCells, stack, ref sp);
        Assert.Equal(1, mapArrays.Get(7, 0));
    }

    [Fact]
    public void DivGlobalArray_DetectsZeroDivisor()
    {
        var arrays = new[] { new int[4] };
        var stack = new int[] { 0, 0 };
        var sp = 2;
        Assert.Equal(
            VmPAcsVmOpcode914Slice.ScriptState.DivideBy0,
            VmPAcsVmOpcode914Slice.DivGlobalArray(arrays, 0, stack, ref sp));
    }

    [Fact]
    public void AndWorldVar_BitwiseAndsStackTop()
    {
        var world = new int[] { 0b1111 };
        var stack = new int[] { 0b1010 };
        var sp = 1;
        VmPAcsVmOpcode914Slice.AndWorldVar(world, 0, stack, ref sp);
        Assert.Equal(0b1010, world[0]);
        Assert.Equal(0, sp);
    }

    [Fact]
    public void AndMapVar_ClearsBits()
    {
        var map = new int[] { 255 };
        var stack = new int[] { 15 };
        var sp = 1;
        VmPAcsVmOpcode914Slice.AndMapVar(map, 0, stack, ref sp);
        Assert.Equal(15, map[0]);
    }

    [Fact]
    public void ModWorldArray_ModulusOnElement()
    {
        var arrays = new[] { new int[6] };
        arrays[0][2] = 53;
        var stack = new int[] { 2, 8 };
        var sp = 2;
        VmPAcsVmOpcode914Slice.ModWorldArray(arrays, 0, stack, ref sp);
        Assert.Equal(5, arrays[0][2]);
    }
}
