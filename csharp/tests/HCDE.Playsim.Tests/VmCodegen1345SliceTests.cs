using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmCodegen1345SliceTests
{
    [Fact]
    public void CompareSizeAndPreIncrement()
    {
        var ctx = new CompileCtx1345();
        var operand = new Expr1345();
        var booleanNot = new VmUnaryNot1345 { Operand = operand };
        var yes = new List<int> { 3, 4 };
        var no = new List<int> { 9 };
        var compareBuild = new VmBuilder1345();
        booleanNot.EmitCompare(compareBuild, false, yes, no);
        Assert.True(operand.SeenInvert);
        Assert.Same(yes, operand.SeenYes);
        Assert.Same(no, operand.SeenNo);
        booleanNot.EmitCompare(compareBuild, true, yes, no);
        Assert.False(operand.SeenInvert);
        Assert.Same(yes, operand.SeenYes);

        var constant = new Expr1345
        {
            Constant = true,
            ValueType = new Type1345 { Size = 4, Align = 8 },
        };
        var sizeofConstant = new VmSizeAlign1345(constant, VmCodegen1345Slice.TkSizeOf);
        Assert.Null(sizeofConstant.Resolve(ctx));
        Only(sizeofConstant.Log, "cannot determine size of a constant");
        Assert.Equal(0, constant.AddressCount);
        Assert.True(constant.Deleted);
        Assert.True(sizeofConstant.Deleted);
        Assert.Same(sizeofConstant, sizeofConstant.Resolve(ctx));
        Assert.Equal(1, constant.ResolveCount);
        Assert.Single(sizeofConstant.Log.Notes);

        var alignConstant = new Expr1345
        {
            Constant = true,
            ValueType = new Type1345 { Size = 4, Align = 8 },
        };
        var alignofConstant = new VmSizeAlign1345(alignConstant, VmCodegen1345Slice.TkAlignOf);
        Assert.Null(alignofConstant.Resolve(ctx));
        Only(alignofConstant.Log, "cannot determine alignment of a constant");
        Assert.Equal(0, alignConstant.AddressCount);

        var blocked = new AddrProbe1345
        {
            ValueType = new Type1345 { Size = 4, Align = 8 },
            AddressResult = false,
        };
        var alignofBlocked = new VmSizeAlign1345(blocked, VmCodegen1345Slice.TkAlignOf);
        Assert.Null(alignofBlocked.Resolve(ctx));
        Only(alignofBlocked.Log, "Operand must be addressable to determine alignment");
        Assert.Null(blocked.SeenWritable);
        Assert.Equal(1, blocked.AddressCount);
        Assert.True(blocked.Deleted);

        var sized = new AddrProbe1345
        {
            ValueType = new Type1345 { Size = 12, Align = 8 },
        };
        var sizeofValue = new VmSizeAlign1345(sized, VmCodegen1345Slice.TkSizeOf);
        var sizeConstant = Assert.IsType<Constant1345>(sizeofValue.Resolve(ctx));
        Assert.Equal(12, sizeConstant.IntValue);
        Assert.True(sizeConstant.Constant);
        Assert.True(sizeConstant.Resolved);
        Assert.Equal(1, sizeConstant.ResolveCount);
        Assert.True(sized.Deleted);
        Assert.Null(sized.SeenWritable);
        Assert.Same(sizeofValue, sizeofValue.Resolve(ctx));
        Assert.Equal(1, sized.ResolveCount);

        var aligned = new AddrProbe1345
        {
            ValueType = new Type1345 { Size = 12, Align = 8 },
        };
        var alignConstantResult = Assert.IsType<Constant1345>(
            new VmSizeAlign1345(aligned, VmCodegen1345Slice.TkAlignOf).Resolve(ctx));
        Assert.Equal(8, alignConstantResult.IntValue);

        var other = new AddrProbe1345
        {
            ValueType = new Type1345 { Size = 24, Align = 8 },
        };
        var otherConstant = Assert.IsType<Constant1345>(new VmSizeAlign1345(other, 0).Resolve(ctx));
        Assert.Equal(24, otherConstant.IntValue);
        Assert.Null(other.SeenWritable);

        var missing = new Expr1345();
        missing.Resolver = _ =>
        {
            missing.Delete();
            return null;
        };
        var missingSize = new VmSizeAlign1345(missing, VmCodegen1345Slice.TkSizeOf);
        Assert.Null(missingSize.Resolve(ctx));
        Assert.Empty(missingSize.Log.Notes);
        Assert.True(missingSize.Deleted);
        Assert.True(missing.Deleted);

        var nil = new VmSizeAlign1345(new Expr1345(), VmCodegen1345Slice.TkSizeOf).Emit(new VmBuilder1345());
        Assert.Equal(VmCodegen1345Slice.RegNil, nil.RegType);
        Assert.Equal(0, nil.RegNum);
        Assert.Equal(1, nil.RegCount);
        Assert.False(nil.Konst);
        Assert.False(nil.Fixed);
        Assert.False(nil.Target);
        Assert.False(nil.Freed);

        var early = new VmPreIncr1345(new Expr1345(), VmCodegen1345Slice.TkIncr);
        var earlyBox = new WritableBox1345 { Value = true };
        Assert.True(early.RequestAddress(ctx, earlyBox));
        Assert.False(earlyBox.Value);
        Assert.False(early.AddressWritable);
        Assert.True(early.AddressRequested);

        var text = new Expr1345 { ValueType = new Type1345 { Numeric = false } };
        var textPre = new VmPreIncr1345(text, VmCodegen1345Slice.TkIncr);
        Assert.Null(textPre.Resolve(ctx));
        Only(textPre.Log, "Numeric type expected");
        Assert.Equal(0, text.AddressCount);
        Assert.True(text.Deleted);

        var flag = new Expr1345 { ValueType = new Type1345 { Numeric = true, Bool = true } };
        var flagPre = new VmPreIncr1345(flag, VmCodegen1345Slice.TkDecr);
        Assert.Null(flagPre.Resolve(ctx));
        Only(flagPre.Log, "'--' is not allowed on type bool");
        Assert.Equal(0, flag.AddressCount);

        var locked = new AddrProbe1345
        {
            ValueType = IntType(),
            AddressResult = false,
            WriteValue = true,
        };
        var lockedPre = new VmPreIncr1345(locked, VmCodegen1345Slice.TkIncr);
        Assert.Null(lockedPre.Resolve(ctx));
        Only(lockedPre.Log, "Expression must be a modifiable value");
        Assert.True(lockedPre.AddressWritable);
        Assert.Equal(1, locked.AddressCount);
        Assert.True(locked.Deleted);

        var cell = new AddrProbe1345 { ValueType = IntType() };
        var cellPre = new VmPreIncr1345(cell, VmCodegen1345Slice.TkIncr);
        Assert.Same(cellPre, cellPre.Resolve(ctx));
        Assert.True(cellPre.AddressWritable);
        Assert.Equal(1, cell.AddressCount);
        var reported = new WritableBox1345();
        Assert.True(cellPre.RequestAddress(ctx, reported));
        Assert.True(reported.Value);
        Assert.Same(cellPre, cellPre.Resolve(ctx));
        Assert.Equal(1, cell.ResolveCount);
        Assert.Equal(1, cell.AddressCount);

        var memory = new AddrProbe1345
        {
            ValueType = IntType(),
            Produced = new Emit1345 { RegNum = 5, RegType = VmCodegen1345Slice.RegInt, Target = false },
        };
        var memoryPre = new VmPreIncr1345(memory, VmCodegen1345Slice.TkIncr);
        Assert.Same(memoryPre, memoryPre.Resolve(ctx));
        var memoryBuild = new VmBuilder1345();
        var memoryValue = memoryPre.Emit(memoryBuild);
        Assert.Equal(new[] { 0 }, memoryBuild.IntConstants);
        Assert.Empty(memoryBuild.FloatConstants);
        Assert.Equal(3, memoryBuild.Code.Count);
        Op(memoryBuild, 0, "LW", 0, 5, 0);
        Op(memoryBuild, 1, "ADDI", 0, 0, 1);
        Op(memoryBuild, 2, "SW", 5, 0, 0);
        Assert.NotSame(memory.Produced, memoryValue);
        Assert.Equal(0, memoryValue.RegNum);
        Assert.Equal(VmCodegen1345Slice.RegInt, memoryValue.RegType);
        Assert.False(memoryValue.Target);
        Assert.False(memoryValue.Freed);
        Assert.True(memory.Produced.Freed);

        var decrMemory = new AddrProbe1345
        {
            ValueType = IntType(),
            Produced = new Emit1345 { RegNum = 6, RegType = VmCodegen1345Slice.RegInt, Target = false },
        };
        var decrPre = new VmPreIncr1345(decrMemory, VmCodegen1345Slice.TkDecr);
        decrPre.Resolve(ctx);
        var decrBuild = new VmBuilder1345();
        decrPre.Emit(decrBuild);
        Op(decrBuild, 1, "ADDI", 0, 0, 255);
        Assert.Equal(new[] { 0 }, decrBuild.IntConstants);

        var register = new AddrProbe1345
        {
            ValueType = IntType(),
            Produced = new Emit1345 { RegNum = 4, RegType = VmCodegen1345Slice.RegInt, Target = true },
        };
        var registerPre = new VmPreIncr1345(register, VmCodegen1345Slice.TkIncr);
        registerPre.Resolve(ctx);
        Assert.True(registerPre.RequestAddress(ctx, new WritableBox1345()));
        var registerBuild = new VmBuilder1345();
        var registerValue = registerPre.Emit(registerBuild);
        Assert.Same(register.Produced, registerValue);
        Assert.False(register.Produced.Freed);
        Assert.True(register.Produced.Target);
        Assert.Equal(new[] { 0 }, registerBuild.IntConstants);
        Assert.Single(registerBuild.Code);
        Op(registerBuild, 0, "ADDI", 4, 4, 1);
        Assert.Equal((VmCodegen1345Slice.RegInt, 4), Assert.Single(registerBuild.Released));

        var scalar = new AddrProbe1345
        {
            ValueType = FloatType(),
            Produced = new Emit1345 { RegNum = 2, RegType = VmCodegen1345Slice.RegFloat, Target = true },
        };
        var scalarPre = new VmPreIncr1345(scalar, VmCodegen1345Slice.TkDecr);
        scalarPre.Resolve(ctx);
        var scalarBuild = new VmBuilder1345();
        var scalarValue = scalarPre.Emit(scalarBuild);
        Assert.NotSame(scalar.Produced, scalarValue);
        Assert.True(scalar.Produced.Freed);
        Assert.False(scalarValue.Freed);
        Assert.False(scalarValue.Target);
        Assert.Equal(2, scalarValue.RegNum);
        Assert.Equal(VmCodegen1345Slice.RegFloat, scalarValue.RegType);
        Assert.Equal(new[] { 0 }, scalarBuild.IntConstants);
        Assert.Equal(new[] { 1.0 }, scalarBuild.FloatConstants);
        Op(scalarBuild, 0, "SUBF_RK", 2, 2, 0);

        var bad = new VmPreIncr1345(new Expr1345(), 9) { ValueType = IntType() };
        var badBuild = new VmBuilder1345();
        var badError = Assert.Throws<InvalidOperationException>(() => bad.Emit(badBuild));
        Assert.Equal("token", badError.Message);
        Assert.Empty(badBuild.Code);
        Assert.Empty(badBuild.IntConstants);
    }

    [Fact]
    public void PostIncrementAndAssign()
    {
        var ctx = new CompileCtx1345();
        var text = new Expr1345 { ValueType = new Type1345 { Numeric = false } };
        var textPost = new VmPostIncr1345(text, VmCodegen1345Slice.TkIncr);
        Assert.Null(textPost.Resolve(ctx));
        Only(textPost.Log, "Numeric type expected");
        Assert.Equal(0, text.AddressCount);

        var flag = new Expr1345 { ValueType = new Type1345 { Numeric = true, Bool = true } };
        var flagPost = new VmPostIncr1345(flag, VmCodegen1345Slice.TkIncr);
        Assert.Null(flagPost.Resolve(ctx));
        Only(flagPost.Log, "'++' is not allowed on type bool");
        Assert.Equal(0, flag.AddressCount);

        var frozen = new AddrProbe1345
        {
            ValueType = IntType(),
            AddressResult = true,
            WriteValue = false,
        };
        var frozenPost = new VmPostIncr1345(frozen, VmCodegen1345Slice.TkDecr);
        Assert.Null(frozenPost.Resolve(ctx));
        Only(frozenPost.Log, "Expression must be a modifiable value");
        Assert.Equal(1, frozen.AddressCount);
        Assert.True(frozen.Deleted);

        var memory = new AddrProbe1345
        {
            ValueType = IntType(),
            Produced = new Emit1345 { RegNum = 5, RegType = VmCodegen1345Slice.RegInt, Target = false },
        };
        var memoryPost = new VmPostIncr1345(memory, VmCodegen1345Slice.TkIncr) { NeedResult = false };
        Assert.Same(memoryPost, memoryPost.Resolve(ctx));
        Assert.True(memoryPost.NeedResult == false);
        var memoryBuild = new VmBuilder1345();
        var memoryValue = memoryPost.Emit(memoryBuild);
        Assert.Equal(new[] { 0 }, memoryBuild.IntConstants);
        Assert.Empty(memoryBuild.FloatConstants);
        Assert.Equal(3, memoryBuild.Code.Count);
        Op(memoryBuild, 0, "LW", 0, 5, 0);
        Op(memoryBuild, 1, "ADDI", 1, 0, 1);
        Op(memoryBuild, 2, "SW", 5, 1, 0);
        Assert.Equal(0, memoryValue.RegNum);
        Assert.Equal(VmCodegen1345Slice.RegInt, memoryValue.RegType);
        Assert.False(memoryValue.Freed);
        Assert.NotEqual(VmCodegen1345Slice.RegNil, memoryValue.RegType);
        Assert.True(memory.Produced.Freed);
        Assert.Contains(memoryBuild.Released, pair => pair == (VmCodegen1345Slice.RegInt, 1));

        var floatMemory = new AddrProbe1345
        {
            ValueType = FloatType(),
            Produced = new Emit1345 { RegNum = 7, RegType = VmCodegen1345Slice.RegFloat, Target = false },
        };
        var floatPost = new VmPostIncr1345(floatMemory, VmCodegen1345Slice.TkDecr) { NeedResult = false };
        floatPost.Resolve(ctx);
        var floatBuild = new VmBuilder1345();
        var floatValue = floatPost.Emit(floatBuild);
        Op(floatBuild, 0, "LF", 0, 7, 0);
        Op(floatBuild, 1, "SUBF_RK", 1, 0, 0);
        Op(floatBuild, 2, "SF", 7, 1, 0);
        Assert.Equal(0, floatValue.RegNum);
        Assert.Equal(VmCodegen1345Slice.RegFloat, floatValue.RegType);
        Assert.False(floatValue.Freed);
        Assert.Equal(new[] { 1.0 }, floatBuild.FloatConstants);
        Assert.Equal(new[] { 0 }, floatBuild.IntConstants);

        var register = new AddrProbe1345
        {
            ValueType = IntType(),
            Produced = new Emit1345 { RegNum = 4, RegType = VmCodegen1345Slice.RegInt, Target = true },
        };
        var registerPost = new VmPostIncr1345(register, VmCodegen1345Slice.TkIncr);
        Assert.True(registerPost.NeedResult);
        registerPost.Resolve(ctx);
        var registerBuild = new VmBuilder1345();
        var registerValue = registerPost.Emit(registerBuild);
        Assert.NotSame(register.Produced, registerValue);
        Assert.Equal(0, registerValue.RegNum);
        Assert.False(registerValue.Freed);
        Assert.True(register.Produced.Freed);
        Op(registerBuild, 0, "MOVE", 0, 4, 0);
        Op(registerBuild, 1, "ADDI", 4, 4, 1);

        var floatRegister = new AddrProbe1345
        {
            ValueType = FloatType(),
            Produced = new Emit1345 { RegNum = 3, RegType = VmCodegen1345Slice.RegFloat, Target = true },
        };
        var floatRegisterPost = new VmPostIncr1345(floatRegister, VmCodegen1345Slice.TkIncr);
        floatRegisterPost.Resolve(ctx);
        var floatRegisterBuild = new VmBuilder1345();
        var floatRegisterValue = floatRegisterPost.Emit(floatRegisterBuild);
        Assert.Equal(0, floatRegisterValue.RegNum);
        Assert.Equal(VmCodegen1345Slice.RegFloat, floatRegisterValue.RegType);
        Op(floatRegisterBuild, 0, "MOVEF", 0, 3, 0);
        Op(floatRegisterBuild, 1, "ADDF_RK", 3, 3, 0);
        Assert.Equal(new[] { 1.0 }, floatRegisterBuild.FloatConstants);

        var quiet = new AddrProbe1345
        {
            ValueType = IntType(),
            Produced = new Emit1345 { RegNum = 8, RegType = VmCodegen1345Slice.RegInt, Target = true },
        };
        var quietPost = new VmPostIncr1345(quiet, VmCodegen1345Slice.TkDecr) { NeedResult = false };
        quietPost.Resolve(ctx);
        var quietBuild = new VmBuilder1345();
        var quietValue = quietPost.Emit(quietBuild);
        Op(quietBuild, 0, "ADDI", 8, 8, 255);
        Assert.Equal(VmCodegen1345Slice.RegNil, quietValue.RegType);
        Assert.Equal(0, quietValue.RegNum);
        Assert.NotSame(quiet.Produced, quietValue);
        Assert.True(quiet.Produced.Freed);
        Assert.Equal(new[] { 0 }, quietBuild.IntConstants);
        Assert.Empty(quietBuild.FloatConstants);

        var bad = new VmPostIncr1345(new Expr1345(), 9) { ValueType = IntType() };
        var badError = Assert.Throws<InvalidOperationException>(() => bad.Emit(new VmBuilder1345()));
        Assert.Equal("token", badError.Message);

        var left = new Expr1345();
        var right = new Expr1345();
        var assign = new VmAssign1345(left, right);
        Assert.Equal(-1, assign.IsBitWrite);
        Assert.False(assign.IsModifyAssign);
        Assert.False(assign.AddressRequested);
        Assert.False(assign.AddressWritable);
        var box = new WritableBox1345 { Value = true };
        Assert.False(assign.RequestAddress(ctx, box));
        Assert.False(box.Value);
        Assert.False(assign.AddressRequested);
        Assert.Equal(0, left.AddressCount);
        Assert.Equal(0, right.AddressCount);
        assign.Delete();
        Assert.True(assign.Deleted);
        Assert.True(left.Deleted);
        Assert.True(right.Deleted);
        Assert.Null(assign.BaseExpr);
        Assert.Null(assign.Right);

        var modify = new VmAssign1345(new Expr1345(), new Expr1345(), true);
        Assert.True(modify.IsModifyAssign);
        Assert.Equal(-1, modify.IsBitWrite);
    }

    static Type1345 IntType()
    {
        return new Type1345
        {
            Numeric = true,
            RegType = VmCodegen1345Slice.RegInt,
            LoadOp = "LW",
            StoreOp = "SW",
            Size = 4,
            Align = 4,
        };
    }

    static Type1345 FloatType()
    {
        return new Type1345
        {
            Numeric = true,
            RegType = VmCodegen1345Slice.RegFloat,
            LoadOp = "LF",
            StoreOp = "SF",
            Size = 8,
            Align = 8,
        };
    }

    static void Only(MessageLog1345 log, string text)
    {
        var note = Assert.Single(log.Notes);
        Assert.Equal("error", note.Level);
        Assert.Equal(text, note.Text);
    }

    static void Op(VmBuilder1345 build, int index, string name, int a, int b, int c)
    {
        var op = build.Code[index];
        Assert.Equal(name, op.Name);
        Assert.Equal(a, op.A);
        Assert.Equal(b, op.B);
        Assert.Equal(c, op.C);
    }

    sealed class AddrProbe1345 : Expr1345
    {
        public bool AddressResult = true;
        public bool WriteValue = true;
        public WritableBox1345? SeenWritable;
        public Emit1345 Produced = new();

        public override bool RequestAddress(CompileCtx1345 ctx, WritableBox1345? writable)
        {
            AddressCount++;
            SeenWritable = writable;
            if (writable != null)
                writable.Value = WriteValue;
            return AddressResult;
        }

        public override Emit1345 Emit(VmBuilder1345 build)
        {
            return Produced;
        }
    }
}
