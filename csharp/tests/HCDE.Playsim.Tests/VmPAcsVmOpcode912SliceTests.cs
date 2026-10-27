namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode912SliceTests
{
    [Fact]
    public void TryApplyGe_ComparesSecondToTop()
    {
        var stack = new int[4];
        var sp = 2;
        stack[0] = 10;
        stack[1] = 10;
        Assert.Equal(VmPAcsVmOpcode912Slice.ScriptState.Running, VmPAcsVmOpcode912Slice.TryApplyGe(stack, ref sp));
        Assert.Equal(1, stack[0]);
        Assert.Equal(1, sp);
    }

    [Fact]
    public void TryApplyGt_MalformedWhenStackTooShallow()
    {
        var stack = new int[2];
        var sp = 1;
        Assert.Equal(
            VmPAcsVmOpcode912Slice.ScriptState.PleaseRemove,
            VmPAcsVmOpcode912Slice.TryApplyGt(stack, ref sp));
    }

    [Fact]
    public void AssignScriptVar_PopsStackTopIntoLocal()
    {
        var locals = new int[4];
        var stack = new int[4];
        var sp = 1;
        stack[0] = 99;
        VmPAcsVmOpcode912Slice.AssignScriptVar(locals, 2, stack, ref sp);
        Assert.Equal(0, sp);
        Assert.Equal(99, locals[2]);
    }

    [Fact]
    public void PushWorldVar_PushesStoredValue()
    {
        var world = new int[] { 0, 0, 42 };
        var stack = new int[4];
        var sp = 0;
        VmPAcsVmOpcode912Slice.PushWorldVar(world, 2, stack, ref sp);
        Assert.Equal(1, sp);
        Assert.Equal(42, stack[0]);
    }

    [Fact]
    public void AssignScriptArray_WritesIndexedCell()
    {
        var arrays = new VmPAcsVmOpcode912Slice.AcsScriptArrayStore();
        var stack = new int[4];
        var sp = 2;
        stack[0] = 3;
        stack[1] = 77;
        VmPAcsVmOpcode912Slice.AssignScriptArray(arrays, 1, stack, ref sp);
        Assert.Equal(0, sp);
        Assert.Equal(77, arrays.Get(1, 3));
    }

    [Fact]
    public void PushMapArray_ResolvesArrayThroughMapVarCell()
    {
        var mapCells = new int[] { 0, 5 };
        var mapArrays = new VmPAcsVmOpcode912Slice.AcsMapArrayStore();
        mapArrays.Set(5, 2, 123);
        var stack = new int[4];
        var sp = 1;
        stack[0] = 2;
        VmPAcsVmOpcode912Slice.PushMapArray(mapArrays, 1, mapCells, stack, ref sp);
        Assert.Equal(123, stack[0]);
    }

    [Fact]
    public void AddGlobalVar_IncrementsSlot()
    {
        var globals = new int[] { 10, 20 };
        var stack = new int[4];
        var sp = 1;
        stack[0] = 5;
        VmPAcsVmOpcode912Slice.AddGlobalVar(globals, 1, stack, ref sp);
        Assert.Equal(0, sp);
        Assert.Equal(25, globals[1]);
    }

    [Fact]
    public void AddWorldArray_AddsToIndexedElement()
    {
        var arrays = new[] { new int[8], new int[8] };
        arrays[1][4] = 100;
        var stack = new int[4];
        var sp = 2;
        stack[0] = 4;
        stack[1] = 7;
        VmPAcsVmOpcode912Slice.AddWorldArray(arrays, 1, stack, ref sp);
        Assert.Equal(0, sp);
        Assert.Equal(107, arrays[1][4]);
    }

    [Fact]
    public void AddMapArray_UsesMapVarAsArrayId()
    {
        var mapCells = new int[] { 0, 9 };
        var mapArrays = new VmPAcsVmOpcode912Slice.AcsMapArrayStore();
        mapArrays.Set(9, 1, 50);
        var stack = new int[4];
        var sp = 2;
        stack[0] = 1;
        stack[1] = 8;
        VmPAcsVmOpcode912Slice.AddMapArray(mapArrays, 1, mapCells, stack, ref sp);
        Assert.Equal(58, mapArrays.Get(9, 1));
    }

    [Fact]
    public void TryApplyLe_ReturnsOneWhenLessOrEqual()
    {
        var stack = new int[4];
        var sp = 2;
        stack[0] = 3;
        stack[1] = 9;
        VmPAcsVmOpcode912Slice.TryApplyLe(stack, ref sp);
        Assert.Equal(1, stack[0]);
    }
}
