using HCDE.MapLoader;

namespace HCDE.Playsim;

public sealed partial class AcsVm
{
    internal SimAcsState CaptureState(ulong? acsRandomState)
    {
        return new SimAcsState
        {
            Globals = (int[])_globals.Clone(),
            MapVariables = (int[])_mapVariables.Clone(),
            WorldVariables = (int[])_worldVariables.Clone(),
            WorldArrays = CaptureSparse(_worldArrays),
            GlobalArrays = CaptureSparse(_globalArrays),
            MapArrays = _mapArrays.Select(array => (int[])array.Clone()).ToArray(),
            GlobalStrings = _globalStrings.Snapshot(),
            Fibers = _fibers.Where(fiber => !fiber.Done).Select(CaptureFiber).ToArray(),
            RandomState = acsRandomState,
            ScriptResult = this.ScriptResult,
        };
    }

    internal void RestoreState(SimAcsState state, AuthoritySimulation sim)
    {
        ArgumentNullException.ThrowIfNull(state);
        _fibers.Clear();
        Array.Copy(state.Globals, _globals, Math.Min(state.Globals.Length, _globals.Length));
        Array.Copy(state.MapVariables, _mapVariables, Math.Min(state.MapVariables.Length, _mapVariables.Length));
        Array.Copy(state.WorldVariables, _worldVariables, Math.Min(state.WorldVariables.Length, _worldVariables.Length));
        RestoreSparse(_worldArrays, state.WorldArrays);
        RestoreSparse(_globalArrays, state.GlobalArrays);
        _mapArrays = state.MapArrays.Select(array => (int[])array.Clone()).ToArray();
        _globalStrings.ReplaceFrom(state.GlobalStrings);
        ScriptResult = state.ScriptResult;
        foreach (var saved in state.Fibers)
        {
            if (!_programs.TryGetValue(saved.ScriptNumber, out var program))
                throw new InvalidOperationException("save-acs-program-missing");
            if (_programHashes[saved.ScriptNumber] != saved.CodeHash)
                throw new InvalidOperationException("save-acs-program-hash");
            var fiber = new Fiber(program, saved.CodeHash, ReadOnlySpan<int>.Empty, saved.Always,
                ResolveActivator(sim, saved.ActivatorId),
                ResolveTriggerLine(sim, saved.TriggerLineIndex),
                saved.BackSide)
            {
                Pc = saved.Pc,
                Wait = saved.Wait,
                ScriptWaitState = saved.ScriptWaitState,
                ScriptWaitTarget = saved.ScriptWaitTarget,
                TagWait = saved.TagWait < 0 ? null : saved.TagWait,
                Suspended = saved.Suspended,
            };
            fiber.Stack.AddRange(saved.Stack);
            for (var i = 0; i < saved.Locals.Count; i++) fiber.Locals[i] = saved.Locals[i];
            _fibers.Add(fiber);
        }
    }

    private static Actor? ResolveActivator(AuthoritySimulation sim, uint activatorId) =>
        activatorId == 0 ? null : sim.Actors.FirstOrDefault(actor => actor.Id == activatorId && !actor.Destroyed);

    private static LevelLine? ResolveTriggerLine(AuthoritySimulation sim, int index) =>
        index < 0 || index >= sim.Level.Lines.Count ? null : sim.Level.Lines[index];

    private static SimAcsFiberState CaptureFiber(Fiber fiber) => new(
        fiber.Number,
        fiber.CodeHash,
        fiber.Always,
        fiber.Suspended,
        fiber.LegacyHexenDelay,
        fiber.Activator is { Destroyed: false } actor ? actor.Id : 0u,
        fiber.TriggerLine?.Index ?? -1,
        fiber.BackSide,
        fiber.Pc,
        fiber.Wait,
        fiber.ScriptWaitState,
        fiber.ScriptWaitTarget,
        fiber.TagWait ?? -1,
        fiber.Stack.ToArray(),
        fiber.Locals.ToArray());

    private static IReadOnlyList<SimSparseIntArray> CaptureSparse(Dictionary<(int Array, int Index), int> arrays)
    {
        if (arrays.Count == 0) return Array.Empty<SimSparseIntArray>();
        return arrays.GroupBy(entry => entry.Key.Array)
            .OrderBy(group => group.Key)
            .Select(group => new SimSparseIntArray(group.Key,
                group.OrderBy(entry => entry.Key.Index)
                    .Select(entry => new SimSparseIntEntry(entry.Key.Index, entry.Value))
                    .ToArray()))
            .ToArray();
    }

    private static void RestoreSparse(Dictionary<(int Array, int Index), int> arrays, IReadOnlyList<SimSparseIntArray> saved)
    {
        arrays.Clear();
        foreach (var array in saved)
            foreach (var entry in array.Entries)
                arrays[(array.ArrayId, entry.Index)] = entry.Value;
    }
}
