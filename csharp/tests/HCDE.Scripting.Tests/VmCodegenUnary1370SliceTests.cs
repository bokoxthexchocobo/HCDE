namespace HCDE.Scripting.Tests;

public class VmCodegenUnary1370SliceTests
{
    [Fact]
    public void PlusTypeCastAndBitwiseNot()
    {
        var slice = new VmCodegenUnary1370Slice();
        Assert.Throws<InvalidOperationException>(() => slice.Create("plus", null));
        var operand = new VmCodegenUnary1370Slice.Node { Kind = "num", Numeric = true, ValueType = "int" };
        var plus = slice.Create("plus", operand);
        var calls = 0;
        slice.ResolveChild = node =>
        {
            calls++;
            return node;
        };
        var resolved = slice.ResolvePlus(plus);
        Assert.Same(operand, resolved);
        Assert.True(plus.Deleted);
        Assert.False(operand.Deleted);
        Assert.Null(plus.Operand);
        Assert.Equal(1, calls);
        plus.Resolved = true;
        plus.Deleted = false;
        Assert.Same(plus, slice.ResolvePlus(plus));
        Assert.Equal(1, calls);

        var bad = slice.Create("plus", new VmCodegenUnary1370Slice.Node { Kind = "name" });
        Assert.Null(slice.ResolvePlus(bad));
        Assert.Equal(new[] { "Numeric type expected" }, slice.Errors);
        Assert.True(bad.Deleted);
        Assert.True(bad.Operand == null || bad.Operand.Deleted);

        var again = slice.Create("plus", operand);
        slice.EmitChild = node =>
        {
            Assert.Same(operand, node);
            return new VmCodegenUnary1370Slice.Emit { RegNum = 4, RegType = VmCodegenUnary1370Slice.RegInt };
        };
        var emitted = slice.EmitPlus(again);
        Assert.Equal(4, emitted.RegNum);
        Assert.Throws<InvalidOperationException>(() => slice.TypeCastEmit());
        slice.Asserts = false;
        var empty = slice.TypeCastEmit();
        Assert.Equal(0, empty.RegNum);
        Assert.Equal(VmCodegenUnary1370Slice.RegNil, empty.RegType);
        Assert.Equal(1, empty.RegCount);
        Assert.False(empty.Konst || empty.Fixed || empty.Final || empty.Target);

        var bit = slice.Create("not", new VmCodegenUnary1370Slice.Node { Kind = "bits", IntValue = 3 });
        slice.Destroy(bit);
        Assert.True(bit.Deleted);
        Assert.Null(bit.Operand);
    }

    [Fact]
    public void MinusResolveAndEmit()
    {
        var slice = new VmCodegenUnary1370Slice();
        var constant = new VmCodegenUnary1370Slice.Node
        {
            Kind = "const",
            Numeric = true,
            Constant = true,
            RegType = VmCodegenUnary1370Slice.RegInt,
            IntValue = 5,
        };
        slice.ResolveChild = node => node;
        var minus = slice.Create("minus", constant);
        var folded = slice.ResolveMinus(minus);
        Assert.NotNull(folded);
        Assert.Equal(-5, folded.IntValue);
        Assert.True(minus.Deleted);
        Assert.True(constant.Deleted);
        Assert.NotSame(constant, folded);

        var floatConst = new VmCodegenUnary1370Slice.Node
        {
            Kind = "const",
            Numeric = true,
            Constant = true,
            RegType = VmCodegenUnary1370Slice.RegFloat,
            FloatValue = 1.5,
        };
        var foldedFloat = slice.ResolveMinus(slice.Create("minus", floatConst));
        Assert.NotNull(foldedFloat);
        Assert.Equal(-1.5, foldedFloat.FloatValue);

        var flag = new VmCodegenUnary1370Slice.Node { Kind = "bool", Numeric = true, Bool = true, ValueType = "bool" };
        slice.ResolveChild = node =>
        {
            if (node.Kind == "intcast")
            {
                Assert.True(node.Nowarn);
                Assert.Same(flag, node.Operand);
                return new VmCodegenUnary1370Slice.Node { Kind = "int", Numeric = true, ValueType = "int", RegType = VmCodegenUnary1370Slice.RegInt, RegCount = 1 };
            }

            return node;
        };
        var boolMinus = slice.Create("minus", flag);
        var kept = slice.ResolveMinus(boolMinus);
        Assert.Same(boolMinus, kept);
        Assert.False(boolMinus.Deleted);
        Assert.Equal("int", boolMinus.ValueType);
        slice.ResolveChild = node => node.Kind == "intcast" ? null : node;
        Assert.Throws<InvalidOperationException>(() => slice.ResolveMinus(slice.Create("minus", flag)));

        var plain = new VmCodegenUnary1370Slice.Node { Kind = "num", Numeric = true, ValueType = "float", RegType = VmCodegenUnary1370Slice.RegFloat, RegCount = 1 };
        slice.ResolveChild = node => node;
        var live = slice.ResolveMinus(slice.Create("minus", plain));
        Assert.NotNull(live);
        Assert.Equal("float", live.ValueType);
        Assert.False(live.Deleted);

        slice.Errors.Clear();
        var name = new VmCodegenUnary1370Slice.Node { Kind = "name" };
        Assert.Null(slice.ResolveMinus(slice.Create("minus", name)));
        Assert.Equal("Numeric type expected", slice.Errors[^1]);
        Assert.True(name.Deleted);

        slice.FindConstantInt = reg =>
        {
            Assert.Equal(3, reg);
            return 9;
        };
        slice.GetConstantInt = value =>
        {
            Assert.Equal(-9, value);
            return 12;
        };
        var konst = slice.EmitMinus(live, new VmCodegenUnary1370Slice.Emit { Konst = true, RegType = VmCodegenUnary1370Slice.RegInt, RegNum = 3 });
        Assert.True(konst.Konst);
        Assert.Equal(12, konst.RegNum);
        Assert.Equal(VmCodegenUnary1370Slice.RegInt, konst.RegType);
        Assert.Empty(slice.Ops);

        Assert.Throws<InvalidOperationException>(() => slice.EmitMinus(live, new VmCodegenUnary1370Slice.Emit { Konst = true, RegType = VmCodegenUnary1370Slice.RegFloat }));
        live.RegCount = 2;
        Assert.Throws<InvalidOperationException>(() => slice.EmitMinus(live, new VmCodegenUnary1370Slice.Emit { RegType = VmCodegenUnary1370Slice.RegFloat, RegCount = 1 }));

        live.RegType = VmCodegenUnary1370Slice.RegInt;
        live.RegCount = 1;
        var same = slice.EmitMinus(live, new VmCodegenUnary1370Slice.Emit { RegType = VmCodegenUnary1370Slice.RegInt, RegNum = 6 });
        Assert.Equal(6, same.RegNum);
        Assert.Equal(new VmCodegenUnary1370Slice.Op("NEG", 6, 6, 0), slice.Ops[^1]);

        slice.Alloc = (type, count) =>
        {
            Assert.Equal(VmCodegenUnary1370Slice.RegFloat, type);
            Assert.Equal(3, count);
            return 20;
        };
        live.RegType = VmCodegenUnary1370Slice.RegFloat;
        live.RegCount = 3;
        var vector = slice.EmitMinus(live, new VmCodegenUnary1370Slice.Emit { Fixed = true, RegType = VmCodegenUnary1370Slice.RegFloat, RegCount = 3, RegNum = 7 });
        Assert.Equal(20, vector.RegNum);
        Assert.False(vector.Fixed);
        Assert.Contains("Free", slice.Trace);
        Assert.Equal(new VmCodegenUnary1370Slice.Op("NEGV3", 20, 7, null), slice.Ops[^1]);

        live.RegCount = 1;
        slice.EmitMinus(live, new VmCodegenUnary1370Slice.Emit { RegType = VmCodegenUnary1370Slice.RegFloat, RegCount = 1, RegNum = 2 });
        Assert.Equal(new VmCodegenUnary1370Slice.Op("FLOP", 2, 2, VmCodegenUnary1370Slice.FlopNeg), slice.Ops[^1]);
        live.RegCount = 5;
        var skipped = slice.EmitMinus(live, new VmCodegenUnary1370Slice.Emit { RegType = VmCodegenUnary1370Slice.RegFloat, RegCount = 5, RegNum = 8 });
        Assert.Equal(8, skipped.RegNum);
        Assert.Equal(3, slice.Ops.Count);
    }
}
