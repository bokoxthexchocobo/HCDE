using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmJitStore1341SliceTests
{
    [Fact]
    public void ScalarStoresNullAndStrings()
    {
        var vm = NewVm();
        vm.Bytes[0x2024] = 0xAB;
        vm.RegD[4] = -1;
        vm.EmitSB();
        AssertBits(vm, 0x2020, 0xFF, 1);
        Assert.False(vm.Bytes.ContainsKey(0x2021));

        vm.RegD[4] = 0x135;
        vm.EmitSB_R();
        AssertBits(vm, 0x2030, 0x35, 1);

        vm.RegD[4] = 0x123456;
        vm.EmitSH();
        AssertBits(vm, 0x2020, 0x3456, 2);

        vm.RegD[4] = -2;
        vm.EmitSH_R();
        AssertBits(vm, 0x2030, 0xFFFE, 2);

        vm.RegD[4] = unchecked((int)0xA1B2C3D4);
        vm.EmitSW();
        AssertBits(vm, 0x2020, 0xA1B2C3D4, 4);
        Assert.Equal(0xAB, vm.Bytes[0x2024]);

        vm.Bytes[0x2034] = 0xCD;
        vm.RegD[4] = -1;
        vm.EmitSW_R();
        AssertBits(vm, 0x2030, 0xFFFFFFFF, 4);
        Assert.Equal(0xCD, vm.Bytes[0x2034]);

        vm.RegF[4] = 0.1;
        vm.EmitSSP();
        AssertBits(vm, 0x2020, SingleBits(0.1), 4);
        Assert.Equal(0xAB, vm.Bytes[0x2024]);

        vm.RegF[4] = -0.0;
        vm.EmitSSP();
        AssertBits(vm, 0x2020, SingleBits(-0.0), 4);
        Assert.Equal(0x80000000u, SingleBits(-0.0));

        vm.RegF[4] = 1e300;
        vm.EmitSSP_R();
        AssertBits(vm, 0x2030, SingleBits(1e300), 4);
        Assert.True(float.IsPositiveInfinity(BitConverter.Int32BitsToSingle(unchecked((int)SingleBits(1e300)))));
        Assert.Equal(0xCD, vm.Bytes[0x2034]);

        vm.RegF[4] = double.NaN;
        vm.EmitSDP();
        AssertBits(vm, 0x2020, DoubleBits(double.NaN), 8);

        vm.RegF[4] = -0.0;
        vm.EmitSDP_R();
        AssertBits(vm, 0x2030, DoubleBits(-0.0), 8);
        Assert.Equal(0x8000000000000000UL, DoubleBits(-0.0));

        vm.KonstD[6] = -8;
        vm.RegD[4] = 0x7F;
        vm.EmitSB();
        Assert.Equal(0x7F, vm.Bytes[0x1FF8]);
        vm.KonstD[6] = 0x20;

        vm.RegD[6] = -16;
        vm.RegD[4] = 0x42;
        vm.EmitSB_R();
        Assert.Equal(0x42, vm.Bytes[0x1FF0]);
        vm.RegD[6] = 0x30;

        vm.RegA[2] = 0;
        vm.Bytes[0x20] = 0x11;
        int noted = vm.Lines.Count;
        var ex = Assert.Throws<VmJitStore1341.StoreAbortException>(() => vm.EmitSW());
        Assert.Equal(VmJitStore1341.XWriteNil, ex.Reason);
        Assert.Equal("X_WRITE_NIL", ex.Message);
        Assert.Equal(0x11, vm.Bytes[0x20]);
        Assert.Equal(noted + 1, vm.Lines.Count);
        Assert.Equal(113, vm.Lines[^1].Line);
        Assert.Equal(VmJitStore1341.XWriteNil, vm.Lines[^1].Reason);
        Assert.Empty(vm.Barriers);
        vm.RegA[2] = 0x2000;

        vm.RegS[4] = "troop";
        vm.EmitSS();
        Assert.Equal("troop", vm.Strings[0x2020]);
        vm.RegS[4] = "next";
        Assert.Equal("troop", vm.Strings[0x2020]);
        vm.RegD[6] = 0x40;
        vm.EmitSS_R();
        Assert.Equal("next", vm.Strings[0x2040]);
        vm.RegS[4] = "troop";
        Assert.Equal("next", vm.Strings[0x2040]);
        vm.RegS[4] = "";
        vm.EmitSS();
        Assert.Equal("", vm.Strings[0x2020]);
        vm.RegS[4] = null;
        Assert.Throws<NullReferenceException>(() => vm.EmitSS());
        Assert.Equal("", vm.Strings[0x2020]);

        vm.Pc = 4;
        vm.RegS[4] = "ok";
        vm.EmitSS();
        Assert.Equal(43, vm.Lines[^1].Line);
        Assert.Equal(vm.Lines.Count, vm.Lines[^1].Label);
        Assert.Equal(1, vm.Lines[0].Label);
    }

    [Fact]
    public void ObjectsVectorsAndBits()
    {
        var vm = NewVm();
        vm.RegA[4] = 0x1122334455667788;
        vm.EmitSO();
        AssertBits(vm, 0x2020, 0x1122334455667788, 8);
        Assert.Equal(new long[] { 0x1122334455667788 }, vm.Barriers);

        vm.RegA[4] = 0;
        vm.EmitSO_R();
        AssertBits(vm, 0x2030, 0, 8);
        Assert.Equal(0L, vm.Barriers[^1]);

        vm.RegA[4] = 0x99;
        vm.EmitSP();
        AssertBits(vm, 0x2020, 0x99, 8);
        vm.EmitSP_R();
        AssertBits(vm, 0x2030, 0x99, 8);
        Assert.Equal(2, vm.Barriers.Count);

        int barriers = vm.Barriers.Count;
        vm.RegA[2] = 0;
        Assert.Throws<VmJitStore1341.StoreAbortException>(() => vm.EmitSO());
        Assert.Equal(barriers, vm.Barriers.Count);
        vm.RegA[2] = 0x2000;

        vm.RegF[4] = 1.5;
        vm.RegF[5] = -2.5;
        vm.RegF[6] = 4.25;
        vm.RegF[7] = double.PositiveInfinity;
        vm.Bytes[0x2030] = 0xEE;
        vm.EmitSV2();
        AssertBits(vm, 0x2020, DoubleBits(1.5), 8);
        AssertBits(vm, 0x2028, DoubleBits(-2.5), 8);
        Assert.Equal(0xEE, vm.Bytes[0x2030]);

        vm.Bytes[0x2040] = 0xEF;
        vm.EmitSV2_R();
        AssertBits(vm, 0x2030, DoubleBits(1.5), 8);
        AssertBits(vm, 0x2038, DoubleBits(-2.5), 8);
        Assert.Equal(0xEF, vm.Bytes[0x2040]);

        vm.Bytes[0x2038] = 0xD1;
        vm.EmitSV3();
        AssertBits(vm, 0x2020, DoubleBits(1.5), 8);
        AssertBits(vm, 0x2028, DoubleBits(-2.5), 8);
        AssertBits(vm, 0x2030, DoubleBits(4.25), 8);
        Assert.Equal(0xD1, vm.Bytes[0x2038]);

        vm.Bytes[0x2048] = 0xD2;
        vm.EmitSV3_R();
        AssertBits(vm, 0x2040, DoubleBits(4.25), 8);
        Assert.Equal(0xD2, vm.Bytes[0x2048]);

        vm.Bytes[0x2040] = 0xD3;
        vm.EmitSV4();
        AssertBits(vm, 0x2020 + 24, DoubleBits(double.PositiveInfinity), 8);
        Assert.Equal(0xD3, vm.Bytes[0x2040]);

        vm.Bytes[0x2050] = 0xD4;
        vm.EmitSV4_R();
        AssertBits(vm, 0x2030, DoubleBits(1.5), 8);
        AssertBits(vm, 0x2030 + 24, DoubleBits(double.PositiveInfinity), 8);
        Assert.Equal(0xD4, vm.Bytes[0x2050]);

        vm.RegF[4] = 1.0 / 3.0;
        vm.RegF[5] = -1.0 / 3.0;
        vm.RegF[6] = 2.5;
        vm.RegF[7] = -0.0;
        vm.Bytes[0x2028] = 0x5A;
        vm.EmitSFV2();
        AssertBits(vm, 0x2020, SingleBits(1.0 / 3.0), 4);
        AssertBits(vm, 0x2024, SingleBits(-1.0 / 3.0), 4);
        Assert.Equal(0x5A, vm.Bytes[0x2028]);

        vm.Bytes[0x2038] = 0x5B;
        vm.EmitSFV2_R();
        AssertBits(vm, 0x2030, SingleBits(1.0 / 3.0), 4);
        AssertBits(vm, 0x2034, SingleBits(-1.0 / 3.0), 4);
        Assert.Equal(0x5B, vm.Bytes[0x2038]);

        vm.Bytes[0x202C] = 0x5C;
        vm.EmitSFV3();
        AssertBits(vm, 0x2028, SingleBits(2.5), 4);
        Assert.Equal(0x5C, vm.Bytes[0x202C]);

        vm.Bytes[0x203C] = 0x5D;
        vm.EmitSFV3_R();
        AssertBits(vm, 0x2038, SingleBits(2.5), 4);
        Assert.Equal(0x5D, vm.Bytes[0x203C]);

        vm.Bytes[0x2030] = 0x5E;
        vm.EmitSFV4();
        AssertBits(vm, 0x202C, SingleBits(-0.0), 4);
        Assert.Equal(0x5E, vm.Bytes[0x2030]);

        vm.Bytes[0x2040] = 0x5F;
        vm.EmitSFV4_R();
        AssertBits(vm, 0x203C, SingleBits(-0.0), 4);
        Assert.Equal(0x5F, vm.Bytes[0x2040]);

        vm.Bytes[0x2000] = 0x3C;
        vm.Bytes[0x2020] = 0x01;
        vm.C = 0xF0;
        vm.RegD[4] = 1;
        vm.EmitSBIT();
        Assert.Equal(0xFC, vm.Bytes[0x2000]);
        Assert.Equal(0x01, vm.Bytes[0x2020]);

        vm.Bytes[0x2000] = 0x3C;
        vm.RegD[4] = 0;
        vm.EmitSBIT();
        Assert.Equal(0x0C, vm.Bytes[0x2000]);

        vm.Bytes[0x2000] = 0x3C;
        vm.RegD[4] = -1;
        vm.EmitSBIT();
        Assert.Equal(0xFC, vm.Bytes[0x2000]);

        vm.Bytes[0x2000] = 0x3C;
        vm.C = 0;
        vm.RegD[4] = 0;
        vm.EmitSBIT();
        Assert.Equal(0x3C, vm.Bytes[0x2000]);
        vm.RegD[4] = 256;
        vm.EmitSBIT();
        Assert.Equal(0x3C, vm.Bytes[0x2000]);

        vm.RegA[2] = 0x4000;
        var missing = Assert.Throws<InvalidOperationException>(() => vm.EmitSBIT());
        Assert.Contains("unmapped", missing.Message);
        Assert.Equal(vm.Lines.Count, vm.Lines[^1].Label);
    }

    static VmJitStore1341 NewVm()
    {
        var vm = new VmJitStore1341();
        vm.PcToLine = pc => pc * 10 + 3;
        vm.A = 2;
        vm.B = 4;
        vm.C = 6;
        vm.Pc = 11;
        vm.RegA[2] = 0x2000;
        vm.KonstD[6] = 0x20;
        vm.RegD[6] = 0x30;
        return vm;
    }

    static uint SingleBits(double value)
    {
        return unchecked((uint)BitConverter.SingleToInt32Bits((float)value));
    }

    static ulong DoubleBits(double value)
    {
        return unchecked((ulong)BitConverter.DoubleToInt64Bits(value));
    }

    static void AssertBits(VmJitStore1341 vm, long addr, ulong bits, int width)
    {
        for (int i = 0; i < width; i++)
        {
            Assert.True(vm.Bytes.TryGetValue(addr + i, out byte stored));
            Assert.Equal((byte)(bits >> (8 * i)), stored);
        }
    }
}
