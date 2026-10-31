namespace HCDE.Playsim.Tests;

public class VmJitFlow1331SliceTests
{
    [Fact]
    public void Flow_BranchesScopeAndIndexedJump()
    {
        var vm = new VmJitFlow1331();
        vm.Code.Add(new VmJitFlow1331.Insn { A = 3, I16u = 65535 });
        vm.EmitTest();
        Assert.Equal(new[] { "cmp d3 65535", "jne 2" }, vm.Ops);

        vm.Ops.Clear();
        vm.Pc = 0;
        vm.Code[0].I16u = 4;
        vm.EmitTestN();
        Assert.Equal(new[] { "cmp d3 -4", "jne 2" }, vm.Ops);

        vm.Ops.Clear();
        vm.Code[0].I24 = -3;
        vm.EmitJmp();
        Assert.Equal("jmp -2", vm.Ops[0]);
        vm.Ops.Clear();
        vm.Code[0].I24 = 5;
        vm.EmitJmp();
        Assert.Equal("jmp 6", vm.Ops[0]);

        vm.Ops.Clear();
        vm.Code.Clear();
        vm.Code.Add(new VmJitFlow1331.Insn { A = 2, I16 = 3 });
        vm.Code.Add(new VmJitFlow1331.Insn { Op = VmJitFlow1331.OpJmp, I24 = 4 });
        vm.Code.Add(new VmJitFlow1331.Insn { Op = 1, I24 = 9 });
        vm.Code.Add(new VmJitFlow1331.Insn { Op = VmJitFlow1331.OpJmp, I24 = -1 });
        vm.Pc = 0;
        vm.EmitIjmp();
        Assert.Equal(3, vm.Pc);
        Assert.Equal(
            new[]
            {
                "mov val d2",
                "cmp val 0",
                "je 6",
                "cmp val 2",
                "je 3",
                "throw 0",
            },
            vm.Ops);
        Assert.Equal(92, VmJitFlow1331.OpJmp);

        vm.Ops.Clear();
        vm.Code.Clear();
        for (int i = 0; i < 4; i++)
            vm.Code.Add(new VmJitFlow1331.Insn());
        vm.Code.Add(new VmJitFlow1331.Insn { A = 1, I16 = 0 });
        vm.Pc = 4;
        vm.EmitIjmp();
        Assert.Equal(4, vm.Pc);
        Assert.Equal(new[] { "mov val d1", "throw 0" }, vm.Ops);

        var scope = new VmJitFlow1331();
        scope.PcToLine = pc => pc + 40;
        scope.Code.Add(new VmJitFlow1331.Insn { A = 6, B = 3, C = 8 });
        scope.KonstA[8] = 77;
        scope.EmitScope();
        Assert.Equal(new VmJitFlow1331.LineNote(1, 40), scope.Lines[0]);
        Assert.Equal(
            new[]
            {
                "test a6",
                "jz 1",
                "mov f konstA8",
                "call ValidateCall a6 f 3",
                "L1 throw 1",
            },
            scope.Ops);
        Assert.Equal(2, VmJitFlow1331.ValidateCallArgument(3));
        Assert.Equal(-1, VmJitFlow1331.ValidateCallArgument(0));

        scope.Ops.Clear();
        scope.Code[0].I16u = 9;
        scope.EmitThrow();
        Assert.Equal("throw 9", scope.Ops[0]);
        Assert.Single(scope.Lines);
    }

    [Fact]
    public void Return_StoresValuesAndBounds()
    {
        var nil = Compiler();
        nil.Code.Add(new VmJitFlow1331.Insn { B = VmJitFlow1331.RegNil });
        nil.EmitRet();
        Assert.Equal(new[] { "pop", "mov v 0", "ret 0" }, nil.Ops);

        var stored = Compiler();
        stored.NumRet = 2;
        stored.RegD[4] = 11;
        stored.KonstD[5] = -6;
        stored.Code.Add(new VmJitFlow1331.Insn
        {
            A = 1,
            B = VmJitFlow1331.RegInt,
            C = 4,
        });
        stored.EmitRet();
        Assert.Contains("store-dword 1 11 at 16", stored.Ops);
        Assert.DoesNotContain(stored.Ops, line => line.StartsWith("pop", StringComparison.Ordinal));

        stored.Ops.Clear();
        stored.Code[0].B = VmJitFlow1331.RegInt | VmJitFlow1331.RegKonst;
        stored.Code[0].C = 5;
        stored.EmitRet();
        Assert.Contains("store-dword 1 -6 at 16", stored.Ops);

        stored.Ops.Clear();
        stored.Code[0].A = 1 | VmJitFlow1331.RetFinal;
        stored.Code[0].B = VmJitFlow1331.RegInt;
        stored.Code[0].C = 4;
        stored.EmitRet();
        Assert.Contains("ret 2", stored.Ops);
        Assert.Contains("endif 3", stored.Ops);
        Assert.Equal(2, stored.Ops.Count(line => line == "pop"));

        stored.Ops.Clear();
        stored.Code[0].A = 5 | VmJitFlow1331.RetFinal;
        stored.EmitRet();
        Assert.Contains("store-dword 5 11 at 80", stored.Ops);
        Assert.Contains("jge 4", stored.Ops);
        Assert.Contains("ret 6", stored.Ops);
        Assert.Contains("ret 2", stored.Ops);
        Assert.Contains("cmp retnum 2", stored.Ops);

        var wide = Compiler();
        wide.NumRet = 1;
        wide.KonstBits[2] = 1;
        wide.KonstBits[3] = 2;
        wide.KonstBits[4] = 3;
        wide.KonstBits[5] = 4;
        wide.Code.Add(new VmJitFlow1331.Insn
        {
            A = 0 | VmJitFlow1331.RetFinal,
            B = VmJitFlow1331.RegFloat | VmJitFlow1331.RegKonst | VmJitFlow1331.RegMulti4,
            C = 2,
        });
        wide.EmitRet();
        Assert.Contains("mov tmp 1", wide.Ops);
        Assert.Contains("store-qword 0+24 4", wide.Ops);
        Assert.Equal(4, wide.Ops.Count(line => line.StartsWith("store-qword", StringComparison.Ordinal)));
        Assert.Contains("ret 1", wide.Ops);

        wide.Ops.Clear();
        wide.Code[0].B = VmJitFlow1331.RegFloat | VmJitFlow1331.RegMulti3;
        wide.RegF[2] = 1.5;
        wide.RegF[3] = -2.5;
        wide.RegF[4] = 0.5;
        wide.EmitRet();
        Assert.Equal(3, wide.Ops.Count(line => line.StartsWith("movsd", StringComparison.Ordinal)));
        Assert.Contains(
            "movsd 0+0 f2 " + BitConverter.DoubleToInt64Bits(1.5).ToString(),
            wide.Ops);
        Assert.Contains(
            "movsd 0+16 f4 " + BitConverter.DoubleToInt64Bits(0.5).ToString(),
            wide.Ops);

        wide.Ops.Clear();
        wide.Code[0].A = 0;
        wide.Code[0].B = VmJitFlow1331.RegFloat | VmJitFlow1331.RegMulti2;
        wide.EmitRet();
        Assert.Equal(2, wide.Ops.Count(line => line.StartsWith("movsd", StringComparison.Ordinal)));
        Assert.DoesNotContain(wide.Ops, line => line.StartsWith("pop", StringComparison.Ordinal));

        wide.Ops.Clear();
        wide.Code[0].B = VmJitFlow1331.RegString | VmJitFlow1331.RegKonst;
        wide.Code[0].C = 1;
        wide.KonstS[1] = "hi";
        wide.EmitRet();
        Assert.Contains("add ptr 0", wide.Ops);
        Assert.Contains("call SetString 0 konstS1", wide.Ops);
        wide.SetString(0, "hi");
        Assert.Equal("hi", wide.Strings[0]);

        wide.Ops.Clear();
        wide.Host64 = false;
        wide.Code64 = false;
        wide.Code[0].B = VmJitFlow1331.RegPointer;
        wide.Code[0].C = 3;
        wide.RegA[3] = 0x1_0000_0004;
        wide.EmitRet();
        Assert.Contains("store-dword 0 4294967300 at 0", wide.Ops);

        wide.Ops.Clear();
        wide.Code64 = true;
        wide.Host64 = true;
        wide.Code[0].B = VmJitFlow1331.RegPointer | VmJitFlow1331.RegKonst;
        wide.KonstA[3] = 99;
        wide.EmitRet();
        Assert.Contains("mov ptr 99", wide.Ops);
        Assert.Contains("store-qword 0 99 at 0", wide.Ops);

        var imm = Compiler();
        imm.NumRet = 1;
        imm.Code.Add(new VmJitFlow1331.Insn { A = VmJitFlow1331.RetFinal, I16 = -2, I16u = 65534 });
        imm.EmitRetI();
        Assert.Contains("store-dword 0 -2 at 0", imm.Ops);
        Assert.Contains("ret 1", imm.Ops);
        Assert.Contains("ret 1", imm.Ops.Where(line => line.StartsWith("ret", StringComparison.Ordinal)));

        var bounds = Compiler();
        bounds.PcToLine = pc => pc * 3;
        for (int i = 0; i < 7; i++)
            bounds.Code.Add(new VmJitFlow1331.Insn());
        bounds.Code.Add(new VmJitFlow1331.Insn { A = 1, B = 2, I16u = 8 });
        bounds.Pc = 7;
        bounds.RegD[1] = 8;
        bounds.EmitBound();
        Assert.Equal(new VmJitFlow1331.LineNote(1, 21), bounds.Lines[0]);
        Assert.Equal(
            new[] { "cmp d1 8", "jae 1", "L1 call ThrowArrayOutOfBounds d1 8" },
            bounds.Ops);
        Assert.Equal("Size = 8, current index = 8\n", bounds.ThrowArrayOutOfBounds(8, 8));
        Assert.Equal("Size = 4294967295, current index = 5\n", bounds.ThrowArrayOutOfBounds(5, -1));
        Assert.Equal("Negative current index = -4\n", bounds.ThrowArrayOutOfBounds(-4, 3));
        Assert.Equal("Negative current index = 2\n", bounds.ThrowArrayOutOfBounds(2, 9));

        bounds.Ops.Clear();
        bounds.KonstD[8] = 12;
        bounds.EmitBoundK();
        Assert.Equal("cmp d1 12", bounds.Ops[0]);
        Assert.Equal(new VmJitFlow1331.LineNote(2, 21), bounds.Lines[1]);

        bounds.Ops.Clear();
        bounds.EmitBoundR();
        Assert.Equal(
            new[] { "cmp d1 d2", "jae 3", "L3 call ThrowArrayOutOfBounds d1 d2" },
            bounds.Ops);

        Assert.Throws<InvalidOperationException>(() => new VmJitFlow1331().EmitBound());
    }

    static VmJitFlow1331 Compiler()
    {
        return new VmJitFlow1331
        {
            PcToLine = _ => 1,
        };
    }
}
