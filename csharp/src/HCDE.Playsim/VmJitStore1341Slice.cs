namespace HCDE.Playsim;

/// <summary>
/// JIT memory stores from <c>jit_store.cpp</c> (1-314). Chunk 1341.
/// Null checks, little-endian x64 stores, string assignment, and the
/// object write barrier are the effects of those emitters. The barrier's
/// color test stays in the collector.
/// </summary>
public sealed class VmJitStore1341
{
    public const int XWriteNil = 2;

    public readonly int[] RegD = new int[260];
    public readonly double[] RegF = new double[260];
    public readonly string?[] RegS = new string?[260];
    public readonly long[] RegA = new long[260];
    public readonly int[] KonstD = new int[256];

    public byte A;
    public byte B;
    public byte C;
    public int Pc;
    public Func<int, int>? PcToLine;

    public readonly Dictionary<long, byte> Bytes = new();
    public readonly Dictionary<long, string> Strings = new();
    public readonly List<long> Barriers = new();
    public readonly List<LineNote> Lines = new();

    int _nextLabel = 1;

    public readonly record struct LineNote(int Label, int Line, int Reason);

    public sealed class StoreAbortException : Exception
    {
        public StoreAbortException(int reason)
            : base("X_WRITE_NIL")
        {
            Reason = reason;
        }

        public int Reason { get; }
    }

    public VmJitStore1341()
    {
        for (int i = 0; i < RegS.Length; i++)
            RegS[i] = "";
    }

    public void EmitSB()
    {
        StoreInt(1, false);
    }

    public void EmitSB_R()
    {
        StoreInt(1, true);
    }

    public void EmitSH()
    {
        StoreInt(2, false);
    }

    public void EmitSH_R()
    {
        StoreInt(2, true);
    }

    public void EmitSW()
    {
        StoreInt(4, false);
    }

    public void EmitSW_R()
    {
        StoreInt(4, true);
    }

    public void EmitSSP()
    {
        StoreSingle(false);
    }

    public void EmitSSP_R()
    {
        StoreSingle(true);
    }

    public void EmitSDP()
    {
        StoreDouble(false);
    }

    public void EmitSDP_R()
    {
        StoreDouble(true);
    }

    public void EmitSS()
    {
        StoreString(false);
    }

    public void EmitSS_R()
    {
        StoreString(true);
    }

    public void EmitSO()
    {
        StorePointer(false, true);
    }

    public void EmitSO_R()
    {
        StorePointer(true, true);
    }

    public void EmitSP()
    {
        StorePointer(false, false);
    }

    public void EmitSP_R()
    {
        StorePointer(true, false);
    }

    public void EmitSV2()
    {
        StoreDoubleVector(2, false);
    }

    public void EmitSV2_R()
    {
        StoreDoubleVector(2, true);
    }

    public void EmitSV3()
    {
        StoreDoubleVector(3, false);
    }

    public void EmitSV3_R()
    {
        StoreDoubleVector(3, true);
    }

    public void EmitSV4()
    {
        StoreDoubleVector(4, false);
    }

    public void EmitSV4_R()
    {
        StoreDoubleVector(4, true);
    }

    public void EmitSFV2()
    {
        StoreFloatVector(2, false);
    }

    public void EmitSFV2_R()
    {
        StoreFloatVector(2, true);
    }

    public void EmitSFV3()
    {
        StoreFloatVector(3, false);
    }

    public void EmitSFV3_R()
    {
        StoreFloatVector(3, true);
    }

    public void EmitSFV4()
    {
        StoreFloatVector(4, false);
    }

    public void EmitSFV4_R()
    {
        StoreFloatVector(4, true);
    }

    public void EmitSBIT()
    {
        NoteWriteNil();
        long addr = RegA[A];
        int current = ReadByte(addr);
        int setBits = current | C;
        int clearBits = current & ~C;
        int stored = RegD[B] == 0 ? clearBits : setBits;
        Bytes[addr] = (byte)stored;
    }

    void StoreInt(int width, bool registerOffset)
    {
        NoteWriteNil();
        uint bits = unchecked((uint)RegD[B]);
        WriteBits(Address(registerOffset), bits, width);
    }

    void StoreSingle(bool registerOffset)
    {
        NoteWriteNil();
        // xorpd clears the temp. cvtsd2ss writes the low single, and movss
        // stores those 32 bits. The cleared upper lanes are not stored.
        float converted = (float)RegF[B];
        uint bits = unchecked((uint)BitConverter.SingleToInt32Bits(converted));
        WriteBits(Address(registerOffset), bits, 4);
    }

    void StoreDouble(bool registerOffset)
    {
        NoteWriteNil();
        ulong bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(RegF[B]));
        WriteBits(Address(registerOffset), bits, 8);
    }

    void StoreString(bool registerOffset)
    {
        NoteWriteNil();
        string? source = RegS[B];
        if (source is null)
            throw new NullReferenceException("FString");
        Strings[Address(registerOffset)] = source;
    }

    void StorePointer(bool registerOffset, bool barrier)
    {
        NoteWriteNil();
        ulong bits = unchecked((ulong)RegA[B]);
        WriteBits(Address(registerOffset), bits, 8);
        if (barrier)
            Barriers.Add(RegA[B]);
    }

    void StoreDoubleVector(int count, bool registerOffset)
    {
        NoteWriteNil();
        long addr = Address(registerOffset);
        for (int i = 0; i < count; i++)
        {
            ulong bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(RegF[B + i]));
            WriteBits(addr + (8L * i), bits, 8);
        }
    }

    void StoreFloatVector(int count, bool registerOffset)
    {
        NoteWriteNil();
        long addr = Address(registerOffset);
        for (int i = 0; i < count; i++)
        {
            float converted = (float)RegF[B + i];
            uint bits = unchecked((uint)BitConverter.SingleToInt32Bits(converted));
            WriteBits(addr + (4L * i), bits, 4);
        }
    }

    long Address(bool registerOffset)
    {
        int displacement = registerOffset ? RegD[C] : KonstD[C];
        return RegA[A] + displacement;
    }

    void NoteWriteNil()
    {
        if (PcToLine is null)
            throw new InvalidOperationException("PCToLine");
        int line = PcToLine(Pc);
        Lines.Add(new LineNote(_nextLabel, line, XWriteNil));
        _nextLabel++;
        if (RegA[A] == 0)
            throw new StoreAbortException(XWriteNil);
    }

    int ReadByte(long addr)
    {
        if (!Bytes.TryGetValue(addr, out byte value))
            throw new InvalidOperationException("unmapped");
        return value;
    }

    void WriteBits(long addr, ulong bits, int width)
    {
        for (int i = 0; i < width; i++)
            Bytes[addr + i] = (byte)(bits >> (8 * i));
    }
}
