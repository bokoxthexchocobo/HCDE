namespace HCDE.Playsim.Tests;

public class VmTOperLogic1179SliceTests
{
    [Fact]
    public void Table_ListsTheOperatorsAndStopsBeforeDivide()
    {
        Assert.Equal(21, VmTOperLogic1179Slice.NumOperators);
        Assert.Equal("=", VmTOperLogic1179Slice.Operators[0].Symbol);
        Assert.Equal(VmTOperLogic1179Slice.Backward, VmTOperLogic1179Slice.Operators[0].Direction);
        Assert.All(
            VmTOperLogic1179Slice.Operators.Skip(1),
            row => Assert.Equal(VmTOperLogic1179Slice.Forward, row.Direction));
        Assert.Equal(
            new[] { "=", "||", "&&", "|", "&", "==", "!=", "<", ">", "<=", ">=", "+", "-", "*", "/", "%", "~", "!", "++", "--", "." },
            VmTOperLogic1179Slice.Operators.Select(row => row.Symbol).ToArray());
        Assert.Equal("OPmultiply", VmTOperLogic1179Slice.Operators[13].Handler);
        Assert.True(VmTOperLogic1179Slice.Operators[13].BodyInThisBand);
        Assert.Equal("OPdivide", VmTOperLogic1179Slice.Operators[14].Handler);
        Assert.False(VmTOperLogic1179Slice.Operators[14].BodyInThisBand);
        Assert.False(VmTOperLogic1179Slice.Operators.Single(row => row.Symbol == "<=").BodyInThisBand);
        Assert.Equal(12, VmTOperLogic1179Slice.Atoi("  12abc"));
        Assert.Equal(-3, VmTOperLogic1179Slice.Atoi("\t-3x"));
        Assert.Equal(98304, VmTOperLogic1179Slice.FixedValue(new VmTOperLogic1179Slice.Value { Type = VmTOperLogic1179Slice.SvtString, Text = "1.5junk" }));
        Assert.Equal(-1, VmTOperLogic1179Slice.IntValue(new VmTOperLogic1179Slice.Value { Type = VmTOperLogic1179Slice.SvtMobj }));
        Assert.Equal(-65536, VmTOperLogic1179Slice.FixedValue(new VmTOperLogic1179Slice.Value { Type = VmTOperLogic1179Slice.SvtMobj }));
        Assert.Equal(-1d, VmTOperLogic1179Slice.FloatValue(new VmTOperLogic1179Slice.Value { Type = VmTOperLogic1179Slice.SvtMobj }));
    }

    [Fact]
    public void Logic_ShortCircuitsComparesAndAssigns()
    {
        var global = new object();
        var slot = new VmTOperLogic1179Slice.Variable { Name = "health" };
        var frame = new VmTOperLogic1179Slice.Frame
        {
            Tokens = ["health", "=", "4"],
            GlobalScript = global,
            FindVariable = (name, script) =>
            {
                Assert.Same(global, script);
                return name == "health" ? slot : null;
            },
            SetValue = (variable, value) => variable.Assigned = value,
            Evaluate = (into, _, _) =>
            {
                into.Type = VmTOperLogic1179Slice.SvtInt;
                into.Int = 4;
            },
        };
        var assigned = new VmTOperLogic1179Slice.Value { Int = 9 };
        VmTOperLogic1179Slice.EqualsOp(frame, assigned, 0, 1, 2);
        Assert.Same(assigned, slot.Assigned);
        Assert.Equal(4, assigned.Int);
        Assert.Empty(frame.Errors);
        Assert.Equal(new[] { (2, 2) }, frame.Spans);

        frame.Spans.Clear();
        frame.Tokens = ["missing"];
        var untouched = new VmTOperLogic1179Slice.Value { Int = 9, Type = VmTOperLogic1179Slice.SvtInt };
        VmTOperLogic1179Slice.EqualsOp(frame, untouched, 0, 1, 2);
        Assert.Equal(9, untouched.Int);
        Assert.Empty(frame.Spans);
        Assert.Equal(new[] { "unknown variable 'missing'\n" }, frame.Errors);

        var logic = new VmTOperLogic1179Slice.Frame
        {
            Evaluate = (into, start, _) =>
            {
                into.Type = VmTOperLogic1179Slice.SvtInt;
                into.Int = start == 0 ? 5 : 0;
            },
        };
        var orResult = new VmTOperLogic1179Slice.Value();
        VmTOperLogic1179Slice.OrOp(logic, orResult, 0, 1, 2);
        Assert.Equal(VmTOperLogic1179Slice.SvtInt, orResult.Type);
        Assert.Equal(1, orResult.Int);
        Assert.Equal(new[] { (0, 0) }, logic.Spans);

        logic.Spans.Clear();
        logic.Evaluate = (into, start, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtString;
            into.Text = start == 0 ? "0" : "3junk";
        };
        VmTOperLogic1179Slice.OrOp(logic, orResult, 0, 1, 2);
        Assert.Equal(1, orResult.Int);
        Assert.Equal(new[] { (0, 0), (2, 2) }, logic.Spans);

        logic.Spans.Clear();
        logic.Evaluate = (into, start, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtInt;
            into.Int = start == 0 ? 0 : 8;
        };
        var andResult = new VmTOperLogic1179Slice.Value();
        VmTOperLogic1179Slice.AndOp(logic, andResult, 0, 1, 2);
        Assert.Equal(0, andResult.Int);
        Assert.Equal(new[] { (0, 0) }, logic.Spans);

        logic.Spans.Clear();
        logic.Evaluate = (into, start, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtInt;
            into.Int = start == 0 ? 2 : 0;
        };
        VmTOperLogic1179Slice.AndOp(logic, andResult, 0, 1, 2);
        Assert.Equal(0, andResult.Int);
        Assert.Equal(new[] { (0, 0), (2, 2) }, logic.Spans);

        var actor = new object();
        var compare = new VmTOperLogic1179Slice.Frame
        {
            Evaluate = (into, start, _) =>
            {
                if (start == 0)
                {
                    into.Type = VmTOperLogic1179Slice.SvtString;
                    into.Text = "same";
                }
                else
                {
                    into.Type = VmTOperLogic1179Slice.SvtString;
                    into.Text = "same";
                }
            },
        };
        var cmp = new VmTOperLogic1179Slice.Value();
        VmTOperLogic1179Slice.CmpOp(compare, cmp, 0, 1, 2);
        Assert.Equal(1, cmp.Int);
        VmTOperLogic1179Slice.NotCmpOp(compare, cmp, 0, 1, 2);
        Assert.Equal(0, cmp.Int);
        Assert.Equal(VmTOperLogic1179Slice.SvtInt, cmp.Type);

        compare.Evaluate = (into, start, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtMobj;
            into.Mobj = start == 0 ? actor : new object();
        };
        VmTOperLogic1179Slice.CmpOp(compare, cmp, 0, 1, 2);
        Assert.Equal(0, cmp.Int);
        compare.Evaluate = (into, _, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtMobj;
            into.Mobj = actor;
        };
        VmTOperLogic1179Slice.CmpOp(compare, cmp, 0, 1, 2);
        Assert.Equal(1, cmp.Int);

        compare.Evaluate = (into, start, _) =>
        {
            if (start == 0)
            {
                into.Type = VmTOperLogic1179Slice.SvtFixed;
                into.Fixed = -1;
            }
            else
            {
                into.Type = VmTOperLogic1179Slice.SvtInt;
                into.Int = 0;
            }
        };
        var less = new VmTOperLogic1179Slice.Value();
        VmTOperLogic1179Slice.LessThanOp(compare, less, 0, 1, 2);
        Assert.Equal(1, less.Int);
        VmTOperLogic1179Slice.GreaterThanOp(compare, less, 0, 1, 2);
        Assert.Equal(0, less.Int);

        compare.Spans.Clear();
        compare.Evaluate = (into, _, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtFixed;
            into.Fixed = 0;
        };
        var negated = new VmTOperLogic1179Slice.Value { Int = 7 };
        VmTOperLogic1179Slice.NotOp(compare, negated, 0, 1, 2);
        Assert.Equal(new[] { (2, 2) }, compare.Spans);
        Assert.Equal(VmTOperLogic1179Slice.SvtInt, negated.Type);
        Assert.Equal(1, negated.Int);
    }

    [Fact]
    public void Arithmetic_AddsFixedBitsAndTruncatesTheProduct()
    {
        var frame = new VmTOperLogic1179Slice.Frame
        {
            Evaluate = (into, start, _) =>
            {
                into.Type = start == 0 ? VmTOperLogic1179Slice.SvtString : VmTOperLogic1179Slice.SvtString;
                into.Text = start == 0 ? "a" : "b";
            },
        };
        var sum = new VmTOperLogic1179Slice.Value();
        VmTOperLogic1179Slice.PlusOp(frame, sum, 0, 1, 2);
        Assert.Equal(VmTOperLogic1179Slice.SvtString, sum.Type);
        Assert.Equal("ab", sum.Text);

        frame.Evaluate = (into, start, _) =>
        {
            if (start == 0)
            {
                into.Type = VmTOperLogic1179Slice.SvtString;
                into.Text = "a";
            }
            else
            {
                into.Type = VmTOperLogic1179Slice.SvtFixed;
                into.Fixed = 98304;
            }
        };
        VmTOperLogic1179Slice.PlusOp(frame, sum, 0, 1, 2);
        Assert.Equal("a1.5000", sum.Text);

        frame.Evaluate = (into, start, _) =>
        {
            if (start == 0)
            {
                into.Type = VmTOperLogic1179Slice.SvtString;
                into.Text = "go";
            }
            else
            {
                into.Type = VmTOperLogic1179Slice.SvtMobj;
                into.Mobj = new object();
            }
        };
        VmTOperLogic1179Slice.PlusOp(frame, sum, 0, 1, 2);
        Assert.Equal("go-1", sum.Text);

        frame.Evaluate = (into, start, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtFixed;
            into.Fixed = start == 0 ? 32768 : 32768;
        };
        VmTOperLogic1179Slice.PlusOp(frame, sum, 0, 1, 2);
        Assert.Equal(VmTOperLogic1179Slice.SvtFixed, sum.Type);
        Assert.Equal(65536, sum.Fixed);

        frame.Evaluate = (into, start, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtInt;
            into.Int = start == 0 ? 2 : 3;
        };
        VmTOperLogic1179Slice.PlusOp(frame, sum, 0, 1, 2);
        Assert.Equal(VmTOperLogic1179Slice.SvtInt, sum.Type);
        Assert.Equal(5, sum.Int);

        frame.Spans.Clear();
        frame.Evaluate = (into, _, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtFixed;
            into.Fixed = 32768;
        };
        var difference = new VmTOperLogic1179Slice.Value();
        VmTOperLogic1179Slice.MinusOp(frame, difference, 1, 1, 2);
        Assert.Equal(new[] { (2, 2) }, frame.Spans);
        Assert.Equal(VmTOperLogic1179Slice.SvtFixed, difference.Type);
        Assert.Equal(-32768, difference.Fixed);

        frame.Evaluate = (into, start, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtInt;
            into.Int = start == 0 ? 5 : 2;
        };
        VmTOperLogic1179Slice.MinusOp(frame, difference, 0, 1, 2);
        Assert.Equal(VmTOperLogic1179Slice.SvtInt, difference.Type);
        Assert.Equal(3, difference.Int);

        frame.Evaluate = (into, start, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtFixed;
            into.Fixed = start == 0 ? 32768 : 16384;
        };
        var product = new VmTOperLogic1179Slice.Value();
        VmTOperLogic1179Slice.MultiplyOp(frame, product, 0, 1, 2);
        Assert.Equal(VmTOperLogic1179Slice.SvtFixed, product.Type);
        Assert.Equal(8192, product.Fixed);

        frame.Evaluate = (into, start, _) =>
        {
            into.Type = VmTOperLogic1179Slice.SvtInt;
            into.Int = start == 0 ? 6 : 7;
        };
        VmTOperLogic1179Slice.MultiplyOp(frame, product, 0, 1, 2);
        Assert.Equal(VmTOperLogic1179Slice.SvtInt, product.Type);
        Assert.Equal(42, product.Int);
    }
}
