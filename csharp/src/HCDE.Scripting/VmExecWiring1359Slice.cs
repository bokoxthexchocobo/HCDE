using System.Globalization;

namespace HCDE.Scripting;

/// <summary>
/// Wiring in <c>src/common/scripting/vm/vmexec.cpp</c> lines 1-248.
/// The two <c>#include "vmexec.h"</c> bodies stay outside this credit.
/// <c>VMEngine_Default</c> stores the checked exec after the <c>NDEBUG</c> store.
/// </summary>
public sealed class VmExecWiring1359Slice
{
    public const int RegtInt = 0;
    public const int RegtFloat = 1;
    public const int RegtString = 2;
    public const int RegtPointer = 3;
    public const int VarfNative = 1 << 3;
    public const int CmpCheck = 1;
    public const int OpJmp = 92;
    public const int EngineDefault = 0;
    public const int EngineUnchecked = 1;
    public const int EngineChecked = 2;
    public const uint MagicId = 0x1337cafe;

    public const string AssertCallee = "assert: calleefunc != NULL && !(calleefunc->VarFlags & VARF_Native)";
    public const string AssertCount = "assert: numparam == calleefunc->NumArgs";
    public const string AssertRegInfo = "assert: reginfo != nullptr";
    public const string AssertPointer = "assert: *reginfo == REGT_POINTER";
    public const string AssertJmp = "assert: pc[1].op == OP_JMP";
    public const string AssertLimitD = "assert: (unsigned)(x) < f->NumRegD";
    public const string AssertLimitF = "assert: (unsigned)(x) < f->NumRegF";
    public const string AssertLimitA = "assert: (unsigned)(x) < f->NumRegA";
    public const string AssertLimitS = "assert: (unsigned)(x) < f->NumRegS";
    public const string AssertKonstD = "assert: sfunc != NULL && (unsigned)(x) < sfunc->NumKonstD";
    public const string AssertKonstF = "assert: sfunc != NULL && (unsigned)(x) < sfunc->NumKonstF";
    public const string AssertKonstA = "assert: sfunc != NULL && (unsigned)(x) < sfunc->NumKonstA";
    public const string AssertKonstS = "assert: sfunc != NULL && (unsigned)(x) < sfunc->NumKonstS";

    static int _nextStack;
    static readonly ThreadLocal<FrameStack> Stacks = new(() => new FrameStack(Interlocked.Increment(ref _nextStack)));

    public bool TuNdebug { get; }
    public bool CompGoto { get; }
    public Engine Checked { get; }
    public Engine Unchecked { get; }
    public Engine Current { get; private set; }
    public List<string> Includes { get; } = new();
    public List<Engine> Writes { get; } = new();
    public int[] VmCycles { get; } = new int[10];
    public int[] VmCalls { get; } = new int[10];
    public SpriteCast? CastSpriteIdToString { get; set; }

    public delegate void SpriteCast(Text slot, uint sprite);
    public delegate void EmitInstruction(FrameStack? stack, object? ret, int numret, int pc);

    public VmExecWiring1359Slice(bool tuNdebug, bool? compgotoDefined = null, bool gnu = false)
    {
        TuNdebug = tuNdebug;
        CompGoto = ResolveCompGoto(compgotoDefined, gnu);
        CastSpriteIdToString = DefaultSpriteCast;
        Includes.Add("checked");
        Checked = new Engine(true);
        Includes.Add("unchecked");
        Unchecked = new Engine(false);
        Current = tuNdebug ? Unchecked : Checked;
    }

    public static FrameStack GlobalStack => Stacks.Value!;

    public static bool ResolveCompGoto(bool? defined, bool gnu)
    {
        if (defined is null) return gnu;
        return defined.Value;
    }

    public static double NumMod(double left, double right)
    {
        return left - Math.Floor(left / right) * right;
    }

    public static void DefaultSpriteCast(Text slot, uint sprite)
    {
        if (slot is null) throw new NullReferenceException("a");
        slot.Value = unchecked((int)sprite).ToString(CultureInfo.InvariantCulture);
    }

    public void ApplySpriteCast(Text slot, uint sprite)
    {
        var cast = CastSpriteIdToString;
        if (cast is null) throw new NullReferenceException("VM_CastSpriteIDToString");
        cast(slot, sprite);
    }

    public void SelectEngine(int engine)
    {
        switch (engine)
        {
            case EngineDefault:
                if (TuNdebug) Assign(Unchecked);
                Assign(Checked);
                break;
            case EngineUnchecked:
                Assign(Unchecked);
                break;
            case EngineChecked:
                Assign(Checked);
                break;
        }
    }

    public void FillParams(Value[]? parms, Frame? callee, int numparam)
    {
        if (callee is null) throw new NullReferenceException("callee");
        callee.BindRegisters();
        var func = callee.Func;
        if (func is null)
        {
            if (TuNdebug) throw new NullReferenceException("calleefunc");
            throw new InvalidOperationException(AssertCallee);
        }
        if (!TuNdebug && (func.VarFlags & VarfNative) != 0)
            throw new InvalidOperationException(AssertCallee);
        if (!TuNdebug && numparam != func.NumArgs)
            throw new InvalidOperationException(AssertCount);
        var reginfo = func.RegTypes;
        if (!TuNdebug && reginfo is null)
            throw new InvalidOperationException(AssertRegInfo);
        var regd = 0;
        var regf = 0;
        var regs = 0;
        var rega = 0;
        for (var i = 0; i < func.NumArgs; i++)
        {
            if (parms is null) throw new NullReferenceException("params");
            if (i >= parms.Length) throw new IndexOutOfRangeException("params");
            var p = parms[i];
            if (reginfo is null) throw new NullReferenceException("reginfo");
            if (i >= reginfo.Length) throw new IndexOutOfRangeException("RegTypes");
            var type = reginfo[i];
            if (type < RegtString)
            {
                if (type == RegtInt)
                {
                    if (regd >= callee.D.Length) throw new IndexOutOfRangeException("d");
                    callee.D[regd] = p.I;
                    regd++;
                }
                else
                {
                    if (regf >= callee.F.Length) throw new IndexOutOfRangeException("f");
                    callee.F[regf] = p.F;
                    regf++;
                }
            }
            else if (type == RegtString)
            {
                var source = p.Sp;
                if (source is null) throw new NullReferenceException("sp");
                var copied = source.Copy();
                if (regs >= callee.S.Length) throw new IndexOutOfRangeException("s");
                callee.S[regs] = copied;
                regs++;
            }
            else
            {
                if (!TuNdebug && type != RegtPointer)
                    throw new InvalidOperationException(AssertPointer);
                if (rega >= callee.A.Length) throw new IndexOutOfRangeException("a");
                callee.A[rega] = p.A;
                rega++;
            }
        }
    }

    public bool AssertObject(Obj? ob, bool msvc)
    {
        if (TuNdebug) throw new InvalidOperationException("AssertObject");
        if (ob is null) return true;
        if (!ob.Readable)
        {
            if (msvc) return false;
            throw new InvalidOperationException("MagicID");
        }
        return ob.Magic == MagicId;
    }

    public void AdvanceSwitch(ref int pc)
    {
        if (CompGoto) throw new InvalidOperationException("COMPGOTO");
        pc++;
    }

    public ComputedStep AdvanceComputed(Word[] code, ref int pc, EmitInstruction? emit, FrameStack? stack, object? ret, int numret)
    {
        if (!CompGoto) throw new InvalidOperationException("COMPGOTO");
        if (code is null) throw new NullReferenceException("code");
        pc++;
        if (emit is null) throw new NullReferenceException("EmitInstructionExecutionEvent");
        emit(stack, ret, numret, pc);
        if ((uint)pc >= (uint)code.Length) throw new IndexOutOfRangeException("pc");
        var word = code[pc];
        return new ComputedStep(pc, word.Op, word.A);
    }

    public static int Kc(int[] konstd, Word word)
    {
        return konstd[word.C];
    }

    public static int Rc(int[] reg, Word word)
    {
        return reg[word.C];
    }

    public static int GetAddr(Buffer? basis, int offset, int reason, ref int ptr)
    {
        if (basis is null) throw new VmAbort(reason);
        ptr = basis.Origin + offset;
        return ptr;
    }

    void Assign(Engine engine)
    {
        Writes.Add(engine);
        Current = engine;
    }

    public readonly struct Word
    {
        public Word(uint raw) { Raw = raw; }
        public uint Raw { get; }
        public byte Op => (byte)Raw;
        public byte A => (byte)(Raw >> 8);
        public byte B => (byte)(Raw >> 16);
        public byte C => (byte)(Raw >> 24);
        public sbyte Cs => unchecked((sbyte)C);
        public ushort Bc => (ushort)(Raw >> 16);
        public short Bcs => unchecked((short)(Raw >> 16));
        public int Abcs => (int)Raw >> 8;
    }

    public readonly struct ComputedStep
    {
        public ComputedStep(int pc, byte op, byte a)
        {
            Pc = pc;
            Op = op;
            A = a;
        }
        public int Pc { get; }
        public byte Op { get; }
        public byte A { get; }
    }

    public readonly struct Value
    {
        public Value(int i, double f, Text? sp, object? a)
        {
            I = i;
            F = f;
            Sp = sp;
            A = a;
        }
        public int I { get; }
        public double F { get; }
        public Text? Sp { get; }
        public object? A { get; }
    }

    public sealed class Text
    {
        public Text(string value) { Value = value; }
        public string Value { get; set; }
        public Text Copy()
        {
            return new Text(Value);
        }
    }

    public sealed class Engine
    {
        public Engine(bool asserts) { Asserts = asserts; }
        public bool Asserts { get; }

        public void AssertD(int index, int limit) => Limit(index, limit, AssertLimitD);
        public void AssertF(int index, int limit) => Limit(index, limit, AssertLimitF);
        public void AssertA(int index, int limit) => Limit(index, limit, AssertLimitA);
        public void AssertS(int index, int limit) => Limit(index, limit, AssertLimitS);

        public void AssertKd(Script? sfunc, int index)
        {
            if (!Asserts) return;
            if (sfunc is null || (uint)index >= (uint)sfunc.NumKonstD)
                throw new InvalidOperationException(AssertKonstD);
        }

        public void AssertKf(Script? sfunc, int index)
        {
            if (!Asserts) return;
            if (sfunc is null || (uint)index >= (uint)sfunc.NumKonstF)
                throw new InvalidOperationException(AssertKonstF);
        }

        public void AssertKa(Script? sfunc, int index)
        {
            if (!Asserts) return;
            if (sfunc is null || (uint)index >= (uint)sfunc.NumKonstA)
                throw new InvalidOperationException(AssertKonstA);
        }

        public void AssertKs(Script? sfunc, int index)
        {
            if (!Asserts) return;
            if (sfunc is null || (uint)index >= (uint)sfunc.NumKonstS)
                throw new InvalidOperationException(AssertKonstS);
        }

        public int CmpJmp(Word[] code, int pc, bool test)
        {
            var check = code[pc].A & CmpCheck;
            var matched = (test ? 1 : 0) == check;
            if (!matched) return pc + 1;
            if (Asserts && code[pc + 1].Op != OpJmp)
                throw new InvalidOperationException(AssertJmp);
            return pc + 1 + code[pc + 1].Abcs;
        }

        void Limit(int index, int limit, string expression)
        {
            if (!Asserts) return;
            if ((uint)index >= (uint)limit) throw new InvalidOperationException(expression);
        }
    }

    public sealed class Script
    {
        public int Flags;
        public int FlagReads;
        public int ArgCount;
        public int NumArgsReads;
        public byte[]? Types;
        public int KonstCount;
        public int KonstReads;
        public int NumKonstF;
        public int NumKonstA;
        public int NumKonstS;

        public int VarFlags
        {
            get
            {
                FlagReads++;
                return Flags;
            }
        }

        public int NumArgs
        {
            get
            {
                NumArgsReads++;
                return ArgCount;
            }
        }

        public byte[]? RegTypes => Types;

        public int NumKonstD
        {
            get
            {
                KonstReads++;
                return KonstCount;
            }
        }
    }

    public sealed class Frame
    {
        public Frame(int d, int f, int s, int a)
        {
            D = new int[d];
            F = new double[f];
            S = new Text[s];
            A = new object[a];
        }

        public Script? Func { get; set; }
        public int[] D { get; }
        public double[] F { get; }
        public Text?[] S { get; }
        public object?[] A { get; }
        public int RegisterBinds { get; private set; }

        public void BindRegisters()
        {
            RegisterBinds++;
        }
    }

    public sealed class Obj
    {
        public bool Readable { get; set; } = true;
        public int MagicReads { get; private set; }
        public uint MagicValue { get; set; } = MagicId;
        public uint Magic
        {
            get
            {
                MagicReads++;
                return MagicValue;
            }
        }
    }

    public sealed class Buffer
    {
        public Buffer(int origin) { Origin = origin; }
        public int Origin { get; }
    }

    public sealed class FrameStack
    {
        public FrameStack(int id) { Id = id; }
        public int Id { get; }
    }

    public sealed class VmAbort : Exception
    {
        public VmAbort(int reason) : base(MessageFor(reason)) { Reason = reason; }
        public int Reason { get; }

        public static string MessageFor(int reason)
        {
            var detail = reason switch
            {
                0 => "",
                1 => "tried to read from address zero.",
                2 => "tried to write to address zero.",
                3 => "too many try-catch blocks.",
                4 => "array access out of bounds.",
                5 => "division by zero.",
                6 => "invalid self pointer.",
                7 => "string format failed.",
                _ => "Unknown reason " + reason.ToString(CultureInfo.InvariantCulture),
            };
            return "VM execution aborted: " + detail;
        }
    }
}
