using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// JIT test, jump, scope, return, throw, and bounds checks from
/// <c>jit_flow.cpp</c> (1-362). Chunk 1331. Frame pop, scope validation,
/// and <c>PCToLine</c> stay callees.
/// </summary>
public sealed class VmJitFlow1331
{
    public const int RegInt = 0;
    public const int RegFloat = 1;
    public const int RegString = 2;
    public const int RegPointer = 3;
    public const int RegTypeMask = 3;
    public const int RegKonst = 4;
    public const int RegMulti2 = 8;
    public const int RegMulti3 = 16;
    public const int RegMulti4 = 64;
    public const int RegNil = 128;
    public const int RetFinal = 0x80;
    public const int XOther = 0;
    public const int XReadNil = 1;
    public const int XArrayOutOfBounds = 4;
    public const int OpJmp = 92;

    public bool Code64 = true;
    public bool Host64 = true;
    public int NumRet;
    public int Pc;
    public Func<int, int>? PcToLine;

    public readonly List<Insn> Code = new();
    public readonly List<string> Ops = new();
    public readonly List<LineNote> Lines = new();
    public readonly int[] RegD = new int[64];
    public readonly long[] RegA = new long[64];
    public readonly double[] RegF = new double[64];
    public readonly string?[] RegS = new string?[64];
    public readonly int[] KonstD = new int[64];
    public readonly long[] KonstBits = new long[64];
    public readonly string?[] KonstS = new string?[64];
    public readonly long[] KonstA = new long[64];
    public readonly List<string> Strings = new();

    int _nextLabel = 1;

    public readonly record struct LineNote(int Label, int Line);

    public sealed class Insn
    {
        public int Op;
        public int A;
        public int B;
        public int C;
        public int I16;
        public int I16u;
        public int I24;
    }

    public int VmReturnSize()
    {
        return Host64 ? 16 : 8;
    }

    public void EmitTest()
    {
        Insn insn = At(Pc);
        Ops.Add("cmp d" + insn.A + " " + insn.I16u.ToString(CultureInfo.InvariantCulture));
        Ops.Add("jne " + (Pc + 2).ToString(CultureInfo.InvariantCulture));
    }

    public void EmitTestN()
    {
        Insn insn = At(Pc);
        int bc = insn.I16u;
        Ops.Add("cmp d" + insn.A + " " + (-bc).ToString(CultureInfo.InvariantCulture));
        Ops.Add("jne " + (Pc + 2).ToString(CultureInfo.InvariantCulture));
    }

    public void EmitJmp()
    {
        int dest = Pc + At(Pc).I24 + 1;
        Ops.Add("jmp " + dest.ToString(CultureInfo.InvariantCulture));
    }

    public void EmitIjmp()
    {
        Insn insn = At(Pc);
        int start = Pc;
        int baseIndex = start + 1;
        Ops.Add("mov val d" + insn.A);
        for (int i = 0; i < insn.I16; i++)
        {
            Insn slot = At(baseIndex + i);
            if (slot.Op == OpJmp)
            {
                int target = baseIndex + i + slot.I24 + 1;
                Ops.Add("cmp val " + i.ToString(CultureInfo.InvariantCulture));
                Ops.Add("je " + target.ToString(CultureInfo.InvariantCulture));
            }
        }
        Pc = start + insn.I16;
        Ops.Add("throw " + XOther.ToString(CultureInfo.InvariantCulture));
    }

    public void EmitScope()
    {
        Insn insn = At(Pc);
        int label = TakeLabel();
        var body = new List<string> { "L" + label + " throw " + XReadNil.ToString(CultureInfo.InvariantCulture) };
        Lines.Add(new LineNote(label, LineOfPc()));
        Ops.Add("test a" + insn.A);
        Ops.Add("jz " + label.ToString(CultureInfo.InvariantCulture));
        Ops.Add("mov f konstA" + insn.C);
        Ops.Add("call ValidateCall a" + insn.A + " f " + insn.B.ToString(CultureInfo.InvariantCulture));
        Ops.AddRange(body);
    }

    public static int ValidateCallArgument(int b)
    {
        int adjusted = b - 1;
        return adjusted;
    }

    public void EmitRet()
    {
        Insn insn = At(Pc);
        if (insn.B == RegNil)
        {
            Ops.Add("pop");
            Ops.Add("mov v 0");
            Ops.Add("ret 0");
            return;
        }
        int retnum = insn.A & ~RetFinal;
        int label = TakeLabel();
        Ops.Add("mov retnum " + retnum.ToString(CultureInfo.InvariantCulture));
        Ops.Add("cmp retnum " + NumRet.ToString(CultureInfo.InvariantCulture));
        Ops.Add("jge " + label.ToString(CultureInfo.InvariantCulture));
        StoreReturn(retnum, insn.B, insn.C);
        if ((insn.A & RetFinal) != 0)
        {
            Ops.Add("add retnum 1");
            Ops.Add("pop");
            Ops.Add("ret " + (retnum + 1).ToString(CultureInfo.InvariantCulture));
        }
        Ops.Add("endif " + label.ToString(CultureInfo.InvariantCulture));
        if ((insn.A & RetFinal) != 0)
        {
            Ops.Add("pop");
            Ops.Add("ret " + NumRet.ToString(CultureInfo.InvariantCulture));
        }
    }

    public void EmitRetI()
    {
        Insn insn = At(Pc);
        int retnum = insn.A & ~RetFinal;
        int label = TakeLabel();
        Ops.Add("mov retnum " + retnum.ToString(CultureInfo.InvariantCulture));
        Ops.Add("cmp retnum " + NumRet.ToString(CultureInfo.InvariantCulture));
        Ops.Add("jge " + label.ToString(CultureInfo.InvariantCulture));
        Ops.Add("store-dword " + retnum.ToString(CultureInfo.InvariantCulture)
            + " " + insn.I16.ToString(CultureInfo.InvariantCulture)
            + " at " + (retnum * VmReturnSize()).ToString(CultureInfo.InvariantCulture));
        if ((insn.A & RetFinal) != 0)
        {
            Ops.Add("add retnum 1");
            Ops.Add("pop");
            Ops.Add("ret " + (retnum + 1).ToString(CultureInfo.InvariantCulture));
        }
        Ops.Add("endif " + label.ToString(CultureInfo.InvariantCulture));
        if ((insn.A & RetFinal) != 0)
        {
            Ops.Add("pop");
            Ops.Add("ret " + NumRet.ToString(CultureInfo.InvariantCulture));
        }
    }

    public void EmitThrow()
    {
        Ops.Add("throw " + At(Pc).I16u.ToString(CultureInfo.InvariantCulture));
    }

    public void EmitBound()
    {
        Insn insn = At(Pc);
        EmitBoundCompare("d" + insn.A, insn.I16u.ToString(CultureInfo.InvariantCulture));
    }

    public void EmitBoundK()
    {
        Insn insn = At(Pc);
        int size = KonstD[insn.I16u];
        EmitBoundCompare("d" + insn.A, size.ToString(CultureInfo.InvariantCulture));
    }

    public void EmitBoundR()
    {
        Insn insn = At(Pc);
        EmitBoundCompare("d" + insn.A, "d" + insn.B);
    }

    public string ThrowArrayOutOfBounds(int index, int size)
    {
        if (index >= size)
        {
            return "Size = " + unchecked((uint)size).ToString(CultureInfo.InvariantCulture)
                + ", current index = " + unchecked((uint)index).ToString(CultureInfo.InvariantCulture)
                + "\n";
        }
        return "Negative current index = " + index.ToString(CultureInfo.InvariantCulture) + "\n";
    }

    public void SetString(int slot, string value)
    {
        while (Strings.Count <= slot)
            Strings.Add("");
        Strings[slot] = value;
        Ops.Add("set-string " + slot.ToString(CultureInfo.InvariantCulture) + " " + value);
    }

    void EmitBoundCompare(string indexText, string sizeText)
    {
        int label = TakeLabel();
        Lines.Add(new LineNote(label, LineOfPc()));
        Ops.Add("cmp " + indexText + " " + sizeText);
        Ops.Add("jae " + label.ToString(CultureInfo.InvariantCulture));
        Ops.Add("L" + label + " call ThrowArrayOutOfBounds " + indexText + " " + sizeText);
    }

    void StoreReturn(int retnum, int regtype, int regnum)
    {
        int type = regtype & RegTypeMask;
        int at = retnum * VmReturnSize();
        bool konst = (regtype & RegKonst) != 0;
        if (type == RegInt)
        {
            int value = konst ? KonstD[regnum] : RegD[regnum];
            Ops.Add("store-dword " + retnum.ToString(CultureInfo.InvariantCulture)
                + " " + value.ToString(CultureInfo.InvariantCulture)
                + " at " + at.ToString(CultureInfo.InvariantCulture));
            return;
        }
        if (type == RegFloat)
        {
            int count = FloatCount(regtype);
            for (int i = 0; i < count; i++)
            {
                int offset = i * 8;
                if (konst)
                {
                    long bits = KonstBits[regnum + i];
                    Ops.Add("mov tmp " + bits.ToString(CultureInfo.InvariantCulture));
                    Ops.Add("store-qword " + retnum.ToString(CultureInfo.InvariantCulture)
                        + "+" + offset.ToString(CultureInfo.InvariantCulture)
                        + " " + bits.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    long bits = BitConverter.DoubleToInt64Bits(RegF[regnum + i]);
                    Ops.Add("movsd " + retnum.ToString(CultureInfo.InvariantCulture)
                        + "+" + offset.ToString(CultureInfo.InvariantCulture)
                        + " f" + (regnum + i).ToString(CultureInfo.InvariantCulture)
                        + " " + bits.ToString(CultureInfo.InvariantCulture));
                }
            }
            return;
        }
        if (type == RegString)
        {
            string source = konst ? "konstS" + regnum.ToString(CultureInfo.InvariantCulture)
                : "s" + regnum.ToString(CultureInfo.InvariantCulture);
            Ops.Add("mov ptr ret");
            Ops.Add("add ptr " + at.ToString(CultureInfo.InvariantCulture));
            Ops.Add("call SetString " + retnum.ToString(CultureInfo.InvariantCulture) + " " + source);
            return;
        }
        if (type == RegPointer)
        {
            string width = Code64 ? "qword" : "dword";
            long value = konst ? KonstA[regnum] : RegA[regnum];
            if (konst)
                Ops.Add("mov ptr " + value.ToString(CultureInfo.InvariantCulture));
            Ops.Add("store-" + width + " " + retnum.ToString(CultureInfo.InvariantCulture)
                + " " + value.ToString(CultureInfo.InvariantCulture)
                + " at " + at.ToString(CultureInfo.InvariantCulture));
        }
    }

    static int FloatCount(int regtype)
    {
        if ((regtype & RegMulti4) != 0)
            return 4;
        if ((regtype & RegMulti3) != 0)
            return 3;
        if ((regtype & RegMulti2) != 0)
            return 2;
        return 1;
    }

    int TakeLabel()
    {
        int label = _nextLabel;
        _nextLabel++;
        return label;
    }

    int LineOfPc()
    {
        if (PcToLine == null)
            throw new InvalidOperationException("PCToLine is missing");
        return PcToLine(Pc);
    }

    Insn At(int index)
    {
        if (index < 0 || index >= Code.Count)
            throw new InvalidOperationException("missing code");
        return Code[index];
    }
}
