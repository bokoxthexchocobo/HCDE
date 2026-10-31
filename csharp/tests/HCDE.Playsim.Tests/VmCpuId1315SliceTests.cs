namespace HCDE.Playsim.Tests;

public class VmCpuId1315SliceTests
{
    [Fact]
    public void Check_DecodesIntelLeavesAndKeepsFamily15ModelFlat()
    {
        var cpu = new VmCpuId1315();
        cpu.CheckWithoutCpuid();
        Assert.Equal(64, cpu.DataL1LineSize);
        Assert.Equal(0, cpu.Vendor[0]);
        Assert.Equal("", cpu.Dump(true));
        Assert.Equal("", cpu.Dump(false));

        uint eax6 = 3u | (0xAu << 4) | (6u << 8) | (1u << 12) | (0x2u << 16);
        uint edx = (1u << 19) | (1u << 26) | (1u << 28);
        uint ecx = 1u | (1u << 9) | (1u << 28);
        var leaves = new Dictionary<(int, int), uint[]>
        {
            [(0, 0)] = new uint[] { 1, Id('G', 'e', 'n', 'u'), Id('n', 't', 'e', 'l'), Id('i', 'n', 'e', 'I') },
            [(1, 0)] = new uint[] { eax6, 0x0800, ecx, edx },
            [(unchecked((int)0x80000000), 0)] = new uint[] { 0x80000004, 0, 0, 0 },
        };
        uint[][] brand = Brand("  Intel   Core  ");
        leaves[(unchecked((int)0x80000002), 0)] = brand[0];
        leaves[(unchecked((int)0x80000003), 0)] = brand[1];
        leaves[(unchecked((int)0x80000004), 0)] = brand[2];
        cpu.Check((function, subleaf) => leaves[(function, subleaf)]);
        Assert.Equal(0, cpu.IsAmd);
        Assert.Equal(6, cpu.Family);
        Assert.Equal(42, cpu.Model);
        Assert.Equal(3, cpu.Stepping);
        Assert.Equal(1, cpu.Type);
        Assert.Equal(1, cpu.HyperThreading);
        Assert.Equal(64, cpu.DataL1LineSize);
        Assert.Equal(edx, cpu.Flags[2]);
        Assert.Equal(
            new (int, int)[]
            {
                (0, 0),
                (1, 0),
                (unchecked((int)0x80000000), 0),
                (unchecked((int)0x80000002), 0),
                (unchecked((int)0x80000003), 0),
                (unchecked((int)0x80000004), 0),
            },
            cpu.Calls);
        Assert.Equal(
            "CPU Vendor ID: GenuineIntel\n  Name: Intel Core \n  Family 6, Model 42, Stepping 3\n  Features: SSE2 SSE3 SSSE3 AVX HyperThreading\n",
            cpu.Dump(true));
        Assert.Equal(cpu.Dump(true), cpu.Dump(false));

        uint eax15 = 1u | (0x1u << 4) | (15u << 8) | (2u << 20);
        leaves[(0, 0)] = new uint[] { 20, Id('G', 'e', 'n', 'u'), Id('n', 't', 'e', 'l'), Id('i', 'n', 'e', 'I') };
        leaves[(1, 0)] = new uint[] { eax15, 0, 0, 0 };
        leaves[(unchecked((int)0x80000000), 0)] = new uint[] { 0x80000000, 0, 0, 0 };
        leaves[(7, 0)] = new uint[] { 0, 1u << 16, 9, 8 };
        leaves[(7, 1)] = new uint[] { 0x55, 0, 0, 0 };
        cpu.Check((function, subleaf) => leaves[(function, subleaf)]);
        Assert.Equal(17, cpu.Family);
        Assert.Equal(1, cpu.Model);
        Assert.Equal(64, cpu.DataL1LineSize);
        Assert.Equal(0, cpu.HyperThreading);
        Assert.Equal(1u << 16, cpu.Flags[4]);
        Assert.Equal(9u, cpu.Flags[5]);
        Assert.Equal(8u, cpu.Flags[6]);
        Assert.Equal(0x55u, cpu.Flags[7]);
        Assert.DoesNotContain(cpu.Calls, call => call.Function == unchecked((int)0x80000002));
        Assert.Contains(cpu.Calls, call => call == (7, 1));
        Assert.Equal(
            "CPU Vendor ID: GenuineIntel\n  Family 17, Model 1, Stepping 1\n  Features: AVX512\n",
            cpu.Dump(false));
    }

    [Fact]
    public void Dump_CondensesTheBrandAndPrintsAmdFamily()
    {
        uint eax = 4u | (0x2u << 4) | (15u << 8) | (1u << 20) | (0x3u << 16);
        var leaves = new Dictionary<(int, int), uint[]>
        {
            [(0, 0)] = new uint[] { 7, Id('A', 'u', 't', 'h'), Id('c', 'A', 'M', 'D'), Id('e', 'n', 't', 'i') },
            [(1, 0)] = new uint[] { 0x600, 0x0800, 1u << 29, 1u << 19 },
            [(unchecked((int)0x80000000), 0)] = new uint[] { 0x80000005, 0, 0, 0 },
            [(unchecked((int)0x80000005), 0)] = new uint[] { 0, 0, 0x11223344, 0 },
            [(unchecked((int)0x80000001), 0)] = new uint[] { eax, 0, 0, 0xABCDu },
            [(7, 0)] = new uint[] { 0, (1u << 3) | (1u << 5) | (1u << 8), 0, 0 },
            [(7, 1)] = new uint[] { 0, 0, 0, 0 },
        };
        uint[][] brand = Brand("AMD  CPU");
        leaves[(unchecked((int)0x80000002), 0)] = brand[0];
        leaves[(unchecked((int)0x80000003), 0)] = brand[1];
        leaves[(unchecked((int)0x80000004), 0)] = brand[2];
        var cpu = new VmCpuId1315();
        cpu.Check((function, subleaf) => leaves[(function, subleaf)]);
        Assert.Equal(1, cpu.IsAmd);
        Assert.Equal(16, cpu.AmdFamily);
        Assert.Equal(0x32, cpu.AmdModel);
        Assert.Equal(4, cpu.AmdStepping);
        Assert.Equal(0x11223344u, cpu.AmdDataL1);
        Assert.Equal(0x44, cpu.DataL1LineSize);
        Assert.Equal(0xABCDu, cpu.Flags[3]);
        Assert.Equal(
            new (int, int)[]
            {
                (0, 0),
                (1, 0),
                (unchecked((int)0x80000000), 0),
                (unchecked((int)0x80000002), 0),
                (unchecked((int)0x80000003), 0),
                (unchecked((int)0x80000004), 0),
                (unchecked((int)0x80000005), 0),
                (unchecked((int)0x80000001), 0),
                (7, 0),
                (7, 1),
            },
            cpu.Calls);
        Assert.Equal(
            "CPU Vendor ID: AuthenticAMD\n  Name: AMD CPU\n  Family 6 (16), Model 50, Stepping 4\n  Features: AVX2 F16C BMI1 BMI2\n",
            cpu.Dump(true));

        cpu.Vendor[0] = 0;
        Assert.Equal("", cpu.Dump(false));
        for (int i = 0; i < 16; i++)
            cpu.Vendor[i] = (byte)'V';
        cpu.Brand[0] = (byte)'Z';
        cpu.Brand[1] = 0;
        Assert.StartsWith("CPU Vendor ID: VVVVVVVVVVVVVVVVZ\n", cpu.Dump(false));
    }

    static uint Id(char a, char b, char c, char d)
    {
        return (uint)a | ((uint)b << 8) | ((uint)c << 16) | ((uint)d << 24);
    }

    static uint[][] Brand(string text)
    {
        var bytes = new byte[48];
        for (int i = 0; i < text.Length && i < bytes.Length; i++)
            bytes[i] = (byte)text[i];
        var parts = new uint[3][];
        for (int part = 0; part < 3; part++)
        {
            parts[part] = new uint[4];
            for (int word = 0; word < 4; word++)
            {
                int offset = (part * 16) + (word * 4);
                parts[part][word] = bytes[offset]
                    | ((uint)bytes[offset + 1] << 8)
                    | ((uint)bytes[offset + 2] << 16)
                    | ((uint)bytes[offset + 3] << 24);
            }
        }

        return parts;
    }
}
