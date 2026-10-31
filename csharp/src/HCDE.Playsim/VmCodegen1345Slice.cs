namespace HCDE.Playsim;

/// <summary>
/// Boolean-not compare, <c>sizeof</c>/<c>alignof</c>, and pre/post increment
/// from <c>codegen.cpp</c> (2386-2684). Chunk 1345. Register allocation and the
/// constant pools are the builder surface those emits call. Assignment's address
/// request stays commented out.
/// </summary>
public static class VmCodegen1345Slice
{
    public const int TkIncr = 1;
    public const int TkDecr = 2;
    public const int TkSizeOf = 3;
    public const int TkAlignOf = 4;
    public const int RegInt = 0;
    public const int RegFloat = 1;
    public const int RegTypeMax = 3;
    public const int RegNil = 128;

    public static string TokenName(int token)
    {
        if (token == TkIncr)
            return "'++'";
        if (token == TkDecr)
            return "'--'";
        return $"Unknown({token})";
    }

    public static int LowByte(int token)
    {
        if (token == TkIncr)
            return 1;
        return unchecked((byte)-1);
    }
}

public sealed class Op1345
{
    public string Name = "";
    public int A;
    public int B;
    public int C;
}

public sealed class Emit1345
{
    public int RegNum;
    public int RegType = VmCodegen1345Slice.RegNil;
    public int RegCount = 1;
    public bool Konst;
    public bool Fixed;
    public bool Target;
    public bool Freed;

    public Emit1345 Copy()
    {
        return new Emit1345
        {
            RegNum = RegNum,
            RegType = RegType,
            RegCount = RegCount,
            Konst = Konst,
            Fixed = Fixed,
            Target = Target,
            Freed = Freed,
        };
    }

    public void Free(VmBuilder1345 build)
    {
        if (!Fixed && !Konst && RegType <= VmCodegen1345Slice.RegTypeMax)
        {
            build.Release(RegType, RegNum);
            Freed = true;
        }
    }
}

public sealed class VmBuilder1345
{
    public List<Op1345> Code = new();
    public List<int> IntConstants = new();
    public List<double> FloatConstants = new();
    public List<(int Type, int Reg)> Released = new();
    readonly HashSet<int>[] used = { new(), new(), new(), new() };

    public int GetConstantInt(int value)
    {
        var index = IntConstants.IndexOf(value);
        if (index >= 0)
            return index;
        IntConstants.Add(value);
        return IntConstants.Count - 1;
    }

    public int FindConstantInt(int index)
    {
        return IntConstants[index];
    }

    public int GetConstantFloat(double value)
    {
        for (var i = 0; i < FloatConstants.Count; i++)
        {
            if (FloatConstants[i] == value)
                return i;
        }

        FloatConstants.Add(value);
        return FloatConstants.Count - 1;
    }

    public Emit1345 Alloc(int type)
    {
        return new Emit1345 { RegNum = Acquire(type), RegType = type };
    }

    public void Op(string name, int a, int b, int c)
    {
        Code.Add(new Op1345 { Name = name, A = a, B = b, C = c });
    }

    public int Acquire(int type)
    {
        var taken = used[type];
        for (var reg = 0; reg < 256; reg++)
        {
            if (!taken.Contains(reg))
            {
                taken.Add(reg);
                return reg;
            }
        }

        throw new InvalidOperationException("registers");
    }

    public void Release(int type, int reg)
    {
        used[type].Remove(reg);
        Released.Add((type, reg));
    }
}

public sealed class Type1345
{
    public bool Numeric;
    public bool Bool;
    public int RegType;
    public string LoadOp = "";
    public string StoreOp = "";
    public int Align;
    public int Size;
}

public sealed class WritableBox1345
{
    public bool Value;
}

public sealed class CompileCtx1345
{
    public int Lump;
}

public sealed class ScriptNote1345
{
    public string Level = "";
    public string Text = "";
}

public sealed class MessageLog1345
{
    public List<ScriptNote1345> Notes = new();

    public void Add(string level, string text)
    {
        Notes.Add(new ScriptNote1345 { Level = level, Text = text });
    }
}

public class Expr1345
{
    public Type1345? ValueType;
    public bool Resolved;
    public bool Deleted;
    public bool Constant;
    public int ResolveCount;
    public int AddressCount;
    public Func<CompileCtx1345, Expr1345?>? Resolver;
    public Func<VmBuilder1345, Emit1345>? Emitter;
    public bool? SeenInvert;
    public List<int>? SeenYes;
    public List<int>? SeenNo;

    public virtual Expr1345? Resolve(CompileCtx1345 ctx)
    {
        ResolveCount++;
        if (Resolver != null)
            return Resolver(ctx);
        Resolved = true;
        return this;
    }

    public virtual bool RequestAddress(CompileCtx1345 ctx, WritableBox1345? writable)
    {
        AddressCount++;
        if (writable != null)
            writable.Value = false;
        return false;
    }

    public virtual void EmitCompare(VmBuilder1345 build, bool invert, List<int> yes, List<int> no)
    {
        SeenInvert = invert;
        SeenYes = yes;
        SeenNo = no;
    }

    public virtual Emit1345 Emit(VmBuilder1345 build)
    {
        if (Emitter != null)
            return Emitter(build);
        throw new InvalidOperationException("emit");
    }

    public virtual void Delete()
    {
        if (Deleted)
            return;
        Deleted = true;
    }
}

public sealed class Constant1345 : Expr1345
{
    public int IntValue;

    public Constant1345(int value)
    {
        IntValue = value;
        Constant = true;
    }
}

public sealed class VmUnaryNot1345
{
    public Expr1345 Operand = new();

    public void EmitCompare(VmBuilder1345 build, bool invert, List<int> yes, List<int> no)
    {
        Operand.EmitCompare(build, !invert, yes, no);
    }
}

public sealed class VmSizeAlign1345 : Expr1345
{
    public Expr1345? Operand;
    public int Which;
    public MessageLog1345 Log = new();

    public VmSizeAlign1345(Expr1345 operand, int which)
    {
        Operand = operand;
        Which = which;
    }

    public override void Delete()
    {
        if (Deleted)
            return;
        Deleted = true;
        if (Operand != null)
        {
            var owned = Operand;
            Operand = null;
            owned.Delete();
        }
    }

    public override Expr1345? Resolve(CompileCtx1345 ctx)
    {
        if (Resolved)
            return this;
        Resolved = true;
        if (Operand == null)
            throw new NullReferenceException("operand");
        var resolved = Operand.Resolve(ctx);
        Operand = resolved;
        if (resolved == null)
        {
            Delete();
            return null;
        }

        var type = resolved.ValueType;
        if (type == null)
            throw new NullReferenceException("value type");
        var kind = Which == VmCodegen1345Slice.TkAlignOf ? "alignment" : "size";
        if (resolved.Constant)
        {
            Log.Add("error", $"cannot determine {kind} of a constant");
            Delete();
            return null;
        }

        if (!resolved.RequestAddress(ctx, null))
        {
            Log.Add("error", $"Operand must be addressable to determine {kind}");
            Delete();
            return null;
        }

        var value = Which == VmCodegen1345Slice.TkAlignOf ? type.Align : type.Size;
        var created = new Constant1345(value);
        Delete();
        return created.Resolve(ctx);
    }

    public override Emit1345 Emit(VmBuilder1345 build)
    {
        return new Emit1345();
    }
}

public sealed class VmPreIncr1345 : Expr1345
{
    public int Token;
    public Expr1345? Base;
    public bool AddressRequested;
    public bool AddressWritable;
    public MessageLog1345 Log = new();

    public VmPreIncr1345(Expr1345 baseExpr, int token)
    {
        Token = token;
        Base = baseExpr;
        AddressRequested = false;
        AddressWritable = false;
    }

    public override void Delete()
    {
        if (Deleted)
            return;
        Deleted = true;
        if (Base != null)
        {
            var owned = Base;
            Base = null;
            owned.Delete();
        }
    }

    public override bool RequestAddress(CompileCtx1345 ctx, WritableBox1345? writable)
    {
        AddressRequested = true;
        if (writable != null)
            writable.Value = AddressWritable;
        return true;
    }

    public override Expr1345? Resolve(CompileCtx1345 ctx)
    {
        if (Resolved)
            return this;
        Resolved = true;
        if (Base == null)
            throw new NullReferenceException("base");
        var resolved = Base.Resolve(ctx);
        Base = resolved;
        if (resolved == null)
        {
            Delete();
            return null;
        }

        ValueType = resolved.ValueType;
        if (ValueType == null)
            throw new NullReferenceException("value type");
        if (!ValueType.Numeric)
        {
            Log.Add("error", "Numeric type expected");
            Delete();
            return null;
        }

        if (ValueType.Bool)
        {
            Log.Add("error", $"{VmCodegen1345Slice.TokenName(Token)} is not allowed on type bool");
            Delete();
            return null;
        }

        var slot = new WritableBox1345();
        if (!resolved.RequestAddress(ctx, slot) || !slot.Value)
        {
            AddressWritable = slot.Value;
            Log.Add("error", "Expression must be a modifiable value");
            Delete();
            return null;
        }

        AddressWritable = slot.Value;
        return this;
    }

    public override Emit1345 Emit(VmBuilder1345 build)
    {
        if (Token != VmCodegen1345Slice.TkIncr && Token != VmCodegen1345Slice.TkDecr)
            throw new InvalidOperationException("token");
        if (Base == null)
            throw new NullReferenceException("base");
        if (ValueType == null)
            throw new NullReferenceException("value type");
        var zero = build.GetConstantInt(0);
        var regtype = ValueType.RegType;
        var pointer = Base.Emit(build);
        var value = pointer.Copy();
        if (!pointer.Target)
        {
            value = build.Alloc(regtype);
            build.Op(ValueType.LoadOp, value.RegNum, pointer.RegNum, zero);
        }

        var imm = VmCodegen1345Slice.LowByte(Token);
        if (regtype == VmCodegen1345Slice.RegInt)
            build.Op("ADDI", value.RegNum, value.RegNum, imm);
        else
            build.Op(Token == VmCodegen1345Slice.TkIncr ? "ADDF_RK" : "SUBF_RK", value.RegNum, value.RegNum, build.GetConstantFloat(1.0));

        if (!pointer.Target)
            build.Op(ValueType.StoreOp, pointer.RegNum, value.RegNum, zero);
        if (AddressRequested)
        {
            value.Free(build);
            return pointer;
        }

        pointer.Free(build);
        value.Target = false;
        return value;
    }
}

public sealed class VmPostIncr1345 : Expr1345
{
    public int Token;
    public Expr1345? Base;
    public bool NeedResult = true;
    public MessageLog1345 Log = new();

    public VmPostIncr1345(Expr1345 baseExpr, int token)
    {
        Token = token;
        Base = baseExpr;
    }

    public override void Delete()
    {
        if (Deleted)
            return;
        Deleted = true;
        if (Base != null)
        {
            var owned = Base;
            Base = null;
            owned.Delete();
        }
    }

    public override Expr1345? Resolve(CompileCtx1345 ctx)
    {
        if (Resolved)
            return this;
        Resolved = true;
        if (Base == null)
            throw new NullReferenceException("base");
        var resolved = Base.Resolve(ctx);
        Base = resolved;
        if (resolved == null)
        {
            Delete();
            return null;
        }

        ValueType = resolved.ValueType;
        if (ValueType == null)
            throw new NullReferenceException("value type");
        if (!ValueType.Numeric)
        {
            Log.Add("error", "Numeric type expected");
            Delete();
            return null;
        }

        if (ValueType.Bool)
        {
            Log.Add("error", $"{VmCodegen1345Slice.TokenName(Token)} is not allowed on type bool");
            Delete();
            return null;
        }

        var slot = new WritableBox1345();
        if (!resolved.RequestAddress(ctx, slot) || !slot.Value)
        {
            Log.Add("error", "Expression must be a modifiable value");
            Delete();
            return null;
        }

        return this;
    }

    public override Emit1345 Emit(VmBuilder1345 build)
    {
        if (Token != VmCodegen1345Slice.TkIncr && Token != VmCodegen1345Slice.TkDecr)
            throw new InvalidOperationException("token");
        if (Base == null)
            throw new NullReferenceException("base");
        if (ValueType == null)
            throw new NullReferenceException("value type");
        var zero = build.GetConstantInt(0);
        var regtype = ValueType.RegType;
        var pointer = Base.Emit(build);
        var imm = VmCodegen1345Slice.LowByte(Token);
        if (!pointer.Target)
        {
            var outgoing = build.Alloc(regtype);
            build.Op(ValueType.LoadOp, outgoing.RegNum, pointer.RegNum, zero);
            var assign = build.Alloc(regtype);
            if (regtype == VmCodegen1345Slice.RegInt)
                build.Op("ADDI", assign.RegNum, outgoing.RegNum, imm);
            else
                build.Op(Token == VmCodegen1345Slice.TkIncr ? "ADDF_RK" : "SUBF_RK", assign.RegNum, outgoing.RegNum, build.GetConstantFloat(1.0));
            build.Op(ValueType.StoreOp, pointer.RegNum, assign.RegNum, zero);
            pointer.Free(build);
            assign.Free(build);
            return outgoing;
        }

        if (NeedResult)
        {
            var outgoing = build.Alloc(regtype);
            if (regtype == VmCodegen1345Slice.RegInt)
            {
                build.Op("MOVE", outgoing.RegNum, pointer.RegNum, 0);
                build.Op("ADDI", pointer.RegNum, pointer.RegNum, imm);
            }
            else
            {
                build.Op("MOVEF", outgoing.RegNum, pointer.RegNum, 0);
                build.Op(Token == VmCodegen1345Slice.TkIncr ? "ADDF_RK" : "SUBF_RK", pointer.RegNum, pointer.RegNum, build.GetConstantFloat(1.0));
            }

            pointer.Free(build);
            return outgoing;
        }

        if (regtype == VmCodegen1345Slice.RegInt)
            build.Op("ADDI", pointer.RegNum, pointer.RegNum, imm);
        else
            build.Op(Token == VmCodegen1345Slice.TkIncr ? "ADDF_RK" : "SUBF_RK", pointer.RegNum, pointer.RegNum, build.GetConstantFloat(1.0));
        pointer.Free(build);
        return new Emit1345();
    }
}

public sealed class VmAssign1345 : Expr1345
{
    public Expr1345? BaseExpr;
    public Expr1345? Right;
    public int IsBitWrite;
    public bool AddressRequested;
    public bool AddressWritable;
    public bool IsModifyAssign;

    public VmAssign1345(Expr1345 baseExpr, Expr1345 right, bool isModify = false)
    {
        BaseExpr = baseExpr;
        Right = right;
        IsBitWrite = -1;
        AddressRequested = false;
        AddressWritable = false;
        IsModifyAssign = isModify;
    }

    public override void Delete()
    {
        if (Deleted)
            return;
        Deleted = true;
        if (BaseExpr != null)
        {
            var owned = BaseExpr;
            BaseExpr = null;
            owned.Delete();
        }

        if (Right != null)
        {
            var owned = Right;
            Right = null;
            owned.Delete();
        }
    }
}
