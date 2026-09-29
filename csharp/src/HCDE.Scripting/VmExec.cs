namespace HCDE.Scripting;

/// <summary>
/// Integer slice of <c>src/common/scripting/vm/vmexec.cpp</c>.
/// Words match the little-endian <c>VMOP</c> layout. Floats, strings, objects,
/// calls, and the JIT are not executed. An unknown opcode stops the function.
/// </summary>
public static class VmExec
{
    public const int Budget = 256;
    private const int Nil = 128;

    public static VmResult Run(ReadOnlySpan<int> code, ReadOnlySpan<int> constants)
    {
        var reg = new int[256];
        var pc = 0;
        var steps = 0;
        while ((uint)pc < (uint)code.Length)
        {
            if (++steps > Budget)
                return new VmResult(false, 0, VmFault.Budget);

            var word = code[pc];
            var op = word & 0xFF;
            var a = (word >> 8) & 0xFF;
            var b = (word >> 16) & 0xFF;
            var c = (word >> 24) & 0xFF;
            var bc = (ushort)(word >> 16);
            switch (op)
            {
                case (int)VmOp.Nop:
                    pc++;
                    break;
                case (int)VmOp.Li:
                    reg[a] = (short)bc;
                    pc++;
                    break;
                case (int)VmOp.Lk:
                    if (bc >= constants.Length)
                        return Stop(VmFault.ConstantRange);
                    reg[a] = constants[bc];
                    pc++;
                    break;
                case (int)VmOp.Move:
                    reg[a] = reg[b];
                    pc++;
                    break;
                case (int)VmOp.Test:
                    if (reg[a] != bc)
                        pc++;
                    pc++;
                    break;
                case (int)VmOp.Jmp:
                    pc += I24(word) + 1;
                    break;
                case (int)VmOp.Ret:
                    if (b == Nil)
                        return new VmResult(true, 0, VmFault.None);
                    if (b != 0)
                        return Stop(VmFault.UnknownOpcode);
                    if ((a & VmCode.RetFinal) != 0)
                        return new VmResult(true, reg[c], VmFault.None);
                    pc++;
                    break;
                case (int)VmOp.Reti:
                    if ((a & VmCode.RetFinal) != 0)
                        return new VmResult(true, (short)bc, VmFault.None);
                    pc++;
                    break;
                case (int)VmOp.AddRr:
                    reg[a] = unchecked(reg[b] + reg[c]);
                    pc++;
                    break;
                case (int)VmOp.AddRk:
                    if (c >= constants.Length)
                        return Stop(VmFault.ConstantRange);
                    reg[a] = unchecked(reg[b] + constants[c]);
                    pc++;
                    break;
                case (int)VmOp.Addi:
                    reg[a] = unchecked(reg[b] + (sbyte)c);
                    pc++;
                    break;
                case (int)VmOp.SubRr:
                    reg[a] = unchecked(reg[b] - reg[c]);
                    pc++;
                    break;
                case (int)VmOp.MulRr:
                    reg[a] = unchecked(reg[b] * reg[c]);
                    pc++;
                    break;
                case (int)VmOp.DivRr:
                case (int)VmOp.ModRr:
                    if (reg[c] == 0)
                        return Stop(VmFault.DivisionByZero);
                    try
                    {
                        reg[a] = op == (int)VmOp.DivRr ? reg[b] / reg[c] : reg[b] % reg[c];
                    }
                    catch (OverflowException)
                    {
                        return Stop(VmFault.Overflow);
                    }

                    pc++;
                    break;
                case (int)VmOp.AndRr:
                    reg[a] = reg[b] & reg[c];
                    pc++;
                    break;
                case (int)VmOp.OrRr:
                    reg[a] = reg[b] | reg[c];
                    pc++;
                    break;
                case (int)VmOp.XorRr:
                    reg[a] = reg[b] ^ reg[c];
                    pc++;
                    break;
                case (int)VmOp.SllRr:
                    if ((uint)reg[c] > 31)
                        return Stop(VmFault.ShiftRange);
                    reg[a] = reg[b] << reg[c];
                    pc++;
                    break;
                case (int)VmOp.SrlRi:
                    if ((uint)c > 31)
                        return Stop(VmFault.ShiftRange);
                    reg[a] = (int)((uint)reg[b] >> c);
                    pc++;
                    break;
                case (int)VmOp.SraRi:
                    if ((uint)c > 31)
                        return Stop(VmFault.ShiftRange);
                    reg[a] = reg[b] >> c;
                    pc++;
                    break;
                case (int)VmOp.Abs:
                    reg[a] = reg[b] < 0 ? unchecked(-reg[b]) : reg[b];
                    pc++;
                    break;
                case (int)VmOp.Neg:
                    reg[a] = unchecked(-reg[b]);
                    pc++;
                    break;
                case (int)VmOp.Not:
                    reg[a] = ~reg[b];
                    pc++;
                    break;
                case (int)VmOp.EqR:
                case (int)VmOp.LtRr:
                case (int)VmOp.LeRr:
                    var test = op switch
                    {
                        (int)VmOp.EqR => reg[b] == reg[c],
                        (int)VmOp.LtRr => reg[b] < reg[c],
                        _ => reg[b] <= reg[c],
                    };
                    if (!Branch(code, ref pc, a, test))
                        return Stop(VmFault.BadJump);
                    break;
                default:
                    return Stop(VmFault.UnknownOpcode);
            }
        }

        return Stop(VmFault.RanOffEnd);
    }

    private static bool Branch(ReadOnlySpan<int> code, ref int pc, int a, bool test)
    {
        var expect = (a & 1) != 0;
        if (test != expect)
        {
            pc += 2;
            return true;
        }

        if ((uint)(pc + 1) >= (uint)code.Length || (code[pc + 1] & 0xFF) != (int)VmOp.Jmp)
            return false;
        pc += 1 + I24(code[pc + 1]) + 1;
        return true;
    }

    private static int I24(int word)
    {
        var value = (word >> 8) & 0xFFFFFF;
        if ((value & 0x800000) != 0)
            value -= 0x1000000;
        return value;
    }

    private static VmResult Stop(VmFault fault) => new(false, 0, fault);
}
