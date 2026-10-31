namespace HCDE.Scripting.Tests;

public class VmCodegenRandom1387SliceTests
{
    [Fact]
    public void PickEmitLoadsConstantsBackpatchesAndMovesADifferentRegister()
    {
        var slice = new VmCodegenRandom1387Slice();
        var rng = new object();
        var func = new object();
        slice.Find = name => name == "BuiltinRandom"
            ? new VmCodegenRandom1387Slice.Symbol { Implementation = func }
            : null;
        var build = slice.NewBuild();
        var left = new VmCodegenRandom1387Slice.Expr { Constant = true, Int = 40000 };
        var right = new VmCodegenRandom1387Slice.Expr { Constant = true, Int = 3 };
        var pick = new VmCodegenRandom1387Slice.Pick { Rng = rng };
        pick.Choices.Add(left);
        pick.Choices.Add(right);
        Assert.Same(pick, pick.ReturnResolved());
        Assert.Equal(2, pick.Choices.Count);
        var result = slice.EmitPick(pick, build);
        Assert.Equal(
            "PARAM:7:0\nPARAMI:0\nPARAMI:1\nCALL_K:1:3:1\nRESULT:0:0:0\nIJMP:0:2\nJMP:1\nJMP:2\nLK:0:0\nJMP:1\nLI:0:3",
            build.Text());
        Assert.Equal(0, result.Num);
        Assert.Equal(VmCodegenRandom1387Slice.RegInt, result.Type);
        Assert.True(build.InUse(VmCodegenRandom1387Slice.RegInt, 0));
        Assert.Equal(1, build.Most[VmCodegenRandom1387Slice.RegInt]);
        Assert.Equal(0, build.ActiveParam);
        Assert.Equal(3, build.MaxParam);
        Assert.Equal(new[] { 40000 }, build.Ints);
        Assert.Same(rng, build.Addrs[0]);
        Assert.Same(func, build.Addrs[1]);
        Assert.Empty(pick.Choices);
        Assert.Equal(0, pick.ChoiceMost);
        Assert.Equal(0, pick.Choices.Capacity);
        Assert.True(left.Deleted);
        Assert.True(right.Deleted);

        var again = new VmCodegenRandom1387Slice.Pick { Rng = rng };
        again.Choices.Add(new VmCodegenRandom1387Slice.Expr { Constant = true, Int = 9 });
        slice.EmitPick(again, build);
        Assert.Equal(2, build.Addrs.Count);

        var floats = slice.NewBuild();
        var wide = new VmCodegenRandom1387Slice.Pick { Rng = rng, ValueRegType = VmCodegenRandom1387Slice.RegFloat };
        wide.Choices.Add(new VmCodegenRandom1387Slice.Expr { Constant = true, Float = 1.5 });
        wide.Choices.Add(new VmCodegenRandom1387Slice.Expr { Constant = true, Float = 1.5 });
        var freg = slice.EmitPick(wide, floats);
        Assert.Equal(VmCodegenRandom1387Slice.RegFloat, freg.Type);
        Assert.True(floats.InUse(VmCodegenRandom1387Slice.RegFloat, 0));
        Assert.False(floats.InUse(VmCodegenRandom1387Slice.RegInt, 0));
        Assert.Contains("LKF:0:0\nJMP:1\nLKF:0:0", floats.Text(), StringComparison.Ordinal);
        Assert.Single(floats.Floats);
        Assert.Equal(1.5, floats.Floats[0]);

        var moved = slice.NewBuild();
        var branch = new VmCodegenRandom1387Slice.Pick { Rng = rng };
        branch.Choices.Add(new VmCodegenRandom1387Slice.Expr
        {
            Emit = host =>
            {
                var low = host.Alloc(VmCodegenRandom1387Slice.RegInt);
                var high = host.Alloc(VmCodegenRandom1387Slice.RegInt);
                host.Free(low);
                return high;
            },
        });
        var movedReg = slice.EmitPick(branch, moved);
        Assert.Equal(0, movedReg.Num);
        Assert.EndsWith("JMP:0\nMOVE:0:1:0", moved.Text(), StringComparison.Ordinal);
        Assert.False(moved.InUse(VmCodegenRandom1387Slice.RegInt, 1));

        var same = slice.NewBuild();
        var held = new VmCodegenRandom1387Slice.Pick { Rng = rng };
        held.Choices.Add(new VmCodegenRandom1387Slice.Expr { Emit = host => host.Alloc(VmCodegenRandom1387Slice.RegInt) });
        slice.EmitPick(held, same);
        Assert.DoesNotContain("MOVE:", same.Text(), StringComparison.Ordinal);

        var empty = new VmCodegenRandom1387Slice.Pick { Rng = rng };
        Assert.Throws<VmCodegenRandom1387Slice.AssertAbort>(() => slice.EmitPick(empty, slice.NewBuild()));
        slice.Find = _ => null;
        var missing = new VmCodegenRandom1387Slice.Pick { Rng = rng };
        missing.Choices.Add(new VmCodegenRandom1387Slice.Expr { Constant = true, Int = 1 });
        Assert.Throws<VmCodegenRandom1387Slice.AssertAbort>(() => slice.EmitPick(missing, slice.NewBuild()));

        var varSlice = new VmCodegenRandom1387Slice
        {
            Find = _ => new VmCodegenRandom1387Slice.Symbol { Implementation = func, Flags = VmCodegenRandom1387Slice.VarArgFlag },
        };
        var varBuild = varSlice.NewBuild();
        var varPick = new VmCodegenRandom1387Slice.Pick { Rng = rng };
        varPick.Choices.Add(new VmCodegenRandom1387Slice.Expr { Constant = true, Int = 7 });
        varSlice.EmitPick(varPick, varBuild);
        Assert.Equal(new byte[] { 3, 0, 0 }, varBuild.LastInfo);
        Assert.Contains("CALL_K:2:4:1", varBuild.Text(), StringComparison.Ordinal);

        var wideConst = slice.NewBuild();
        var direct = new VmCodegenRandom1387Slice.Call(new VmCodegenRandom1387Slice.Symbol { Implementation = func });
        direct.IntConst(8388608);
        direct.AddReturn(VmCodegenRandom1387Slice.RegInt);
        direct.Emit(wideConst);
        Assert.StartsWith("PARAM:4:0", wideConst.Text(), StringComparison.Ordinal);
        Assert.Equal(new[] { 8388608 }, wideConst.Ints);
    }

    [Fact]
    public void FRandomScalesUnitsAndRandom2DeletesTheMask()
    {
        uint half = 0x80000000;
        double t = half * Math.ScaleB(1.0, -32);
        var rng = new VmCodegenRandom1387Slice.Rng { Next = () => half };
        Assert.Equal((1.25 * (1.0 - t)) + (3.75 * t), VmCodegenRandom1387Slice.NativeFRandom(rng, 1.25, 3.75));
        rng = new VmCodegenRandom1387Slice.Rng { Next = () => 0 };
        Assert.Equal(1.25, VmCodegenRandom1387Slice.NativeFRandom(rng, 1.25, 9));

        var slice = new VmCodegenRandom1387Slice();
        var slots = new[]
        {
            new VmCodegenRandom1387Slice.Slot { Ptr = rng },
            new VmCodegenRandom1387Slice.Slot { Float = 2 },
            new VmCodegenRandom1387Slice.Slot { Float = 4 },
        };
        int[] info = [VmCodegenRandom1387Slice.RegPointer, VmCodegenRandom1387Slice.RegFloat, VmCodegenRandom1387Slice.RegFloat];
        var ret = new VmCodegenRandom1387Slice.Slot { Float = 9 };
        Assert.Equal(0, slice.BuiltinFRandom(slots, info, 0, ret));
        Assert.Equal(9, ret.Float);
        Assert.Equal(1, slice.BuiltinFRandom(slots, info, 1, ret));
        Assert.Equal(2, ret.Float);
        Assert.Throws<VmCodegenRandom1387Slice.AssertAbort>(() => slice.BuiltinFRandom(slots, [0, 1, 1], 1, ret));
        Assert.Throws<VmCodegenRandom1387Slice.AssertAbort>(() => slice.BuiltinFRandom(slots, info, 1, null));
        slots[0].Ptr = null;
        Assert.Throws<NullReferenceException>(() => slice.BuiltinFRandom(slots, info, 1, ret));

        var mi = new VmCodegenRandom1387Slice.Expr { Pos = 4 };
        var ma = new VmCodegenRandom1387Slice.Expr { Pos = 5 };
        var node = slice.ConstructF(rng, mi, ma, 8);
        Assert.Equal("Random", node.Kind);
        Assert.Equal("FRandom", node.Requested);
        Assert.Equal(8, node.Pos);
        Assert.Equal(VmCodegenRandom1387Slice.RegFloat, node.ValueRegType);
        Assert.Equal("FloatCast", node.Min!.Kind);
        Assert.Same(mi, node.Min.Inner);
        Assert.Equal(4, node.Min.Pos);
        Assert.Same(ma, node.Max!.Inner);
        Assert.Throws<VmCodegenRandom1387Slice.AssertAbort>(() => slice.ConstructF(rng, mi, null, 1));
        slice.Asserts = false;
        Assert.Throws<NullReferenceException>(() => slice.ConstructF(rng, mi, null, 1));
        Assert.Throws<NullReferenceException>(() => slice.ConstructF(rng, null, ma, 1));
        slice.Asserts = true;

        var func = new object();
        slice.Find = name => name == "BuiltinFRandom"
            ? new VmCodegenRandom1387Slice.Symbol { Implementation = func, ProtoSize = 2, ProtoTail = { (VmCodegenRandom1387Slice.RegInt, 1) } }
            : null;
        var build = slice.NewBuild();
        build.VmJit = true;
        node.Min.Emit = host =>
        {
            var reg = host.Alloc(VmCodegenRandom1387Slice.RegFloat);
            host.Emit16("LKF", reg.Num, host.ConstFloat(1.25));
            return reg;
        };
        node.Max.Emit = host =>
        {
            var reg = host.Alloc(VmCodegenRandom1387Slice.RegFloat);
            host.Emit16("LKF", reg.Num, host.ConstFloat(2.5));
            return reg;
        };
        node.Rng = new object();
        var emitted = slice.EmitF(node, build);
        Assert.Equal(
            "LKF:0:0\nLKF:1:1\nPARAM:7:0\nPARAM:1:0\nPARAM:1:1\nCALL_K:1:3:2\nRESULT:0:1:0\nRESULT:0:0:0",
            build.Text());
        Assert.Equal(VmCodegenRandom1387Slice.RegFloat, emitted.Type);
        Assert.True(build.InUse(VmCodegenRandom1387Slice.RegFloat, 0));
        Assert.False(build.InUse(VmCodegenRandom1387Slice.RegInt, 0));
        Assert.Equal(0, build.ActiveParam);
        Assert.Equal(3, build.MaxParam);

        var nilBuild = slice.NewBuild();
        node.Max.Emit = _ => new VmCodegenRandom1387Slice.Reg { Type = VmCodegenRandom1387Slice.RegNil, Num = 4 };
        slice.EmitF(node, nilBuild);
        Assert.Equal("Attempted to pass a non-value", Assert.Single(nilBuild.Messages));
        Assert.Contains("PARAM:128:4", nilBuild.Text(), StringComparison.Ordinal);

        var plain = new VmCodegenRandom1387Slice.Expr { Pos = 2 };
        var made = slice.Construct2(rng, null, 6, true);
        Assert.Equal("Random2", made.Kind);
        Assert.Equal(VmCodegenRandom1387Slice.RegInt, made.ValueRegType);
        Assert.Equal(6, made.Pos);
        Assert.Equal("Constant", made.Mask!.Kind);
        Assert.Equal(-1, made.Mask.Int);
        Assert.True(made.Mask.Resolved);
        var casted = slice.Construct2(rng, plain, 6, true);
        Assert.Equal("IntCast", casted.Mask!.Kind);
        Assert.True(casted.Mask.NoWarn);
        Assert.False(casted.Mask.Resolved);
        Assert.Same(plain, casted.Mask.Inner);
        casted.Destroy();
        Assert.Null(casted.Mask);
        Assert.True(plain.Deleted);
        casted.Destroy();
        Assert.Null(casted.Mask);
    }
}
