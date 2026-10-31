using HCDE.Scripting;

namespace HCDE.Scripting.Tests;

public class VmExecWiring1359SliceTests
{
    [Fact]
    public void EnginesFillAndAssert()
    {
        foreach (var ndebug in new[] { false, true })
        {
            var built = new VmExecWiring1359Slice(ndebug);
            Assert.Equal(new[] { "checked", "unchecked" }, built.Includes);
            Assert.True(built.Checked.Asserts);
            Assert.False(built.Unchecked.Asserts);
            Assert.Same(ndebug ? built.Unchecked : built.Checked, built.Current);
            Assert.Empty(built.Writes);
            Assert.Equal(10, built.VmCycles.Length);
            Assert.Equal(10, built.VmCalls.Length);
            Assert.NotSame(built.VmCycles, built.VmCalls);
            Assert.All(built.VmCycles, value => Assert.Equal(0, value));
            Assert.All(built.VmCalls, value => Assert.Equal(0, value));
        }

        var debug = new VmExecWiring1359Slice(false);
        debug.SelectEngine(VmExecWiring1359Slice.EngineDefault);
        Assert.Same(debug.Checked, Assert.Single(debug.Writes));
        Assert.Same(debug.Checked, debug.Current);
        debug.SelectEngine(VmExecWiring1359Slice.EngineUnchecked);
        Assert.Same(debug.Unchecked, debug.Writes[1]);
        Assert.Same(debug.Unchecked, debug.Current);
        debug.SelectEngine(99);
        debug.SelectEngine(-1);
        Assert.Equal(2, debug.Writes.Count);
        Assert.Same(debug.Unchecked, debug.Current);
        debug.SelectEngine(VmExecWiring1359Slice.EngineChecked);
        Assert.Same(debug.Checked, debug.Current);

        var release = new VmExecWiring1359Slice(true);
        release.SelectEngine(VmExecWiring1359Slice.EngineDefault);
        Assert.Same(release.Unchecked, release.Writes[0]);
        Assert.Same(release.Checked, release.Writes[1]);
        Assert.Equal(2, release.Writes.Count);
        Assert.Same(release.Checked, release.Current);
        release.SelectEngine(VmExecWiring1359Slice.EngineUnchecked);
        Assert.Same(release.Unchecked, release.Current);
        Assert.Equal(3, release.Writes.Count);

        Assert.Throws<NullReferenceException>(() => debug.FillParams(null, null, 0));
        var bare = Host(null);
        var missing = Assert.Throws<InvalidOperationException>(() => debug.FillParams(null, bare, 0));
        Assert.Equal(VmExecWiring1359Slice.AssertCallee, missing.Message);
        Assert.Equal(1, bare.RegisterBinds);
        var releasedBare = Host(null);
        var releasedMissing = Assert.Throws<NullReferenceException>(() => release.FillParams(null, releasedBare, 1));
        Assert.Equal("calleefunc", releasedMissing.Message);
        Assert.Equal(1, releasedBare.RegisterBinds);

        var native = ScriptOf(2);
        native.Flags = VmExecWiring1359Slice.VarfNative | 1;
        var nativeFrame = Host(native);
        var nativeEx = Assert.Throws<InvalidOperationException>(() => debug.FillParams(null, nativeFrame, 0));
        Assert.Equal(VmExecWiring1359Slice.AssertCallee, nativeEx.Message);
        Assert.Equal(1, native.FlagReads);
        Assert.Equal(0, native.NumArgsReads);
        Assert.Equal(1, nativeFrame.RegisterBinds);

        var mismatch = ScriptOf(2);
        var mismatchFrame = Host(mismatch);
        var countEx = Assert.Throws<InvalidOperationException>(() => debug.FillParams(null, mismatchFrame, 0));
        Assert.Equal(VmExecWiring1359Slice.AssertCount, countEx.Message);
        Assert.Equal(1, mismatch.NumArgsReads);
        Assert.Equal(0, mismatchFrame.D[0]);

        var noTypes = ScriptOf(0);
        noTypes.Types = null;
        var noTypeFrame = Host(noTypes);
        var infoEx = Assert.Throws<InvalidOperationException>(() => debug.FillParams(null, noTypeFrame, 0));
        Assert.Equal(VmExecWiring1359Slice.AssertRegInfo, infoEx.Message);
        Assert.Equal(1, noTypes.NumArgsReads);

        var releaseEmpty = ScriptOf(0);
        releaseEmpty.Types = null;
        release.FillParams(null, Host(releaseEmpty), 9);
        Assert.Equal(0, releaseEmpty.FlagReads);
        Assert.Equal(1, releaseEmpty.NumArgsReads);

        var banks = ScriptOf(3, 0, 1, 0);
        var marker = new object();
        var parms = new[]
        {
            new VmExecWiring1359Slice.Value(4, 1.5, null, null),
            new VmExecWiring1359Slice.Value(99, 2.5, null, null),
            new VmExecWiring1359Slice.Value(8, 3.5, null, marker),
        };
        var bankFrame = Host(banks, 2, 1, 1, 1);
        debug.FillParams(parms, bankFrame, 3);
        Assert.Equal(4, bankFrame.D[0]);
        Assert.Equal(8, bankFrame.D[1]);
        Assert.Equal(2.5, bankFrame.F[0]);
        Assert.Equal(1, banks.FlagReads);
        Assert.Equal(5, banks.NumArgsReads);
        Assert.All(debug.VmCalls, value => Assert.Equal(0, value));

        var text = new VmExecWiring1359Slice.Text("gun");
        var strings = ScriptOf(1, VmExecWiring1359Slice.RegtString);
        var stringFrame = Host(strings, s: 1);
        debug.FillParams(new[] { new VmExecWiring1359Slice.Value(0, 0, text, null) }, stringFrame, 1);
        text.Value = "mutated";
        Assert.Equal("gun", stringFrame.S[0]!.Value);
        Assert.NotSame(text, stringFrame.S[0]);

        var nullSp = ScriptOf(2, VmExecWiring1359Slice.RegtString, VmExecWiring1359Slice.RegtString);
        var nullSpFrame = Host(nullSp, s: 0);
        var spEx = Assert.Throws<NullReferenceException>(() => debug.FillParams(new[]
        {
            new VmExecWiring1359Slice.Value(0, 0, null, null),
            new VmExecWiring1359Slice.Value(0, 0, new VmExecWiring1359Slice.Text("x"), null),
        }, nullSpFrame, 2));
        Assert.Equal("sp", spEx.Message);

        var tight = ScriptOf(1, VmExecWiring1359Slice.RegtString);
        var tightEx = Assert.Throws<IndexOutOfRangeException>(() => debug.FillParams(
            new[] { new VmExecWiring1359Slice.Value(0, 0, new VmExecWiring1359Slice.Text("z"), null) },
            Host(tight, s: 0), 1));
        Assert.Equal("s", tightEx.Message);

        var konst = ScriptOf(2, VmExecWiring1359Slice.RegtInt, VmExecWiring1359Slice.RegtPointer + 1);
        var konstFrame = Host(konst, 1, 0, 0, 1);
        var pointer = new object();
        var konstEx = Assert.Throws<InvalidOperationException>(() => debug.FillParams(new[]
        {
            new VmExecWiring1359Slice.Value(5, 0, null, null),
            new VmExecWiring1359Slice.Value(7, 0, null, pointer),
        }, konstFrame, 2));
        Assert.Equal(VmExecWiring1359Slice.AssertPointer, konstEx.Message);
        Assert.Equal(5, konstFrame.D[0]);
        Assert.Null(konstFrame.A[0]);

        var stored = ScriptOf(1, 4);
        var storedFrame = Host(stored, a: 1);
        var box = new object();
        release.FillParams(new[] { new VmExecWiring1359Slice.Value(5, 0, null, box) }, storedFrame, 0);
        Assert.Same(box, storedFrame.A[0]);
        Assert.Equal(0, stored.FlagReads);

        var nil = ScriptOf(1, 128);
        var nilEx = Assert.Throws<InvalidOperationException>(() => debug.FillParams(
            new[] { new VmExecWiring1359Slice.Value(1, 0, null, box) }, Host(nil, a: 1), 1));
        Assert.Equal(VmExecWiring1359Slice.AssertPointer, nilEx.Message);

        var shortTypes = ScriptOf(2, VmExecWiring1359Slice.RegtInt);
        var shortFrame = Host(shortTypes, 2, 0, 0, 0);
        var typeEx = Assert.Throws<IndexOutOfRangeException>(() => debug.FillParams(new[]
        {
            new VmExecWiring1359Slice.Value(3, 0, null, null),
            new VmExecWiring1359Slice.Value(4, 0, null, null),
        }, shortFrame, 2));
        Assert.Equal("RegTypes", typeEx.Message);
        Assert.Equal(3, shortFrame.D[0]);
        Assert.Equal(0, shortFrame.D[1]);

        var shortParms = ScriptOf(2, VmExecWiring1359Slice.RegtInt, VmExecWiring1359Slice.RegtInt);
        var shortParmFrame = Host(shortParms, 2, 0, 0, 0);
        var parmEx = Assert.Throws<IndexOutOfRangeException>(() => debug.FillParams(
            new[] { new VmExecWiring1359Slice.Value(6, 0, null, null) }, shortParmFrame, 2));
        Assert.Equal("params", parmEx.Message);
        Assert.Equal(6, shortParmFrame.D[0]);

        var bothMissing = ScriptOf(1);
        bothMissing.Types = null;
        var bothEx = Assert.Throws<NullReferenceException>(() => release.FillParams(null, Host(bothMissing), 0));
        Assert.Equal("params", bothEx.Message);
        var infoOnly = ScriptOf(1);
        infoOnly.Types = null;
        var infoOnlyEx = Assert.Throws<NullReferenceException>(() => release.FillParams(
            new[] { new VmExecWiring1359Slice.Value(1, 0, null, null) }, Host(infoOnly), 0));
        Assert.Equal("reginfo", infoOnlyEx.Message);

        var floatOnly = ScriptOf(1, VmExecWiring1359Slice.RegtFloat);
        var floatFrame = Host(floatOnly, 1, 0, 0, 0);
        var floatEx = Assert.Throws<IndexOutOfRangeException>(() => debug.FillParams(
            new[] { new VmExecWiring1359Slice.Value(5, 1.5, null, null) }, floatFrame, 1));
        Assert.Equal("f", floatEx.Message);
        Assert.Equal(0, floatFrame.D[0]);

        var other = ScriptOf(1, VmExecWiring1359Slice.RegtInt);
        other.Flags = 1;
        debug.FillParams(new[] { new VmExecWiring1359Slice.Value(2, 0, null, null) }, Host(other, 1, 0, 0, 0), 1);
        Assert.Equal(1, other.FlagReads);

        var alive = new VmExecWiring1359Slice.Obj();
        Assert.True(debug.AssertObject(null, false));
        Assert.True(debug.AssertObject(alive, false));
        Assert.Equal(1, alive.MagicReads);
        alive.MagicValue = 1;
        Assert.False(debug.AssertObject(alive, true));
        var blank = new VmExecWiring1359Slice.Obj { Readable = false, MagicValue = VmExecWiring1359Slice.MagicId };
        Assert.False(debug.AssertObject(blank, true));
        Assert.Equal(0, blank.MagicReads);
        var fault = Assert.Throws<InvalidOperationException>(() => debug.AssertObject(blank, false));
        Assert.Equal("MagicID", fault.Message);
        var hidden = Assert.Throws<InvalidOperationException>(() => release.AssertObject(null, true));
        Assert.Equal("AssertObject", hidden.Message);
        var good = new VmExecWiring1359Slice.Obj();
        var absent = Assert.Throws<InvalidOperationException>(() => release.AssertObject(good, false));
        Assert.Equal("AssertObject", absent.Message);
        Assert.Equal(0, good.MagicReads);

        LimitChecks(debug.Checked, debug.Unchecked);
    }

    [Fact]
    public void DispatchCastAndStack()
    {
        Assert.Null(typeof(VmExecWiring1359Slice).GetMethod("ThrowVmException"));
        Assert.Equal(92, VmExecWiring1359Slice.OpJmp);
        Assert.True(VmExecWiring1359Slice.ResolveCompGoto(null, true));
        Assert.False(VmExecWiring1359Slice.ResolveCompGoto(null, false));
        Assert.False(VmExecWiring1359Slice.ResolveCompGoto(false, true));
        Assert.True(VmExecWiring1359Slice.ResolveCompGoto(true, false));

        var word = new VmExecWiring1359Slice.Word(0xFF80FF80);
        Assert.Equal(0x80, word.Op);
        Assert.Equal(0xFF, word.A);
        Assert.Equal(0x80, word.B);
        Assert.Equal(0xFF, word.C);
        Assert.Equal(-1, word.Cs);
        Assert.Equal(65408, word.Bc);
        Assert.Equal(-128, word.Bcs);
        Assert.Equal(-32513, word.Abcs);
        Assert.Equal(30, VmExecWiring1359Slice.Kc(new[] { 10, 20, 30 }, new VmExecWiring1359Slice.Word(2u << 24)));
        Assert.Equal(8, VmExecWiring1359Slice.Rc(new[] { 8 }, new VmExecWiring1359Slice.Word(0)));
        Assert.Throws<IndexOutOfRangeException>(() => VmExecWiring1359Slice.Kc(new[] { 1 }, new VmExecWiring1359Slice.Word(1u << 24)));

        Assert.Equal(2, VmExecWiring1359Slice.NumMod(5, 3));
        Assert.Equal(1, VmExecWiring1359Slice.NumMod(-5, 3));
        Assert.Equal(-1, VmExecWiring1359Slice.NumMod(5, -3));
        Assert.Equal(-2, VmExecWiring1359Slice.NumMod(-5, -3));
        Assert.Equal(0.5, VmExecWiring1359Slice.NumMod(-1.5, 1));
        Assert.Equal(0, VmExecWiring1359Slice.NumMod(4.5, 1.5));
        Assert.Equal(0, VmExecWiring1359Slice.NumMod(-4.5, 1.5));
        Assert.Equal(0L, BitConverter.DoubleToInt64Bits(VmExecWiring1359Slice.NumMod(-0.0, 1)));
        Assert.True(double.IsNaN(VmExecWiring1359Slice.NumMod(1, 0)));
        Assert.True(double.IsNaN(VmExecWiring1359Slice.NumMod(0, 0)));

        var slot = new VmExecWiring1359Slice.Text("old");
        var wiring = new VmExecWiring1359Slice(false);
        wiring.ApplySpriteCast(slot, 0);
        Assert.Equal("0", slot.Value);
        wiring.ApplySpriteCast(slot, 2147483648);
        Assert.Equal("-2147483648", slot.Value);
        wiring.ApplySpriteCast(slot, 4294967295);
        Assert.Equal("-1", slot.Value);
        wiring.CastSpriteIdToString = (text, id) => text.Value = "s" + id.ToString();
        wiring.ApplySpriteCast(slot, 4);
        Assert.Equal("s4", slot.Value);
        wiring.CastSpriteIdToString = null;
        var dropped = Assert.Throws<NullReferenceException>(() => wiring.ApplySpriteCast(slot, 1));
        Assert.Equal("VM_CastSpriteIDToString", dropped.Message);
        Assert.Equal("s4", slot.Value);
        wiring.CastSpriteIdToString = VmExecWiring1359Slice.DefaultSpriteCast;
        var nullSlot = Assert.Throws<NullReferenceException>(() => wiring.ApplySpriteCast(null!, 3));
        Assert.Equal("a", nullSlot.Message);
        var both = new VmExecWiring1359Slice(false) { CastSpriteIdToString = null };
        var bothEx = Assert.Throws<NullReferenceException>(() => both.ApplySpriteCast(null!, 1));
        Assert.Equal("VM_CastSpriteIDToString", bothEx.Message);

        var plain = new VmExecWiring1359Slice(false);
        var pc = 4;
        plain.AdvanceSwitch(ref pc);
        Assert.Equal(5, pc);
        var stayed = pc;
        var switchEx = Assert.Throws<InvalidOperationException>(() => plain.AdvanceComputed(
            new[] { new VmExecWiring1359Slice.Word(1) }, ref stayed, (_, _, _, _) => { }, null, null, 1));
        Assert.Equal("COMPGOTO", switchEx.Message);
        Assert.Equal(5, stayed);

        var computed = new VmExecWiring1359Slice(false, gnu: true);
        Assert.True(computed.CompGoto);
        var held = 4;
        var holdEx = Assert.Throws<InvalidOperationException>(() => computed.AdvanceSwitch(ref held));
        Assert.Equal("COMPGOTO", holdEx.Message);
        Assert.Equal(4, held);
        var code = new[]
        {
            new VmExecWiring1359Slice.Word(1 | (2 << 8)),
            new VmExecWiring1359Slice.Word(3 | (4 << 8)),
        };
        var stepPc = 0;
        var seen = new List<(object? stack, object? ret, int numret, int pc)>();
        var ret = new object();
        var stack = VmExecWiring1359Slice.GlobalStack;
        var step = computed.AdvanceComputed(code, ref stepPc, (s, r, n, p) => seen.Add((s, r, n, p)), stack, ret, 6);
        Assert.Equal(1, stepPc);
        Assert.Equal(1, step.Pc);
        Assert.Equal(3, step.Op);
        Assert.Equal(4, step.A);
        var only = Assert.Single(seen);
        Assert.Same(stack, only.stack);
        Assert.Same(ret, only.ret);
        Assert.Equal(6, only.numret);
        Assert.Equal(1, only.pc);
        var bumped = 0;
        var seenPc = -1;
        var past = Assert.Throws<IndexOutOfRangeException>(() => computed.AdvanceComputed(
            new[] { new VmExecWiring1359Slice.Word(9) }, ref bumped, (_, _, _, p) => seenPc = p, null, null, 0));
        Assert.Equal("pc", past.Message);
        Assert.Equal(1, bumped);
        Assert.Equal(1, seenPc);
        var nullEmit = 0;
        var emitEx = Assert.Throws<NullReferenceException>(() => computed.AdvanceComputed(
            code, ref nullEmit, null, null, null, 0));
        Assert.Equal("EmitInstructionExecutionEvent", emitEx.Message);
        Assert.Equal(1, nullEmit);

        var jumpPc = 5;
        var program = new VmExecWiring1359Slice.Word[7];
        program[5] = new VmExecWiring1359Slice.Word(1 << 8);
        program[6] = new VmExecWiring1359Slice.Word(unchecked((uint)(((-3 & 0xFFFFFF) << 8) | VmExecWiring1359Slice.OpJmp)));
        Assert.Equal(3, plain.Checked.CmpJmp(program, jumpPc, true));
        program[5] = new VmExecWiring1359Slice.Word(3 << 8);
        Assert.Equal(3, plain.Checked.CmpJmp(program, jumpPc, true));
        program[5] = new VmExecWiring1359Slice.Word(2 << 8);
        Assert.Equal(6, plain.Checked.CmpJmp(program, jumpPc, true));
        Assert.Equal(3, plain.Checked.CmpJmp(program, jumpPc, false));
        program[5] = new VmExecWiring1359Slice.Word(1 << 8);
        Assert.Equal(6, plain.Unchecked.CmpJmp(program, jumpPc, false));
        program[6] = new VmExecWiring1359Slice.Word(5 << 8);
        Assert.Equal(11, plain.Unchecked.CmpJmp(program, jumpPc, true));
        var bad = Assert.Throws<InvalidOperationException>(() => plain.Checked.CmpJmp(program, jumpPc, true));
        Assert.Equal(VmExecWiring1359Slice.AssertJmp, bad.Message);
        var lone = new[] { new VmExecWiring1359Slice.Word(1 << 8) };
        Assert.Equal(1, plain.Checked.CmpJmp(lone, 0, false));
        Assert.Throws<IndexOutOfRangeException>(() => plain.Checked.CmpJmp(lone, 0, true));

        var ptr = 123;
        var read = Assert.Throws<VmExecWiring1359Slice.VmAbort>(() => VmExecWiring1359Slice.GetAddr(null, 4, 1, ref ptr));
        Assert.Equal(1, read.Reason);
        Assert.Equal(VmExecWiring1359Slice.VmAbort.MessageFor(1), read.Message);
        Assert.Equal(123, ptr);
        var landed = VmExecWiring1359Slice.GetAddr(new VmExecWiring1359Slice.Buffer(10), -4, 1, ref ptr);
        Assert.Equal(6, landed);
        Assert.Equal(6, ptr);
        Assert.Equal("VM execution aborted: ", VmExecWiring1359Slice.VmAbort.MessageFor(0));
        Assert.Equal("VM execution aborted: tried to write to address zero.", VmExecWiring1359Slice.VmAbort.MessageFor(2));
        Assert.Equal("VM execution aborted: too many try-catch blocks.", VmExecWiring1359Slice.VmAbort.MessageFor(3));
        Assert.Equal("VM execution aborted: array access out of bounds.", VmExecWiring1359Slice.VmAbort.MessageFor(4));
        Assert.Equal("VM execution aborted: division by zero.", VmExecWiring1359Slice.VmAbort.MessageFor(5));
        Assert.Equal("VM execution aborted: invalid self pointer.", VmExecWiring1359Slice.VmAbort.MessageFor(6));
        Assert.Equal("VM execution aborted: string format failed.", VmExecWiring1359Slice.VmAbort.MessageFor(7));
        Assert.Equal("VM execution aborted: Unknown reason 40", VmExecWiring1359Slice.VmAbort.MessageFor(40));
        Assert.Equal("VM execution aborted: Unknown reason -1", VmExecWiring1359Slice.VmAbort.MessageFor(-1));

        var here = VmExecWiring1359Slice.GlobalStack;
        Assert.Same(here, VmExecWiring1359Slice.GlobalStack);
        VmExecWiring1359Slice.FrameStack? there = null;
        var thread = new Thread(() => there = VmExecWiring1359Slice.GlobalStack);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(5)));
        Assert.NotNull(there);
        Assert.NotSame(here, there);
        Assert.Same(here, VmExecWiring1359Slice.GlobalStack);
    }

    static void LimitChecks(VmExecWiring1359Slice.Engine checks, VmExecWiring1359Slice.Engine open)
    {
        var low = Assert.Throws<InvalidOperationException>(() => checks.AssertD(-1, 4));
        Assert.Equal(VmExecWiring1359Slice.AssertLimitD, low.Message);
        checks.AssertD(3, 4);
        Assert.Throws<InvalidOperationException>(() => checks.AssertD(0, 0));
        var floatLimit = Assert.Throws<InvalidOperationException>(() => checks.AssertF(5, 5));
        Assert.Equal(VmExecWiring1359Slice.AssertLimitF, floatLimit.Message);
        checks.AssertF(4, 5);
        var addr = Assert.Throws<InvalidOperationException>(() => checks.AssertA(1, 1));
        Assert.Equal(VmExecWiring1359Slice.AssertLimitA, addr.Message);
        var str = Assert.Throws<InvalidOperationException>(() => checks.AssertS(2, 2));
        Assert.Equal(VmExecWiring1359Slice.AssertLimitS, str.Message);
        open.AssertD(-1, 0);
        open.AssertF(-1, 0);
        open.AssertA(-1, 0);
        open.AssertS(-1, 0);

        var script = new VmExecWiring1359Slice.Script { KonstCount = 2, NumKonstF = 0, NumKonstA = 1, NumKonstS = 3 };
        var missing = Assert.Throws<InvalidOperationException>(() => checks.AssertKd(null, 0));
        Assert.Equal(VmExecWiring1359Slice.AssertKonstD, missing.Message);
        Assert.Equal(0, script.KonstReads);
        checks.AssertKd(script, 1);
        Assert.Equal(1, script.KonstReads);
        var wide = Assert.Throws<InvalidOperationException>(() => checks.AssertKd(script, -1));
        Assert.Equal(VmExecWiring1359Slice.AssertKonstD, wide.Message);
        Assert.Equal(2, script.KonstReads);
        open.AssertKd(null, -1);
        open.AssertKd(script, 99);
        Assert.Equal(2, script.KonstReads);
        var kf = Assert.Throws<InvalidOperationException>(() => checks.AssertKf(script, 0));
        Assert.Equal(VmExecWiring1359Slice.AssertKonstF, kf.Message);
        Assert.Equal(2, script.KonstReads);
        checks.AssertKa(script, 0);
        var ks = Assert.Throws<InvalidOperationException>(() => checks.AssertKs(script, 3));
        Assert.Equal(VmExecWiring1359Slice.AssertKonstS, ks.Message);
        var ka = Assert.Throws<InvalidOperationException>(() => checks.AssertKa(null, 0));
        Assert.Equal(VmExecWiring1359Slice.AssertKonstA, ka.Message);
        open.AssertKf(null, 0);
        open.AssertKa(null, 0);
        open.AssertKs(script, 99);
    }

    static VmExecWiring1359Slice.Script ScriptOf(int args, params byte[] types)
    {
        return new VmExecWiring1359Slice.Script
        {
            ArgCount = args,
            Types = types,
        };
    }

    static VmExecWiring1359Slice.Frame Host(VmExecWiring1359Slice.Script? func, int d = 4, int f = 4, int s = 4, int a = 4)
    {
        var frame = new VmExecWiring1359Slice.Frame(d, f, s, a);
        frame.Func = func;
        return frame;
    }
}
