using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim;

public sealed class AcsProgram
{
    public int Number { get; init; }
    public byte[] Code { get; init; } = Array.Empty<byte>();
    /// <summary>Original module offset of Code[0], used by absolute branch operands and table alignment.</summary>
    public int CodeBaseOffset { get; init; }
    public bool LegacyHexenDelay { get; init; }
    public int LocalVariableCount { get; init; } = 20;
    public int ArgumentCount { get; init; }
    public int[] LocalArraySizes { get; init; } = Array.Empty<int>();
    /// <summary>Jump-table targets expressed as byte offsets within Code.</summary>
    public int[] JumpPoints { get; init; } = Array.Empty<int>();
    /// <summary>BEHAVIOR string table entries indexed by ACS string ids. DECORATE replacements are absent.</summary>
    public string[] StringTable { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Word-format ACS subset: push, arithmetic, delay, goto, and selected line specials.
/// Unknown opcodes stop the fiber. This is not <c>p_acs.cpp</c>.
/// </summary>
public sealed class AcsVm
{
    private const int InstructionBudget = 64;
    private readonly Dictionary<int, AcsProgram> _programs = new();
    private readonly List<Fiber> _fibers = new();
    private readonly int[] _globals = new int[64];
    private readonly int[] _mapVariables = new int[128];
    private readonly int[] _worldVariables = new int[256];
    private readonly Dictionary<(int Array, int Index), int> _worldArrays = new();
    private readonly Dictionary<(int Array, int Index), int> _globalArrays = new();
    private int[][] _mapArrays = Array.Empty<int[]>();
    public const int MaximumMapArrayElements = 1_048_576;
    public const int MaximumMapArrays = 4096;
    public const int MaximumLocalSlots = 1_048_576;
    private readonly Dictionary<int, uint> _programHashes = new();
    private readonly AcsGlobalStrings _globalStrings = new();

    /// <summary>Dynamic ACS string returned by <c>GetActorClass</c> and similar CallFunc entries.</summary>
    public string GlobalStringAt(int stringId) =>
        AcsStringIds.IsGlobalPool(stringId)
            ? _globalStrings.GetByIndex(AcsStringIds.GlobalIndex(stringId))
            : string.Empty;

    public uint Checksum
    {
        get
        {
            var hash = 2166136261u;
            void Mix(int value) => hash = unchecked((hash ^ (uint)value) * 16777619u);
            foreach (var value in _globals) Mix(value);
            foreach (var value in _mapVariables) Mix(value);
            foreach (var value in _worldVariables) Mix(value);
            void MixArrays(Dictionary<(int Array, int Index), int> arrays)
            {
                Mix(arrays.Count);
                foreach (var entry in arrays.OrderBy(entry => entry.Key.Array).ThenBy(entry => entry.Key.Index))
                { Mix(entry.Key.Array); Mix(entry.Key.Index); Mix(entry.Value); }
            }
            MixArrays(_worldArrays); MixArrays(_globalArrays);
            Mix(_mapArrays.Length);
            foreach (var array in _mapArrays)
            { Mix(array.Length); foreach (var value in array) Mix(value); }
            Mix(_programs.Count);
            foreach (var pair in _programs.OrderBy(pair => pair.Key))
            {
                Mix(pair.Key); Mix(unchecked((int)_programHashes[pair.Key]));
                Mix(pair.Value.LegacyHexenDelay ? 1 : 0);
            }
            Mix(_fibers.Count);
            foreach (var fiber in _fibers) // Execution order is observable when scripts share state.
            {
                Mix(fiber.Number); Mix(unchecked((int)fiber.CodeHash)); Mix(fiber.LegacyHexenDelay ? 1 : 0);
                Mix(fiber.Activator is { Destroyed: false } actor ? unchecked((int)actor.Id) : 0);
                Mix(fiber.TriggerLine is null ? 0 : 1);
                Mix(fiber.BackSide ? 1 : 0);
                if (fiber.TriggerLine is { } line)
                {
                    Mix(line.Index); Mix(line.SideFront); Mix(line.SideBack); Mix(line.Special); Mix(line.Tag);
                    Mix(line.Arg0); Mix(line.Arg1); Mix(line.Arg2); Mix(line.Arg3); Mix(line.Arg4); Mix(line.Repeat ? 1 : 0);
                }
                Mix(fiber.Pc); Mix(fiber.Wait); Mix(fiber.Done ? 1 : 0); Mix(fiber.Suspended ? 1 : 0); Mix(fiber.Always ? 1 : 0);
                Mix(fiber.ScriptWaitState); Mix(fiber.ScriptWaitTarget);
                Mix(fiber.TagWait.HasValue ? 1 : 0); Mix(fiber.TagWait ?? 0);
                Mix(fiber.Stack.Count);
                foreach (var value in fiber.Stack) Mix(value);
                foreach (var value in fiber.Locals) Mix(value);
            }
            return hash;
        }
    }

    public int ProgramCount => _programs.Count;
    public int RunningCount => _fibers.Count(fiber => !fiber.Done);
    public int SuspendedCount => _fibers.Count(fiber => !fiber.Done && fiber.Suspended);

    /// <summary>Replaces one module's map variables and arrays while no scripts are active. Imports and string tags are not supported.</summary>
    public bool TryLoadMapArrays(MapBehaviorMapArrayMetadata metadata, out string? error)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        error = null;
        if (RunningCount != 0) { error = "acs-map-arrays-scripts-active"; return false; }
        if (metadata.Declarations.Count > MaximumMapArrays) { error = "acs-map-array-limit"; return false; }
        long elements = 0;
        foreach (var declaration in metadata.Declarations)
        {
            if ((uint)declaration.MapVariable >= 128) { error = "acs-map-array-variable-invalid"; return false; }
            elements += declaration.Length;
            if (elements > MaximumMapArrayElements) { error = "acs-map-array-limit"; return false; }
        }
        foreach (var initializer in metadata.Initializers)
            if ((uint)initializer.MapVariable >= 128) { error = "acs-map-array-variable-invalid"; return false; }
        foreach (var initializer in metadata.MapInitializers)
            if ((uint)initializer.FirstVariable > 128 || initializer.Values.Count > 128 - initializer.FirstVariable)
            { error = "acs-map-initializer-range-invalid"; return false; }
        // Native module load clears map variables, then applies MINI before ARAY and AINI.
        var variables = new int[128];
        foreach (var initializer in metadata.MapInitializers)
            for (var i = 0; i < initializer.Values.Count; i++) variables[initializer.FirstVariable + i] = initializer.Values[i];
        var arrays = new int[metadata.Declarations.Count][];
        for (var i = 0; i < arrays.Length; i++)
        {
            var declaration = metadata.Declarations[i];
            variables[declaration.MapVariable] = i;
            arrays[i] = new int[(int)declaration.Length];
        }
        foreach (var initializer in metadata.Initializers)
        {
            var id = variables[initializer.MapVariable];
            if ((uint)id >= (uint)arrays.Length) continue;
            var array = arrays[id];
            for (var i = 0; i < Math.Min(array.Length, initializer.Values.Count); i++)
                array[i] = initializer.Values[i];
        }
        variables.CopyTo(_mapVariables, 0);
        _mapArrays = arrays;
        return true;
    }

    public void Add(AcsProgram program)
    {
        ArgumentNullException.ThrowIfNull(program);
        if (program.CodeBaseOffset < 0 || (long)program.CodeBaseOffset + program.Code.Length > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(program), "ACS code region exceeds supported module offsets.");
        if (program.JumpPoints.Length > MaximumLocalSlots)
            throw new ArgumentOutOfRangeException(nameof(program), "ACS jump table exceeds supported limits.");
        var jumpPoints = (int[])program.JumpPoints.Clone();
        foreach (var target in jumpPoints)
            if (target < 0 || target > program.Code.Length - 4)
                throw new ArgumentOutOfRangeException(nameof(program), "ACS jump target is outside program code.");
        var sizes = (int[])program.LocalArraySizes.Clone();
        long slots = program.LocalVariableCount;
        if (slots < 0 || slots > MaximumLocalSlots || sizes.Length > MaximumMapArrays)
            throw new ArgumentOutOfRangeException(nameof(program), "ACS local layout exceeds supported limits.");
        if (program.ArgumentCount < 0 || program.ArgumentCount > program.LocalVariableCount)
            throw new ArgumentOutOfRangeException(nameof(program), "ACS arguments must fit in scalar local storage.");
        foreach (var size in sizes)
        {
            slots += size;
            if (size < 0 || slots > MaximumLocalSlots)
                throw new ArgumentOutOfRangeException(nameof(program), "ACS local layout exceeds supported limits.");
        }
        // Keep registered/running code stable if the caller later edits its input array.
        var stringTable = program.StringTable.Length == 0
            ? Array.Empty<string>()
            : program.StringTable.Select(entry => entry ?? string.Empty).ToArray();
        var owned = new AcsProgram { Number = program.Number, Code = (byte[])program.Code.Clone(), LegacyHexenDelay = program.LegacyHexenDelay,
            LocalVariableCount = program.LocalVariableCount, LocalArraySizes = sizes, ArgumentCount = program.ArgumentCount,
            JumpPoints = jumpPoints, CodeBaseOffset = program.CodeBaseOffset, StringTable = stringTable };
        _programs[owned.Number] = owned;
        var hash = HashCodeBytes(owned.Code);
        hash = unchecked((hash ^ (uint)owned.CodeBaseOffset) * 16777619u);
        hash = unchecked((hash ^ (uint)owned.LocalVariableCount) * 16777619u);
        hash = unchecked((hash ^ (uint)owned.ArgumentCount) * 16777619u);
        hash = unchecked((hash ^ (uint)sizes.Length) * 16777619u);
        foreach (var size in sizes) hash = unchecked((hash ^ (uint)size) * 16777619u);
        hash = unchecked((hash ^ (uint)jumpPoints.Length) * 16777619u);
        foreach (var target in jumpPoints) hash = unchecked((hash ^ (uint)target) * 16777619u);
        foreach (var entry in stringTable)
        {
            foreach (var ch in entry) hash = unchecked((hash ^ ch) * 16777619u);
            hash = unchecked((hash ^ 0xFFu) * 16777619u);
        }
        _programHashes[owned.Number] = hash;
    }

    private static uint HashCodeBytes(byte[] code)
    {
        var hash = unchecked((2166136261u ^ (uint)code.Length) * 16777619u);
        foreach (var value in code) hash = unchecked((hash ^ value) * 16777619u);
        return hash;
    }

    public bool Enqueue(int number) => Enqueue(number, ReadOnlySpan<int>.Empty);

    /// <summary>Starts or resumes a normal ACS_Execute request, rejecting an already running script.</summary>
    public bool TryExecute(int number, ReadOnlySpan<int> arguments, Actor? activator = null, LevelLine? triggerLine = null, bool backSide = false)
    {
        var active = FindControlledScript(number);
        if (active is not null)
        {
            if (!active.Suspended) return false;
            active.Suspended = false;
            active.Wait = 0; // Native resume changes state to Running, discarding the old delay state.
            active.ScriptWaitState = 0;
            active.ScriptWaitTarget = 0;
            active.TagWait = null;
            return true; // Resume retains the original locals and arguments.
        }
        return Enqueue(number, arguments, activator, triggerLine, backSide);
    }

    public bool Enqueue(int number, ReadOnlySpan<int> arguments, Actor? activator = null, LevelLine? triggerLine = null, bool backSide = false) => StartFiber(number, arguments, false, activator, triggerLine, backSide);

    /// <summary>Starts an independent instance excluded from numbered execute/suspend/terminate controls.</summary>
    public bool ExecuteAlways(int number, ReadOnlySpan<int> arguments, Actor? activator = null, LevelLine? triggerLine = null, bool backSide = false) => StartFiber(number, arguments, true, activator, triggerLine, backSide);

    private Fiber? FindControlledScript(int number) => _fibers.FirstOrDefault(fiber => !fiber.Done && !fiber.Always && fiber.Number == number);

    private bool StartFiber(int number, ReadOnlySpan<int> arguments, bool always, Actor? activator, LevelLine? triggerLine, bool backSide)
    {
        if (!_programs.TryGetValue(number, out var program))
            return false;
        _fibers.Add(new Fiber(program, _programHashes[number], arguments, always, activator, triggerLine, backSide));
        return true;
    }

    public bool SuspendScript(int number)
    {
        var active = FindControlledScript(number);
        if (active is not null) active.Suspended = true;
        return true; // Native current-map control specials succeed even when no script matches.
    }

    public bool TerminateScript(int number)
    {
        var active = FindControlledScript(number);
        if (active is not null) active.Done = true;
        return true;
    }

    public void Tick(AuthoritySimulation sim) => Tick(sim, sim.Thinkers.Clock.Tic);

    internal void Tick(AuthoritySimulation sim, int levelTic)
    {
        var snapshot = _fibers.ToArray();
        foreach (var fiber in snapshot)
        {
            if (fiber.Done || fiber.Suspended)
                continue;
            if (fiber.TagWait is int tag)
            {
                // Paused movers still own their plane, just as native floor/ceiling data does.
                if (sim.Motions.Any(motion => !motion.Completed && sim.Level.Sectors[motion.SectorIndex].MatchesTag(tag))) continue;
                fiber.TagWait = null;
            }
            if (fiber.ScriptWaitState != 0)
            {
                var target = FindControlledScript(fiber.ScriptWaitTarget);
                if (fiber.ScriptWaitState == 1)
                {
                    if (target is not null) fiber.ScriptWaitState = 2;
                    continue;
                }
                if (target is not null) continue;
                fiber.ScriptWaitState = 0;
                fiber.ScriptWaitTarget = 0;
            }
            if (fiber.Wait > 0)
            {
                fiber.Wait--;
                if (fiber.Wait > 0) continue;
            }

            Run(sim, fiber, levelTic);
        }
        _fibers.RemoveAll(fiber => fiber.Done);
    }

    private void Run(AuthoritySimulation sim, Fiber fiber, int levelTic)
    {
        var steps = 0;
        while (!fiber.Done && !fiber.Suspended && fiber.Wait == 0 && steps < InstructionBudget)
        {
            steps++;
            if (fiber.Pc < 0 || fiber.Pc > fiber.Code.Length - 4)
            {
                fiber.Done = true;
                return;
            }

            var opcode = ReadI32(fiber);
            switch (opcode)
            {
                case (int)AcsPcode.Nop:
                    break;
                case (int)AcsPcode.PushByte:
                case (int)AcsPcode.Push2Bytes:
                case (int)AcsPcode.Push3Bytes:
                case (int)AcsPcode.Push4Bytes:
                case (int)AcsPcode.Push5Bytes:
                case (int)AcsPcode.PushBytes:
                    var byteCount = opcode == (int)AcsPcode.PushByte ? 1 : opcode - (int)AcsPcode.Push2Bytes + 2;
                    if (opcode == (int)AcsPcode.PushBytes)
                    {
                        if (fiber.Pc >= fiber.Code.Length) { fiber.Done = true; return; }
                        byteCount = fiber.Code[fiber.Pc++];
                    }
                    if (byteCount > fiber.Code.Length - fiber.Pc) { fiber.Done = true; return; }
                    for (var i = 0; i < byteCount; i++) fiber.Stack.Add(fiber.Code[fiber.Pc++]);
                    break;
                case (int)AcsPcode.Terminate:
                    fiber.Done = true;
                    return;
                case (int)AcsPcode.Suspend:
                    fiber.Suspended = true;
                    return;
                case (int)AcsPcode.ScriptWait:
                case (int)AcsPcode.ScriptWaitDirect:
                {
                    var target = opcode == (int)AcsPcode.ScriptWait ? Pop(fiber) : ReadI32(fiber);
                    if (fiber.Done) return;
                    fiber.ScriptWaitTarget = target;
                    // Native COMPATF2_SCRIPTWAIT skips the pre-start wait only for the direct form.
                    var legacyDirect = opcode == (int)AcsPcode.ScriptWaitDirect
                        && sim.Compat.HasFlag(CompatSurface.LegacyScriptWaitDirect);
                    fiber.ScriptWaitState = legacyDirect || FindControlledScript(target) is not null ? 2 : 1;
                    return;
                }
                case (int)AcsPcode.TagWait:
                case (int)AcsPcode.TagWaitDirect:
                {
                    var tag = opcode == (int)AcsPcode.TagWait ? Pop(fiber) : ReadI32(fiber);
                    if (fiber.Done) return;
                    fiber.TagWait = tag;
                    return;
                }
                case (int)AcsPcode.PushNumber:
                    fiber.Stack.Add(ReadI32(fiber));
                    break;
                case (int)AcsPcode.Dup:
                    if (fiber.Stack.Count == 0) { fiber.Done = true; return; }
                    fiber.Stack.Add(fiber.Stack[^1]);
                    break;
                case (int)AcsPcode.Swap:
                    if (fiber.Stack.Count < 2) { fiber.Done = true; return; }
                    (fiber.Stack[^2], fiber.Stack[^1]) = (fiber.Stack[^1], fiber.Stack[^2]);
                    break;
                case (int)AcsPcode.Restart:
                    // This subset stores one script per buffer, starting at offset zero.
                    Jump(fiber, 0, moduleAddress: false);
                    break;
                case 25 or 28 or 31 or 34 or 37 or 40 or 43 or 46 or 49:
                case 26 or 29 or 32 or 35 or 38 or 41 or 44 or 47 or 50:
                case 27 or 30 or 33 or 36 or 39 or 42 or 45 or 48 or 51:
                case >= (int)AcsPcode.AssignGlobalVar and <= (int)AcsPcode.DecGlobalVar:
                {
                    var variables = opcode >= 100 ? _globals : ((opcode - 25) % 3) switch
                    { 0 => fiber.Locals, 1 => _mapVariables, _ => _worldVariables };
                    var operation = opcode < 100 ? 181 + (opcode - 25) / 3 : opcode;
                    var index = ReadI32(fiber);
                    if (fiber.Done || (uint)index >= (uint)variables.Length) { fiber.Done = true; return; }
                    if (operation == (int)AcsPcode.PushGlobalVar) { fiber.Stack.Add(variables[index]); break; }
                    if (operation == (int)AcsPcode.IncGlobalVar) { variables[index] = unchecked(variables[index] + 1); break; }
                    if (operation == (int)AcsPcode.DecGlobalVar) { variables[index] = unchecked(variables[index] - 1); break; }
                    if (fiber.Stack.Count == 0) { fiber.Done = true; return; }
                    var operand = Pop(fiber);
                    if (operation is (int)AcsPcode.DivGlobalVar or (int)AcsPcode.ModGlobalVar && operand == 0)
                    { fiber.Done = true; return; }
                    var current = variables[index];
                    variables[index] = operation switch
                    {
                        (int)AcsPcode.AssignGlobalVar => operand,
                        (int)AcsPcode.AddGlobalVar => unchecked(current + operand),
                        (int)AcsPcode.SubGlobalVar => unchecked(current - operand),
                        (int)AcsPcode.MulGlobalVar => unchecked(current * operand),
                        (int)AcsPcode.DivGlobalVar => unchecked((int)((long)current / operand)),
                        _ => (int)((long)current % operand),
                    };
                    break;
                }
                case (int)AcsPcode.AndScriptVar or (int)AcsPcode.AndMapVar or (int)AcsPcode.AndGlobalVar:
                case (int)AcsPcode.EorScriptVar or (int)AcsPcode.EorMapVar or (int)AcsPcode.EorGlobalVar:
                case (int)AcsPcode.OrScriptVar or (int)AcsPcode.OrMapVar or (int)AcsPcode.OrGlobalVar:
                case (int)AcsPcode.LsScriptVar or (int)AcsPcode.LsMapVar or (int)AcsPcode.LsGlobalVar:
                case (int)AcsPcode.RsScriptVar or (int)AcsPcode.RsMapVar or (int)AcsPcode.RsGlobalVar:
                case (int)AcsPcode.AndWorldVar or (int)AcsPcode.EorWorldVar or (int)AcsPcode.OrWorldVar:
                case (int)AcsPcode.LsWorldVar or (int)AcsPcode.RsWorldVar:
                {
                    var scope = (opcode - (int)AcsPcode.AndScriptVar) % 7;
                    var variables = scope switch
                    { 0 => fiber.Locals, 1 => _mapVariables, 2 => _worldVariables, _ => _globals };
                    var index = ReadI32(fiber);
                    if (fiber.Done || (uint)index >= (uint)variables.Length || fiber.Stack.Count == 0)
                    { fiber.Done = true; return; }
                    var operand = Pop(fiber);
                    variables[index] = ((opcode - (int)AcsPcode.AndScriptVar) / 7) switch
                    {
                        0 => variables[index] & operand,
                        1 => variables[index] ^ operand,
                        2 => variables[index] | operand,
                        3 => variables[index] << operand,
                        _ => variables[index] >> operand,
                    };
                    break;
                }
                case >= (int)AcsPcode.AssignScriptArray and <= (int)AcsPcode.RsScriptArray:
                    RunLocalArray(fiber, opcode);
                    break;
                case >= (int)AcsPcode.PushMapArray and <= (int)AcsPcode.DecMapArray:
                case (int)AcsPcode.AndMapArray or (int)AcsPcode.EorMapArray or (int)AcsPcode.OrMapArray:
                case (int)AcsPcode.LsMapArray or (int)AcsPcode.RsMapArray:
                    RunMapArray(fiber, opcode);
                    break;
                case >= (int)AcsPcode.PushWorldArray and <= (int)AcsPcode.DecGlobalArray:
                case (int)AcsPcode.AndWorldArray or (int)AcsPcode.AndGlobalArray:
                case (int)AcsPcode.EorWorldArray or (int)AcsPcode.EorGlobalArray:
                case (int)AcsPcode.OrWorldArray or (int)AcsPcode.OrGlobalArray:
                case (int)AcsPcode.LsWorldArray or (int)AcsPcode.LsGlobalArray:
                case (int)AcsPcode.RsWorldArray or (int)AcsPcode.RsGlobalArray:
                    RunArray(fiber, opcode);
                    break;
                case (int)AcsPcode.Lspec6:
                    fiber.Stack.Add(sim.Invasion.Wave);
                    break;
                case (int)AcsPcode.Lspec6Direct:
                    fiber.Stack.Add(sim.Invasion.ClassicState);
                    break;
                case (int)AcsPcode.CallFunc:
                {
                    var argCount = ReadI32(fiber);
                    var function = ReadI32(fiber);
                    if (fiber.Done)
                        return;
                    if (argCount == 0 && sim.Invasion.QueryAcs(function) is { } invasionResult)
                    {
                        fiber.Stack.Add(invasionResult);
                        break;
                    }
                    if (AcsCallFunctions.TryInvoke(sim, fiber.Stack, fiber.ActivatorBinding, fiber.StringTable, _globalStrings, function, argCount, out var callResult))
                    {
                        fiber.Stack.Add(callResult);
                        break;
                    }
                    fiber.Done = true;
                    return;
                }
                case >= (int)AcsPcode.Add and <= (int)AcsPcode.Ge:
                case >= (int)AcsPcode.AndLogical and <= (int)AcsPcode.EorBitwise:
                case (int)AcsPcode.Lshift:
                case (int)AcsPcode.Rshift:
                case (int)AcsPcode.FixedMul:
                case (int)AcsPcode.FixedDiv:
                    Binary(fiber, opcode);
                    break;
                case (int)AcsPcode.NegateLogical:
                case (int)AcsPcode.UnaryMinus:
                case (int)AcsPcode.NegateBinary:
                    var unary = Pop(fiber);
                    if (!fiber.Done) fiber.Stack.Add(opcode switch
                    {
                        (int)AcsPcode.NegateLogical => unary == 0 ? 1 : 0,
                        (int)AcsPcode.NegateBinary => ~unary,
                        _ => unchecked(-unary),
                    });
                    break;
                case (int)AcsPcode.DelayDirect:
                case (int)AcsPcode.DelayDirectB:
                case (int)AcsPcode.Delay:
                    var delay = opcode == (int)AcsPcode.DelayDirect ? ReadI32(fiber)
                        : opcode == (int)AcsPcode.DelayDirectB ? ReadByte(fiber) : Pop(fiber);
                    if (fiber.Done) return;
                    fiber.Wait = (int)Math.Clamp((long)delay + (fiber.LegacyHexenDelay ? 1 : 0), 0, int.MaxValue);
                    if (fiber.Wait > 0) return;
                    break;
                case (int)AcsPcode.Lspec1:
                case (int)AcsPcode.Lspec2:
                case (int)AcsPcode.Lspec3:
                case (int)AcsPcode.Lspec4:
                case (int)AcsPcode.Lspec5:
                case (int)AcsPcode.Lspec5Ex:
                case (int)AcsPcode.Lspec5ExResult:
                case (int)AcsPcode.Lspec5Result:
                {
                    var stackSpecial = ReadI32(fiber);
                    var stackCount = opcode is (int)AcsPcode.Lspec5Ex or (int)AcsPcode.Lspec5ExResult or (int)AcsPcode.Lspec5Result
                        ? 5 : opcode - (int)AcsPcode.Lspec1 + 1;
                    if (fiber.Done || fiber.Stack.Count < stackCount)
                    {
                        fiber.Done = true;
                        return;
                    }
                    var stackArgs = new int[5];
                    var start = fiber.Stack.Count - stackCount;
                    for (var i = 0; i < stackCount; i++) stackArgs[i] = fiber.Stack[start + i];
                    fiber.Stack.RemoveRange(start, stackCount);
                    var stackResult = LineSpecials.ExecuteExitSpecial(sim, stackSpecial, fiber.Activator is { Destroyed: false } ? fiber.Activator : null)
                        ?? SectorGravity.ExecuteSpecial(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2])
                        ?? ThingRemove.ExecuteSpecial(sim, stackSpecial, fiber.Activator, stackArgs[0])
                        ?? ThingThrust.ExecuteSpecial(sim, stackSpecial, fiber.Activator, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3])
                        ?? ThingThrustZ.ExecuteSpecial(sim, stackSpecial, fiber.Activator, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3])
                        ?? ThingChangeTid.ExecuteSpecial(sim, stackSpecial, fiber.Activator, stackArgs[0], stackArgs[1])
                        ?? ThingStop.ExecuteSpecial(sim, stackSpecial, fiber.Activator, stackArgs[0])
                        ?? ThingActivation.ExecuteSpecial(sim, stackSpecial, fiber.Activator, stackArgs[0])
                        ?? LineSpecials.ExecuteTeleportSpecial(sim, stackSpecial, stackArgs[0], stackArgs[1], fiber.Activator, fiber.BackSide)
                        ?? LineSpecials.ExecuteSectorRotation(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2])
                        ?? WallScrollActions.ExecuteAcs(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3], stackArgs[4])
                        ?? WallTextureOffset.Execute(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3], stackArgs[4])
                        ?? WallTextureScale.Execute(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3], stackArgs[4])
                        ?? SectorTextureScale.Execute(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3], stackArgs[4])
                        ?? SectorTexturePanning.Execute(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3], stackArgs[4])
                        ?? SectorTextureAlignment.Execute(sim, stackSpecial, stackArgs[0], stackArgs[1])
                        ?? GeometryHealthActions.Execute(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2])
                        ?? LineSpecials.ExecuteScriptControl(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3], stackArgs[4], fiber.Activator, fiber.TriggerLine, fiber.BackSide)
                        ?? SectorDamage.ExecuteSpecial(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3], stackArgs[4])
                        ?? LineSpecials.ExecuteDoorSpecial(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3], fiber.TriggerLine)
                        ?? LineSpecials.ExecuteStairSpecial(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3], stackArgs[4], fiber.TriggerLine)
                        ?? LineSpecials.ExecuteFloorSpecial(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3], stackArgs[4], fiber.TriggerLine)
                        ?? LineSpecials.ExecuteCeilingSpecial(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3], stackArgs[4], fiber.TriggerLine)
                        ?? LightActions.ExecuteSpecial(sim, stackSpecial, stackArgs[0], stackArgs[1], stackArgs[2], stackArgs[3], stackArgs[4]);
                    if (stackResult is null)
                        fiber.Done = true;
                    else if (opcode is (int)AcsPcode.Lspec5ExResult or (int)AcsPcode.Lspec5Result)
                        fiber.Stack.Add(stackResult.Value ? 1 : 0);
                    break;
                }
                case (int)AcsPcode.Lspec1Direct:
                case (int)AcsPcode.Lspec2Direct:
                case (int)AcsPcode.Lspec3Direct:
                case (int)AcsPcode.Lspec4Direct:
                case (int)AcsPcode.Lspec5Direct:
                case (int)AcsPcode.Lspec1DirectB:
                case (int)AcsPcode.Lspec2DirectB:
                case (int)AcsPcode.Lspec3DirectB:
                case (int)AcsPcode.Lspec4DirectB:
                case (int)AcsPcode.Lspec5DirectB:
                    var packedSpecial = opcode >= (int)AcsPcode.Lspec1DirectB;
                    var special = packedSpecial ? ReadByte(fiber) : ReadI32(fiber);
                    var count = opcode - (packedSpecial ? (int)AcsPcode.Lspec1DirectB : (int)AcsPcode.Lspec1Direct) + 1;
                    var args = new int[5];
                    for (var i = 0; i < count; i++) args[i] = packedSpecial ? ReadByte(fiber) : ReadI32(fiber);
                    if (!fiber.Done)
                    {
                        var result = LineSpecials.ExecuteExitSpecial(sim, special, fiber.Activator is { Destroyed: false } ? fiber.Activator : null)
                            ?? SectorGravity.ExecuteSpecial(sim, special, args[0], args[1], args[2])
                            ?? ThingRemove.ExecuteSpecial(sim, special, fiber.Activator, args[0])
                            ?? ThingThrust.ExecuteSpecial(sim, special, fiber.Activator, args[0], args[1], args[2], args[3])
                            ?? ThingThrustZ.ExecuteSpecial(sim, special, fiber.Activator, args[0], args[1], args[2], args[3])
                            ?? ThingChangeTid.ExecuteSpecial(sim, special, fiber.Activator, args[0], args[1])
                            ?? ThingStop.ExecuteSpecial(sim, special, fiber.Activator, args[0])
                            ?? ThingActivation.ExecuteSpecial(sim, special, fiber.Activator, args[0])
                            ?? LineSpecials.ExecuteTeleportSpecial(sim, special, args[0], args[1], fiber.Activator, fiber.BackSide)
                            ?? LineSpecials.ExecuteSectorRotation(sim, special, args[0], args[1], args[2])
                            ?? WallScrollActions.ExecuteAcs(sim, special, args[0], args[1], args[2], args[3], args[4])
                            ?? WallTextureOffset.Execute(sim, special, args[0], args[1], args[2], args[3], args[4])
                            ?? WallTextureScale.Execute(sim, special, args[0], args[1], args[2], args[3], args[4])
                            ?? SectorTextureScale.Execute(sim, special, args[0], args[1], args[2], args[3], args[4])
                            ?? SectorTexturePanning.Execute(sim, special, args[0], args[1], args[2], args[3], args[4])
                            ?? SectorTextureAlignment.Execute(sim, special, args[0], args[1])
                            ?? GeometryHealthActions.Execute(sim, special, args[0], args[1], args[2])
                            ?? LineSpecials.ExecuteScriptControl(sim, special, args[0], args[1], args[2], args[3], args[4], fiber.Activator, fiber.TriggerLine, fiber.BackSide)
                            ?? SectorDamage.ExecuteSpecial(sim, special, args[0], args[1], args[2], args[3], args[4])
                            ?? LineSpecials.ExecuteDoorSpecial(sim, special, args[0], args[1], args[2], args[3], fiber.TriggerLine)
                            ?? LineSpecials.ExecuteStairSpecial(sim, special, args[0], args[1], args[2], args[3], args[4], fiber.TriggerLine)
                            ?? LineSpecials.ExecuteFloorSpecial(sim, special, args[0], args[1], args[2], args[3], args[4], fiber.TriggerLine)
                            ?? LineSpecials.ExecuteCeilingSpecial(sim, special, args[0], args[1], args[2], args[3], args[4], fiber.TriggerLine)
                            ?? LightActions.ExecuteSpecial(sim, special, args[0], args[1], args[2], args[3], args[4]);
                        // Preserve the existing one-argument internal API for unconverted actions.
                        if (result is null)
                        {
                            if (count == 1) LineSpecials.Execute(sim, fiber.Activator is { Destroyed: false } ? fiber.Activator : null, special, args[0], fiber.TriggerLine);
                            else fiber.Done = true; // Do not continue through unimplemented multi-argument actions.
                        }
                    }
                    break;
                case (int)AcsPcode.Goto:
                    Jump(fiber, ReadI32(fiber));
                    break;
                case (int)AcsPcode.GotoStack:
                    var jumpIndex = Pop(fiber);
                    if (fiber.Done) return;
                    if ((uint)jumpIndex >= (uint)fiber.JumpPoints.Length) { fiber.Done = true; return; }
                    Jump(fiber, fiber.JumpPoints[jumpIndex], moduleAddress: false);
                    break;
                case (int)AcsPcode.LineSide:
                    fiber.Stack.Add(fiber.BackSide ? 1 : 0);
                    break;
                case (int)AcsPcode.Sin:
                case (int)AcsPcode.Cos:
                    var trigAngle = Pop(fiber);
                    if (fiber.Done) break;
                    var radians = (trigAngle % 65536) * (Math.PI / 32768.0);
                    fiber.Stack.Add(Fixed.FromDouble(opcode == (int)AcsPcode.Sin
                        ? Math.Sin(radians) : Math.Cos(radians)).Raw);
                    break;
                case (int)AcsPcode.VectorAngle:
                    if (fiber.Stack.Count < 2) { fiber.Done = true; return; }
                    var vectorY = Pop(fiber);
                    var vectorX = Pop(fiber);
                    var degrees = Math.Atan2(vectorY, vectorX) * (180.0 / Math.PI);
                    if (degrees < 0) degrees += 360;
                    fiber.Stack.Add((int)(degrees * (16384.0 / 90.0)));
                    break;
                case (int)AcsPcode.IsNetworkGame:
                    fiber.Stack.Add(sim.IsNetworkGame ? 1 : 0);
                    break;
                case (int)AcsPcode.GameType:
                    fiber.Stack.Add(sim.GameMode switch
                    {
                        SpawnGameMode.Cooperative => 1,
                        SpawnGameMode.Deathmatch => 2,
                        _ => 0,
                    });
                    break;
                case (int)AcsPcode.GameSkill:
                    fiber.Stack.Add(sim.Skill);
                    break;
                case (int)AcsPcode.Timer:
                    fiber.Stack.Add(levelTic);
                    break;
                case (int)AcsPcode.PlayerCount:
                    fiber.Stack.Add(sim.Players.Count(player => !player.Destroyed));
                    break;
                case (int)AcsPcode.SinglePlayer:
                    fiber.Stack.Add(sim.GameMode == SpawnGameMode.Single ? 1 : 0);
                    break;
                case (int)AcsPcode.PlayerInGame:
                {
                    var playerNum = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(sim.IsPlayerInGame(playerNum) ? 1 : 0);
                    break;
                }
                case (int)AcsPcode.PlayerIsBot:
                {
                    var botPlayerNum = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(sim.IsPlayerBot(botPlayerNum) ? 1 : 0);
                    break;
                }
                case (int)AcsPcode.ThingCount:
                {
                    if (fiber.Stack.Count < 2) { fiber.Done = true; return; }
                    var thingTid = Pop(fiber);
                    var type = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(ThingCount(sim, type, thingTid));
                    break;
                }
                case (int)AcsPcode.ThingCountDirect:
                {
                    var type = ReadI32(fiber);
                    var thingTid = ReadI32(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(ThingCount(sim, type, thingTid));
                    break;
                }
                case (int)AcsPcode.ThingCountSector:
                {
                    if (fiber.Stack.Count < 3) { fiber.Done = true; return; }
                    // Native PCD_THINGCOUNTSECTOR: ThingCount(STACK(3), -1, STACK(2), STACK(1)); top = tag.
                    var sectorTag = Pop(fiber);
                    var sectorTid = Pop(fiber);
                    var sectorType = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(ThingCount(sim, sectorType, sectorTid, sectorTag));
                    break;
                }
                case (int)AcsPcode.ThingCountName:
                {
                    if (fiber.Stack.Count < 2) { fiber.Done = true; return; }
                    // Native PCD_THINGCOUNTNAME: ThingCount(-1, STACK(2), STACK(1), -1); top = tid.
                    var nameTid = Pop(fiber);
                    var nameStringId = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(ThingCountByName(fiber, sim, nameStringId, nameTid, tag: -1));
                    break;
                }
                case (int)AcsPcode.ThingCountNameSector:
                {
                    if (fiber.Stack.Count < 3) { fiber.Done = true; return; }
                    // Native PCD_THINGCOUNTNAMESECTOR: ThingCount(-1, STACK(3), STACK(2), STACK(1)); top = tag.
                    var nameSectorTag = Pop(fiber);
                    var nameSectorTid = Pop(fiber);
                    var nameSectorStringId = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(ThingCountByName(fiber, sim, nameSectorStringId, nameSectorTid, nameSectorTag));
                    break;
                }
                case (int)AcsPcode.PlayerHealth:
                    fiber.Stack.Add(fiber.Activator is { Destroyed: false } healthActor ? healthActor.Health : 0);
                    break;
                case (int)AcsPcode.PlayerArmorPoints:
                    fiber.Stack.Add(fiber.Activator is PlayerPawn { Destroyed: false } armorPlayer
                        ? armorPlayer.Inventory.Armor : 0);
                    break;
                case (int)AcsPcode.PlayerTeam:
                    fiber.Stack.Add(0);
                    break;
                case (int)AcsPcode.PlayerFrags:
                    fiber.Stack.Add(fiber.Activator is PlayerPawn { Destroyed: false } fragPlayer
                        ? fragPlayer.FragCount : 0);
                    break;
                case (int)AcsPcode.PlayerBlueSkull:
                case (int)AcsPcode.PlayerBlueCard:
                    fiber.Stack.Add(AcsPlayerInventory.HasKey(fiber.Activator, PickupCatalog.KeyColor.Blue));
                    break;
                case (int)AcsPcode.PlayerRedSkull:
                case (int)AcsPcode.PlayerRedCard:
                    fiber.Stack.Add(AcsPlayerInventory.HasKey(fiber.Activator, PickupCatalog.KeyColor.Red));
                    break;
                case (int)AcsPcode.PlayerYellowSkull:
                case (int)AcsPcode.PlayerYellowCard:
                    fiber.Stack.Add(AcsPlayerInventory.HasKey(fiber.Activator, PickupCatalog.KeyColor.Yellow));
                    break;
                case (int)AcsPcode.PlayerMasterSkull:
                case (int)AcsPcode.PlayerMasterCard:
                    fiber.Stack.Add(AcsPlayerInventory.HasMasterKeys(fiber.Activator));
                    break;
                case (int)AcsPcode.PlayerBlackSkull:
                case (int)AcsPcode.PlayerSilverSkull:
                case (int)AcsPcode.PlayerGoldSkull:
                case (int)AcsPcode.PlayerBlackCard:
                case (int)AcsPcode.PlayerSilverCard:
                case (int)AcsPcode.PlayerGoldCard:
                    fiber.Stack.Add(0);
                    break;
                case (int)AcsPcode.ClearInventory:
                    AcsPlayerInventory.ScriptClear(sim, fiber.Activator);
                    break;
                case (int)AcsPcode.ClearActorInventory:
                {
                    var clearTid = Pop(fiber);
                    if (fiber.Done) break;
                    if (clearTid == 0) AcsPlayerInventory.ScriptClear(sim, null);
                    else foreach (var actor in AcsActorTid.AllFromTid(sim, clearTid))
                        AcsPlayerInventory.Clear(actor);
                    break;
                }
                case (int)AcsPcode.GiveInventory:
                {
                    if (fiber.Stack.Count < 2) { fiber.Done = true; return; }
                    var giveAmount = Pop(fiber);
                    var giveStringId = Pop(fiber);
                    if (fiber.Done) break;
                    AcsPlayerInventory.ScriptGive(sim, fiber.Activator, fiber.StringTable, giveStringId, giveAmount);
                    break;
                }
                case (int)AcsPcode.GiveInventoryDirect:
                {
                    var giveDirectStringId = ReadI32(fiber);
                    var giveDirectAmount = ReadI32(fiber);
                    if (fiber.Done) break;
                    AcsPlayerInventory.ScriptGive(sim, fiber.Activator, fiber.StringTable, giveDirectStringId, giveDirectAmount);
                    break;
                }
                case (int)AcsPcode.TakeInventory:
                {
                    if (fiber.Stack.Count < 2) { fiber.Done = true; return; }
                    var takeAmount = Pop(fiber);
                    var takeStringId = Pop(fiber);
                    if (fiber.Done) break;
                    AcsPlayerInventory.ScriptTake(sim, fiber.Activator, fiber.StringTable, takeStringId, takeAmount);
                    break;
                }
                case (int)AcsPcode.TakeInventoryDirect:
                {
                    var takeDirectStringId = ReadI32(fiber);
                    var takeDirectAmount = ReadI32(fiber);
                    if (fiber.Done) break;
                    AcsPlayerInventory.ScriptTake(sim, fiber.Activator, fiber.StringTable, takeDirectStringId, takeDirectAmount);
                    break;
                }
                case (int)AcsPcode.GiveActorInventory:
                {
                    if (fiber.Stack.Count < 3) { fiber.Done = true; return; }
                    var giveActorAmount = Pop(fiber);
                    var giveActorStringId = Pop(fiber);
                    var giveActorTid = Pop(fiber);
                    if (fiber.Done) break;
                    if (giveActorTid == 0)
                        AcsPlayerInventory.ScriptGive(sim, null, fiber.StringTable, giveActorStringId, giveActorAmount);
                    else foreach (var actor in AcsActorTid.AllFromTid(sim, giveActorTid))
                        AcsPlayerInventory.Give(actor, fiber.StringTable, giveActorStringId, giveActorAmount);
                    break;
                }
                case (int)AcsPcode.TakeActorInventory:
                {
                    if (fiber.Stack.Count < 3) { fiber.Done = true; return; }
                    var takeActorAmount = Pop(fiber);
                    var takeActorStringId = Pop(fiber);
                    var takeActorTid = Pop(fiber);
                    if (fiber.Done) break;
                    if (takeActorTid == 0)
                        AcsPlayerInventory.ScriptTake(sim, null, fiber.StringTable, takeActorStringId, takeActorAmount);
                    else foreach (var actor in AcsActorTid.AllFromTid(sim, takeActorTid))
                        AcsPlayerInventory.Take(actor, fiber.StringTable, takeActorStringId, takeActorAmount);
                    break;
                }
                case (int)AcsPcode.CheckInventory:
                {
                    if (fiber.Stack.Count < 1) { fiber.Done = true; return; }
                    var invStringId = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(AcsPlayerInventory.Count(fiber.Activator, fiber.StringTable, invStringId, max: false));
                    break;
                }
                case (int)AcsPcode.CheckInventoryDirect:
                {
                    var directStringId = ReadI32(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(AcsPlayerInventory.Count(fiber.Activator, fiber.StringTable, directStringId, max: false));
                    break;
                }
                case (int)AcsPcode.CheckActorInventory:
                {
                    if (fiber.Stack.Count < 2) { fiber.Done = true; return; }
                    var actorInvStringId = Pop(fiber);
                    var actorInvTid = Pop(fiber);
                    if (fiber.Done) break;
                    var actorInvTarget = AcsActorTid.SingleFromTid(sim, actorInvTid);
                    fiber.Stack.Add(AcsPlayerInventory.Count(actorInvTarget, fiber.StringTable, actorInvStringId, max: false));
                    break;
                }
                case (int)AcsPcode.UseInventory:
                {
                    if (fiber.Stack.Count < 1) { fiber.Done = true; return; }
                    var useStringId = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(AcsPlayerInventory.Use(fiber.Activator, fiber.StringTable, useStringId));
                    break;
                }
                case (int)AcsPcode.UseActorInventory:
                {
                    if (fiber.Stack.Count < 2) { fiber.Done = true; return; }
                    var useActorStringId = Pop(fiber);
                    var useActorTid = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(AcsPlayerInventory.UseByTid(sim, useActorTid, fiber.StringTable, useActorStringId));
                    break;
                }
                case (int)AcsPcode.CheckWeapon:
                {
                    if (fiber.Stack.Count < 1) { fiber.Done = true; return; }
                    var checkWeaponStringId = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(AcsPlayerInventory.CheckWeapon(fiber.Activator, fiber.StringTable, checkWeaponStringId));
                    break;
                }
                case (int)AcsPcode.SetWeapon:
                {
                    if (fiber.Stack.Count < 1) { fiber.Done = true; return; }
                    var setWeaponStringId = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(AcsPlayerInventory.SetWeapon(fiber.Activator, fiber.StringTable, setWeaponStringId));
                    break;
                }
                case (int)AcsPcode.GetAmmoCapacity:
                {
                    if (fiber.Stack.Count < 1) { fiber.Done = true; return; }
                    var ammoCapStringId = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(AcsPlayerInventory.AmmoCapacity(fiber.Activator, fiber.StringTable, ammoCapStringId));
                    break;
                }
                case (int)AcsPcode.SetAmmoCapacity:
                {
                    if (fiber.Stack.Count < 2) { fiber.Done = true; return; }
                    var ammoCapValue = Pop(fiber);
                    var ammoCapSetStringId = Pop(fiber);
                    if (fiber.Done) break;
                    AcsPlayerInventory.SetAmmoCapacity(fiber.Activator, fiber.StringTable, ammoCapSetStringId, ammoCapValue);
                    break;
                }
                case (int)AcsPcode.SetActorProperty:
                {
                    if (fiber.Stack.Count < 3) { fiber.Done = true; return; }
                    var setValue = Pop(fiber);
                    var setProperty = Pop(fiber);
                    var setTid = Pop(fiber);
                    if (fiber.Done) break;
                    AcsActorProperties.Set(sim, fiber.Activator, setTid, setProperty, setValue);
                    break;
                }
                case (int)AcsPcode.GetActorProperty:
                {
                    if (fiber.Stack.Count < 2) { fiber.Done = true; return; }
                    var getProperty = Pop(fiber);
                    var getTid = Pop(fiber);
                    if (fiber.Done) break;
                    fiber.Stack.Add(AcsActorProperties.Get(sim, fiber.Activator, getTid, getProperty));
                    break;
                }
                case (int)AcsPcode.PlayerNumber:
                    fiber.Stack.Add(fiber.Activator is PlayerPawn { Destroyed: false } player ? player.PlayerNum : -1);
                    break;
                case (int)AcsPcode.ActivatorTid:
                    fiber.Stack.Add(fiber.Activator is { Destroyed: false } activator ? activator.ThingId : 0);
                    break;
                case (int)AcsPcode.SetActorAngle:
                case (int)AcsPcode.SetActorPitch:
                    if (fiber.Stack.Count < 2) { fiber.Done = true; break; }
                    var angleValue = Pop(fiber);
                    var newAngle = new BamAngle(unchecked((uint)angleValue << 16));
                    var pitchValue = unchecked((short)angleValue) * (360.0 / 65536);
                    var angleTid = Pop(fiber);
                    if (angleTid == 0)
                    {
                        if (fiber.Activator is { Destroyed: false } angleActor)
                        {
                            if (opcode == (int)AcsPcode.SetActorPitch) angleActor.PitchDegrees = pitchValue;
                            else angleActor.Angle = newAngle;
                        }
                    }
                    else
                    {
                        foreach (var angleActor in sim.Actors)
                            if (!angleActor.Destroyed && angleActor.ThingId == angleTid)
                            {
                                if (opcode == (int)AcsPcode.SetActorPitch) angleActor.PitchDegrees = pitchValue;
                                else angleActor.Angle = newAngle;
                            }
                    }
                    break;
                case (int)AcsPcode.GetActorX:
                case (int)AcsPcode.GetActorY:
                case (int)AcsPcode.GetActorZ:
                case (int)AcsPcode.GetActorFloorZ:
                case (int)AcsPcode.GetActorCeilingZ:
                case (int)AcsPcode.GetActorAngle:
                case (int)AcsPcode.GetActorPitch:
                case (int)AcsPcode.GetActorLightLevel:
                    var tid = Pop(fiber);
                    if (fiber.Done) break;
                    var queriedActor = tid == 0 ? fiber.Activator
                        : sim.Actors.LastOrDefault(candidate => !candidate.Destroyed && candidate.ThingId == tid);
                    fiber.Stack.Add(queriedActor is null || queriedActor.Destroyed ? 0 : opcode switch
                    {
                        (int)AcsPcode.GetActorX => queriedActor.X.Raw,
                        (int)AcsPcode.GetActorY => queriedActor.Y.Raw,
                        (int)AcsPcode.GetActorFloorZ => Fixed.FromDouble(sim.FloorOf(queriedActor.SectorIndex)).Raw,
                        (int)AcsPcode.GetActorCeilingZ => Fixed.FromDouble(sim.CeilingOf(queriedActor.SectorIndex)).Raw,
                        (int)AcsPcode.GetActorAngle => (int)(queriedActor.Angle.Raw >> 16),
                        (int)AcsPcode.GetActorPitch => unchecked((int)BamAngle.FromDegrees(queriedActor.PitchDegrees).Raw) / 65536,
                        (int)AcsPcode.GetActorLightLevel => sim.LightOf(queriedActor.SectorIndex),
                        _ => queriedActor.Z.Raw,
                    });
                    break;
                case (int)AcsPcode.GetLineRowOffset:
                    var frontSide = fiber.TriggerLine?.SideFront ?? -1;
                    fiber.Stack.Add((uint)frontSide < (uint)sim.Level.Sides.Count
                        ? (int)sim.Level.Sides[frontSide].MidTextureOffsetY : 0);
                    break;
                case (int)AcsPcode.GetSectorFloorZ:
                case (int)AcsPcode.GetSectorCeilingZ:
                    if (fiber.Stack.Count < 3) { fiber.Done = true; return; }
                    // Native ACS takes integer map coordinates here, not fixed-point coordinates.
                    var sectorY = Pop(fiber);
                    var sectorX = Pop(fiber);
                    var planeTag = Pop(fiber);
                    var planeSector = -1;
                    if (planeTag == 0) planeSector = ActorPhysics.SectorAt(sim.Level, sectorX, sectorY);
                    else
                        for (var i = 0; i < sim.Level.Sectors.Count; i++)
                            if (sim.Level.Sectors[i].MatchesTag(planeTag)) { planeSector = i; break; }
                    fiber.Stack.Add(planeSector < 0 ? 0 : Fixed.FromDouble(
                        opcode == (int)AcsPcode.GetSectorFloorZ
                            ? sim.FloorOf(planeSector) : sim.CeilingOf(planeSector)).Raw);
                    break;
                case (int)AcsPcode.GetSectorLightLevel:
                    var lightTag = Pop(fiber);
                    if (fiber.Done) break;
                    var lightSector = -1;
                    for (var i = 0; i < sim.Level.Sectors.Count; i++)
                        if (sim.Level.Sectors[i].MatchesTag(lightTag)) { lightSector = i; break; }
                    fiber.Stack.Add(lightSector < 0 ? -1 : sim.LightOf(lightSector));
                    break;
                case (int)AcsPcode.IfGoto:
                case (int)AcsPcode.IfNotGoto:
                    var dest = ReadI32(fiber);
                    var condition = Pop(fiber);
                    if (!fiber.Done && (opcode == (int)AcsPcode.IfGoto ? condition != 0 : condition == 0))
                        Jump(fiber, dest);
                    break;
                case (int)AcsPcode.CaseGoto:
                {
                    var caseValue = ReadI32(fiber);
                    var caseDest = ReadI32(fiber);
                    if (fiber.Done || fiber.Stack.Count == 0) { fiber.Done = true; return; }
                    if (fiber.Stack[^1] == caseValue) { Pop(fiber); Jump(fiber, caseDest); }
                    break;
                }
                case (int)AcsPcode.CaseGotoSorted:
                {
                    var alignedTable = (((long)fiber.CodeBaseOffset + fiber.Pc + 3) & ~3L) - fiber.CodeBaseOffset;
                    if (alignedTable > fiber.Code.Length - 4) { fiber.Done = true; return; }
                    fiber.Pc = (int)alignedTable;
                    var cases = ReadI32(fiber);
                    if (fiber.Done || fiber.Stack.Count == 0 || cases < 0 || cases > (fiber.Code.Length - fiber.Pc) / 8)
                    { fiber.Done = true; return; }
                    var table = fiber.Pc;
                    fiber.Pc += cases * 8;
                    var low = 0; var high = cases - 1;
                    while (low <= high)
                    {
                        var mid = low + (high - low) / 2;
                        var offset = table + mid * 8;
                        var value = BinaryPrimitives.ReadInt32LittleEndian(fiber.Code.AsSpan(offset));
                        if (value == fiber.Stack[^1])
                        {
                            Pop(fiber);
                            Jump(fiber, BinaryPrimitives.ReadInt32LittleEndian(fiber.Code.AsSpan(offset + 4)));
                            break;
                        }
                        if (value < fiber.Stack[^1]) low = mid + 1;
                        else high = mid - 1;
                    }
                    break;
                }
                case (int)AcsPcode.Drop:
                    Pop(fiber);
                    break;
                default:
                    fiber.Done = true;
                    return;
            }
        }
    }

    private void RunArray(Fiber fiber, int opcode)
    {
        var bitwise = opcode >= (int)AcsPcode.AndWorldArray;
        var global = bitwise ? (opcode - (int)AcsPcode.AndWorldArray) % 7 == 1 : opcode >= (int)AcsPcode.PushGlobalArray;
        var operation = bitwise ? 9 + (opcode - (int)AcsPcode.AndWorldArray) / 7
            : opcode - (global ? (int)AcsPcode.PushGlobalArray : (int)AcsPcode.PushWorldArray);
        var array = ReadI32(fiber);
        var required = operation is 0 or 7 or 8 ? 1 : 2;
        if (fiber.Done || (uint)array >= (global ? 64u : 256u) || fiber.Stack.Count < required)
        { fiber.Done = true; return; }
        var operand = required == 2 ? Pop(fiber) : 0;
        var key = (array, Pop(fiber)); // Native sparse array keys are signed 32-bit integers.
        var arrays = global ? _globalArrays : _worldArrays;
        arrays.TryGetValue(key, out var current);
        if (operation == 0) { fiber.Stack.Add(current); return; }
        if (operation is 5 or 6 && operand == 0) { fiber.Done = true; return; }
        var result = ArrayResult(operation, current, operand);
        // Missing entries read as zero; canonicalize zeros for stable state hashes.
        if (result == 0) arrays.Remove(key);
        else arrays[key] = result;
    }

    private void RunMapArray(Fiber fiber, int opcode)
    {
        var operation = opcode >= (int)AcsPcode.AndMapArray ? 9 + (opcode - (int)AcsPcode.AndMapArray) / 7
            : opcode - (int)AcsPcode.PushMapArray;
        var variable = ReadI32(fiber);
        var required = operation is 0 or 7 or 8 ? 1 : 2;
        if (fiber.Done || (uint)variable >= 128 || fiber.Stack.Count < required)
        { fiber.Done = true; return; }
        var operand = required == 2 ? Pop(fiber) : 0;
        var index = Pop(fiber);
        var id = _mapVariables[variable];
        var array = (uint)id < (uint)_mapArrays.Length ? _mapArrays[id] : null;
        var inRange = array is not null && (uint)index < (uint)array.Length;
        var current = inRange ? array![index] : 0;
        if (operation == 0) { fiber.Stack.Add(current); return; }
        if (operation is 5 or 6 && operand == 0) { fiber.Done = true; return; }
        if (inRange) array![index] = ArrayResult(operation, current, operand);
    }

    private static void RunLocalArray(Fiber fiber, int opcode)
    {
        var operation = opcode switch { 364 => 1, 365 => 0, _ => opcode - 364 };
        var id = ReadI32(fiber);
        var required = operation is 0 or 7 or 8 ? 1 : 2;
        if (fiber.Done || fiber.Stack.Count < required) { fiber.Done = true; return; }
        var operand = required == 2 ? Pop(fiber) : 0;
        var index = Pop(fiber);
        var inRange = (uint)id < (uint)fiber.ArraySizes.Length && (uint)index < (uint)fiber.ArraySizes[id];
        var offset = inRange ? fiber.ArrayOffsets[id] + index : 0;
        var current = inRange ? fiber.Locals[offset] : 0;
        if (operation == 0) { fiber.Stack.Add(current); return; }
        if (operation is 5 or 6 && operand == 0) { fiber.Done = true; return; }
        if (inRange) fiber.Locals[offset] = ArrayResult(operation, current, operand);
    }

    private static int ArrayResult(int operation, int current, int operand) => operation switch
        {
            1 => operand,
            2 => unchecked(current + operand),
            3 => unchecked(current - operand),
            4 => unchecked(current * operand),
            5 => unchecked((int)((long)current / operand)),
            6 => (int)((long)current % operand),
            7 => unchecked(current + 1),
            8 => unchecked(current - 1),
            9 => current & operand,
            10 => current ^ operand,
            11 => current | operand,
            12 => current << operand,
            13 => current >> operand,
            _ => throw new InvalidOperationException("Unsupported ACS array operation."),
        };

    private static void Jump(Fiber fiber, int destination, bool moduleAddress = true)
    {
        var relative = (long)destination - (moduleAddress ? fiber.CodeBaseOffset : 0);
        if (fiber.Done || relative < 0 || relative > fiber.Code.Length - 4)
            fiber.Done = true;
        else fiber.Pc = (int)relative;
    }

    private static void Binary(Fiber fiber, int opcode)
    {
        if (fiber.Stack.Count < 2) { fiber.Done = true; return; }
        var right = Pop(fiber);
        var left = Pop(fiber);
        if (opcode is (int)AcsPcode.Divide or (int)AcsPcode.Modulus && right == 0)
        {
            fiber.Stack.Add(0);
            return;
        }
        fiber.Stack.Add(opcode switch
        {
            14 => unchecked(left + right), 15 => unchecked(left - right), 16 => unchecked(left * right),
            17 => unchecked((int)((long)left / right)), 18 => (int)((long)left % right),
            19 => left == right ? 1 : 0, 20 => left != right ? 1 : 0,
            21 => left < right ? 1 : 0, 22 => left > right ? 1 : 0,
            23 => left <= right ? 1 : 0, 24 => left >= right ? 1 : 0,
            70 => left != 0 && right != 0 ? 1 : 0, 71 => left != 0 || right != 0 ? 1 : 0,
            72 => left & right, 73 => left | right, 74 => left ^ right,
            76 => left << right, 77 => left >> right,
            (int)AcsPcode.FixedMul => unchecked((int)(((long)left * right) >> 16)),
            (int)AcsPcode.FixedDiv => FixedDivide(left, right),
            _ => throw new InvalidOperationException("Unsupported binary ACS opcode."),
        });
    }

    private static int FixedDivide(int numerator, int denominator)
    {
        // Match PCD_FIXEDDIV's conservative overflow guard, including zero divisors.
        // Widen before taking absolute values so INT_MIN has magnitude 2^31.
        if ((Math.Abs((long)numerator) >> 15) >= Math.Abs((long)denominator))
            return (numerator ^ denominator) < 0 ? int.MinValue : int.MaxValue;
        return (int)(((long)numerator << 16) / denominator);
    }

    private static int ThingCount(AuthoritySimulation sim, int type, int tid, int tag = -1)
    {
        var count = 0;
        foreach (var actor in sim.Actors)
        {
            if (actor.Destroyed || actor.Health <= 0 || !actor.IsMapActor) continue;
            if (type > 0 && actor.DoomEdNum != type) continue;
            if (tid != 0 && actor.ThingId != tid) continue;
            if (tag >= 0)
            {
                if (actor.SectorIndex < 0 || actor.SectorIndex >= sim.Level.Sectors.Count) continue;
                if (!sim.Level.Sectors[actor.SectorIndex].MatchesTag(tag)) continue;
            }
            count++;
        }
        return count;
    }

    private static int ThingCountByName(Fiber fiber, AuthoritySimulation sim, int stringId, int tid, int tag)
    {
        if (stringId < 0 || stringId >= fiber.StringTable.Length)
            return 0;
        if (!DoomActorCatalog.TryEditorNumberForClassName(fiber.StringTable[stringId], out var editorNumber))
            return 0;
        return ThingCount(sim, editorNumber, tid, tag);
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

    private static int ReadByte(Fiber fiber)
    {
        if ((uint)fiber.Pc >= (uint)fiber.Code.Length) { fiber.Done = true; return 0; }
        return fiber.Code[fiber.Pc++];
    }

    private static int ReadI32(Fiber fiber)
    {
        if (fiber.Pc < 0 || fiber.Pc > fiber.Code.Length - 4)
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
        public Fiber(AcsProgram program, uint codeHash, ReadOnlySpan<int> arguments, bool always, Actor? activator, LevelLine? triggerLine, bool backSide)
        {
            ActivatorBinding = new AcsActivatorBinding { Value = activator };
            TriggerLine = triggerLine; BackSide = backSide;
            Number = program.Number;
            Always = always;
            CodeHash = codeHash;
            Code = program.Code;
            CodeBaseOffset = program.CodeBaseOffset;
            JumpPoints = program.JumpPoints;
            StringTable = program.StringTable;
            LegacyHexenDelay = program.LegacyHexenDelay;
            ArraySizes = program.LocalArraySizes;
            ArrayOffsets = new int[ArraySizes.Length];
            var slots = program.LocalVariableCount;
            for (var i = 0; i < ArraySizes.Length; i++) { ArrayOffsets[i] = slots; slots += ArraySizes[i]; }
            Locals = new int[slots];
            arguments[..Math.Min(arguments.Length, program.ArgumentCount)].CopyTo(Locals);
        }

        public byte[] Code { get; }
        public int CodeBaseOffset { get; }
        public int[] JumpPoints { get; }
        public string[] StringTable { get; }
        public AcsActivatorBinding ActivatorBinding { get; }
        public Actor? Activator
        {
            get => ActivatorBinding.Value;
            set => ActivatorBinding.Value = value;
        }
        public LevelLine? TriggerLine { get; }
        public bool BackSide { get; }
        public int Number { get; }
        public bool Always { get; }
        public uint CodeHash { get; }
        public bool LegacyHexenDelay { get; }
        public int Pc { get; set; }
        public int Wait { get; set; }
        // 0: running, 1: waiting for the numbered script to start, 2: waiting for it to finish.
        public int ScriptWaitState { get; set; }
        public int ScriptWaitTarget { get; set; }
        public int? TagWait { get; set; }
        public bool Done { get; set; }
        public bool Suspended { get; set; }
        public List<int> Stack { get; } = new();
        public int[] Locals { get; }
        public int[] ArraySizes { get; }
        public int[] ArrayOffsets { get; }
    }
}
