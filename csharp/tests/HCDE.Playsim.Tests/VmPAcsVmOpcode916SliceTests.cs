namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode916SliceTests
{
    [Fact]
    public void LsScriptArray_ShiftsCellLeft()
    {
        var arrays = new VmPAcsVmOpcode916Slice.AcsScriptArrayStore();
        arrays.Set(0, 1, 2);
        var stack = new int[] { 1, 2 };
        var sp = 2;
        VmPAcsVmOpcode916Slice.LsScriptArray(arrays, 0, stack, ref sp);
        Assert.Equal(8, arrays.Get(0, 1));
    }

    [Fact]
    public void RsGlobalVar_RightShiftsLocal()
    {
        var globals = new int[] { 32 };
        var stack = new int[] { 2 };
        var sp = 1;
        VmPAcsVmOpcode916Slice.RsGlobalVar(globals, 0, stack, ref sp);
        Assert.Equal(8, globals[0]);
    }

    [Fact]
    public void RsWorldArray_ShiftsElement()
    {
        var arrays = new[] { new int[4] };
        arrays[0][2] = 64;
        var stack = new int[] { 2, 3 };
        var sp = 2;
        VmPAcsVmOpcode916Slice.RsWorldArray(arrays, 0, stack, ref sp);
        Assert.Equal(8, arrays[0][2]);
    }

    [Fact]
    public void IncScriptVar_Increments()
    {
        var locals = new int[] { 4 };
        VmPAcsVmOpcode916Slice.IncScriptVar(locals, 0);
        Assert.Equal(5, locals[0]);
    }

    [Fact]
    public void IncMapArray_ResolvesArrayId()
    {
        var mapCells = new int[] { 0, 3 };
        var mapArrays = new VmPAcsVmOpcode916Slice.AcsMapArrayStore();
        mapArrays.Set(3, 0, 9);
        var stack = new int[] { 0 };
        var sp = 1;
        VmPAcsVmOpcode916Slice.IncMapArray(mapArrays, 1, mapCells, stack, ref sp);
        Assert.Equal(10, mapArrays.Get(3, 0));
    }

    [Fact]
    public void DecWorldVar_Decrements()
    {
        var world = new int[] { 7 };
        VmPAcsVmOpcode916Slice.DecWorldVar(world, 0);
        Assert.Equal(6, world[0]);
    }

    [Fact]
    public void LsMapArray_ShiftsThroughMapVar()
    {
        var mapCells = new int[] { 0, 2 };
        var mapArrays = new VmPAcsVmOpcode916Slice.AcsMapArrayStore();
        mapArrays.Set(2, 1, 1);
        var stack = new int[] { 1, 3 };
        var sp = 2;
        VmPAcsVmOpcode916Slice.LsMapArray(mapArrays, 1, mapCells, stack, ref sp);
        Assert.Equal(8, mapArrays.Get(2, 1));
    }

    [Fact]
    public void RsMapArray_RightShiftsCell()
    {
        var mapCells = new int[] { 0, 1 };
        var mapArrays = new VmPAcsVmOpcode916Slice.AcsMapArrayStore();
        mapArrays.Set(1, 0, 100);
        var stack = new int[] { 0, 2 };
        var sp = 2;
        VmPAcsVmOpcode916Slice.RsMapArray(mapArrays, 1, mapCells, stack, ref sp);
        Assert.Equal(25, mapArrays.Get(1, 0));
    }

    [Fact]
    public void IncGlobalArray_IncrementsElement()
    {
        var arrays = new[] { new int[8] };
        arrays[0][4] = 2;
        var stack = new int[] { 4 };
        var sp = 1;
        VmPAcsVmOpcode916Slice.IncGlobalArray(arrays, 0, stack, ref sp);
        Assert.Equal(3, arrays[0][4]);
    }

    [Fact]
    public void DecGlobalVar_DecrementsSlot()
    {
        var globals = new int[] { 1 };
        VmPAcsVmOpcode916Slice.DecGlobalVar(globals, 0);
        Assert.Equal(0, globals[0]);
    }
}
