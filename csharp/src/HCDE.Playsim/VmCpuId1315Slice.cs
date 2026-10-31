using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// CPUID leaves, the brand-string spaces, and the feature dump from
/// <c>x86.cpp</c> (1-218). Chunk 1315.
/// </summary>
public sealed class VmCpuId1315
{
    public const int DefaultLineSize = 64;

    public byte[] Vendor { get; } = new byte[16];

    public byte[] Brand { get; } = new byte[48];

    public byte Stepping { get; set; }

    public byte Model { get; set; }

    public byte Family { get; set; }

    public byte Type { get; set; }

    public byte HyperThreading { get; set; }

    public byte AmdStepping { get; set; }

    public byte AmdModel { get; set; }

    public byte AmdFamily { get; set; }

    public byte IsAmd { get; set; }

    public uint AmdDataL1 { get; set; }

    public uint[] Flags { get; } = new uint[8];

    public List<(int Function, int Subleaf)> Calls { get; } = new();

    public byte DataL1LineSize
    {
        get { return (byte)AmdDataL1; }
        set { AmdDataL1 = (AmdDataL1 & 0xFFFFFF00u) | value; }
    }

    public void CheckWithoutCpuid()
    {
        Clear();
        DataL1LineSize = DefaultLineSize;
    }

    public void Check(Func<int, int, uint[]> cpuid)
    {
        Clear();
        DataL1LineSize = DefaultLineSize;
        uint[] leaf = Read(cpuid, 0, 0);
        int maxid = unchecked((int)leaf[0]);
        Store(Vendor, 0, leaf[1]);
        Store(Vendor, 4, leaf[3]);
        Store(Vendor, 8, leaf[2]);
        if (leaf[1] == MakeId('A', 'u', 't', 'h')
            && leaf[3] == MakeId('e', 'n', 't', 'i')
            && leaf[2] == MakeId('c', 'A', 'M', 'D'))
            IsAmd = 1;

        leaf = Read(cpuid, 1, 0);
        Flags[0] = leaf[1];
        Flags[1] = leaf[2];
        Flags[2] = leaf[3];
        if ((leaf[3] & (1u << 28)) > 0)
            HyperThreading = 1;
        if ((leaf[3] & (1u << 19)) != 0)
            DataL1LineSize = (byte)((leaf[1] & 0xFF00u) >> 5);

        uint eax = leaf[0];
        Stepping = (byte)(eax & 0x0Fu);
        Type = (byte)((eax & 0x3000u) >> 12);
        Model = (byte)((eax & 0xF0u) >> 4);
        Family = (byte)((eax & 0xF00u) >> 8);
        if (Family == 15)
            Family = (byte)(Family + ((eax >> 20) & 0xFF));
        if (Family == 6 || Family == 15)
            Model = (byte)(Model | ((eax >> 12) & 0xF0));

        leaf = Read(cpuid, unchecked((int)0x80000000), 0);
        uint maxext = leaf[0];
        if (maxext >= 0x80000004u)
        {
            StoreWords(Brand, 0, Read(cpuid, unchecked((int)0x80000002), 0));
            StoreWords(Brand, 16, Read(cpuid, unchecked((int)0x80000003), 0));
            StoreWords(Brand, 32, Read(cpuid, unchecked((int)0x80000004), 0));
        }

        if (IsAmd != 0)
        {
            if (maxext >= 0x80000005u)
                AmdDataL1 = Read(cpuid, unchecked((int)0x80000005), 0)[2];
            if (maxext >= 0x80000001u)
            {
                uint[] ext = Read(cpuid, unchecked((int)0x80000001), 0);
                AmdStepping = (byte)(ext[0] & 0x0Fu);
                AmdModel = (byte)((ext[0] & 0xF0u) >> 4);
                AmdFamily = (byte)((ext[0] & 0xF00u) >> 8);
                if (AmdFamily == 15)
                {
                    AmdFamily = (byte)(AmdFamily + ((ext[0] >> 20) & 0xFF));
                    AmdModel = (byte)(AmdModel | ((ext[0] >> 12) & 0xF0));
                }

                Flags[3] = ext[3];
            }
        }

        if (maxid >= 7)
        {
            uint[] low = Read(cpuid, 7, 0);
            Flags[4] = low[1];
            Flags[5] = low[2];
            Flags[6] = low[3];
            Flags[7] = Read(cpuid, 7, 1)[0];
        }
    }

    public string Dump(bool brief)
    {
        _ = brief;
        string name = Condense(Brand);
        if (Vendor[0] == 0)
            return "";

        var text = new StringBuilder();
        text.Append("CPU Vendor ID: ");
        text.Append(ReadVendor());
        text.Append('\n');
        if (name.Length > 0)
        {
            text.Append("  Name: ");
            text.Append(name);
            text.Append('\n');
        }

        if (IsAmd != 0)
        {
            text.Append("  Family ");
            text.Append(Family);
            text.Append(" (");
            text.Append(AmdFamily);
            text.Append("), Model ");
            text.Append(AmdModel);
            text.Append(", Stepping ");
            text.Append(AmdStepping);
            text.Append('\n');
        }
        else
        {
            text.Append("  Family ");
            text.Append(Family);
            text.Append(", Model ");
            text.Append(Model);
            text.Append(", Stepping ");
            text.Append(Stepping);
            text.Append('\n');
        }

        text.Append("  Features:");
        if (Bit(2, 26)) text.Append(" SSE2");
        if (Bit(1, 0)) text.Append(" SSE3");
        if (Bit(1, 9)) text.Append(" SSSE3");
        if (Bit(1, 19)) text.Append(" SSE4.1");
        if (Bit(1, 20)) text.Append(" SSE4.2");
        if (Bit(1, 28)) text.Append(" AVX");
        if (Bit(4, 5)) text.Append(" AVX2");
        if (Bit(4, 16)) text.Append(" AVX512");
        if (Bit(1, 29)) text.Append(" F16C");
        if (Bit(1, 12)) text.Append(" FMA3");
        if (Bit(4, 3)) text.Append(" BMI1");
        if (Bit(4, 8)) text.Append(" BMI2");
        if (HyperThreading != 0)
            text.Append(" HyperThreading");
        text.Append('\n');
        return text.ToString();
    }

    void Clear()
    {
        Array.Clear(Vendor);
        Array.Clear(Brand);
        Array.Clear(Flags);
        Stepping = 0;
        Model = 0;
        Family = 0;
        Type = 0;
        HyperThreading = 0;
        AmdStepping = 0;
        AmdModel = 0;
        AmdFamily = 0;
        IsAmd = 0;
        AmdDataL1 = 0;
        Calls.Clear();
    }

    uint[] Read(Func<int, int, uint[]> cpuid, int function, int subleaf)
    {
        Calls.Add((function, subleaf));
        uint[] words = cpuid(function, subleaf);
        if (words == null || words.Length < 4)
            throw new NullReferenceException("cpuid");
        return words;
    }

    bool Bit(int word, int index)
    {
        return (Flags[word] & (1u << index)) != 0;
    }

    string ReadVendor()
    {
        var text = new StringBuilder();
        for (int i = 0; i < Vendor.Length && Vendor[i] != 0; i++)
            text.Append((char)Vendor[i]);
        if (text.Length == Vendor.Length)
        {
            for (int i = 0; i < Brand.Length && Brand[i] != 0; i++)
                text.Append((char)Brand[i]);
        }

        return text.ToString();
    }

    static string Condense(byte[] brand)
    {
        int i = 0;
        while (i < brand.Length && brand[i] == (byte)' ')
            i++;
        var text = new StringBuilder();
        for (; i < brand.Length && brand[i] != 0; i++)
        {
            if (brand[i] == (byte)' ' && brand[i - 1] == (byte)' ')
                continue;
            text.Append((char)brand[i]);
        }

        return text.ToString();
    }

    static uint MakeId(char a, char b, char c, char d)
    {
        return (uint)a | ((uint)b << 8) | ((uint)c << 16) | ((uint)d << 24);
    }

    static void Store(byte[] dest, int index, uint value)
    {
        dest[index] = (byte)value;
        dest[index + 1] = (byte)(value >> 8);
        dest[index + 2] = (byte)(value >> 16);
        dest[index + 3] = (byte)(value >> 24);
    }

    static void StoreWords(byte[] dest, int index, uint[] words)
    {
        for (int i = 0; i < 4; i++)
            Store(dest, index + (i * 4), words[i]);
    }
}
