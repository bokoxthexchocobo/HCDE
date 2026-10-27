namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode915SliceTests
{
    [Fact]
    public void AndScriptArray_BitwiseAndsCell()
    {
        var arrays = new VmPAcsVmOpcode915Slice.AcsScriptArrayStore();
        arrays.Set(1, 2, 0b1111);
        var stack = new int[] { 2, 0b1010 };
        var sp = 2;
        VmPAcsVmOpcode915Slice.AndScriptArray(arrays, 1, stack, ref sp);
        Assert.Equal(0b1010, arrays.Get(1, 2));
    }

    [Fact]
    public void EorGlobalVar_TogglesBits()
    {
        var globals = new int[] { 0b1100 };
        var stack = new int[] { 0b1010 };
        var sp = 1;
        VmPAcsVmOpcode915Slice.EorGlobalVar(globals, 0, stack, ref sp);
        Assert.Equal(0b0110, globals[0]);
    }

    [Fact]
    public void OrWorldArray_OrCombinesElement()
    {
        var arrays = new[] { new int[4] };
        arrays[0][1] = 1;
        var stack = new int[] { 1, 2 };
        var sp = 2;
        VmPAcsVmOpcode915Slice.OrWorldArray(arrays, 0, stack, ref sp);
        Assert.Equal(3, arrays[0][1]);
    }

    [Fact]
    public void OrMapArray_ResolvesArrayThroughMapVar()
    {
        var mapCells = new int[] { 0, 5 };
        var mapArrays = new VmPAcsVmOpcode915Slice.AcsMapArrayStore();
        mapArrays.Set(5, 0, 4);
        var stack = new int[] { 0, 1 };
        var sp = 2;
        VmPAcsVmOpcode915Slice.OrMapArray(mapArrays, 1, mapCells, stack, ref sp);
        Assert.Equal(5, mapArrays.Get(5, 0));
    }

    [Fact]
    public void LsScriptVar_ShiftsLocalLeft()
    {
        var locals = new int[] { 3 };
        var stack = new int[] { 2 };
        var sp = 1;
        VmPAcsVmOpcode915Slice.LsScriptVar(locals, 0, stack, ref sp);
        Assert.Equal(12, locals[0]);
    }

    [Fact]
    public void LsGlobalVar_ShiftsByStackAmount()
    {
        var globals = new int[] { 1 };
        var stack = new int[] { 4 };
        var sp = 1;
        VmPAcsVmOpcode915Slice.LsGlobalVar(globals, 0, stack, ref sp);
        Assert.Equal(16, globals[0]);
    }

    [Fact]
    public void AndGlobalArray_AndsInPlace()
    {
        var arrays = new[] { new int[8] };
        arrays[0][3] = 255;
        var stack = new int[] { 3, 15 };
        var sp = 2;
        VmPAcsVmOpcode915Slice.AndGlobalArray(arrays, 0, stack, ref sp);
        Assert.Equal(15, arrays[0][3]);
    }

    [Fact]
    public void EorScriptArray_UpdatesStoredValue()
    {
        var arrays = new VmPAcsVmOpcode915Slice.AcsScriptArrayStore();
        arrays.Set(0, 1, 0xFF);
        var stack = new int[] { 1, 0x0F };
        var sp = 2;
        VmPAcsVmOpcode915Slice.EorScriptArray(arrays, 0, stack, ref sp);
        Assert.Equal(0xF0, arrays.Get(0, 1));
    }

    [Fact]
    public void OrScriptVar_MergesBits()
    {
        var locals = new int[] { 1 };
        var stack = new int[] { 4 };
        var sp = 1;
        VmPAcsVmOpcode915Slice.OrScriptVar(locals, 0, stack, ref sp);
        Assert.Equal(5, locals[0]);
    }

    [Fact]
    public void LsMapVar_LeftShiftsMapSlot()
    {
        var map = new int[] { 2 };
        var stack = new int[] { 3 };
        var sp = 1;
        VmPAcsVmOpcode915Slice.LsMapVar(map, 0, stack, ref sp);
        Assert.Equal(16, map[0]);
    }
}
