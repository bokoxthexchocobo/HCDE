using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim;

public sealed class AcsProgram
{
    public int Number { get; init; }
    public byte[] Code { get; init; } = Array.Empty<byte>();
}

/// <summary>
/// Word-format ACS subset: push, arithmetic, delay, goto, and direct line specials.
/// Unknown opcodes stop the fiber. This is not <c>p_acs.cpp</c>.
/// </summary>
public sealed class AcsVm
{
    private const int InstructionBudget = 64;
    private readonly Dictionary<int, AcsProgram> _programs = new();
    private readonly List<Fiber> _fibers = new();

    public int ProgramCount => _programs.Count;
    public int RunningCount => _fibers.Count(fiber => !fiber.Done);

    public void Add(AcsProgram program) => _programs[program.Number] = program;

    public bool Enqueue(int number)
    {
        if (!_programs.TryGetValue(number, out var program))
            return false;
        _fibers.Add(new Fiber(program));
        return true;
    }

    public void Tick(AuthoritySimulation sim)
    {
        var snapshot = _fibers.ToArray();
        foreach (var fiber in snapshot)
        {
            if (fiber.Done)
                continue;
            if (fiber.Wait > 0)
            {
                fiber.Wait--;
                continue;
            }

            Run(sim, fiber);
        }
    }

    private static void Run(AuthoritySimulation sim, Fiber fiber)
    {
        var steps = 0;
        while (!fiber.Done && fiber.Wait == 0 && steps < InstructionBudget)
        {
            steps++;
            if (fiber.Pc + 4 > fiber.Code.Length)
            {
                fiber.Done = true;
                return;
            }

            var opcode = ReadI32(fiber);
            switch (opcode)
            {
                case (int)AcsPcode.Nop:
                    break;
                case (int)AcsPcode.Terminate:
                case (int)AcsPcode.Suspend:
                    fiber.Done = true;
                    return;
                case (int)AcsPcode.PushNumber:
                    fiber.Stack.Add(ReadI32(fiber));
                    break;
                case (int)AcsPcode.Add:
                    Binary(fiber, (left, right) => left + right);
                    break;
                case (int)AcsPcode.Subtract:
                    Binary(fiber, (left, right) => left - right);
                    break;
                case (int)AcsPcode.DelayDirect:
                    fiber.Wait = Math.Max(0, ReadI32(fiber));
                    return;
                case (int)AcsPcode.Lspec1Direct:
                    var special = ReadI32(fiber);
                    var arg = ReadI32(fiber);
                    LineSpecials.Execute(sim, null, special, arg);
                    break;
                case (int)AcsPcode.Goto:
                    fiber.Pc = ReadI32(fiber);
                    break;
                case (int)AcsPcode.IfGoto:
                    var dest = ReadI32(fiber);
                    if (Pop(fiber) != 0)
                        fiber.Pc = dest;
                    break;
                case (int)AcsPcode.Drop:
                    Pop(fiber);
                    break;
                default:
                    fiber.Done = true;
                    return;
            }
        }
    }

    private static void Binary(Fiber fiber, Func<int, int, int> op)
    {
        var right = Pop(fiber);
        var left = Pop(fiber);
        fiber.Stack.Add(op(left, right));
    }

    private static int Pop(Fiber fiber)
    {
        if (fiber.Stack.Count == 0)
        {
            fiber.Done = true;
            return 0;
        }

        var value = fiber.Stack[^1];
        fiber.Stack.RemoveAt(fiber.Stack.Count - 1);
        return value;
    }

    private static int ReadI32(Fiber fiber)
    {
        if (fiber.Pc + 4 > fiber.Code.Length)
        {
            fiber.Done = true;
            return 0;
        }

        var value = BinaryPrimitives.ReadInt32LittleEndian(fiber.Code.AsSpan(fiber.Pc));
        fiber.Pc += 4;
        return value;
    }

    private sealed class Fiber
    {
        public Fiber(AcsProgram program)
        {
            Code = program.Code;
        }

        public byte[] Code { get; }
        public int Pc { get; set; }
        public int Wait { get; set; }
        public bool Done { get; set; }
        public List<int> Stack { get; } = new();
    }
}
