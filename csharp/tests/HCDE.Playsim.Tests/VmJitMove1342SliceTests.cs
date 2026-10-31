using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmJitMove1342SliceTests
{
    [Fact]
    public void MovesAndNumericCasts()
    {
        var vm = NewVm();
        vm.RegD[1] = -1;
        vm.EmitMOVE();
        Assert.Equal(-1, vm.RegD[3]);

        vm.RegF[1] = -0.0;
        vm.EmitMOVEF();
        Assert.Equal(unchecked((ulong)BitConverter.DoubleToInt64Bits(-0.0)), unchecked((ulong)BitConverter.DoubleToInt64Bits(vm.RegF[3])));

        vm.RegS[1] = "troop";
        vm.EmitMOVES();
        Assert.Equal("troop", vm.RegS[3]);
        vm.RegS[1] = "next";
        Assert.Equal("troop", vm.RegS[3]);
        vm.RegS[1] = null;
        Assert.Throws<NullReferenceException>(() => vm.EmitMOVES());

        vm.RegA[1] = 0x99;
        vm.EmitMOVEA();
        Assert.Equal(0x99, vm.RegA[3]);

        vm.RegF[0] = 1;
        vm.RegF[1] = 2;
        vm.RegF[2] = 3;
        vm.A = 1;
        vm.B = 0;
        vm.EmitMOVEV2();
        Assert.Equal(1, vm.RegF[1]);
        Assert.Equal(1, vm.RegF[2]);

        vm.RegF[4] = 4;
        vm.RegF[5] = 5;
        vm.RegF[6] = 6;
        vm.RegF[7] = 7;
        vm.A = 0;
        vm.B = 4;
        vm.EmitMOVEV3();
        Assert.Equal(4, vm.RegF[0]);
        Assert.Equal(5, vm.RegF[1]);
        Assert.Equal(6, vm.RegF[2]);
        vm.EmitMOVEV4();
        Assert.Equal(7, vm.RegF[3]);

        vm.A = 3;
        vm.B = 1;
        vm.C = (byte)VmJitMove1342.CastI2F;
        vm.RegD[1] = -2;
        vm.EmitCAST();
        Assert.Equal(-2, vm.RegF[3]);

        vm.C = (byte)VmJitMove1342.CastU2F;
        vm.RegD[1] = -1;
        vm.EmitCAST();
        Assert.Equal(4294967295.0, vm.RegF[3]);

        vm.C = (byte)VmJitMove1342.CastF2I;
        vm.RegF[1] = -1.9;
        vm.EmitCAST();
        Assert.Equal(-1, vm.RegD[3]);
        vm.RegF[1] = double.NaN;
        vm.EmitCAST();
        Assert.Equal(int.MinValue, vm.RegD[3]);
        vm.RegF[1] = 2147483648.0;
        vm.EmitCAST();
        Assert.Equal(int.MinValue, vm.RegD[3]);
        vm.RegF[1] = -2147483648.5;
        vm.EmitCAST();
        Assert.Equal(int.MinValue, vm.RegD[3]);

        vm.C = (byte)VmJitMove1342.CastF2U;
        vm.RegF[1] = 4294967296.0 + 5;
        vm.EmitCAST();
        Assert.Equal(5, vm.RegD[3]);
        vm.RegF[1] = double.NaN;
        vm.EmitCAST();
        Assert.Equal(0, vm.RegD[3]);
        vm.RegF[1] = -1.9;
        vm.EmitCAST();
        Assert.Equal(-1, vm.RegD[3]);

        vm.C = (byte)VmJitMove1342.CastI2S;
        vm.RegD[1] = int.MinValue;
        vm.EmitCAST();
        Assert.Equal("-2147483648", vm.RegS[3]);
        vm.C = (byte)VmJitMove1342.CastU2S;
        vm.RegD[1] = -1;
        vm.EmitCAST();
        Assert.Equal("4294967295", vm.RegS[3]);

        vm.C = (byte)VmJitMove1342.CastF2S;
        vm.RegF[1] = 1.0 / 3.0;
        vm.EmitCAST();
        Assert.Equal("0.33333", vm.RegS[3]);
        vm.RegF[1] = -0.0;
        vm.EmitCAST();
        Assert.Equal("-0.00000", vm.RegS[3]);
        vm.RegF[1] = double.PositiveInfinity;
        vm.EmitCAST();
        Assert.Equal("Inf", vm.RegS[3]);
        vm.RegF[1] = double.NaN;
        vm.EmitCAST();
        Assert.Equal("NaN", vm.RegS[3]);
        vm.RegF[1] = 1.25;
        vm.EmitCAST();
        Assert.Equal("1.25000", vm.RegS[3]);

        vm.C = (byte)VmJitMove1342.CastV22S;
        vm.RegF[1] = 1.0 / 3.0;
        vm.RegF[2] = -0.0;
        vm.EmitCAST();
        Assert.Equal("(0.33333, -0.00000)", vm.RegS[3]);
        vm.C = (byte)VmJitMove1342.CastV32S;
        vm.RegF[3] = 1.5;
        vm.EmitCAST();
        Assert.Equal("(0.33333, -0.00000, 1.50000)", vm.RegS[3]);
        vm.C = (byte)VmJitMove1342.CastV42S;
        vm.RegF[4] = 2;
        vm.EmitCAST();
        Assert.Equal("(0.33333, -0.00000, 1.50000, 2.00000)", vm.RegS[3]);

        vm.C = (byte)VmJitMove1342.CastP2S;
        vm.RegA[1] = 0;
        vm.EmitCAST();
        Assert.Equal("null", vm.RegS[3]);
        vm.RegA[1] = 0x99;
        vm.EmitCAST();
        Assert.Equal("0000000000000099", vm.RegS[3]);
        vm.RegA[1] = unchecked((long)0x1122334455667788);
        vm.EmitCAST();
        Assert.Equal("1122334455667788", vm.RegS[3]);

        vm.C = (byte)VmJitMove1342.CastS2I;
        vm.RegS[1] = "010";
        vm.EmitCAST();
        Assert.Equal(8, vm.RegD[3]);
        vm.RegS[1] = "0x10";
        vm.EmitCAST();
        Assert.Equal(16, vm.RegD[3]);
        vm.RegS[1] = "  -12abc";
        vm.EmitCAST();
        Assert.Equal(-12, vm.RegD[3]);
        vm.RegS[1] = "";
        vm.EmitCAST();
        Assert.Equal(0, vm.RegD[3]);
        vm.RegS[1] = "9223372036854775807";
        vm.EmitCAST();
        Assert.Equal(-1, vm.RegD[3]);

        vm.C = (byte)VmJitMove1342.CastS2F;
        vm.RegS[1] = "  1.5e1xyz";
        vm.EmitCAST();
        Assert.Equal(15, vm.RegF[3]);
        vm.RegS[1] = "0x1.8p1";
        vm.EmitCAST();
        Assert.Equal(3, vm.RegF[3]);
        vm.RegS[1] = "08";
        vm.EmitCAST();
        Assert.Equal(8, vm.RegF[3]);
        vm.RegS[1] = "nan";
        vm.EmitCAST();
        Assert.True(double.IsNaN(vm.RegF[3]));
        vm.RegS[1] = "";
        vm.EmitCAST();
        Assert.Equal(0, vm.RegF[3]);

        vm.C = (byte)VmJitMove1342.CastBInt;
        vm.RegD[1] = 0;
        vm.EmitCASTB();
        Assert.Equal(0, vm.RegD[3]);
        vm.RegD[1] = -5;
        vm.EmitCASTB();
        Assert.Equal(1, vm.RegD[3]);
        vm.C = (byte)VmJitMove1342.CastBFloat;
        vm.RegF[1] = -0.0;
        vm.EmitCASTB();
        Assert.Equal(0, vm.RegD[3]);
        vm.RegF[1] = double.NaN;
        vm.EmitCASTB();
        Assert.Equal(1, vm.RegD[3]);
        vm.C = (byte)VmJitMove1342.CastBPointer;
        vm.RegA[1] = 0;
        vm.EmitCASTB();
        Assert.Equal(0, vm.RegD[3]);
        vm.RegA[1] = 4;
        vm.EmitCASTB();
        Assert.Equal(1, vm.RegD[3]);
        vm.C = (byte)VmJitMove1342.CastBString;
        vm.RegS[1] = "";
        vm.EmitCASTB();
        Assert.Equal(0, vm.RegD[3]);
        vm.RegS[1] = "x";
        vm.EmitCASTB();
        Assert.Equal(1, vm.RegD[3]);
        vm.C = 99;
        var unknown = Assert.Throws<InvalidOperationException>(() => vm.EmitCAST());
        Assert.Equal("Unknown OP_CAST type\n", unknown.Message);
    }

    [Fact]
    public void NamesColorsAndDynamicCasts()
    {
        var vm = NewVm();
        int nameCalls = 0;
        vm.NumNames = 3;
        vm.NameIndex = text =>
        {
            nameCalls++;
            return text == "Health" ? 2 : 9;
        };
        vm.NameChars = index => index == 2 ? "Health" : "None";
        vm.C = (byte)VmJitMove1342.CastS2N;
        vm.RegS[1] = "";
        vm.EmitCAST();
        Assert.Equal(VmJitMove1342.NameNone, vm.RegD[3]);
        Assert.Equal(0, nameCalls);
        vm.RegS[1] = "Health";
        vm.EmitCAST();
        Assert.Equal(2, vm.RegD[3]);
        Assert.Equal(1, nameCalls);

        vm.C = (byte)VmJitMove1342.CastN2S;
        vm.RegD[1] = 2;
        vm.EmitCAST();
        Assert.Equal("Health", vm.RegS[3]);
        vm.RegD[1] = -1;
        vm.EmitCAST();
        Assert.Equal("", vm.RegS[3]);
        vm.RegD[1] = 3;
        vm.EmitCAST();
        Assert.Equal("", vm.RegS[3]);

        vm.ColorOf = text => text == "ff 00 00" ? 0x00FF0000 : 7;
        vm.C = (byte)VmJitMove1342.CastS2Co;
        vm.RegS[1] = "ff 00 00";
        vm.EmitCAST();
        Assert.Equal(0x00FF0000, vm.RegD[3]);
        vm.C = (byte)VmJitMove1342.CastCo2S;
        vm.RegD[1] = 0x00FF8001;
        vm.EmitCAST();
        Assert.Equal("ff 80 01", vm.RegS[3]);

        vm.SoundIndex = text => text == "weapons/pistol" ? 12 : -1;
        vm.SoundName = index => index == 12 ? "weapons/pistol" : "";
        vm.C = (byte)VmJitMove1342.CastS2So;
        vm.RegS[1] = "weapons/pistol";
        vm.EmitCAST();
        Assert.Equal(12, vm.RegD[3]);
        vm.C = (byte)VmJitMove1342.CastSo2S;
        vm.RegD[1] = 12;
        vm.EmitCAST();
        Assert.Equal("weapons/pistol", vm.RegS[3]);

        vm.C = (byte)VmJitMove1342.CastSid2S;
        vm.RegD[1] = -1;
        vm.EmitCAST();
        Assert.Equal("-1", vm.RegS[3]);
        vm.SpriteString = id => "SPR" + id.ToString();
        vm.EmitCAST();
        Assert.Equal("SPR4294967295", vm.RegS[3]);

        vm.TextureName = id => id == 4 ? "STARTAN" : null;
        vm.C = (byte)VmJitMove1342.CastTid2S;
        vm.RegD[1] = 4;
        vm.EmitCAST();
        Assert.Equal("STARTAN", vm.RegS[3]);
        vm.RegD[1] = 5;
        vm.EmitCAST();
        Assert.Equal("(null)", vm.RegS[3]);

        vm.IsKindOf = (obj, cls) => obj == 10 && cls == 20;
        vm.IsDescendantOf = (cls1, cls2) => cls1 == 30 && cls2 == 40;
        vm.A = 3;
        vm.B = 1;
        vm.C = 2;
        vm.RegA[1] = 0;
        vm.RegA[2] = 20;
        vm.EmitDYNCAST_R();
        Assert.Equal(0, vm.RegA[3]);
        vm.RegA[1] = 10;
        vm.EmitDYNCAST_R();
        Assert.Equal(10, vm.RegA[3]);
        vm.RegA[2] = 21;
        vm.EmitDYNCAST_R();
        Assert.Equal(0, vm.RegA[3]);
        vm.KonstA[2] = 20;
        vm.EmitDYNCAST_K();
        Assert.Equal(10, vm.RegA[3]);

        vm.RegA[1] = 0;
        vm.RegA[2] = 40;
        vm.EmitDYNCASTC_R();
        Assert.Equal(0, vm.RegA[3]);
        vm.RegA[1] = 30;
        vm.EmitDYNCASTC_R();
        Assert.Equal(30, vm.RegA[3]);
        vm.KonstA[2] = 40;
        vm.RegA[2] = 0;
        vm.EmitDYNCASTC_K();
        Assert.Equal(30, vm.RegA[3]);
        vm.KonstA[2] = 41;
        vm.EmitDYNCASTC_K();
        Assert.Equal(0, vm.RegA[3]);
    }

    static VmJitMove1342 NewVm()
    {
        var vm = new VmJitMove1342();
        vm.A = 3;
        vm.B = 1;
        vm.C = 2;
        return vm;
    }
}
