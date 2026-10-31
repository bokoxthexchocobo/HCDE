namespace HCDE.Scripting;

/// <summary>Random-pick emit, floating random, and the random2 construct from codegen.cpp 6316-6508. The pick resolve loop and the random2 resolve body stay on chunk 538.</summary>
public sealed class VmCodegenRandom1387Slice
{
    public const int RegInt = 0, RegFloat = 1, RegPointer = 3, RegType = 3, RegKonst = 4, RegMulti2 = 8, RegMulti3 = 16, RegMulti4 = 64, RegNil = 128, VarArgFlag = 1 << 19;
    public bool Asserts = true;
    public Func<string, Symbol?>? Find;
    public sealed class AssertAbort(string message) : Exception(message);
    public sealed class Reg { public int Num, Type, Count = 1; public bool Konst, Fixed; }
    public sealed class Expr
    {
        public string Kind = "expr";
        public int RegType, Int, Pos;
        public double Float;
        public bool Constant, Resolved, NoWarn, Deleted;
        public Expr? Inner;
        public Func<Build, Reg>? Emit;
        public void Release() { Deleted = true; if ((Kind == "FloatCast" || Kind == "IntCast") && Inner != null) { Inner.Release(); Inner = null; } }
    }
    public sealed class Op
    {
        public string Name = "";
        public int Form, A, B, C, I16, I24;
        public string Text() => Form == 24 ? $"{Name}:{I24}" : Form == 16 ? $"{Name}:{A}:{I16}" : $"{Name}:{A}:{B}:{C}";
    }
    public sealed class Symbol { public object? Implementation; public int Flags, ProtoSize = 1; public List<(int Type, int Count)> ProtoTail = []; }
    public sealed class Rng { public Func<uint>? Next; public uint GenRand32() => Next!(); }
    public sealed class Slot { public object? Ptr; public double Float; }
    public sealed class Build
    {
        public bool Asserts = true, VmJit;
        public readonly List<Op> Code = [];
        public readonly List<string> Messages = [];
        public readonly List<int> Ints = [];
        public readonly List<double> Floats = [];
        public readonly List<object?> Addrs = [];
        public byte[] LastInfo = [];
        public int ActiveParam, MaxParam;
        public readonly int[] Most = new int[4];
        readonly uint[] used = new uint[32];
        public void Check(bool ok, string why) { if (!ok && Asserts) throw new AssertAbort(why); }
        public bool InUse(int type, int reg) => (used[(type * 8) + (reg / 32)] & (1u << (reg & 31))) != 0;
        public int Get(int type, int count)
        {
            if (count < 1 || count > 32) return -1;
            for (int i = 0; i <= 256 - count; i++)
            {
                var free = true;
                for (int k = 0; k < count && free; k++) if (InUse(type, i + k)) free = false;
                if (!free) continue;
                for (int k = 0; k < count; k++) used[(type * 8) + ((i + k) / 32)] |= 1u << ((i + k) & 31);
                if (i + count > Most[type]) Most[type] = i + count;
                return i;
            }
            return -1;
        }
        public void ReturnReg(int type, int reg, int count)
        {
            Check(count >= 1 && count <= 32 && reg >= 0 && reg + count <= 256, "return");
            for (int k = 0; k < count; k++) used[(type * 8) + ((reg + k) / 32)] &= ~(1u << ((reg + k) & 31));
        }
        public bool TryReuse(int type, int reg)
        {
            Check(reg >= 0 && reg <= 255 && reg < Most[type], "reuse");
            if (InUse(type, reg)) return false;
            used[(type * 8) + (reg / 32)] |= 1u << (reg & 31);
            return true;
        }
        public Reg Alloc(int type, int count = 1) => new() { Num = Get(type, count), Type = type, Count = count };
        public void Free(Reg reg) { if (!reg.Fixed && !reg.Konst && reg.Type <= RegType) ReturnReg(reg.Type, reg.Num, reg.Count); }
        public void Reuse(Reg reg) { if (reg.Fixed || reg.Konst) return; Check(reg.Count == 1, "count"); Check(TryReuse(reg.Type, reg.Num), "Attempt to reuse a register that is already in use"); }
        public int ConstInt(int value) { int at = Ints.IndexOf(value); if (at >= 0) return at; Ints.Add(value); return Ints.Count - 1; }
        public int ConstFloat(double value) { for (int i = 0; i < Floats.Count; i++) if (Floats[i] == value) return i; Floats.Add(value); return Floats.Count - 1; }
        public int ConstAddr(object? ptr) { for (int i = 0; i < Addrs.Count; i++) if (ReferenceEquals(Addrs[i], ptr)) return i; Addrs.Add(ptr); return Addrs.Count - 1; }
        void Param(int delta) { Check(delta > 0 || -delta <= ActiveParam, "param"); ActiveParam += delta; if (ActiveParam > MaxParam) MaxParam = ActiveParam; }
        public void Emit24(string name, int imm) { Check(imm >= -(1 << 23) && imm <= (1 << 24) - 1, "i24"); if (name == "PARAMI") Param(1); Code.Add(new Op { Name = name, Form = 24, I24 = imm }); }
        public void Emit16(string name, int a, int imm) { Check(a >= 0 && a <= 255, "a"); if (name == "PARAM") Param((a & RegMulti2) != 0 ? 2 : (a & RegMulti3) != 0 ? 3 : (a & RegMulti4) != 0 ? 4 : 1); Code.Add(new Op { Name = name, Form = 16, A = a, I16 = (short)imm }); }
        public void Emit4(string name, int a, int b, int c) { Check(a >= 0 && b >= 0 && c >= 0, "abc"); if (a > 255 || b > 255 || c > 255) throw new InvalidOperationException("Register limit exceeded"); if (name == "CALL_K") Param(-b); Code.Add(new Op { Name = name, Form = 4, A = a, B = b, C = c }); }
        public void Backpatch(int loc) { Check(loc >= 0 && loc < Code.Count, "loc"); int offset = Code.Count - loc - 1; Check(unchecked((offset << 8) >> 8) == offset, "offset"); var op = Code[loc]; op.Name = "JMP"; op.Form = 24; op.I24 = offset; }
        public void EmitLoadInt(int regnum, int value) { Check(regnum >= 0 && regnum < Most[RegInt], "load"); if (value >= -32768 && value <= 32767) Emit16("LI", regnum, value); else Emit16("LK", regnum, ConstInt(value)); }
        public string Text() => string.Join("\n", Code.ConvertAll(op => op.Text()));
    }
    public sealed class Call
    {
        public readonly object Target;
        public readonly bool VarArg;
        public readonly int ProtoSize;
        public readonly List<(int Type, int Count)> ProtoTail;
        readonly List<Func<Build, int>> steps = [];
        readonly List<(int Type, int Count)> rets = [];
        readonly List<int> info = [];
        int num;
        public Call(Symbol sym)
        {
            if (sym.Implementation == null) throw new NullReferenceException();
            Target = sym.Implementation; VarArg = (sym.Flags & VarArgFlag) != 0; ProtoSize = sym.ProtoSize; ProtoTail = sym.ProtoTail;
        }
        public void PointerConst(object? ptr) { num++; if (VarArg) info.Add(RegPointer); steps.Add(b => { b.Emit16("PARAM", RegPointer | RegKonst, b.ConstAddr(ptr)); return 1; }); }
        public void IntConst(int value) { num++; if (VarArg) info.Add(RegInt); steps.Add(b => { if (unchecked((value << 8) >> 8) == value) b.Emit24("PARAMI", value); else b.Emit16("PARAM", RegInt | RegKonst, b.ConstInt(value)); return 1; }); }
        public void Operand(Build build, Expr expr)
        {
            if (expr.Emit == null) throw new NullReferenceException();
            Reg where = expr.Emit(build);
            if (where.Type == RegNil) build.Messages.Add("Attempted to pass a non-value");
            num += where.Count;
            if (VarArg) for (int i = 0; i < where.Count; i++) info.Add(where.Type & RegType);
            steps.Add(host => { if (where.Type == RegNil) { host.Emit16("PARAM", where.Type, where.Num); return 1; } host.Emit16("PARAM", Encode(where), where.Num); host.Free(where); return where.Count; });
        }
        public void AddReturn(int type, int count = 1) => rets.Add((type, count));
        public Reg Emit(Build build)
        {
            int count = 0;
            foreach (var step in steps) count += step(build);
            build.Check(count == num, "paramcount");
            if (VarArg) { build.LastInfo = info.ConvertAll(x => (byte)x).ToArray(); build.Emit16("PARAM", RegPointer | RegKonst, build.ConstAddr(build.LastInfo)); count++; }
            build.Emit4("CALL_K", build.ConstAddr(Target), count, build.VmJit ? ProtoSize : rets.Count);
            Reg last = new() { Type = RegNil };
            for (int i = 0; i < rets.Count; i++) { last = build.Alloc(rets[i].Type, rets[i].Count); build.Emit4("RESULT", 0, Encode(last), last.Num); }
            if (build.VmJit) for (int i = 0; i < ProtoTail.Count; i++) { var extra = build.Alloc(ProtoTail[i].Type, ProtoTail[i].Count); build.Emit4("RESULT", 0, Encode(extra), extra.Num); build.Free(extra); }
            return last;
        }
    }
    public static int Encode(Reg reg)
    {
        int kind = reg.Type;
        if (reg.Konst) kind |= RegKonst; else if (reg.Count == 2) kind |= RegMulti2; else if (reg.Count == 3) kind |= RegMulti3; else if (reg.Count == 4) kind |= RegMulti4;
        return kind;
    }
    public Build NewBuild() => new() { Asserts = Asserts };
    public sealed class Pick
    {
        public readonly List<Expr> Choices = [];
        public object? Rng;
        public int ValueRegType = RegInt, ChoiceMost = -1;
        public Pick ReturnResolved() => this;
        public void ClearChoices() { foreach (var choice in Choices) choice.Release(); Choices.Clear(); Choices.TrimExcess(); ChoiceMost = 0; }
    }
    public Reg EmitPick(Pick pick, Build build)
    {
        build.Check(pick.Choices.Count > 0, "choices");
        var sym = Find?.Invoke("BuiltinRandom");
        build.Check(sym != null, "sym");
        if (sym == null) throw new NullReferenceException();
        var call = new Call(sym);
        call.PointerConst(pick.Rng); call.IntConst(0); call.IntConst(pick.Choices.Count - 1); call.AddReturn(RegInt);
        Reg result = call.Emit(build);
        build.Emit16("IJMP", result.Num, pick.Choices.Count);
        build.Free(result);
        if (pick.ValueRegType == RegFloat) { result = build.Alloc(RegFloat); build.Free(result); }
        int table = build.Code.Count;
        build.Emit24("JMP", 0);
        for (int i = 1; i < pick.Choices.Count; i++) build.Emit24("JMP", 0);
        var finishes = new List<int>();
        for (int i = 0; i < pick.Choices.Count; i++)
        {
            build.Backpatch(table + i);
            var choice = pick.Choices[i];
            if (choice.Constant) { if (result.Type == RegFloat) build.Emit16("LKF", result.Num, build.ConstFloat(choice.Float)); else build.EmitLoadInt(result.Num, choice.Int); }
            else
            {
                if (choice.Emit == null) throw new NullReferenceException();
                var caseReg = choice.Emit(build);
                if (caseReg.Num != result.Num) { build.Reuse(result); build.Emit4(pick.ValueRegType == RegInt ? "MOVE" : "MOVEF", result.Num, caseReg.Num, 0); build.Free(result); }
                build.Free(caseReg);
            }
            if (i + 1 < pick.Choices.Count) { finishes.Add(build.Code.Count); build.Emit24("JMP", 0); }
        }
        for (int i = 0; i < pick.Choices.Count - 1; i++) build.Backpatch(finishes[i]);
        build.Reuse(result);
        pick.ClearChoices();
        return result;
    }
    public sealed class FRandom { public string Kind = "Random", Requested = ""; public object? Rng; public Expr? Min, Max; public int ValueRegType, Pos; }
    public FRandom ConstructF(object? rng, Expr? min, Expr? max, int pos)
    {
        var node = new FRandom { Rng = rng, Requested = "FRandom", Kind = "Random", Pos = pos };
        if ((min == null || max == null) && Asserts) throw new AssertAbort("mi && ma");
        if (min == null) throw new NullReferenceException();
        node.Min = new Expr { Kind = "FloatCast", Inner = min, RegType = RegFloat, Pos = min.Pos };
        if (max == null) throw new NullReferenceException();
        node.Max = new Expr { Kind = "FloatCast", Inner = max, RegType = RegFloat, Pos = max.Pos };
        node.ValueRegType = RegFloat;
        return node;
    }
    public static double NativeFRandom(Rng rng, double min, double max) { double t = rng.GenRand32() * Math.ScaleB(1.0, -32); return (min * (1.0 - t)) + (max * t); }
    public int BuiltinFRandom(IReadOnlyList<Slot> param, IReadOnlyList<int> info, int numret, Slot? ret)
    {
        if (!(param.Count > 0 && info[0] == RegPointer)) throw new AssertAbort("param");
        if (!(param.Count > 1 && info[1] == RegFloat)) throw new AssertAbort("param");
        if (!(param.Count > 2 && info[2] == RegFloat)) throw new AssertAbort("param");
        double value = NativeFRandom((Rng?)param[0].Ptr!, param[1].Float, param[2].Float);
        if (numret <= 0) return 0;
        if (ret == null) throw new AssertAbort("ret");
        ret.Float = value;
        return 1;
    }
    public Reg EmitF(FRandom node, Build build)
    {
        build.Check(node.Min != null && node.Max != null, "min && max");
        if (node.Min == null || node.Max == null) throw new NullReferenceException();
        var sym = Find?.Invoke("BuiltinFRandom");
        build.Check(sym != null, "sym");
        if (sym == null) throw new NullReferenceException();
        var call = new Call(sym);
        call.PointerConst(node.Rng); call.Operand(build, node.Min); call.Operand(build, node.Max); call.AddReturn(RegFloat);
        return call.Emit(build);
    }
    public sealed class Random2
    {
        public string Kind = "Random2";
        public object? Rng;
        public Expr? Mask;
        public int ValueRegType = RegInt, Pos;
        public void Destroy() { if (Mask != null) { Mask.Release(); Mask = null; } }
    }
    public Random2 Construct2(object? rng, Expr? mask, int pos, bool nowarn)
    {
        return new Random2
        {
            Rng = rng,
            Pos = pos,
            Mask = mask != null
                ? new Expr { Kind = "IntCast", Inner = mask, RegType = RegInt, NoWarn = nowarn, Pos = mask.Pos }
                : new Expr { Kind = "Constant", Int = -1, RegType = RegInt, Resolved = true, Pos = pos },
        };
    }
}
