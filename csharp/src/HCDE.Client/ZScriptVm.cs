using System.Buffers.Binary;
using HCDE.Playsim;

namespace HCDE.Client;

public enum ZsOpcode : byte
{
    Nop = 0,
    LoadImm = 1,
    Add = 2,
    JumpIfZero = 3,
    Return = 4,
    CallNative = 5,
}

public static class ZsNative
{
    public const int Log = 1;
    public const int PlayerHealth = 2;
    public const int DamagePlayer = 3;
}

public sealed class ZsProgram
{
    public string Name { get; init; } = "";
    public byte[] Code { get; init; } = Array.Empty<byte>();
}

/// <summary>
/// Managed ZScript stand-in. Native asmjit is probed and not called.
/// Unknown opcodes stop the fiber. This is not the C++ VM in <c>src/common/scripting/</c>.
/// </summary>
public sealed class ZScriptVm
{
    private const int InstructionBudget = 64;
    private readonly Dictionary<string, ZsProgram> _programs = new(StringComparer.Ordinal);
    private readonly List<Fiber> _fibers = new();

    public List<string> Log { get; } = new();
    public int RunningCount => _fibers.Count(fiber => !fiber.Done);

    public void Add(ZsProgram program) => _programs[program.Name] = program;

    public bool Start(string name)
    {
        if (!_programs.TryGetValue(name, out var program))
            return false;
        _fibers.Add(new Fiber(program));
        return true;
    }

    public void Tick(AuthoritySimulation sim)
    {
        foreach (var fiber in _fibers.ToArray())
        {
            if (fiber.Done)
                continue;
            Run(sim, fiber);
        }
    }

    private void Run(AuthoritySimulation sim, Fiber fiber)
    {
        var steps = 0;
        while (!fiber.Done && steps < InstructionBudget)
        {
            steps++;
            if (fiber.Pc >= fiber.Code.Length)
            {
                fiber.Done = true;
                return;
            }

            var opcode = (ZsOpcode)fiber.Code[fiber.Pc++];
            switch (opcode)
            {
                case ZsOpcode.Nop:
                    break;
                case ZsOpcode.LoadImm:
                    fiber.Stack.Add(ReadI32(fiber));
                    break;
                case ZsOpcode.Add:
                    Binary(fiber, (left, right) => left + right);
                    break;
                case ZsOpcode.JumpIfZero:
                    var dest = ReadI32(fiber);
                    if (Pop(fiber) == 0)
                        fiber.Pc = dest;
                    break;
                case ZsOpcode.Return:
                    fiber.Done = true;
                    return;
                case ZsOpcode.CallNative:
                    Call(sim, fiber, ReadI32(fiber));
                    break;
                default:
                    fiber.Done = true;
                    return;
            }
        }
    }

    private void Call(AuthoritySimulation sim, Fiber fiber, int native)
    {
        var player = sim.Players.FirstOrDefault();
        switch (native)
        {
            case ZsNative.Log:
                Log.Add(fiber.Stack.Count == 0 ? "" : fiber.Stack[^1].ToString());
                break;
            case ZsNative.PlayerHealth:
                fiber.Stack.Add(player?.Health ?? 0);
                break;
            case ZsNative.DamagePlayer:
                if (player == null)
                    return;
                var amount = Pop(fiber);
                player.Health = Math.Max(0, player.Health - Math.Max(0, amount));
                break;
            default:
                fiber.Done = true;
                break;
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
        public Fiber(ZsProgram program) => Code = program.Code;
        public byte[] Code { get; }
        public int Pc { get; set; }
        public bool Done { get; set; }
        public List<int> Stack { get; } = new();
    }
}
